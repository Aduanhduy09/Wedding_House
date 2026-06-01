using System;
using System.Collections.Generic;

namespace Wedding_House.Models;

public partial class Chitietdichvu
{
    public int MaCtdv { get; set; }

    public int MaDatTiecCuoi { get; set; }

    public int MaDichVu { get; set; }

    public int? SoLuong { get; set; }

    public decimal? DonGia { get; set; }

    public decimal? ThanhTien { get; set; }

    public virtual Tieccuoi MaDatTiecCuoiNavigation { get; set; } = null!;

    public virtual Dichvu MaDichVuNavigation { get; set; } = null!;
}
