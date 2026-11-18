using System;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using WebsiteCara.Models;

namespace WebsiteCara.Areas.Admin.Controllers
{
    // Quản lý biến thể (size / màu / tồn kho) của từng sản phẩm: Admin + Nhân viên
    public class BienTheController : BaseAdminController
    {
        // id = MaSanPham
        public ActionResult Index(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return HttpNotFound();

            var ds = db.ChiTietSanPhams
                       .Include(c => c.KichThuoc)
                       .Include(c => c.MauSac)
                       .Where(c => c.MaSanPham == id)
                       .OrderBy(c => c.MaKichThuoc).ThenBy(c => c.MaMauSac)
                       .ToList();

            ViewBag.SanPham = sp;
            ViewBag.KichThuocs = db.KichThuocs.OrderBy(k => k.MaKichThuoc).ToList();
            ViewBag.MauSacs = db.MauSacs.OrderBy(m => m.MaMauSac).ToList();
            return View(ds);
        }

        // Thêm biến thể (1 cặp size + màu) cho sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int maSanPham, int maKichThuoc, int maMauSac, int soLuongTon)
        {
            if (db.SanPhams.Find(maSanPham) == null) return HttpNotFound();

            if (soLuongTon < 0)
            {
                TempData["Error"] = "Số lượng tồn không được âm.";
            }
            else if (db.KichThuocs.Find(maKichThuoc) == null || db.MauSacs.Find(maMauSac) == null)
            {
                TempData["Error"] = "Size hoặc màu không hợp lệ.";
            }
            else if (db.ChiTietSanPhams.Any(c => c.MaSanPham == maSanPham && c.MaKichThuoc == maKichThuoc && c.MaMauSac == maMauSac))
            {
                TempData["Error"] = "Biến thể này đã có rồi. Hãy sửa số lượng ở bảng bên dưới.";
            }
            else
            {
                db.ChiTietSanPhams.Add(new ChiTietSanPham
                {
                    MaSanPham = maSanPham,
                    MaKichThuoc = maKichThuoc,
                    MaMauSac = maMauSac,
                    SoLuongTon = soLuongTon
                });
                db.SaveChanges();
                TempData["Success"] = "Đã thêm biến thể.";
            }
            return RedirectToAction("Index", new { id = maSanPham });
        }

        // id = MaChiTietSP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStock(int id, int soLuongTon)
        {
            var ct = db.ChiTietSanPhams.Find(id);
            if (ct == null) return HttpNotFound();

            if (soLuongTon < 0)
            {
                TempData["Error"] = "Số lượng tồn không được âm.";
            }
            else
            {
                ct.SoLuongTon = soLuongTon;
                db.SaveChanges();
                TempData["Success"] = "Đã cập nhật tồn kho.";
            }
            return RedirectToAction("Index", new { id = ct.MaSanPham });
        }

        // id = MaChiTietSP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var ct = db.ChiTietSanPhams.Find(id);
            if (ct == null) return HttpNotFound();

            if (db.ChiTietDonHangs.Any(d => d.MaChiTietSP == id))
            {
                TempData["Error"] = "Biến thể này đã có trong đơn hàng nên không xóa được. Hãy đặt số lượng tồn về 0 để ngừng bán.";
            }
            else
            {
                db.ChiTietSanPhams.Remove(ct); // giỏ hàng liên quan tự xóa theo (ON DELETE CASCADE)
                db.SaveChanges();
                TempData["Success"] = "Đã xóa biến thể.";
            }
            return RedirectToAction("Index", new { id = ct.MaSanPham });
        }

        // Thêm size mới vào danh sách chung (vd: XXL, Free Size)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddSize(int maSanPham, string tenKichThuoc)
        {
            tenKichThuoc = (tenKichThuoc ?? "").Trim();
            if (tenKichThuoc.Length == 0 || tenKichThuoc.Length > 20)
            {
                TempData["Error"] = "Tên size phải từ 1 đến 20 ký tự.";
            }
            else if (db.KichThuocs.Any(k => k.TenKichThuoc == tenKichThuoc))
            {
                TempData["Error"] = "Size \"" + tenKichThuoc + "\" đã có rồi.";
            }
            else
            {
                db.KichThuocs.Add(new KichThuoc { TenKichThuoc = tenKichThuoc });
                db.SaveChanges();
                TempData["Success"] = "Đã thêm size \"" + tenKichThuoc + "\".";
            }
            return RedirectToAction("Index", new { id = maSanPham });
        }

        // Thêm màu mới vào danh sách chung (vd: Đỏ, Hồng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMau(int maSanPham, string tenMauSac, string maMauHex)
        {
            tenMauSac = (tenMauSac ?? "").Trim();
            if (tenMauSac.Length == 0 || tenMauSac.Length > 50)
            {
                TempData["Error"] = "Tên màu phải từ 1 đến 50 ký tự.";
            }
            else if (db.MauSacs.Any(m => m.TenMauSac == tenMauSac))
            {
                TempData["Error"] = "Màu \"" + tenMauSac + "\" đã có rồi.";
            }
            else
            {
                string hex = Regex.IsMatch(maMauHex ?? "", "^#[0-9a-fA-F]{6}$") ? maMauHex : null;
                db.MauSacs.Add(new MauSac { TenMauSac = tenMauSac, MaMauHex = hex });
                db.SaveChanges();
                TempData["Success"] = "Đã thêm màu \"" + tenMauSac + "\".";
            }
            return RedirectToAction("Index", new { id = maSanPham });
        }
    }
}
