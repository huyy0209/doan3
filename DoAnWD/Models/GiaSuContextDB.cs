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
    }
}
