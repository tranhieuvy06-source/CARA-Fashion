-- 1. Tạo Database
CREATE DATABASE WebCaraDb;
GO

USE WebCaraDb;
GO

-- 2. Bảng Phân quyền (Admin / Customer) theo mô hình RBAC trong báo cáo
CREATE TABLE VaiTro (
    MaVaiTro INT IDENTITY(1,1) PRIMARY KEY,
    TenVaiTro NVARCHAR(50) NOT NULL -- 'Admin', 'Customer'
);
GO

-- 3. Bảng Người dùng (Khách hàng & Quản trị viên)
CREATE TABLE NguoiDung (
    MaNguoiDung INT IDENTITY(1,1) PRIMARY KEY,
    MaVaiTro INT NOT NULL DEFAULT 2, -- Default: Customer
    HoTen NVARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL UNIQUE,
    MatKhau VARCHAR(255) NOT NULL,
    DienThoai VARCHAR(20) NULL,
    DiaChi NVARCHAR(250) NULL,
    NgayTao DATETIME DEFAULT GETDATE(),
    TrangThai BIT DEFAULT 1, -- 1: Hoạt động, 0: Khóa
    CONSTRAINT FK_NguoiDung_VaiTro FOREIGN KEY (MaVaiTro) REFERENCES VaiTro(MaVaiTro)
);
GO

-- 4. Bảng Danh mục sản phẩm (Áo, Quần, Đầm, Phụ kiện...)
CREATE TABLE DanhMuc (
    MaDanhMuc INT IDENTITY(1,1) PRIMARY KEY,
    TenDanhMuc NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(255) NULL
);
GO

-- 5. Bảng Sản phẩm chính
CREATE TABLE SanPham (
    MaSanPham INT IDENTITY(1,1) PRIMARY KEY,
    MaDanhMuc INT NOT NULL,
    TenSanPham NVARCHAR(200) NOT NULL,
    GiaGoc DECIMAL(18, 2) NOT NULL DEFAULT 0,
    GiaKhuyenMai DECIMAL(18, 2) NULL,
    HinhAnhChinh NVARCHAR(500) NULL,
    MoTa NVARCHAR(MAX) NULL,
    NgayTao DATETIME DEFAULT GETDATE(),
    TrangThai BIT DEFAULT 1,
    CONSTRAINT FK_SanPham_DanhMuc FOREIGN KEY (MaDanhMuc) REFERENCES DanhMuc(MaDanhMuc) ON DELETE CASCADE
);
GO

-- 6. Bảng Kích thước (Size: S, M, L, XL...)
CREATE TABLE KichThuoc (
    MaKichThuoc INT IDENTITY(1,1) PRIMARY KEY,
    TenKichThuoc VARCHAR(20) NOT NULL -- 'S', 'M', 'L', 'XL'
);
GO

-- 7. Bảng Màu sắc (Đen, Trắng, Đỏ...)
CREATE TABLE MauSac (
    MaMauSac INT IDENTITY(1,1) PRIMARY KEY,
    TenMauSac NVARCHAR(50) NOT NULL,
    MaMauHex VARCHAR(10) NULL -- Ví dụ: #000000
);
GO

-- 8. Bảng Biến thể sản phẩm (Quản lý kho theo Size & Màu sắc)
CREATE TABLE ChiTietSanPham (
    MaChiTietSP INT IDENTITY(1,1) PRIMARY KEY,
    MaSanPham INT NOT NULL,
    MaKichThuoc INT NOT NULL,
    MaMauSac INT NOT NULL,
    SoLuongTon INT NOT NULL DEFAULT 0 CHECK (SoLuongTon >= 0),
    CONSTRAINT FK_ChiTietSP_SanPham FOREIGN KEY (MaSanPham) REFERENCES SanPham(MaSanPham) ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietSP_KichThuoc FOREIGN KEY (MaKichThuoc) REFERENCES KichThuoc(MaKichThuoc),
    CONSTRAINT FK_ChiTietSP_MauSac FOREIGN KEY (MaMauSac) REFERENCES MauSac(MaMauSac)
);
GO

-- 9. Bảng Giỏ hàng
CREATE TABLE GioHang (
    MaGioHang INT IDENTITY(1,1) PRIMARY KEY,
    MaNguoiDung INT NOT NULL,
    MaChiTietSP INT NOT NULL, -- Lưu biến thể cụ thể (Size & Màu)
    SoLuong INT NOT NULL DEFAULT 1 CHECK (SoLuong > 0),
    NgayCapNhat DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_GioHang_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung) ON DELETE CASCADE,
    CONSTRAINT FK_GioHang_ChiTietSP FOREIGN KEY (MaChiTietSP) REFERENCES ChiTietSanPham(MaChiTietSP) ON DELETE CASCADE
);
GO

-- 10. Bảng Đơn hàng
CREATE TABLE DonHang (
    MaDonHang INT IDENTITY(1,1) PRIMARY KEY,
    MaNguoiDung INT NOT NULL,
    NgayDat DATETIME DEFAULT GETDATE(),
    TongTien DECIMAL(18, 2) NOT NULL DEFAULT 0,
    TrangThai NVARCHAR(50) DEFAULT N'Chờ xử lý', -- 'Chờ xử lý', 'Đang giao', 'Đã hoàn thành', 'Đã hủy'
    PhuongThucThanhToan NVARCHAR(50) DEFAULT N'COD',
    DiaChiGiaoHang NVARCHAR(250) NOT NULL,
    DienThoaiNhanHang VARCHAR(20) NOT NULL,
    GhiChu NVARCHAR(500) NULL,
    CONSTRAINT FK_DonHang_NguoiDung FOREIGN KEY (MaNguoiDung) REFERENCES NguoiDung(MaNguoiDung)
);
GO

-- 11. Bảng Chi tiết đơn hàng
CREATE TABLE ChiTietDonHang (
    MaDonHang INT NOT NULL,
    MaChiTietSP INT NOT NULL,
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    DonGia DECIMAL(18, 2) NOT NULL,
    PRIMARY KEY (MaDonHang, MaChiTietSP),
    CONSTRAINT FK_ChiTietDonHang_DonHang FOREIGN KEY (MaDonHang) REFERENCES DonHang(MaDonHang) ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietDonHang_ChiTietSP FOREIGN KEY (MaChiTietSP) REFERENCES ChiTietSanPham(MaChiTietSP)
);
GO
-- Thêm Vai trò
INSERT INTO VaiTro (TenVaiTro) VALUES (N'Admin'), (N'Customer');

-- Thêm Tài khoản
INSERT INTO NguoiDung (MaVaiTro, HoTen, Email, MatKhau, DienThoai, DiaChi) VALUES
(1, N'Quản Trị Viên Cara', 'admin@cara.com', '123456', '0900000000', N'Thủ Dầu Một, Bình Dương'),
(2, N'Nguyễn Văn A', 'khachhang@gmail.com', '123456', '0912345678', N'TP. Hồ Chí Minh');

-- Thêm Danh mục & Kích thước / Màu sắc
INSERT INTO DanhMuc (TenDanhMuc) VALUES (N'Áo Thun'), (N'Áo Sơ Mi'), (N'Quần Jean'), (N'Đầm Nữ');
INSERT INTO KichThuoc (TenKichThuoc) VALUES ('S'), ('M'), ('L'), ('XL');
INSERT INTO MauSac (TenMauSac, MaMauHex) VALUES (N'Đen', '#000000'), (N'Trắng', '#FFFFFF'), (N'Xanh Dương', '#0000FF');

-- Thêm Sản phẩm
INSERT INTO SanPham (MaDanhMuc, TenSanPham, GiaGoc, GiaKhuyenMai, HinhAnhChinh, MoTa) VALUES
(1, N'Áo Thun Form Rộng Cara Basic', 250000, 199000, 'aothun_basic.jpg', N'Chất liệu 100% Cotton thoáng mát'),
(2, N'Áo Sơ Mi Lụa Công Sở', 350000, NULL, 'somi_lua.jpg', N'Thiết kế thanh lịch, chống nhăn');

-- Thêm Biến thể sản phẩm (Size & Màu)
INSERT INTO ChiTietSanPham (MaSanPham, MaKichThuoc, MaMauSac, SoLuongTon) VALUES
(1, 1, 1, 20), -- Áo thun - Size S - Màu Đen: 20 cái
(1, 2, 1, 30), -- Áo thun - Size M - Màu Đen: 30 cái
(1, 2, 2, 15); -- Áo thun - Size M - Màu Trắng: 15 cái

GO
SELECT * FROM ChiTietSanPham WHERE MaSanPham = 1
SELECT MaSanPham, TenSanPham FROM SanPham;
INSERT INTO ChiTietSanPham (MaSanPham, MaKichThuoc, MaMauSac, SoLuongTon) VALUES
(2, 1, 1, 20), -- Áo sơ mi - Size 1 - Màu 1: 20 cái
(2, 2, 1, 30), -- Áo sơ mi - Size 2 - Màu 1: 30 cái
(2, 2, 2, 15); -- Áo sơ mi - Size 2 - Màu 2: 15 cái


UPDATE NguoiDung 
SET MaVaiTro = 1 
WHERE Email = '2424802010338@student.tdmu.edu.vn';

SELECT * FROM NguoiDung;