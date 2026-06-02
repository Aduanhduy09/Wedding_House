using System;
using System.Collections.Generic;

namespace Wedding_House.Models;

public partial class Sanh
{
    public int MaSanh { get; set; }

    public string TenSanh { get; set; } = null!;

    public string? LoaiSanh { get; set; }

    public decimal? GiaThue { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<Tieccuoi> Tieccuois { get; set; } = new List<Tieccuoi>();
}
