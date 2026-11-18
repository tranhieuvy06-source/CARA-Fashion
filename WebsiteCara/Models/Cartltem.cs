using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteCara.Models
{
    public class CartItem
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public decimal Gia { get; set; }
        public string KichThuoc { get; set; }
        public string MauSac { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien => SoLuong * Gia;
    }
}