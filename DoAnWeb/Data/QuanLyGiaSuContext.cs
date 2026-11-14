using System;
using System.Collections.Generic;
using DoAnWeb.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DoAnWeb.Data;

public partial class QuanLyGiaSuContext : DbContext
{
    public QuanLyGiaSuContext(DbContextOptions<QuanLyGiaSuContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BuoiHoc> BuoiHocs { get; set; }

    public virtual DbSet<CaiDat> CaiDats { get; set; }

    public virtual DbSet<GiaSu> GiaSus { get; set; }

    public virtual DbSet<HocVien> HocViens { get; set; }

    public virtual DbSet<LopHoc> LopHocs { get; set; }

    public virtual DbSet<MonHocDanhMuc> MonHocs { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<ThanhToan> ThanhToans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BuoiHoc>(entity =>
        {
            entity.HasKey(e => e.MaBuoi);

            entity.HasIndex(e => new { e.NgayHoc, e.CaHoc }, "IX_BuoiHoc_HocBu_Ngay").HasFilter("([MaBuoiGoc] IS NOT NULL)");

            entity.HasIndex(e => e.MaBuoiGoc, "UX_BuoiHoc_BuoiGoc")
                .IsUnique()
                .HasFilter("([MaBuoiGoc] IS NOT NULL)");

            entity.HasIndex(e => new { e.MaLop, e.NgayHoc }, "UX_BuoiHoc_Lop_Ngay_Thuong")
                .IsUnique()
                .HasFilter("([MaBuoiGoc] IS NULL)");

            entity.Property(e => e.CaHoc).HasMaxLength(50);
            entity.Property(e => e.CapDoThi).HasMaxLength(50);
            entity.Property(e => e.Diem).HasColumnType("decimal(4, 2)");
            entity.Property(e => e.DiemDanh)
                .IsRequired()
                .HasMaxLength(30)
                .HasDefaultValue("Có mặt");
            entity.Property(e => e.NhanXet).HasMaxLength(300);
            entity.Property(e => e.NgayHoc).HasColumnType("date");

            entity.HasOne(d => d.BuoiGoc).WithOne(p => p.BuoiBu)
                .HasForeignKey<BuoiHoc>(d => d.MaBuoiGoc)
                .HasConstraintName("FK_BuoiHoc_BuoiGoc");

            entity.HasOne(d => d.GiaSuDay).WithMany(p => p.BuoiHocs)
                .HasForeignKey(d => d.MaGSDay)
                .HasConstraintName("FK_BuoiHoc_GiaSuDay");

            entity.HasOne(d => d.LopHoc).WithMany(p => p.BuoiHocs)
                .HasForeignKey(d => d.MaLop)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BuoiHoc_LopHoc");
        });

        modelBuilder.Entity<CaiDat>(entity =>
        {
            entity.HasKey(e => e.Khoa);

            entity.Property(e => e.Khoa)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GiaTri).HasMaxLength(500);
        });

        modelBuilder.Entity<GiaSu>(entity =>
        {
            entity.HasKey(e => e.MaGS).HasName("PK__GiaSu__2725AE8ED63F74C1");

            entity.Property(e => e.CaNhanDay).HasMaxLength(200);
            entity.Property(e => e.ChuyenMon).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.HocVan).HasMaxLength(150);
            entity.Property(e => e.SDT)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasDefaultValue("Sẵn sàng");
        });

        modelBuilder.Entity<HocVien>(entity =>
        {
            entity.HasKey(e => e.MaHV).HasName("PK__HocVien__2725A6D203B23E92");

            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.GhiChu).HasMaxLength(500);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.SDT)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SDTPhuHuynh)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TenPhuHuynh).HasMaxLength(100);
            entity.Property(e => e.TrinhDo).HasMaxLength(50);
            entity.Property(e => e.TruongLop).HasMaxLength(150);
        });

        modelBuilder.Entity<LopHoc>(entity =>
        {
            entity.HasKey(e => e.MaLop).HasName("PK__LopHoc__3B98D2738A2E23FF");

            entity.HasIndex(e => new { e.TrangThai, e.ThoiGianBatDau, e.ThoiGianKetThuc }, "IX_LopHoc_TrangThai_ThoiGian");

            entity.HasIndex(e => e.TenLop, "UX_LopHoc_TenLop")
                .IsUnique()
                .HasFilter("([TenLop] IS NOT NULL)");

            entity.Property(e => e.CaHoc).HasMaxLength(50);
            entity.Property(e => e.CapDoDauVao).HasMaxLength(50);
            entity.Property(e => e.HocPhi).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MonHoc).HasMaxLength(50);
            entity.Property(e => e.NgayHocTrongTuan).HasMaxLength(50);
            entity.Property(e => e.TenLop).HasMaxLength(150);
            entity.Property(e => e.ThoiGianBatDau).HasColumnType("datetime");
            entity.Property(e => e.ThoiGianKetThuc).HasColumnType("datetime");
            entity.Property(e => e.ThuHoc)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai).HasMaxLength(50);

            entity.HasOne(d => d.GiaSu).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.MaGS)
                .HasConstraintName("FK__LopHoc__MaGS__3C69FB99");

            entity.HasOne(d => d.HocVien).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.MaHV)
                .HasConstraintName("FK__LopHoc__MaHV__3D5E1FD2");

            entity.HasOne(d => d.MonHocDM).WithMany(p => p.LopHocs)
                .HasForeignKey(d => d.MaMon)
                .HasConstraintName("FK_LopHoc_MonHoc");
        });

        modelBuilder.Entity<MonHocDanhMuc>(entity =>
        {
            entity.ToTable("MonHoc");
            entity.HasKey(e => e.MaMon);

            entity.HasIndex(e => e.TenMon, "UQ_MonHoc_TenMon").IsUnique();

            entity.Property(e => e.CapDo).HasMaxLength(500);
            entity.Property(e => e.LoaiMon)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("Chính");
            entity.Property(e => e.TenMon)
                .IsRequired()
                .HasMaxLength(50);
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTK);

            entity.HasIndex(e => e.TenDangNhap, "UQ_TaiKhoan_TenDangNhap").IsUnique();

            entity.HasIndex(e => e.MaGS, "UX_TaiKhoan_MaGS")
                .IsUnique()
                .HasFilter("([MaGS] IS NOT NULL)");

            entity.Property(e => e.HoatDong).HasDefaultValue(true);
            entity.Property(e => e.KhoaDen).HasColumnType("datetime");
            entity.Property(e => e.MatKhauHash)
                .IsRequired()
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TenDangNhap)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VaiTro)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasOne(d => d.GiaSu).WithOne(p => p.TaiKhoan)
                .HasForeignKey<TaiKhoan>(d => d.MaGS)
                .HasConstraintName("FK_TaiKhoan_GiaSu");
        });

        modelBuilder.Entity<ThanhToan>(entity =>
        {
            entity.HasKey(e => e.MaTT);

            entity.HasIndex(e => e.MaLop, "IX_ThanhToan_MaLop");

            entity.Property(e => e.GhiChu).HasMaxLength(200);
            entity.Property(e => e.HinhThuc).HasMaxLength(30);
            entity.Property(e => e.NgayDong)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.LopHoc).WithMany(p => p.ThanhToans)
                .HasForeignKey(d => d.MaLop)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ThanhToan_LopHoc");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
