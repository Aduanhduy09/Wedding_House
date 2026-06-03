using System;
using System.Collections.Generic;

namespace Wedding_House.Models;

public partial class Tieccuoi
{
    public int MaDatTiecCuoi { get; set; }

    public int MaKhachHang { get; set; }

    public string TenChuRe { get; set; } = null!;

    public string TenCoDau { get; set; } = null!;

    public DateOnly NgayDaiTiec { get; set; }

    public int MaCa { get; set; }

    public int MaSanh { get; set; }

    public int SoLuongBan { get; set; }

    public int? SoBanDuTru { get; set; }

    public DateTime NgayLap { get; set; } = DateTime.Now;

    public decimal? TienDatCoc { get; set; }

    public int TrangThai { get; set; }

    public virtual ICollection<Chitietdatmon> Chitietdatmons { get; set; } = new List<Chitietdatmon>();

    public virtual ICollection<Chitietdichvu> Chitietdichvus { get; set; } = new List<Chitietdichvu>();

    public virtual Hoadon? Hoadon { get; set; }

    public virtual Ca MaCaNavigation { get; set; } = null!;

    public virtual Khachhang MaKhachHangNavigation { get; set; } = null!;

    public virtual Sanh MaSanhNavigation { get; set; } = null!;

    public virtual ICollection<Phancong> Phancongs { get; set; } = new List<Phancong>();
}
