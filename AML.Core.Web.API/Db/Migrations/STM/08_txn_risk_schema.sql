-- =====================================================================
-- Transaction Risk schema for STM (Insurance + Real Estate)
--
-- A transaction is scored against a set of *risk factors* tied to its
-- sector. Each factor has one or more *bands* (value-range → score).
-- The engine walks the factors, picks the band that matches the
-- transaction's field value, sums the scores, and assigns a rating.
--
-- Tables:
--   stm_txn_risk_factor   - per-sector factor definitions
--   stm_txn_risk_band     - score bands (one factor -> many bands)
--   stm_txn_risk_result   - per-transaction computed score + items
-- =====================================================================

CREATE TABLE IF NOT EXISTS stm_txn_risk_factor (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    factor_code     VARCHAR(60)  NOT NULL,
    factor_name     VARCHAR(200) NOT NULL,
    description     VARCHAR(500) NULL,
    sector_id       INT          NOT NULL,
    field_name      VARCHAR(100) NOT NULL,   -- which transaction field to read
    factor_type     VARCHAR(30)  NOT NULL,   -- NUMERIC / ENUM / FLAG / LIST / RELATIONSHIP
    weight          INT          NOT NULL DEFAULT 1,
    is_active       TINYINT(1)   NOT NULL DEFAULT 1,
    sequence_no     INT          NOT NULL DEFAULT 0,
    client_id       INT          NOT NULL DEFAULT 0,
    created_on      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by      INT          NULL,
    UNIQUE KEY uq_stm_tf_code_client (factor_code, client_id),
    INDEX idx_stm_tf_sector (sector_id, is_active),
    CONSTRAINT fk_stm_tf_sector FOREIGN KEY (sector_id) REFERENCES stm_sector(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS stm_txn_risk_band (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    factor_id       INT          NOT NULL,
    band_label      VARCHAR(150) NOT NULL,   -- e.g. ">= 100,000", "Cash", "in high-risk list"
    -- One of (numeric_min, numeric_max, match_value, match_in_list) is used per row
    numeric_min     DECIMAL(20,4) NULL,
    numeric_max     DECIMAL(20,4) NULL,
    match_value     VARCHAR(200) NULL,        -- exact string equal
    match_in_list   VARCHAR(2000) NULL,       -- comma-separated list of values
    score           INT          NOT NULL,    -- 1=Low, 2=Medium, 3=High typically
    rating          VARCHAR(20)  NOT NULL,    -- Low / Medium / High
    sequence_no     INT          NOT NULL DEFAULT 0,
    INDEX idx_stm_tb_factor (factor_id),
    CONSTRAINT fk_stm_tb_factor FOREIGN KEY (factor_id) REFERENCES stm_txn_risk_factor(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS stm_txn_risk_result (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    transaction_id      INT          NOT NULL,
    sector_id           INT          NOT NULL,
    total_score         INT          NOT NULL DEFAULT 0,
    risk_rating         VARCHAR(20)  NOT NULL DEFAULT 'Low',
    factor_count        INT          NOT NULL DEFAULT 0,
    factor_breakdown    TEXT         NULL,        -- JSON array of factor results for audit
    client_id           INT          NOT NULL DEFAULT 0,
    computed_on         DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY uq_stm_trr_tran (transaction_id),
    INDEX idx_stm_trr_rating (risk_rating, sector_id),
    CONSTRAINT fk_stm_trr_tran FOREIGN KEY (transaction_id) REFERENCES stm_transaction(id) ON DELETE CASCADE,
    CONSTRAINT fk_stm_trr_sector FOREIGN KEY (sector_id) REFERENCES stm_sector(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
