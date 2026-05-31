CREATE DATABASE IF NOT EXISTS FamilyPillsDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE FamilyPillsDB;

CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    full_name VARCHAR(255),
    last_login TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bang nguoi dung';

CREATE TABLE IF NOT EXISTS refresh_tokens (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    token_hash VARCHAR(255) NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    revoked_at TIMESTAMP NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_refresh_tokens_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bang refresh token neu can invalidate logout';

CREATE TABLE IF NOT EXISTS medicines (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NULL,
    name VARCHAR(255) NOT NULL,
    barcode VARCHAR(100),
    total_quantity INT NOT NULL DEFAULT 0,
    unit VARCHAR(50),
    expiry_date VARCHAR(50),
    image_path VARCHAR(500),
    quantity VARCHAR(100),
    last_updated VARCHAR(50),
    is_running_low BOOLEAN DEFAULT false,
    is_expired BOOLEAN DEFAULT false,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_medicines_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bang quan ly thuoc';

CREATE TABLE IF NOT EXISTS medicine_images (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NULL,
    medicine_id INT NULL,
    image_path VARCHAR(500) NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    file_size BIGINT NOT NULL DEFAULT 0,
    content_type VARCHAR(100),
    uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_medicine_images_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE SET NULL,
    CONSTRAINT fk_medicine_images_medicine
        FOREIGN KEY (medicine_id) REFERENCES medicines(id)
        ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bang metadata anh thuoc';

CREATE TABLE IF NOT EXISTS medicine_inventory_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NULL,
    medicine_id INT NOT NULL,
    change_amount INT NOT NULL,
    quantity_before INT NULL,
    quantity_after INT NULL,
    reason VARCHAR(255),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_inventory_logs_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE SET NULL,
    CONSTRAINT fk_inventory_logs_medicine
        FOREIGN KEY (medicine_id) REFERENCES medicines(id)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bang lich su thay doi so luong thuoc';

CREATE TABLE IF NOT EXISTS user_settings (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL UNIQUE,
    low_stock_threshold INT NOT NULL DEFAULT 10,
    expiry_warning_days INT NOT NULL DEFAULT 30,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_user_settings_user
        FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Bang cau hinh rieng cua nguoi dung';

DROP PROCEDURE IF EXISTS add_column_if_missing;
DROP PROCEDURE IF EXISTS add_index_if_missing;
DROP PROCEDURE IF EXISTS add_fk_if_missing;

DELIMITER $$

CREATE PROCEDURE add_column_if_missing(
    IN table_name_value VARCHAR(64),
    IN column_name_value VARCHAR(64),
    IN column_definition_value TEXT
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = table_name_value
          AND COLUMN_NAME = column_name_value
    ) THEN
        SET @ddl = CONCAT('ALTER TABLE ', table_name_value, ' ADD COLUMN ', column_name_value, ' ', column_definition_value);
        PREPARE stmt FROM @ddl;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

CREATE PROCEDURE add_index_if_missing(
    IN table_name_value VARCHAR(64),
    IN index_name_value VARCHAR(64),
    IN index_definition_value TEXT
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM INFORMATION_SCHEMA.STATISTICS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = table_name_value
          AND INDEX_NAME = index_name_value
    ) THEN
        SET @ddl = CONCAT('ALTER TABLE ', table_name_value, ' ADD INDEX ', index_name_value, ' ', index_definition_value);
        PREPARE stmt FROM @ddl;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

CREATE PROCEDURE add_fk_if_missing(
    IN table_name_value VARCHAR(64),
    IN constraint_name_value VARCHAR(64),
    IN fk_definition_value TEXT
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
        WHERE TABLE_SCHEMA = DATABASE()
          AND TABLE_NAME = table_name_value
          AND CONSTRAINT_NAME = constraint_name_value
          AND CONSTRAINT_TYPE = 'FOREIGN KEY'
    ) THEN
        SET @ddl = CONCAT('ALTER TABLE ', table_name_value, ' ADD CONSTRAINT ', constraint_name_value, ' ', fk_definition_value);
        PREPARE stmt FROM @ddl;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END$$

DELIMITER ;

CALL add_column_if_missing('users', 'last_login', 'TIMESTAMP NULL');
CALL add_column_if_missing('medicines', 'user_id', 'INT NULL');
CALL add_column_if_missing('medicines', 'created_at', 'TIMESTAMP DEFAULT CURRENT_TIMESTAMP');
CALL add_column_if_missing('medicines', 'updated_at', 'TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP');

CALL add_fk_if_missing('medicines', 'fk_medicines_user', 'FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE');

CALL add_index_if_missing('users', 'idx_users_email', '(email)');
CALL add_index_if_missing('refresh_tokens', 'idx_refresh_tokens_user_id', '(user_id)');
CALL add_index_if_missing('refresh_tokens', 'idx_refresh_tokens_expires_at', '(expires_at)');
CALL add_index_if_missing('medicines', 'idx_medicines_user_id', '(user_id)');
CALL add_index_if_missing('medicines', 'idx_medicines_name', '(name)');
CALL add_index_if_missing('medicines', 'idx_medicines_barcode', '(barcode)');
CALL add_index_if_missing('medicines', 'idx_medicines_running_low', '(is_running_low)');
CALL add_index_if_missing('medicines', 'idx_medicines_expired', '(is_expired)');
CALL add_index_if_missing('medicine_images', 'idx_medicine_images_user_id', '(user_id)');
CALL add_index_if_missing('medicine_images', 'idx_medicine_images_medicine_id', '(medicine_id)');
CALL add_index_if_missing('medicine_inventory_logs', 'idx_inventory_logs_user_id', '(user_id)');
CALL add_index_if_missing('medicine_inventory_logs', 'idx_inventory_logs_medicine_id', '(medicine_id)');

DROP PROCEDURE add_column_if_missing;
DROP PROCEDURE add_index_if_missing;
DROP PROCEDURE add_fk_if_missing;

INSERT INTO medicines (name, barcode, total_quantity, unit, expiry_date, quantity, is_running_low, is_expired)
SELECT 'Aspirin', '1234567890', 100, 'vien', '2025-12-31', '100', false, false
WHERE NOT EXISTS (SELECT 1 FROM medicines WHERE barcode = '1234567890');

INSERT INTO medicines (name, barcode, total_quantity, unit, expiry_date, quantity, is_running_low, is_expired)
SELECT 'Paracetamol', '1234567891', 50, 'vien', '2025-06-30', '50', true, false
WHERE NOT EXISTS (SELECT 1 FROM medicines WHERE barcode = '1234567891');

INSERT INTO medicines (name, barcode, total_quantity, unit, expiry_date, quantity, is_running_low, is_expired)
SELECT 'Vitamin C', '1234567892', 200, 'vien', '2026-01-15', '200', false, false
WHERE NOT EXISTS (SELECT 1 FROM medicines WHERE barcode = '1234567892');
