-- =============================================
-- PostgreSQL Schema for Inventory Management
-- Run this against your PostgreSQL database
-- =============================================

-- 1. Users table
CREATE TABLE IF NOT EXISTS users (
    id              SERIAL PRIMARY KEY,
    username        VARCHAR(100) NOT NULL UNIQUE,
    email           VARCHAR(255) NOT NULL UNIQUE,
    password        VARCHAR(255) NOT NULL,
    role            VARCHAR(20)  NOT NULL DEFAULT 'user',
    created_at      TIMESTAMP    NOT NULL DEFAULT NOW(),
    reset_token     VARCHAR(255),
    reset_token_expiry TIMESTAMP,
    phone_number    VARCHAR(20)
);

-- 2. Category table
CREATE TABLE IF NOT EXISTS category (
    category_id     SERIAL PRIMARY KEY,
    category_name   VARCHAR(255) NOT NULL,
    threshold       INTEGER      NOT NULL DEFAULT 0
);

-- 3. Consumable items table
CREATE TABLE IF NOT EXISTS consumable_items (
    item_id             SERIAL PRIMARY KEY,
    name                VARCHAR(255) NOT NULL,
    category_id         INTEGER      NOT NULL REFERENCES category(category_id),
    model_no            VARCHAR(100) NOT NULL DEFAULT '',
    brand               VARCHAR(100) NOT NULL DEFAULT '',
    quantity            INTEGER      NOT NULL DEFAULT 0,
    storage_loc_l1      VARCHAR(255) NOT NULL DEFAULT '',
    storage_loc_l2      VARCHAR(255) NOT NULL DEFAULT '',
    warranty_expiration DATE
);

-- 4. Purchase details table
CREATE TABLE IF NOT EXISTS purchase_details (
    order_id        SERIAL PRIMARY KEY,
    item_id         INTEGER      NOT NULL REFERENCES consumable_items(item_id),
    quantity        INTEGER      NOT NULL,
    price           NUMERIC(12,2) NOT NULL,
    purchase_date   VARCHAR(20)  NOT NULL
);

-- 5. Issue records table
CREATE TABLE IF NOT EXISTS issue_records (
    issue_id        SERIAL PRIMARY KEY,
    issued_to       VARCHAR(255) NOT NULL,
    department      VARCHAR(255) NOT NULL,
    quantity        INTEGER      NOT NULL,
    item_id         INTEGER      NOT NULL REFERENCES consumable_items(item_id),
    issue_date      TIMESTAMP    NOT NULL DEFAULT NOW(),
    status          VARCHAR(20)  NOT NULL DEFAULT 'pending',
    requested_by    VARCHAR(100) NOT NULL DEFAULT 'user',
    return_status   VARCHAR(20)  NOT NULL DEFAULT 'none'
);

-- 6. Alert log table
CREATE TABLE IF NOT EXISTS alert_log (
    log_id           SERIAL PRIMARY KEY,
    item_id          INTEGER,
    category_id      INTEGER,
    category_name    VARCHAR(255),
    current_quantity INTEGER      NOT NULL,
    alert_message    TEXT,
    alert_time       TIMESTAMP    NOT NULL DEFAULT NOW()
);

-- =============================================
-- Optional: Indexes for commonly queried columns
-- =============================================
CREATE INDEX IF NOT EXISTS idx_consumable_items_category ON consumable_items(category_id);
CREATE INDEX IF NOT EXISTS idx_purchase_details_item     ON purchase_details(item_id);
CREATE INDEX IF NOT EXISTS idx_issue_records_item        ON issue_records(item_id);
CREATE INDEX IF NOT EXISTS idx_issue_records_status      ON issue_records(status);
CREATE INDEX IF NOT EXISTS idx_alert_log_time            ON alert_log(alert_time);
CREATE INDEX IF NOT EXISTS idx_users_email               ON users(email);
CREATE OR REPLACE FUNCTION check_stock_threshold()
RETURNS TRIGGER AS $$
DECLARE
    cat_threshold INT;
    cat_name VARCHAR;
BEGIN
    -- Only act if the quantity actually decreased
    IF NEW.quantity < OLD.quantity THEN
        -- Get the category threshold and name
        SELECT threshold, category_name INTO cat_threshold, cat_name
        FROM category
        WHERE category_id = NEW.category_id;

        -- If it dropped below threshold
        IF NEW.quantity < cat_threshold THEN
            -- Insert into alert_log
            INSERT INTO alert_log (item_id, category_id, category_name, current_quantity, alert_message, alert_time)
            VALUES (
                NEW.item_id,
                NEW.category_id,
                cat_name,
                NEW.quantity,
                'Stock for item ' || NEW.name || ' dropped to ' || NEW.quantity || ' (Below threshold of ' || cat_threshold || ')',
                NOW()
            );
        END IF;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trigger_check_stock_threshold ON consumable_items;

CREATE TRIGGER trigger_check_stock_threshold
AFTER UPDATE OF quantity ON consumable_items
FOR EACH ROW
EXECUTE FUNCTION check_stock_threshold();
