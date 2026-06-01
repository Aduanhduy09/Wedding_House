using System;
using System.Collections.Generic;

namespace Wedding_House.Models;

public partial class Dichvu
{
    public int MaDichVu { get; set; }

    public string TenDichVu { get; set; } = null!;

    public virtual ICollection<Chitietdichvu> Chitietdichvus { get; set; } = new List<Chitietdichvu>();
}
