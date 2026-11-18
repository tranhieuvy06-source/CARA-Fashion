using System;
using System.Web.Mvc;

namespace WebsiteCara.Helpers
{
    // Mã vai trò (khớp với bảng VaiTro trong CSDL)
    public static class Roles
    {
        public const int Admin = 1;
        public const int Customer = 2;
        public const int Staff = 3;   // Nhân viên

        public static bool IsBackOffice(int maVaiTro)
        {
            return maVaiTro == Admin || maVaiTro == Staff;
        }
    }

    public static class ImageHelper
    {
        // Trả về URL ảnh: link online giữ nguyên, tên file thì trỏ vào ~/Content/images/
        public static string Url(UrlHelper url, string hinhAnh)
        {
            if (string.IsNullOrWhiteSpace(hinhAnh))
                return url.Content("~/Content/images/default.jpg");

            if (hinhAnh.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                hinhAnh.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return hinhAnh;

            return url.Content("~/Content/images/" + hinhAnh);
        }
    }
}
