USE WebCaraDb;
GO

-- =============================================
-- 1. CẬP NHẬT ẢNH ĐẸP CHO 2 SẢN PHẨM CŨ (MaSanPham = 1 và 2)
-- =============================================
UPDATE SanPham 
SET HinhAnhChinh = 'https://images.unsplash.com/photo-1521572267360-ee0c2909d518?w=600'
WHERE MaSanPham = 1;

UPDATE SanPham 
SET HinhAnhChinh = 'https://images.unsplash.com/photo-1598554747436-c9293d6a588f?w=600'
WHERE MaSanPham = 2;


-- =============================================
-- 2. THÊM 6 SẢN PHẨM MỚI VÀO BẢNG SanPham
-- =============================================
INSERT INTO SanPham (MaDanhMuc, TenSanPham, GiaGoc, GiaKhuyenMai, HinhAnhChinh, MoTa) VALUES
-- Danh mục 1: Áo Thun
(1, N'Áo Polo Nam Form Regular Premium', 350000, 290000, 'https://images.unsplash.com/photo-1625910513413-5fc283407a53?w=600', N'Chất liệu thun CVC cá sấu co dãn 4 chiều, thấm hút mồ hôi tốt.'),

-- Danh mục 2: Áo Sơ Mi
(2, N'Áo Sơ Mi Nam Oxford Trắng', 420000, NULL, 'https://images.unsplash.com/photo-1602810318383-e386cc2a3ccf?w=600', N'Áo sơ mi vải Oxford dày dặn, đứng phom, phong cách lịch lãm.'),

-- Danh mục 3: Quần Jean
(3, N'Quần Jeans Nam Straight Fit Vintage', 550000, 480000, 'https://images.unsplash.com/photo-1541099649105-f69ad21f3246?w=600', N'Chất liệu Denim cao cấp, phom dáng suông cổ điển, dễ phối đồ.'),
(3, N'Quần Jeans Slimfit Co Giãn', 490000, NULL, 'https://images.unsplash.com/photo-1582552938357-32b906df40cb?w=600', N'Thiết kế ôm chân vừa vặn, chất vải co giãn thoải mái khi vận động.'),

-- Danh mục 4: Đầm Nữ
(4, N'Đầm Lụa Tơ Tằm Dự Tiệc Sang Trọng', 890000, 750000, 'https://images.unsplash.com/photo-1566174053879-31528523f8ae?w=600', N'Chất liệu lụa satin mềm mịn, óng ả, tôn dáng quyến rũ cho phái đẹp.'),
(4, N'Đầm Xòe Dáng Dài Tinh Tế', 650000, NULL, 'https://images.unsplash.com/photo-1572804013309-59a88b7e92f1?w=600', N'Thiết kế cổ V thanh lịch, chiết eo tôn dáng nhẹ nhàng.');


-- =============================================
-- 3. THÊM BIẾN THỂ KHO HÀNG (Size & Màu) CHO CÁC SẢN PHẨM MỚI
-- (KichThuoc: 1=S, 2=M, 3=L, 4=XL | MauSac: 1=Đen, 2=Trắng, 3=Xanh)
-- =============================================

-- Tự động thêm biến thể cho MaSanPham từ 3 đến 8
INSERT INTO ChiTietSanPham (MaSanPham, MaKichThuoc, MaMauSac, SoLuongTon) VALUES
-- Sản phẩm 3 (Áo Polo Nam)
(3, 1, 1, 15), (3, 2, 1, 20), (3, 2, 2, 10), (3, 3, 3, 25),

-- Sản phẩm 4 (Áo Sơ Mi Oxford)
(4, 1, 2, 30), (4, 2, 2, 25), (4, 3, 2, 15),

-- Sản phẩm 5 (Quần Jeans Straight)
(5, 2, 1, 20), (5, 3, 1, 30), (5, 4, 1, 15),

-- Sản phẩm 6 (Quần Jeans Slimfit)
(6, 1, 3, 10), (6, 2, 3, 25), (6, 3, 3, 20),

-- Sản phẩm 7 (Đầm Lụa Dự Tiệc)
(7, 1, 1, 12), (7, 2, 1, 18), (7, 2, 2, 10),

-- Sản phẩm 8 (Đầm Xòe Dáng Dài)
(8, 1, 2, 15), (8, 2, 2, 20), (8, 3, 3, 10);
GO