-- 2026-05-02_aml_tracker_compliance_columns.sql
-- Adds the 8 columns referenced by the AML Tracker compliance Excel export
-- (Bucket C — fields with no existing source in the schema).
--
-- These are read by:
--   AML.Core.Repository/AmlTracker/AmlTrackerRepository.cs   (GetTrackerCases SELECT)
--   AML.Web/Controllers/AmlTracker/AmlTrackerController.cs   (BuildExcel / BuildCsv)
--
-- Backwards compatibility:
--   - All columns are NULL-able and have no default. Existing rows stay untouched.
--   - The Bucket-A and Bucket-B columns the export uses are already in the schema
--     (customermaster.cif_number / product_name / product_value / cust_id_*, case_comment,
--      transaction_risk_individual.dateofassessment, etc.) and need no migration.
--
-- Roll-forward: run this script once against the target MySQL database.
-- Roll-back:    see the DROP COLUMN block at the bottom (commented out).

ALTER TABLE customercase
    ADD COLUMN policy_issue_date          DATE         NULL COMMENT 'Compliance export: policy issue date',
    ADD COLUMN date_of_response           DATETIME     NULL COMMENT 'Compliance export: date the case was responded to',
    ADD COLUMN client_product_number      VARCHAR(64)  NULL COMMENT 'Compliance export: client-side product / line number',
    ADD COLUMN policy_holder              VARCHAR(255) NULL COMMENT 'Compliance export: policy holder name when different from customer',
    ADD COLUMN kyc_check                  VARCHAR(16)  NULL COMMENT 'Compliance export: KYC check result (Pass / Fail / NA)',
    ADD COLUMN kyc_check_comments         TEXT         NULL COMMENT 'Compliance export: free-text notes for the KYC check',
    ADD COLUMN senior_mgmt_approval_date  DATETIME     NULL COMMENT 'Compliance export: senior management sign-off timestamp',
    ADD COLUMN sanctions_screening_date   DATETIME     NULL COMMENT 'Compliance export: timestamp of the most recent sanction screening run for this case';

-- Optional helper indexes if the new fields ever drive filters / sorts.
-- Commented out by default — uncomment if query patterns warrant them.
-- CREATE INDEX idx_customercase_policy_issue_date         ON customercase (policy_issue_date);
-- CREATE INDEX idx_customercase_senior_mgmt_approval_date ON customercase (senior_mgmt_approval_date);
-- CREATE INDEX idx_customercase_sanctions_screening_date  ON customercase (sanctions_screening_date);

-- Roll-back (uncomment to revert):
-- ALTER TABLE customercase
--     DROP COLUMN policy_issue_date,
--     DROP COLUMN date_of_response,
--     DROP COLUMN client_product_number,
--     DROP COLUMN policy_holder,
--     DROP COLUMN kyc_check,
--     DROP COLUMN kyc_check_comments,
--     DROP COLUMN senior_mgmt_approval_date,
--     DROP COLUMN sanctions_screening_date;
