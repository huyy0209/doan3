using System.Text.Json;

namespace DoAnWeb.Infrastructure;

/// <summary>Mốc thu hồi cookie được lưu ngoài CSDL vì restore thay toàn bộ nội dung DB.</summary>
public sealed class MocKhoiPhucPhien
{
    private readonly object _gate = new();
    private readonly string _path;
    private DateTimeOffset? _utc;

    public MocKhoiPhucPhien(IWebHostEnvironment environment)
    {
        var folder = Path.Combine(environment.ContentRootPath, "logs");
        Directory.CreateDirectory(folder);
        _path = Path.Combine(folder, "restore-state.json");
        if (!File.Exists(_path)) return;
        try
        {
            _utc = JsonSerializer.Deserialize<State>(File.ReadAllText(_path))?.RestoredUtc;
        }
        catch (JsonException) { }
        // Tệp hỏng: không chặn khởi động, coi như vừa khôi phục lúc tệp được ghi (thu hồi mọi phiên cũ hơn).
        _utc ??= new DateTimeOffset(File.GetLastWriteTimeUtc(_path), TimeSpan.Zero);
    }

    public DateTimeOffset? RestoredUtc { get { lock (_gate) return _utc; } }

    public void GhiMoc(DateTimeOffset utc)
    {
        utc = utc.ToUniversalTime();
        lock (_gate)
        {
            var temp = _path + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(new State(utc)));
            File.Move(temp, _path, true);
            _utc = utc;
        }
    }

    private sealed record State(DateTimeOffset RestoredUtc);
}
