using System;
using System.Collections.Generic;

namespace Wedding_House.Models;

public partial class Baocaodoanhso
{
    public int MaBaoCaoDoanhSo { get; set; }

    public string Thang { get; set; } = null!;

    public decimal? TongDoanhThu { get; set; }

    public virtual ICollection<Chitietbcd> Chitietbcds { get; set; } = new List<Chitietbcd>();
}
