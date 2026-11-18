using System.Linq;
using System.Web.Mvc;
using WebsiteCara.Helpers;
using WebsiteCara.Models;

namespace WebsiteCara.Controllers
{
    public class AccountController : Controller
    {
        private WebCaraDbEntities db = new WebCaraDbEntities();

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(NguoiDung model)
        {
            if (ModelState.IsValid)
            {
                var checkEmail = db.NguoiDungs.FirstOrDefault(u => u.Email == model.Email);
                if (checkEmail != null)
                {
                    ViewBag.Error = "Email này đã được sử dụng!";
                    return View(model);
                }

                if (string.IsNullOrEmpty(model.MatKhau) || model.MatKhau.Length < 6)
                {
                    ViewBag.Error = "Mật khẩu phải có ít nhất 6 ký tự!";
                    return View(model);
                }

                model.MatKhau = PasswordHelper.Hash(model.MatKhau); // lưu mật khẩu đã băm
                model.MaVaiTro = Roles.Customer;
                model.TrangThai = true;

                db.NguoiDungs.Add(model);
                db.SaveChanges();

                TempData["Success"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string matKhau)
        {
            var user = db.NguoiDungs.FirstOrDefault(u => u.Email == email && u.TrangThai == true);

            if (user != null && PasswordHelper.Verify(matKhau, user.MatKhau))
            {
                // Tài khoản cũ còn lưu mật khẩu dạng chữ thường -> băm lại ngay lần đăng nhập này
                if (!PasswordHelper.IsHashed(user.MatKhau))
                {
                    user.MatKhau = PasswordHelper.Hash(matKhau);
                    db.SaveChanges();
                }

                Session["User"] = user;
                if (Roles.IsBackOffice(user.MaVaiTro))
                {
                    return RedirectToAction("Index", "DonHang", new { area = "Admin" });
                }

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Email hoặc mật khẩu không chính xác!";
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
    }
}