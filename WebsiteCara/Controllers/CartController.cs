using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebsiteCara.Models;

namespace WebsiteCara.Controllers
{
    public class CartController : Controller
    {
        private WebCaraDbEntities db = new WebCaraDbEntities();

        // Lấy giỏ hàng từ Session
        private List<CartItem> GetCart()
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session["Cart"] = cart;
            }
            return cart;
        }

        // Xem giỏ hàng
        public ActionResult Index()
        {
            var cart = GetCart();
            ViewBag.TongTien = cart.Sum(item => item.ThanhTien);
            return View(cart);
        }

        // Thêm sản phẩm (kiểm tra biến thể có tồn tại và còn đủ hàng)
        [HttpPost]
        public ActionResult AddToCart(int maSanPham, string kichThuoc, string mauSac, int soLuong = 1)
        {
            var sp = db.SanPhams.FirstOrDefault(s => s.MaSanPham == maSanPham && s.TrangThai == true);
            if (sp == null) return HttpNotFound();

            if (soLuong < 1) soLuong = 1;

            var ctsp = db.ChiTietSanPhams.FirstOrDefault(c =>
                c.MaSanPham == maSanPham &&
                c.KichThuoc.TenKichThuoc == kichThuoc &&
                c.MauSac.TenMauSac == mauSac);

            if (ctsp == null)
            {
                TempData["Error"] = "Phân loại bạn chọn không tồn tại.";
                return RedirectToAction("Details", "Home", new { id = maSanPham });
            }

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(i => i.MaSanPham == maSanPham && i.KichThuoc == kichThuoc && i.MauSac == mauSac);
            int daCoTrongGio = existingItem != null ? existingItem.SoLuong : 0;

            if (daCoTrongGio + soLuong > ctsp.SoLuongTon)
            {
                TempData["Error"] = ctsp.SoLuongTon <= 0
                    ? "Phân loại này đã hết hàng."
                    : "Chỉ còn " + ctsp.SoLuongTon + " sản phẩm cho phân loại này" +
                      (daCoTrongGio > 0 ? " (trong giỏ bạn đã có " + daCoTrongGio + ")." : ".");
                return RedirectToAction("Details", "Home", new { id = maSanPham });
            }

            if (existingItem != null)
            {
                existingItem.SoLuong += soLuong;
            }
            else
            {
                decimal giaBan = (sp.GiaKhuyenMai.HasValue && sp.GiaKhuyenMai > 0) ? sp.GiaKhuyenMai.Value : sp.GiaGoc;
                cart.Add(new CartItem
                {
                    MaSanPham = sp.MaSanPham,
                    TenSanPham = sp.TenSanPham,
                    HinhAnh = sp.HinhAnhChinh,
                    Gia = giaBan,
                    KichThuoc = kichThuoc,
                    MauSac = mauSac,
                    SoLuong = soLuong
                });
            }

            return RedirectToAction("Index");
        }

        // Xóa sản phẩm
        public ActionResult Remove(int maSanPham, string kichThuoc, string mauSac)
        {
            var cart = GetCart();
            var itemToRemove = cart.FirstOrDefault(i => i.MaSanPham == maSanPham && i.KichThuoc == kichThuoc && i.MauSac == mauSac);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
            }
            return RedirectToAction("Index");
        }
    }
}
