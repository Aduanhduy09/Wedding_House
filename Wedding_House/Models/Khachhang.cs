using System;
using System.Collections.Generic;

namespace Wedding_House.Models;

public partial class Khachhang
{
    public int MaKhachHang { get; set; }

    public string HoTen { get; set; } = null!;

    public string? DienThoai { get; set; }

    public int? MaTaiKhoan { get; set; }

    public virtual Taikhoan? MaTaiKhoanNavigation { get; set; }

    public virtual ICollection<Tieccuoi> Tieccuois { get; set; } = new List<Tieccuoi>();
}
