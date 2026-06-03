using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Wedding_House.Models;

public partial class WeddingHouseContext : DbContext
{
    public WeddingHouseContext()
    {
    }

    public WeddingHouseContext(DbContextOptions<WeddingHouseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Baocaodoanhso> Baocaodoanhsos { get; set; }

    public virtual DbSet<Ca> Cas { get; set; }

    public virtual DbSet<Chitietbcd> Chitietbcds { get; set; }

    public virtual DbSet<Chitietdatmon> Chitietdatmons { get; set; }

    public virtual DbSet<Chitietdichvu> Chitietdichvus { get; set; }

    public virtual DbSet<Dichvu> Dichvus { get; set; }

    public virtual DbSet<Hoadon> Hoadons { get; set; }

    public virtual DbSet<Khachhang> Khachhangs { get; set; }

    public virtual DbSet<Monan> Monans { get; set; }

    public virtual DbSet<Nhanvien> Nhanviens { get; set; }

    public virtual DbSet<Phancong> Phancongs { get; set; }

    public virtual DbSet<Sanh> Sanhs { get; set; }

    public virtual DbSet<Taikhoan> Taikhoans { get; set; }

    public virtual DbSet<Thamso> Thamsos { get; set; }

    public virtual DbSet<Tieccuoi> Tieccuois { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-6GM9FIJ\\MSSQLSERVER01;Database=Wedding_House_DB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Baocaodoanhso>(entity =>
        {
            entity.HasKey(e => e.MaBaoCaoDoanhSo).HasName("PK__BAOCAODO__7DA17E82890B654C");

            entity.ToTable("BAOCAODOANHSO");

            entity.HasIndex(e => e.Thang, "UQ__BAOCAODO__2DD4F54B502672C3").IsUnique();

            entity.Property(e => e.Thang).HasMaxLength(20);
            entity.Property(e => e.TongDoanhThu)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Ca>(entity =>
        {
            entity.HasKey(e => e.MaCa).HasName("PK__CA__27258E7B39B967E8");

            entity.ToTable("CA");

            entity.HasIndex(e => e.TenCa, "UQ__CA__4CF92211980001E6").IsUnique();

            entity.Property(e => e.Gio).HasMaxLength(50);
            entity.Property(e => e.TenCa).HasMaxLength(50);
        });

        modelBuilder.Entity<Chitietbcd>(entity =>
        {
            entity.HasKey(e => e.MaChiTietBcds).HasName("PK__CHITIETB__133304D9572CCFE8");

            entity.ToTable("CHITIETBCDS");

            entity.HasIndex(e => new { e.MaBaoCaoDoanhSo, e.Ngay }, "UQ__CHITIETB__4B1DB0F821C88831").IsUnique();

            entity.Property(e => e.MaChiTietBcds).HasColumnName("MaChiTietBCDS");
            entity.Property(e => e.DoanhThu)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SoLuongTiecCuoi).HasDefaultValue(0);

            entity.HasOne(d => d.MaBaoCaoDoanhSoNavigation).WithMany(p => p.Chitietbcds)
                .HasForeignKey(d => d.MaBaoCaoDoanhSo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHITIETBC__MaBao__4316F928");
        });

        modelBuilder.Entity<Chitietdatmon>(entity =>
        {
            entity.HasKey(e => e.MaCtdatMon).HasName("PK__CHITIETD__A4F143B6F8222001");

            entity.ToTable("CHITIETDATMON");

            entity.HasIndex(e => new { e.MaDatTiecCuoi, e.MaMonAn }, "UQ__CHITIETD__D097B593EB2F5333").IsUnique();

            entity.Property(e => e.MaCtdatMon).HasColumnName("MaCTDatMon");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.GhiChu).HasMaxLength(255);

            entity.HasOne(d => d.MaDatTiecCuoiNavigation).WithMany(p => p.Chitietdatmons)
                .HasForeignKey(d => d.MaDatTiecCuoi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHITIETDA__MaDat__5FB337D6");

            entity.HasOne(d => d.MaMonAnNavigation).WithMany(p => p.Chitietdatmons)
                .HasForeignKey(d => d.MaMonAn)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHITIETDA__MaMon__60A75C0F");
        });

        modelBuilder.Entity<Chitietdichvu>(entity =>
        {
            entity.HasKey(e => e.MaCtdv).HasName("PK__CHITIETD__1E4E40E639859D70");

            entity.ToTable("CHITIETDICHVU");

            entity.HasIndex(e => new { e.MaDatTiecCuoi, e.MaDichVu }, "UQ__CHITIETD__7788A91938A17886").IsUnique();

            entity.Property(e => e.MaCtdv).HasColumnName("MaCTDV");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SoLuong).HasDefaultValue(1);
            entity.Property(e => e.ThanhTien).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaDatTiecCuoiNavigation).WithMany(p => p.Chitietdichvus)
                .HasForeignKey(d => d.MaDatTiecCuoi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHITIETDI__MaDat__656C112C");

            entity.HasOne(d => d.MaDichVuNavigation).WithMany(p => p.Chitietdichvus)
                .HasForeignKey(d => d.MaDichVu)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CHITIETDI__MaDic__66603565");
        });

        modelBuilder.Entity<Dichvu>(entity =>
        {
            entity.HasKey(e => e.MaDichVu).HasName("PK__DICHVU__C0E6DE8F3EFAFAF5");

            entity.ToTable("DICHVU");

            entity.HasIndex(e => e.TenDichVu, "UQ__DICHVU__A77D068921134544").IsUnique();

            entity.Property(e => e.TenDichVu).HasMaxLength(100);
        });

        modelBuilder.Entity<Hoadon>(entity =>
        {
            entity.HasKey(e => e.MaHoaDon).HasName("PK__HOADON__835ED13B62EB1C79");

            entity.ToTable("HOADON");

            entity.HasIndex(e => e.MaDatTiecCuoi, "UQ__HOADON__9B86C4F148FDFA98").IsUnique();

            entity.Property(e => e.TienConLai)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TienDatCoc)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TienPhat)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TongTienBan)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TongTienDichVu)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TongTienHoaDon)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaDatTiecCuoiNavigation).WithOne(p => p.Hoadon)
                .HasForeignKey<Hoadon>(d => d.MaDatTiecCuoi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__HOADON__MaDatTie__571DF1D5");
        });

        modelBuilder.Entity<Khachhang>(entity =>
        {
            entity.HasKey(e => e.MaKhachHang).HasName("PK__KHACHHAN__88D2F0E55B1858DC");

            entity.ToTable("KHACHHANG");

            entity.HasIndex(e => e.MaTaiKhoan, "UQ__KHACHHAN__AD7C65287182B141").IsUnique();

            entity.Property(e => e.DienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.HoTen).HasMaxLength(100);

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithOne(p => p.Khachhang)
                .HasForeignKey<Khachhang>(d => d.MaTaiKhoan)
                .HasConstraintName("FK__KHACHHANG__MaTai__3D5E1FD2");
        });

        modelBuilder.Entity<Monan>(entity =>
        {
            entity.HasKey(e => e.MaMonAn).HasName("PK__MONAN__B11716250315D7BF");

            entity.ToTable("MONAN");

            entity.HasIndex(e => e.TenMonAn, "UQ__MONAN__987DD97AC6EF6DB0").IsUnique();

            entity.Property(e => e.TenMonAn).HasMaxLength(100);
        });

        modelBuilder.Entity<Nhanvien>(entity =>
        {
            entity.HasKey(e => e.MaNhanVien).HasName("PK__NHANVIEN__77B2CA4784065D5B");

            entity.ToTable("NHANVIEN");

            entity.HasIndex(e => e.MaTaiKhoan, "UQ__NHANVIEN__AD7C652826A2B538").IsUnique();

            entity.Property(e => e.TenNhanVien).HasMaxLength(100);

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithOne(p => p.Nhanvien)
                .HasForeignKey<Nhanvien>(d => d.MaTaiKhoan)
                .HasConstraintName("FK__NHANVIEN__MaTaiK__398D8EEE");
        });

        modelBuilder.Entity<Phancong>(entity =>
        {
            entity.HasKey(e => e.MaPhanCong).HasName("PK__PHANCONG__C279D91668FCA4B5");

            entity.ToTable("PHANCONG");

            entity.HasIndex(e => new { e.MaNhanVien, e.MaDatTiecCuoi }, "UQ__PHANCONG__6E0AA609570D3ECF").IsUnique();

            entity.HasOne(d => d.MaDatTiecCuoiNavigation).WithMany(p => p.Phancongs)
                .HasForeignKey(d => d.MaDatTiecCuoi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PHANCONG__MaDatT__5BE2A6F2");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.Phancongs)
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PHANCONG__MaNhan__5AEE82B9");
        });

        modelBuilder.Entity<Sanh>(entity =>
        {
            entity.HasKey(e => e.MaSanh).HasName("PK__SANH__B234A777D78EAB21");

            entity.ToTable("SANH");

            entity.HasIndex(e => e.TenSanh, "UQ__SANH__CA6B2CA03FB269A6").IsUnique();

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.LoaiSanh).HasMaxLength(50);
            entity.Property(e => e.TenSanh).HasMaxLength(100);
        });

        modelBuilder.Entity<Taikhoan>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan).HasName("PK__TAIKHOAN__AD7C652973352E50");

            entity.ToTable("TAIKHOAN");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TAIKHOAN__55F68FC0A1488E8D").IsUnique();

            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.VaiTro).HasMaxLength(50);
        });

        modelBuilder.Entity<Thamso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__THAMSO__3214EC078B9A9F87");

            entity.ToTable("THAMSO");

            entity.Property(e => e.DonGiaBanToiThieu).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.LoaiSanh).HasMaxLength(50);
        });

        modelBuilder.Entity<Tieccuoi>(entity =>
        {
            entity.HasKey(e => e.MaDatTiecCuoi).HasName("PK__TIECCUOI__9B86C4F0062A4634");

            entity.ToTable("TIECCUOI");

            entity.HasIndex(e => new { e.NgayDaiTiec, e.MaCa, e.MaSanh }, "UQ_LichDatSanh").IsUnique();

            entity.Property(e => e.SoBanDuTru).HasDefaultValue(0);
            entity.Property(e => e.TenChuRe).HasMaxLength(100);
            entity.Property(e => e.TenCoDau).HasMaxLength(100);
            entity.Property(e => e.TienDatCoc)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.MaCaNavigation).WithMany(p => p.Tieccuois)
                .HasForeignKey(d => d.MaCa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TIECCUOI__MaCa__49C3F6B7");

            entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.Tieccuois)
                .HasForeignKey(d => d.MaKhachHang)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TIECCUOI__MaKhac__48CFD27E");

            entity.HasOne(d => d.MaSanhNavigation).WithMany(p => p.Tieccuois)
                .HasForeignKey(d => d.MaSanh)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TIECCUOI__MaSanh__4AB81AF0");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
