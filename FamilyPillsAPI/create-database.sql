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
CALL add_index_if_missing('medicines', 'idx_medicines_user_id', '(user_id)');
CALL add_index_if_missing('medicines', 'idx_medicines_name', '(name)');
CALL add_index_if_missing('medicines', 'idx_medicines_barcode', '(barcode)');
CALL add_index_if_missing('medicines', 'idx_medicines_running_low', '(is_running_low)');
CALL add_index_if_missing('medicines', 'idx_medicines_expired', '(is_expired)');

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
