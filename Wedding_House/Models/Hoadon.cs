using System;
using System.Collections.Generic;

namespace Wedding_House.Models;

public partial class Hoadon
{
    public int MaHoaDon { get; set; }

    public int MaDatTiecCuoi { get; set; }

    public decimal? TongTienBan { get; set; }

    public decimal? TongTienDichVu { get; set; }

    public decimal? TongTienHoaDon { get; set; }

    public decimal? TienDatCoc { get; set; }

    public decimal? TienConLai { get; set; }

    public decimal? TienPhat { get; set; }

    public virtual Tieccuoi MaDatTiecCuoiNavigation { get; set; } = null!;
}
