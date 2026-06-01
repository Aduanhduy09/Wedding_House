using System;
using System.Collections.Generic;

namespace Wedding_House.Models;

public partial class Monan
{
    public int MaMonAn { get; set; }

    public string TenMonAn { get; set; } = null!;

    public virtual ICollection<Chitietdatmon> Chitietdatmons { get; set; } = new List<Chitietdatmon>();
}
