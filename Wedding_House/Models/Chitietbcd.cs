using System;
using System.Collections.Generic;

namespace Wedding_House.Models;

public partial class Chitietbcd
{
    public int MaChiTietBcds { get; set; }

    public int MaBaoCaoDoanhSo { get; set; }

    public DateOnly Ngay { get; set; }

    public int? SoLuongTiecCuoi { get; set; }

    public decimal? DoanhThu { get; set; }

    public double? TiLe { get; set; }

    public virtual Baocaodoanhso MaBaoCaoDoanhSoNavigation { get; set; } = null!;
}
