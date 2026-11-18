using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebsiteCara.Helpers;
using WebsiteCara.Models;

namespace WebsiteCara.Areas.Admin.Controllers
{
    // Danh mục: chỉ Admin
    public class DanhMucController : BaseAdminController
    {
        protected override int[] AllowedRoles
        {
            get { return new[] { Roles.Admin }; }
        }

        public ActionResult Index()
        {
            var list = db.DanhMucs.ToList();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DanhMuc danhMuc)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(danhMuc.TenDanhMuc))
            {
                db.DanhMucs.Add(danhMuc);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DanhMuc danhMuc)
        {
            if (ModelState.IsValid)
            {
                db.Entry(danhMuc).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            var dm = db.DanhMucs.Find(id);
            if (dm != null)
            {
                db.DanhMucs.Remove(dm);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
