using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using WebsiteCara.Helpers;
using WebsiteCara.Models;

namespace WebsiteCara.Areas.Admin.Controllers
{
    // Controller gốc của khu Admin: kiểm tra đăng nhập + phân quyền theo vai trò.
    // Mặc định cho phép Admin và Nhân viên. Controller con override AllowedRoles để siết lại.
    public abstract class BaseAdminController : Controller
    {
        protected WebCaraDbEntities db = new WebCaraDbEntities();

        protected virtual int[] AllowedRoles
        {
            get { return new[] { Roles.Admin, Roles.Staff }; }
        }

        protected NguoiDung CurrentUser { get; private set; }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var sessionUser = Session["User"] as NguoiDung;
            if (sessionUser == null)
            {
                filterContext.Result = RedirectTo("Account", "Login");
                return;
            }

            // Đọc lại từ DB để đổi quyền / khóa tài khoản có hiệu lực ngay, không phải chờ đăng nhập lại
            var user = db.NguoiDungs.Find(sessionUser.MaNguoiDung);
            if (user == null || user.TrangThai != true)
            {
                Session.Clear();
                filterContext.Result = RedirectTo("Account", "Login");
                return;
            }

            Session["User"] = user;
            CurrentUser = user;

            if (!AllowedRoles.Contains(user.MaVaiTro))
            {
                TempData["Error"] = "Bạn không có quyền truy cập chức năng này.";
                filterContext.Result = RedirectTo("Home", "Index");
                return;
            }

            base.OnActionExecuting(filterContext);
        }

        private static ActionResult RedirectTo(string controller, string action)
        {
            return new RedirectToRouteResult(
                new RouteValueDictionary(new { controller = controller, action = action, area = "" }));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
