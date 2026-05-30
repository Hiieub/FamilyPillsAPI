CREATE DATABASE IF NOT EXISTS FamilyPillsDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE FamilyPillsDB;

CREATE TABLE IF NOT EXISTS medicines (
    id INT AUTO_INCREMENT PRIMARY KEY COMMENT 'Mã định danh thuốc',
    name VARCHAR(255) NOT NULL COMMENT 'Tên thuốc',
    barcode VARCHAR(100) COMMENT 'Mã vạch thuốc',
    total_quantity INT NOT NULL DEFAULT 0 COMMENT 'Tổng số lượng thuốc',
    unit VARCHAR(50) COMMENT 'Đơn vị (viên, ml, g...)',
    expiry_date VARCHAR(50) COMMENT 'Ngày hết hạn',
    image_path VARCHAR(500) COMMENT 'Đường dẫn hình ảnh thuốc',
    quantity VARCHAR(100) COMMENT 'Số lượng (khoảng hoặc cụ thể)',
    last_updated VARCHAR(50) COMMENT 'Lần cập nhật cuối cùng',
    is_running_low BOOLEAN DEFAULT false COMMENT 'Có đang hết hàng không',
    is_expired BOOLEAN DEFAULT false COMMENT 'Có hết hạn không',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP COMMENT 'Ngày tạo',
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP COMMENT 'Ngày cập nhật'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bảng quản lý thuốc';

CREATE INDEX idx_medicine_name ON medicines(name);
CREATE INDEX idx_medicine_barcode ON medicines(barcode);
CREATE INDEX idx_medicine_running_low ON medicines(is_running_low);
CREATE INDEX idx_medicine_expired ON medicines(is_expired);

INSERT INTO medicines (name, barcode, total_quantity, unit, expiry_date, is_running_low, is_expired) 
VALUES 
('Aspirin', '1234567890', 100, 'viên', '2025-12-31', false, false),
('Paracetamol', '1234567891', 50, 'viên', '2025-06-30', true, false),
('Vitamin C', '1234567892', 200, 'viên', '2026-01-15', false, false);

SELECT * FROM medicines;
