using System;
using System.Collections.Generic;

namespace Wedding_House.Models;

public partial class Chitietdatmon
{
    public int MaCtdatMon { get; set; }

    public int MaDatTiecCuoi { get; set; }

    public int MaMonAn { get; set; }

    public decimal? DonGia { get; set; }

    public string? GhiChu { get; set; }

    public virtual Tieccuoi MaDatTiecCuoiNavigation { get; set; } = null!;

    public virtual Monan MaMonAnNavigation { get; set; } = null!;
}
