-- =====================================================================
-- Sectoral Transaction Monitoring System (STM) - Schema
-- Target DB: MySQL (database: lemon_uat)
-- Run order: 01_stm_schema.sql -> 02_stm_seed_sectors.sql -> 03_stm_seed_rules.sql
-- =====================================================================

-- ---------------------------------------------------------------------
-- Sector master (Insurance, Real Estate)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS stm_sector (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    sector_code     VARCHAR(50)  NOT NULL UNIQUE,
    sector_name     VARCHAR(150) NOT NULL,
    description     VARCHAR(500) NULL,
    is_active       TINYINT(1)   NOT NULL DEFAULT 1,
    client_id       INT          NOT NULL DEFAULT 0,
    created_on      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by      INT          NULL,
    updated_on      DATETIME     NULL,
    updated_by      INT          NULL,
    INDEX idx_stm_sector_client (client_id, is_active)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------------
-- Rule master (rule definition per sector)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS stm_rule (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    rule_code           VARCHAR(50)  NOT NULL,
    rule_name           VARCHAR(250) NOT NULL,
    rule_description    VARCHAR(2000) NULL,
    sector_id           INT          NOT NULL,
    risk_rating         VARCHAR(20)  NOT NULL DEFAULT 'High', -- High / Medium / Low
    rule_score          INT          NOT NULL DEFAULT 50,
    logical_operator    VARCHAR(10)  NOT NULL DEFAULT 'AND',  -- AND / OR
    action_on_hit       VARCHAR(50)  NOT NULL DEFAULT 'CREATE_CASE',
    is_active           TINYINT(1)   NOT NULL DEFAULT 1,
    is_system_rule      TINYINT(1)   NOT NULL DEFAULT 0,
    client_id           INT          NOT NULL DEFAULT 0,
    created_on          DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          INT          NULL,
    updated_on          DATETIME     NULL,
    updated_by          INT          NULL,
    UNIQUE KEY uq_stm_rule_code_client (rule_code, client_id),
    INDEX idx_stm_rule_sector (sector_id, is_active),
    CONSTRAINT fk_stm_rule_sector FOREIGN KEY (sector_id) REFERENCES stm_sector(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------------
-- Rule conditions (normalized; one rule -> many conditions)
-- Each row represents: <field> <operator> <value> [aggregation/timeframe]
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS stm_rule_condition (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    rule_id             INT NOT NULL,
    sequence_no         INT NOT NULL DEFAULT 1,
    field_name          VARCHAR(100) NOT NULL,   -- e.g. Amount, Country, CustomerType, etc.
    field_aggregation   VARCHAR(50)  NULL,       -- Sum / Count / Avg / Min / Max / null
    operator            VARCHAR(50)  NOT NULL,   -- equals, not_equals, gt, gte, lt, lte, in, not_in, contains, between, within
    compare_value       VARCHAR(2000) NULL,      -- single value or JSON array
    compare_field       VARCHAR(100) NULL,       -- when comparing two fields
    timeframe_value     INT          NULL,
    timeframe_unit      VARCHAR(20)  NULL,       -- Day / Week / Month / Year
    conjunction         VARCHAR(10)  NOT NULL DEFAULT 'AND',  -- AND / OR (joins to NEXT condition)
    description         VARCHAR(500) NULL,
    created_on          DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_stm_rc_rule (rule_id),
    CONSTRAINT fk_stm_rc_rule FOREIGN KEY (rule_id) REFERENCES stm_rule(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------------
-- Transaction (the actual transaction being monitored)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS stm_transaction (
    id                      INT AUTO_INCREMENT PRIMARY KEY,
    tran_ref_no             VARCHAR(100) NOT NULL,
    sector_id               INT          NOT NULL,
    tran_date               DATETIME     NOT NULL,
    tran_type               VARCHAR(50)  NOT NULL,  -- Credit / Debit / Premium / Claim / Purchase / Sale / Rental
    tran_mode               VARCHAR(50)  NULL,      -- Cash / Cheque / Bank / Wire / Card / Crypto
    delivery_channel        VARCHAR(50)  NULL,      -- Branch / Online / Agent / Broker
    product                 VARCHAR(100) NULL,      -- Insurance product or Real Estate product
    amount                  DECIMAL(20,4) NOT NULL DEFAULT 0,
    currency                VARCHAR(10)  NOT NULL DEFAULT 'AED',
    -- Primary customer (Single-party path)
    customer_id             VARCHAR(100) NULL,
    customer_name           VARCHAR(250) NULL,
    customer_type           VARCHAR(20)  NULL,      -- I (Individual) / C (Corporate)
    -- Counter-parties
    remitter_id             VARCHAR(100) NULL,
    remitter_name           VARCHAR(250) NULL,
    remitter_country        VARCHAR(10)  NULL,
    beneficiary_id          VARCHAR(100) NULL,
    beneficiary_name        VARCHAR(250) NULL,
    beneficiary_country     VARCHAR(10)  NULL,
    -- Sector-specific
    policy_no               VARCHAR(100) NULL,      -- Insurance
    property_ref            VARCHAR(100) NULL,      -- Real Estate property identifier
    property_value          DECIMAL(20,4) NULL,     -- Real Estate
    purpose                 VARCHAR(500) NULL,
    branch_code             VARCHAR(50)  NULL,
    -- Flags
    is_high_risk_country    TINYINT(1)   NOT NULL DEFAULT 0,
    is_high_risk_customer   TINYINT(1)   NOT NULL DEFAULT 0,
    is_multi_party          TINYINT(1)   NOT NULL DEFAULT 0,
    rule_hit_status         VARCHAR(20)  NOT NULL DEFAULT 'PENDING', -- PENDING / HIT / NO_HIT
    -- Audit
    client_id               INT          NOT NULL DEFAULT 0,
    created_on              DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by              INT          NULL,
    updated_on              DATETIME     NULL,
    updated_by              INT          NULL,
    UNIQUE KEY uq_stm_tran_ref (tran_ref_no, client_id),
    INDEX idx_stm_tran_sector (sector_id, tran_date),
    INDEX idx_stm_tran_customer (customer_id),
    INDEX idx_stm_tran_status (rule_hit_status),
    CONSTRAINT fk_stm_tran_sector FOREIGN KEY (sector_id) REFERENCES stm_sector(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------------
-- Multi-party (one transaction -> many parties, e.g. joint policy holders)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS stm_transaction_party (
    id                      INT AUTO_INCREMENT PRIMARY KEY,
    transaction_id          INT NOT NULL,
    party_role              VARCHAR(50)  NOT NULL,  -- PolicyHolder / Insured / Nominee / Buyer / Seller / Agent / Broker / Beneficiary / Sender
    customer_id             VARCHAR(100) NULL,
    customer_master_id      INT          NULL,
    customer_name           VARCHAR(250) NULL,
    nationality             VARCHAR(50)  NULL,
    customer_type           VARCHAR(20)  NULL,
    id_type                 VARCHAR(50)  NULL,
    id_number               VARCHAR(100) NULL,
    mobile                  VARCHAR(50)  NULL,
    address                 VARCHAR(500) NULL,
    relation_to_primary     VARCHAR(100) NULL,
    share_percentage        DECIMAL(6,2) NULL,
    created_on              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_stm_tp_tran (transaction_id),
    INDEX idx_stm_tp_cust (customer_id),
    CONSTRAINT fk_stm_tp_tran FOREIGN KEY (transaction_id) REFERENCES stm_transaction(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------------
-- Case (created when one or more rules hit a transaction)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS stm_case (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    case_ref_no         VARCHAR(50)  NOT NULL,
    transaction_id      INT          NOT NULL,
    sector_id           INT          NOT NULL,
    rules_violated      VARCHAR(2000) NULL,   -- JSON array of rule_ids
    rule_names          VARCHAR(2000) NULL,   -- denormalized comma separated for display
    total_risk_score    INT          NOT NULL DEFAULT 0,
    risk_rating         VARCHAR(20)  NOT NULL DEFAULT 'High',
    status              VARCHAR(30)  NOT NULL DEFAULT 'OPEN',
        -- OPEN / IN_REVIEW / APPROVED / REJECTED / ESCALATED / CLOSED
    assigned_to         INT          NULL,
    reviewed_by         INT          NULL,
    reviewed_on         DATETIME     NULL,
    review_decision     VARCHAR(30)  NULL, -- TRUE_POSITIVE / FALSE_POSITIVE / ESCALATE
    review_remarks      VARCHAR(2000) NULL,
    client_id           INT          NOT NULL DEFAULT 0,
    created_on          DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by          INT          NULL,
    updated_on          DATETIME     NULL,
    updated_by          INT          NULL,
    UNIQUE KEY uq_stm_case_ref (case_ref_no, client_id),
    INDEX idx_stm_case_status (status, sector_id),
    INDEX idx_stm_case_tran (transaction_id),
    CONSTRAINT fk_stm_case_tran FOREIGN KEY (transaction_id) REFERENCES stm_transaction(id),
    CONSTRAINT fk_stm_case_sector FOREIGN KEY (sector_id) REFERENCES stm_sector(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------------
-- Case comment / audit trail
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS stm_case_comment (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    case_id         INT NOT NULL,
    comment_text    VARCHAR(2000) NOT NULL,
    action_type     VARCHAR(50) NULL,   -- COMMENT / STATUS_CHANGE / ASSIGN / DECISION
    created_on      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by      INT NULL,
    created_user    VARCHAR(150) NULL,
    INDEX idx_stm_cc_case (case_id),
    CONSTRAINT fk_stm_cc_case FOREIGN KEY (case_id) REFERENCES stm_case(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- ---------------------------------------------------------------------
-- Rule execution log (for traceability)
-- ---------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS stm_rule_exec_log (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    transaction_id  INT          NOT NULL,
    rule_id         INT          NOT NULL,
    is_hit          TINYINT(1)   NOT NULL,
    hit_details     TEXT         NULL,
    executed_on     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_stm_rel_tran (transaction_id),
    INDEX idx_stm_rel_rule (rule_id),
    CONSTRAINT fk_stm_rel_tran FOREIGN KEY (transaction_id) REFERENCES stm_transaction(id) ON DELETE CASCADE,
    CONSTRAINT fk_stm_rel_rule FOREIGN KEY (rule_id) REFERENCES stm_rule(id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
