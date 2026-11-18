using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using WebsiteCara.Helpers;
using WebsiteCara.Models;

namespace WebsiteCara.Areas.Admin.Controllers
{
    // Đơn hàng: Admin + Nhân viên
    public class DonHangController : BaseAdminController
    {
        private static readonly string[] ValidStatuses = { "Chờ xử lý", "Đã duyệt", "Đang giao", "Hoàn thành", "Đã hủy" };

        public ActionResult Index()
        {
            var donHangs = db.DonHangs.Include(d => d.NguoiDung)
                                      .OrderByDescending(d => d.MaDonHang)
                                      .ToList();
            return View(donHangs);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            DonHang donHang = db.DonHangs.Include(d => d.NguoiDung).FirstOrDefault(d => d.MaDonHang == id);
            if (donHang == null) return HttpNotFound();

            var chiTiets = db.ChiTietDonHangs
                             .Include(c => c.ChiTietSanPham.SanPham)
                             .Include(c => c.ChiTietSanPham.KichThuoc)
                             .Include(c => c.ChiTietSanPham.MauSac)
                             .Where(c => c.MaDonHang == id)
                             .ToList();

            ViewBag.ChiTietDonHang = chiTiets;
            return View(donHang);
        }

        // Cập nhật trạng thái đơn (dùng chung cho form ở trang Danh sách và trang Chi tiết)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string trangThai, string from)
        {
            DonHang donHang = db.DonHangs.Find(id);
            if (donHang == null) return HttpNotFound();

            string hienTai = donHang.TrangThai;

            if (!ValidStatuses.Contains(trangThai))
            {
                TempData["Error"] = "Trạng thái không hợp lệ.";
            }
            else if (hienTai == "Đã hủy" || hienTai == "Hoàn thành" || hienTai == "Đã hoàn thành")
            {
                TempData["Error"] = "Đơn #" + id + " đã " + (hienTai == "Đã hủy" ? "hủy" : "hoàn thành") + ", không thể đổi trạng thái.";
            }
            else if (hienTai == trangThai)
            {
                TempData["Error"] = "Đơn #" + id + " đang ở trạng thái này rồi.";
            }
            else
            {
                bool capNhatDuoc = false;
                using (var tx = db.Database.BeginTransaction())
                {
                    try
                    {
                        // UPDATE có điều kiện trạng thái cũ -> tránh đè lên thay đổi vừa xảy ra (vd: khách vừa hủy đơn)
                        int affected = db.Database.ExecuteSqlCommand(
                            "UPDATE DonHang SET TrangThai = @p0 WHERE MaDonHang = @p1 AND TrangThai = @p2",
                            new SqlParameter("@p0", trangThai),
                            new SqlParameter("@p1", id),
                            new SqlParameter("@p2", hienTai));

                        if (affected > 0)
                        {
                            if (trangThai == "Đã hủy")
                            {
                                StockHelper.RestoreStock(db, id); // hủy đơn -> trả hàng về kho
                            }
                            tx.Commit();
                            capNhatDuoc = true;
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

                if (capNhatDuoc)
                    TempData["Success"] = "Đã cập nhật đơn #" + id + " → " + trangThai + (trangThai == "Đã hủy" ? " (đã hoàn lại tồn kho)." : "");
                else
                    TempData["Error"] = "Trạng thái đơn #" + id + " vừa bị thay đổi bởi người khác. Vui lòng kiểm tra lại.";
            }

            if (from == "list") return RedirectToAction("Index");
            return RedirectToAction("Details", new { id = id });
        }
    }
}
