using System;
using System.Collections.Generic;

namespace Wedding_House.Models;

public partial class Nhanvien
{
    public int MaNhanVien { get; set; }

    public string TenNhanVien { get; set; } = null!;

    public int? MaTaiKhoan { get; set; }

    public virtual Taikhoan? MaTaiKhoanNavigation { get; set; }

    public virtual ICollection<Phancong> Phancongs { get; set; } = new List<Phancong>();
}
