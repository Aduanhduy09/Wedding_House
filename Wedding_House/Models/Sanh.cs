using System;
using System.Collections.Generic;

namespace Wedding_House.Models;

public partial class Sanh
{
    public int MaSanh { get; set; }

    public string TenSanh { get; set; } = null!;

    public string? LoaiSanh { get; set; }

    // Thêm các tham số cấu hình cho từng sảnh (số lượng bàn tối đa và đơn giá bàn tối thiểu)
    public int? SoLuongBanToiDa { get; set; }

    public decimal? DonGiaBanToiThieu { get; set; }

    public decimal? GiaThue { get; set; }

    public string? GhiChu { get; set; }

    public virtual ICollection<Tieccuoi> Tieccuois { get; set; } = new List<Tieccuoi>();
}
