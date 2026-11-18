USE WebCaraDb;
GO

BEGIN TRANSACTION;

-- Kiểm tra tài khoản có đang sử dụng vai trò 4 đến 11 không
IF EXISTS (
    SELECT 1
    FROM NguoiDung
    WHERE MaVaiTro BETWEEN 4 AND 11
)
BEGIN
    ROLLBACK TRANSACTION;
    THROW 50001, N'Có tài khoản đang sử dụng vai trò dư. Không thể xóa.', 1;
END;

-- Kiểm tra bảng phân quyền
IF EXISTS (
    SELECT 1
    FROM PhanQuyen
    WHERE MaVaiTro BETWEEN 4 AND 11
)
BEGIN
    ROLLBACK TRANSACTION;
    THROW 50002, N'Có dữ liệu phân quyền đang sử dụng vai trò dư.', 1;
END;

-- Xóa các vai trò trùng lặp
DELETE FROM VaiTro
WHERE MaVaiTro BETWEEN 4 AND 11;

COMMIT TRANSACTION;
GO

-- Kiểm tra kết quả
SELECT *
FROM VaiTro
ORDER BY MaVaiTro;