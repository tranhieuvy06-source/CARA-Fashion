using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using WebsiteCara.Helpers;
using WebsiteCara.Models;

namespace WebsiteCara.Controllers
{
    public class OrderController : Controller
    {
        private WebCaraDbEntities db = new WebCaraDbEntities();

        private class OrderLine
        {
            public CartItem Item;
            public int MaChiTietSP;
            public decimal Gia;
        }

        private List<CartItem> GetCart()
        {
            return Session["Cart"] as List<CartItem> ?? new List<CartItem>();
        }

        // GET: /Order/Checkout
        public ActionResult Checkout()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var user = Session["User"] as NguoiDung;
            ViewBag.User = user;
            ViewBag.Cart = cart;
            ViewBag.TongTien = cart.Sum(x => x.ThanhTien);

            return View();
        }

        // POST: /Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(string hoTenNguoiNhan, string dienThoaiNhan, string diaChiGiao, string ghiChu)
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var user = Session["User"] as NguoiDung;
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            string diaChi = string.IsNullOrWhiteSpace(diaChiGiao) ? user.DiaChi : diaChiGiao.Trim();
            string dienThoai = string.IsNullOrWhiteSpace(dienThoaiNhan) ? user.DienThoai : dienThoaiNhan.Trim();
            if (string.IsNullOrWhiteSpace(diaChi) || string.IsNullOrWhiteSpace(dienThoai))
            {
                TempData["Error"] = "Vui lòng nhập đầy đủ địa chỉ và số điện thoại nhận hàng.";
                return RedirectToAction("Checkout");
            }

            // 1. Đối chiếu từng dòng giỏ hàng với biến thể thật trong DB và lấy giá hiện hành
            var lines = new List<OrderLine>();
            foreach (var item in cart)
            {
                var ctsp = db.ChiTietSanPhams.Include(c => c.SanPham).FirstOrDefault(c =>
                    c.MaSanPham == item.MaSanPham &&
                    c.KichThuoc.TenKichThuoc == item.KichThuoc &&
                    c.MauSac.TenMauSac == item.MauSac);

                if (ctsp == null || ctsp.SanPham.TrangThai != true)
                {
                    TempData["Error"] = "Sản phẩm \"" + item.TenSanPham + "\" (" + item.KichThuoc + " / " + item.MauSac + ") hiện không còn bán. Vui lòng xóa khỏi giỏ hàng.";
                    return RedirectToAction("Index", "Cart");
                }

                var sp = ctsp.SanPham;
                decimal gia = (sp.GiaKhuyenMai.HasValue && sp.GiaKhuyenMai > 0) ? sp.GiaKhuyenMai.Value : sp.GiaGoc;
                lines.Add(new OrderLine { Item = item, MaChiTietSP = ctsp.MaChiTietSP, Gia = gia });
            }

            // 2. Tạo đơn + trừ kho trong cùng 1 transaction: thiếu hàng ở bất kỳ dòng nào thì hủy toàn bộ
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    var donHang = new DonHang
                    {
                        MaNguoiDung = user.MaNguoiDung,
                        NgayDat = DateTime.Now,
                        TongTien = lines.Sum(l => l.Gia * l.Item.SoLuong),
                        TrangThai = "Chờ xử lý",
                        PhuongThucThanhToan = "Thanh toán khi nhận hàng (COD)",
                        DiaChiGiaoHang = diaChi,
                        DienThoaiNhanHang = dienThoai,
                        GhiChu = ghiChu
                    };
                    db.DonHangs.Add(donHang);
                    db.SaveChanges();

                    foreach (var l in lines)
                    {
                        // Trừ kho có điều kiện: chỉ trừ được khi còn đủ hàng (an toàn khi nhiều người mua cùng lúc)
                        int ok = db.Database.ExecuteSqlCommand(
                            "UPDATE ChiTietSanPham SET SoLuongTon = SoLuongTon - @p0 WHERE MaChiTietSP = @p1 AND SoLuongTon >= @p0",
                            l.Item.SoLuong, l.MaChiTietSP);

                        if (ok == 0)
                        {
                            tx.Rollback();
                            TempData["Error"] = "Rất tiếc, \"" + l.Item.TenSanPham + "\" (" + l.Item.KichThuoc + " / " + l.Item.MauSac + ") không còn đủ hàng. Vui lòng giảm số lượng hoặc xóa khỏi giỏ.";
                            return RedirectToAction("Index", "Cart");
                        }

                        db.ChiTietDonHangs.Add(new ChiTietDonHang
                        {
                            MaDonHang = donHang.MaDonHang,
                            MaChiTietSP = l.MaChiTietSP,
                            SoLuong = l.Item.SoLuong,
                            DonGia = l.Gia
                        });
                    }

                    db.SaveChanges();
                    tx.Commit();

                    Session["Cart"] = null;
                    TempData["MaDonHang"] = donHang.MaDonHang;
                    return RedirectToAction("Success");
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        // Xem danh sách đơn hàng đã mua
        public ActionResult History()
        {
            var user = Session["User"] as NguoiDung;
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var listDonHang = db.DonHangs
                                .Where(d => d.MaNguoiDung == user.MaNguoiDung)
                                .OrderByDescending(d => d.NgayDat)
                                .ToList();

            return View(listDonHang);
        }

        // Xem chi tiết đơn hàng cụ thể
        public ActionResult Details(int id)
        {
            var user = Session["User"] as NguoiDung;
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var donHang = db.DonHangs.FirstOrDefault(d => d.MaDonHang == id && d.MaNguoiDung == user.MaNguoiDung);
            if (donHang == null)
            {
                return HttpNotFound();
            }

            return View(donHang);
        }

        // Khách hàng tự hủy đơn: chỉ được khi đơn còn "Chờ xử lý". Hủy xong hoàn lại tồn kho.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id, string from)
        {
            var user = Session["User"] as NguoiDung;
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var donHang = db.DonHangs.FirstOrDefault(d => d.MaDonHang == id && d.MaNguoiDung == user.MaNguoiDung);
            if (donHang == null)
            {
                return HttpNotFound();
            }

            bool daHuy = false;
            using (var tx = db.Database.BeginTransaction())
            {
                try
                {
                    // UPDATE có điều kiện trạng thái -> nếu nhân viên vừa duyệt đơn cùng lúc thì không hủy nhầm
                    int affected = db.Database.ExecuteSqlCommand(
                        "UPDATE DonHang SET TrangThai = @p0 WHERE MaDonHang = @p1 AND MaNguoiDung = @p2 AND TrangThai = @p3",
                        new SqlParameter("@p0", "Đã hủy"),
                        new SqlParameter("@p1", id),
                        new SqlParameter("@p2", user.MaNguoiDung),
                        new SqlParameter("@p3", "Chờ xử lý"));

                    if (affected > 0)
                    {
                        StockHelper.RestoreStock(db, id);
                        tx.Commit();
                        daHuy = true;
                    }
                    else
                    {
                        tx.Rollback();
                    }
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }

            if (daHuy)
            {
                TempData["Success"] = "Đã hủy đơn hàng #" + id + ".";
            }
            else
            {
                TempData["Error"] = "Đơn hàng #" + id + " đã được xử lý nên không thể hủy. Vui lòng liên hệ cửa hàng.";
            }

            if (from == "details")
            {
                return RedirectToAction("Details", new { id = id });
            }
            return RedirectToAction("History");
        }

        // Trang thông báo đặt hàng thành công
        public ActionResult Success()
        {
            if (TempData["MaDonHang"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.MaDonHang = TempData["MaDonHang"];
            return View();
        }
    }
}
