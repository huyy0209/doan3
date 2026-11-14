using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace DoAnWD.Models
{
    public partial class GiaSuContextDB : DbContext
    {
        static GiaSuContextDB()
        {
            // CSDL được tạo bằng script Database/01_QuanLyGiaSu.sql
            // => tắt cơ chế tự tạo/kiểm tra model của EF
            Database.SetInitializer<GiaSuContextDB>(null);
        }

        public GiaSuContextDB()
            : base("name=GiaSuContextDB")
        {
        }

        public virtual DbSet<GiaSu> GiaSus { get; set; }
        public virtual DbSet<HocVien> HocViens { get; set; }
        public virtual DbSet<LopHoc> LopHocs { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<BuoiHoc> BuoiHocs { get; set; }
        public virtual DbSet<MonHocDanhMuc> MonHocs { get; set; }
        public virtual DbSet<CaiDat> CaiDats { get; set; }
        public virtual DbSet<ThanhToan> ThanhToans { get; set; }
        public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GiaSu>()
                .Property(e => e.SDT)
                .IsUnicode(false);

            modelBuilder.Entity<HocVien>()
                .Property(e => e.SDT)
                .IsUnicode(false);

            modelBuilder.Entity<HocVien>()
                .Property(e => e.SDTPhuHuynh)
                .IsUnicode(false);

            modelBuilder.Entity<TaiKhoan>()
                .Property(e => e.TenDangNhap)
                .IsUnicode(false);

            modelBuilder.Entity<TaiKhoan>()
                .Property(e => e.MatKhauHash)
                .IsUnicode(false);

            modelBuilder.Entity<BuoiHoc>()
                .Property(e => e.Diem)
                .HasPrecision(4, 2);
        }

        /// <summary>
        /// LOW-02: Tự động tra cứu và đồng bộ MaMon cho LopHoc từ tên môn MonHoc
        /// </summary>
        public override int SaveChanges()
        {
            DongBoMaMonLopHoc();
            return base.SaveChanges();
        }

        private void DongBoMaMonLopHoc()
        {
            try
            {
                var entries = ChangeTracker.Entries<LopHoc>()
                    .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                    .ToList();

                if (entries.Count == 0) return;

                // Đồng bộ nếu đối tượng MonHocDM được gán trực tiếp
                foreach (var entry in entries)
                {
                    if (entry.Entity.MonHocDM != null)
                    {
                        if (string.IsNullOrEmpty(entry.Entity.MonHoc))
                            entry.Entity.MonHoc = entry.Entity.MonHocDM.TenMon;
                        if (!entry.Entity.MaMon.HasValue)
                            entry.Entity.MaMon = entry.Entity.MonHocDM.MaMon;
                    }
                }

                // Lấy danh sách tên môn cần tra cứu
                var tenMons = entries
                    .Select(e => e.Entity.MonHoc)
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Distinct()
                    .ToList();

                if (tenMons.Count > 0)
                {
                    // Tra cứu danh mục MonHoc (kết hợp cả Local cache và Database)
                    var localMon = MonHocs.Local.Where(m => tenMons.Contains(m.TenMon, StringComparer.OrdinalIgnoreCase)).ToList();
                    var dbMon = MonHocs.Where(m => tenMons.Contains(m.TenMon)).ToList();
                    var monDict = localMon.Concat(dbMon)
                        .GroupBy(m => m.TenMon, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(g => g.Key, g => g.First().MaMon, StringComparer.OrdinalIgnoreCase);

                    foreach (var entry in entries)
                    {
                        var lop = entry.Entity;
                        if (!string.IsNullOrWhiteSpace(lop.MonHoc))
                        {
                            int ma;
                            if (monDict.TryGetValue(lop.MonHoc, out ma))
                                lop.MaMon = ma;
                            else
                                lop.MaMon = null; // Tên không khớp trong danh mục thì để NULL theo yêu cầu
                        }
                        else
                        {
                            lop.MaMon = null;
                        }
                    }
                }
            }
            catch
            {
                // LOW-02: Không làm gián đoạn SaveChanges nếu có lỗi tra cứu
            }
        }
    }
}
