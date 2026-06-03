using System;
using System.Collections.Generic;

namespace Wedding_House.Models;

public partial class Thamso
{
    public int Id { get; set; }

    public int? SoLuongBanToiDa { get; set; }

    public decimal? DonGiaBanToiThieu { get; set; }

    public string? LoaiSanh { get; set; }

    public int? SoLuongMonAn { get; set; }

    public int? SoLuongDichVu { get; set; }
}
