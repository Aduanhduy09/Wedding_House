using System;
using System.Collections.Generic;

namespace Wedding_House.Models;

public partial class Ca
{
    public int MaCa { get; set; }

    public string TenCa { get; set; } = null!;

    public string? Gio { get; set; }

    public virtual ICollection<Tieccuoi> Tieccuois { get; set; } = new List<Tieccuoi>();
}
