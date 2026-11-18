using WebsiteCara.Models;

namespace WebsiteCara.Helpers
{
    public static class StockHelper
    {
        // Cộng lại tồn kho cho toàn bộ sản phẩm trong đơn (gọi khi đơn bị hủy, bên trong transaction)
        public static void RestoreStock(WebCaraDbEntities db, int maDonHang)
        {
            db.Database.ExecuteSqlCommand(
                "UPDATE ct SET ct.SoLuongTon = ct.SoLuongTon + d.SoLuong " +
                "FROM ChiTietSanPham ct INNER JOIN ChiTietDonHang d ON d.MaChiTietSP = ct.MaChiTietSP " +
                "WHERE d.MaDonHang = @p0", maDonHang);
        }
    }
}
