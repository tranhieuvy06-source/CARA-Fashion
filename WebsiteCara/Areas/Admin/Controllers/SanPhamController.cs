using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteCara.Models;

namespace WebsiteCara.Areas.Admin.Controllers
{
    // Sản phẩm: Admin + Nhân viên
    public class SanPhamController : BaseAdminController
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const int MaxImageBytes = 5 * 1024 * 1024; // 5MB

        private void LoadDanhMuc(int? selected = null)
        {
            ViewBag.DanhMucList = new SelectList(db.DanhMucs.OrderBy(d => d.TenDanhMuc), "MaDanhMuc", "TenDanhMuc", selected);
        }

        // Lưu ảnh upload vào ~/Content/images/. Trả về tên file đã lưu, hoặc null + error nếu lỗi.
        private string SaveImage(HttpPostedFileBase file, out string error)
        {
            error = null;

            var ext = Path.GetExtension(file.FileName ?? "").ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                error = "Chỉ chấp nhận ảnh định dạng .jpg, .jpeg, .png, .gif, .webp";
                return null;
            }
            if (file.ContentLength > MaxImageBytes)
            {
                error = "Ảnh quá lớn (tối đa 5MB).";
                return null;
            }

            // Thư mục chưa có thì tự tạo (nguyên nhân chính khiến upload ảnh bị lỗi trước đây)
            string dir = Server.MapPath("~/Content/images/");
            Directory.CreateDirectory(dir);

            // Đặt tên ngẫu nhiên để không bị trùng / ghi đè / dính ký tự lạ
            string fileName = Guid.NewGuid().ToString("N") + ext;
            file.SaveAs(Path.Combine(dir, fileName));
            return fileName;
        }

        public ActionResult Index()
        {
            var sanPhams = db.SanPhams.OrderByDescending(s => s.MaSanPham).ToList();
            ViewBag.TonKho = db.ChiTietSanPhams
                               .GroupBy(c => c.MaSanPham)
                               .Select(g => new { MaSanPham = g.Key, Tong = g.Sum(x => x.SoLuongTon) })
                               .ToDictionary(x => x.MaSanPham, x => x.Tong);
            return View(sanPhams);
        }

        public ActionResult Create()
        {
            LoadDanhMuc();
            return View(new SanPham());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SanPham sanPham, HttpPostedFileBase HinhAnhFile)
        {
            if (string.IsNullOrWhiteSpace(sanPham.TenSanPham))
                ModelState.AddModelError("TenSanPham", "Vui lòng nhập tên sản phẩm.");
            if (sanPham.GiaGoc < 0)
                ModelState.AddModelError("GiaGoc", "Giá không hợp lệ.");
            if (!db.DanhMucs.Any(d => d.MaDanhMuc == sanPham.MaDanhMuc))
                ModelState.AddModelError("MaDanhMuc", "Vui lòng chọn danh mục.");

            string fileName = null;
            if (HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
            {
                string error;
                fileName = SaveImage(HinhAnhFile, out error);
                if (error != null) ModelState.AddModelError("", error);
            }

            if (!ModelState.IsValid)
            {
                LoadDanhMuc(sanPham.MaDanhMuc);
                return View(sanPham);
            }

            sanPham.HinhAnhChinh = fileName ?? "default.jpg";
            sanPham.TrangThai = true;
            sanPham.NgayTao = DateTime.Now;
            db.SanPhams.Add(sanPham);
            db.SaveChanges();
            TempData["Success"] = "Đã thêm sản phẩm. Hãy thêm size/màu và số lượng tồn để khách có thể mua.";
            return RedirectToAction("Index", "BienThe", new { id = sanPham.MaSanPham });
        }

        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            SanPham sanPham = db.SanPhams.Find(id);
            if (sanPham == null) return HttpNotFound();

            LoadDanhMuc(sanPham.MaDanhMuc);
            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SanPham sanPham, HttpPostedFileBase HinhAnhFile)
        {
            var spDb = db.SanPhams.Find(sanPham.MaSanPham);
            if (spDb == null) return HttpNotFound();

            if (string.IsNullOrWhiteSpace(sanPham.TenSanPham))
                ModelState.AddModelError("TenSanPham", "Vui lòng nhập tên sản phẩm.");
            if (sanPham.GiaGoc < 0)
                ModelState.AddModelError("GiaGoc", "Giá không hợp lệ.");
            if (!db.DanhMucs.Any(d => d.MaDanhMuc == sanPham.MaDanhMuc))
                ModelState.AddModelError("MaDanhMuc", "Vui lòng chọn danh mục.");

            string fileName = null;
            if (HinhAnhFile != null && HinhAnhFile.ContentLength > 0)
            {
                string error;
                fileName = SaveImage(HinhAnhFile, out error);
                if (error != null) ModelState.AddModelError("", error);
            }

            if (!ModelState.IsValid)
            {
                sanPham.HinhAnhChinh = spDb.HinhAnhChinh; // giữ ảnh hiện tại để view hiển thị lại
                LoadDanhMuc(sanPham.MaDanhMuc);
                return View(sanPham);
            }

            spDb.TenSanPham = sanPham.TenSanPham;
            spDb.MaDanhMuc = sanPham.MaDanhMuc;
            spDb.GiaGoc = sanPham.GiaGoc;
            spDb.GiaKhuyenMai = sanPham.GiaKhuyenMai;
            spDb.MoTa = sanPham.MoTa;
            spDb.TrangThai = sanPham.TrangThai ?? false; // checkbox bỏ tick thì form không gửi gì
            if (fileName != null) spDb.HinhAnhChinh = fileName;

            db.SaveChanges();
            TempData["Success"] = "Đã cập nhật sản phẩm.";
            return RedirectToAction("Index");
        }

        // Xóa hẳn sản phẩm. Sản phẩm đã có trong đơn hàng thì không xóa được (giữ lịch sử đơn) -> dùng "Ẩn".
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int id)
        {
            SanPham sanPham = db.SanPhams.Find(id);
            if (sanPham == null) return HttpNotFound();

            bool daCoDonHang = db.ChiTietDonHangs.Any(c => c.ChiTietSanPham.MaSanPham == id);
            if (daCoDonHang)
            {
                TempData["Error"] = "Sản phẩm \"" + sanPham.TenSanPham + "\" đã có trong đơn hàng nên không thể xóa hẳn. Hãy dùng nút Ẩn để ngừng bán.";
                return RedirectToAction("Index");
            }

            string hinh = sanPham.HinhAnhChinh;

            // Biến thể (size/màu) và giỏ hàng liên quan sẽ tự xóa theo (ON DELETE CASCADE trong CSDL)
            db.SanPhams.Remove(sanPham);
            db.SaveChanges();

            // Xóa file ảnh đã upload (chỉ ảnh cục bộ, không đụng link online / default.jpg / ảnh đang dùng chung)
            if (!string.IsNullOrEmpty(hinh) && hinh != "default.jpg" && !hinh.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                && !db.SanPhams.Any(s => s.HinhAnhChinh == hinh))
            {
                try
                {
                    string path = Path.Combine(Server.MapPath("~/Content/images/"), Path.GetFileName(hinh));
                    if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                }
                catch { /* không xóa được file ảnh cũng không sao */ }
            }

            TempData["Success"] = "Đã xóa sản phẩm \"" + sanPham.TenSanPham + "\".";
            return RedirectToAction("Index");
        }

        // Xóa mềm: chuyển sang trạng thái Ẩn để tránh lỗi khóa ngoại
        [HttpPost]
        public ActionResult Delete(int id)
        {
            SanPham sanPham = db.SanPhams.Find(id);
            if (sanPham != null)
            {
                sanPham.TrangThai = false;
                db.Entry(sanPham).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
