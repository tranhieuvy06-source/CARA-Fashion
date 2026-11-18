using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebsiteCara.Helpers;
using WebsiteCara.Models;

namespace WebsiteCara.Areas.Admin.Controllers
{
    // Quản lý người dùng & phân quyền: chỉ Admin
    public class NguoiDungController : BaseAdminController
    {
        protected override int[] AllowedRoles
        {
            get { return new[] { Roles.Admin }; }
        }

        public ActionResult Index(string q, int? role)
        {
            var users = db.NguoiDungs.Include(u => u.VaiTro).AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                users = users.Where(u => u.HoTen.Contains(q) || u.Email.Contains(q) || u.DienThoai.Contains(q));
            }
            if (role.HasValue)
            {
                users = users.Where(u => u.MaVaiTro == role.Value);
            }

            ViewBag.VaiTros = db.VaiTroes.OrderBy(v => v.MaVaiTro).ToList();
            ViewBag.Q = q;
            ViewBag.Role = role;
            ViewBag.CurrentUserId = CurrentUser.MaNguoiDung;

            return View(users.OrderBy(u => u.MaVaiTro).ThenBy(u => u.HoTen).ToList());
        }

        // Đổi vai trò: Khách hàng <-> Nhân viên <-> Admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateRole(int id, int maVaiTro)
        {
            if (id == CurrentUser.MaNguoiDung)
            {
                TempData["Error"] = "Bạn không thể tự đổi vai trò của chính mình.";
                return RedirectToAction("Index");
            }

            var user = db.NguoiDungs.Find(id);
            if (user == null) return HttpNotFound();

            if (!db.VaiTroes.Any(v => v.MaVaiTro == maVaiTro))
            {
                TempData["Error"] = "Vai trò không hợp lệ.";
                return RedirectToAction("Index");
            }

            user.MaVaiTro = maVaiTro;
            db.SaveChanges();
            TempData["Success"] = "Đã cập nhật vai trò cho " + user.HoTen + ".";
            return RedirectToAction("Index");
        }

        // Khóa / mở khóa tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            if (id == CurrentUser.MaNguoiDung)
            {
                TempData["Error"] = "Bạn không thể tự khóa tài khoản của chính mình.";
                return RedirectToAction("Index");
            }

            var user = db.NguoiDungs.Find(id);
            if (user == null) return HttpNotFound();

            user.TrangThai = !(user.TrangThai == true);
            db.SaveChanges();
            TempData["Success"] = (user.TrangThai == true ? "Đã mở khóa " : "Đã khóa ") + user.HoTen + ".";
            return RedirectToAction("Index");
        }
    }
}
