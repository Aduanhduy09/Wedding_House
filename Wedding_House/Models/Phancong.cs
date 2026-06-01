using System;
using System.Collections.Generic;

namespace Wedding_House.Models;

public partial class Phancong
{
    public int MaPhanCong { get; set; }

    public int MaNhanVien { get; set; }

    public int MaDatTiecCuoi { get; set; }

    public virtual Tieccuoi MaDatTiecCuoiNavigation { get; set; } = null!;

    public virtual Nhanvien MaNhanVienNavigation { get; set; } = null!;
}
