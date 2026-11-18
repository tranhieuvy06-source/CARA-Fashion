using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using WebsiteCara.Models;

namespace WebsiteCara.Controllers
{
    public class HomeController : Controller
    {
        private WebCaraDbEntities db = new WebCaraDbEntities();

      
        private const int PageSize = 12;

        public ActionResult Index(string searchStr, int? danhMuc, int page = 1)
        {
            var sanPhams = db.SanPhams.Where(s => s.TrangThai == true);

            // Lọc theo danh mục
            if (danhMuc.HasValue)
            {
                sanPhams = sanPhams.Where(s => s.MaDanhMuc == danhMuc.Value);
            }

            // Nếu người dùng nhập từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(searchStr))
            {
                searchStr = searchStr.Trim();
                sanPhams = sanPhams.Where(s => s.TenSanPham.Contains(searchStr) ||
                                               (s.MoTa != null && s.MoTa.Contains(searchStr)));

                ViewBag.SearchTerm = searchStr;
            }

            // Phân trang
            int total = sanPhams.Count();
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var list = sanPhams.OrderByDescending(s => s.MaSanPham)
                               .Skip((page - 1) * PageSize)
                               .Take(PageSize)
                               .ToList();

            ViewBag.DanhMucs = db.DanhMucs.OrderBy(d => d.TenDanhMuc).ToList();
            ViewBag.CurrentDanhMuc = danhMuc;
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;

            return View(list);
        }

        // GET: Home/ChinhSachDoiTra
        public ActionResult ChinhSachDoiTra()
        {
            ViewBag.Title = "Chính sách đổi trả - CARA BOUTIQUE";
            return View();
        }

        // GET: Home/HuongDanChonSize
        public ActionResult HuongDanChonSize()
        {
            ViewBag.Title = "Hướng dẫn chọn size - CARA BOUTIQUE";
            return View();
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            // Tìm sản phẩm theo MaSanPham và đang ở trạng thái hoạt động
            SanPham sanPham = db.SanPhams.FirstOrDefault(s => s.MaSanPham == id && s.TrangThai == true);

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            // Nạp kèm bảng KichThuoc và MauSac bằng Include
            var dsChiTiet = db.ChiTietSanPhams
                              .Include(c => c.KichThuoc)
                              .Include(c => c.MauSac)
                              .Where(c => c.MaSanPham == id && c.SoLuongTon > 0)
                              .ToList();

            // Truy cập thuộc tính tên TenKichThuoc và TenMauSac
            ViewBag.DanhSachSize = dsChiTiet
                .Where(c => c.KichThuoc != null && !string.IsNullOrEmpty(c.KichThuoc.TenKichThuoc))
                .Select(c => c.KichThuoc.TenKichThuoc)
                .Distinct()
                .ToList();

            ViewBag.DanhSachMau = dsChiTiet
                .Where(c => c.MauSac != null && !string.IsNullOrEmpty(c.MauSac.TenMauSac))
                .Select(c => c.MauSac.TenMauSac)
                .Distinct()
                .ToList();

            ViewBag.BienThe = dsChiTiet;

            return View(sanPham);
        }
    }
}