-- =====================================================================
-- STM - Additional monitoring inputs on stm_transaction
--
-- These columns supply data that several rules need but the system never
-- captured before. Without them the rules engine had no honest source for
-- the values and fell back to faking them (e.g. PolicyAgeMonths -> Amount),
-- producing wrong HIT/MISS results.
--
--   policy_inception_date  -> drives PolicyAgeMonths (Insurance)
--   property_purchase_date -> drives PreviousOwnershipDays (Real Estate)
--   customer_nationality   -> single-party customer nationality
--   has_pep                -> politically-exposed-person flag
--
-- (remitter_id already exists on stm_transaction.)
-- =====================================================================

ALTER TABLE stm_transaction
    ADD COLUMN policy_inception_date  DATETIME     NULL AFTER policy_no,
    ADD COLUMN property_purchase_date DATETIME     NULL AFTER property_value,
    ADD COLUMN customer_nationality   VARCHAR(50)  NULL AFTER customer_type,
    ADD COLUMN has_pep                TINYINT(1)   NOT NULL DEFAULT 0 AFTER is_high_risk_customer;
