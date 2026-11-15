using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace DoAnWeb.Tests.Integration;

/// <summary>Một trang trả về: mã trạng thái, redirect, HTML đã phân tích.</summary>
public sealed class Trang
{
    public string Url { get; init; }
    public HttpStatusCode Status { get; init; }
    public string Location { get; init; }
    public string Html { get; init; }
    public IHtmlDocument Doc { get; init; }

    public bool LaRedirect => (int)Status >= 300 && (int)Status < 400;

    /// <summary>Redirect có đích là path này (so path, bỏ query).</summary>
    public bool RedirectToi(string path)
    {
        if (!LaRedirect || Location == null) return false;
        string p = Location.Contains("://") ? new Uri(Location).PathAndQuery : Location;
        p = p.Split('?')[0];
        return string.Equals(p, path, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Chuyển về trang đăng nhập (cookie chưa có / bị từ chối)</summary>
    public bool VeDangNhap => LaRedirect && Location != null &&
        Location.Contains("/TaiKhoan/DangNhap", StringComparison.OrdinalIgnoreCase);

    /// <summary>Bị chặn: 403, hoặc redirect tới trang không có quyền / đăng nhập</summary>
    public bool BiChan => Status == HttpStatusCode.Forbidden ||
        (LaRedirect && Location != null &&
         (Location.Contains("KhongCoQuyen", StringComparison.OrdinalIgnoreCase) ||
          Location.Contains("/TaiKhoan/DangNhap", StringComparison.OrdinalIgnoreCase)));

    public string ThongBaoThanhCong => (Doc?.QuerySelector(".alert-success") ?? Doc?.QuerySelector(".vo-toast-success"))?.TextContent.Trim();
    public string ThongBaoLoi => (Doc?.QuerySelector(".alert-danger") ?? Doc?.QuerySelector(".vo-toast-danger"))?.TextContent.Trim();

    /// <summary>Toàn bộ chữ trong trang (để tìm câu lỗi validate/summary)</summary>
    public string Chu => Doc?.Body?.TextContent ?? Html ?? "";

    /// <summary>Lấy form (theo CSS selector) thành danh sách field như trình duyệt sẽ gửi.</summary>
    public FormDuLieu Form(string selector)
    {
        var form = Doc.QuerySelector(selector) as IHtmlFormElement
                   ?? throw new InvalidOperationException($"Không thấy form '{selector}' trên {Url}");
        var f = new FormDuLieu { Action = form.GetAttribute("action") ?? Url, Url = Url };
        foreach (var el in form.QuerySelectorAll("input, select, textarea"))
        {
            string name = el.GetAttribute("name");
            if (string.IsNullOrEmpty(name) || el.HasAttribute("disabled")) continue;
            switch (el)
            {
                case IHtmlInputElement inp:
                    string type = (inp.Type ?? "text").ToLowerInvariant();
                    if (type is "submit" or "button" or "image" or "reset" or "file") continue;
                    if (type is "checkbox" or "radio")
                    {
                        if (!inp.IsChecked) continue;
                        f.Fields.Add(new(name, inp.Value ?? "on"));
                    }
                    else f.Fields.Add(new(name, inp.Value ?? ""));
                    break;
                case IHtmlSelectElement sel:
                    var chon = sel.SelectedOptions.FirstOrDefault() ?? sel.Options.FirstOrDefault();
                    if (chon != null) f.Fields.Add(new(name, chon.Value));
                    break;
                case IHtmlTextAreaElement ta:
                    f.Fields.Add(new(name, ta.Value ?? ""));
                    break;
            }
        }
        return f;
    }

    /// <summary>Giá trị thuộc tính của phần tử đầu tiên khớp selector (null nếu không có)</summary>
    public string Thuoc(string selector, string attr) => Doc.QuerySelector(selector)?.GetAttribute(attr);

    /// <summary>Token antiforgery của form đầu tiên có token trong trang</summary>
    public string Token =>
        (Doc.QuerySelector("input[name=__RequestVerificationToken]") as IHtmlInputElement)?.Value;
}

/// <summary>Dữ liệu một form HTML (đã điền như trình duyệt), chỉnh được trước khi gửi.</summary>
public sealed class FormDuLieu
{
    public string Url { get; init; }
    public string Action { get; set; }
    public List<KeyValuePair<string, string>> Fields { get; } = new();

    /// <summary>Đặt giá trị một field (thay mọi giá trị cũ cùng tên; thêm mới nếu chưa có)</summary>
    public FormDuLieu Dat(string name, string value)
    {
        Fields.RemoveAll(x => x.Key == name);
        Fields.Add(new(name, value));
        return this;
    }

    public FormDuLieu Bo(string name) { Fields.RemoveAll(x => x.Key == name); return this; }

    public string Lay(string name) => Fields.FirstOrDefault(x => x.Key == name).Value;
}

/// <summary>Client giả lập trình duyệt: giữ cookie, không tự theo redirect, gửi form kèm antiforgery.</summary>
public sealed class TrinhDuyet : IDisposable
{
    private static readonly HtmlParser Parser = new();
    public HttpClient Client { get; }

    public TrinhDuyet(WebAppFactory factory) { Client = factory.TaoClient(); }

    public async Task<Trang> Get(string url) => await Doc(await Client.GetAsync(url), url);

    /// <summary>GET rồi theo redirect cho tới khi ra trang 200 (tối đa 5 lần) – như trình duyệt tự theo.</summary>
    public async Task<Trang> GetTheoRedirect(string url)
    {
        var t = await Get(url);
        for (int i = 0; i < 5 && t.LaRedirect; i++) t = await Get(t.Location);
        return t;
    }

    /// <summary>Theo một redirect của trang trước (thường để đọc thông báo TempData)</summary>
    public async Task<Trang> Theo(Trang t) =>
        t.LaRedirect ? await Get(t.Location) : throw new InvalidOperationException($"{t.Url} không redirect (status {(int)t.Status})");

    public async Task<Trang> Gui(FormDuLieu f)
    {
        var url = Gop(f.Url, f.Action);
        return await Doc(await Client.PostAsync(url, new FormUrlEncodedContent(f.Fields)), url);
    }

    /// <summary>POST thẳng (không có form): tự thêm token antiforgery lấy từ trang <paramref name="trangLayToken"/>.</summary>
    public async Task<Trang> Post(string url, IEnumerable<KeyValuePair<string, string>> fields, string trangLayToken = "/TaiKhoan/DoiMatKhau", bool kemToken = true)
    {
        var list = fields.ToList();
        if (kemToken)
        {
            var token = (await Get(trangLayToken)).Token
                        ?? throw new InvalidOperationException($"Không lấy được token từ {trangLayToken}");
            list.Add(new("__RequestVerificationToken", token));
        }
        return await Doc(await Client.PostAsync(url, new FormUrlEncodedContent(list)), url);
    }

    public async Task<Trang> DangNhap(string ten, string matKhau)
    {
        var trang = await Get("/TaiKhoan/DangNhap");
        var f = trang.Form("form[action$='/TaiKhoan/DangNhap']").Dat("TenDangNhap", ten).Dat("MatKhau", matKhau);
        return await Gui(f);
    }

    private static string Gop(string trang, string action)
    {
        if (action.Contains("://")) return new Uri(action).PathAndQuery;
        if (action.StartsWith('/')) return action;
        return new Uri(new Uri("http://localhost" + trang), action).PathAndQuery;
    }

    private async Task<Trang> Doc(HttpResponseMessage r, string url)
    {
        string html = await r.Content.ReadAsStringAsync();
        string ct = r.Content.Headers.ContentType?.MediaType ?? "";
        return new Trang
        {
            Url = url,
            Status = r.StatusCode,
            Location = r.Headers.Location?.OriginalString,
            Html = html,
            Doc = Parser.ParseDocument(ct.Contains("html") ? html : "")
        };
    }

    public void Dispose() => Client.Dispose();
}
