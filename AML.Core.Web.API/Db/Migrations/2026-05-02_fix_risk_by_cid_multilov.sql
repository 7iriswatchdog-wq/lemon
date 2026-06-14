-- ════════════════════════════════════════════════════════════════════════════════
-- 2026-05-02_fix_risk_by_cid_multilov.sql
--
-- Fixes the lov_type_id Ø-delimited multi-LOV crash that's been throwing
--    System.Data.DataException: Error parsing column 23 (lov_type_id=1572Ø104582Ø186Ø9Ø - String)
-- in RiskRepository.GetRiskDetailsOfIndividualByCID / GetRiskDetailsOfCorporateByCID.
--
-- ROOT CAUSE
--    The save flow at AML.Core.Repository/Risk/RiskRepository.cs ~line 108 builds:
--        riskItemlist += Id + "Ø" + SelectedItemId + "Ø"
--    and stores the whole concatenated blob in @p_risk_data via ins_transaction_risk_individual.
--    Some legacy `get_risk_individual_by_cid` / `get_tran_risk_corporate_by_cid` SPs
--    return that raw blob in the lov_type_id column instead of splitting it back into
--    one row per (typeId, valueId) pair. The DTO declares lov_type_id as `int`, so Dapper
--    crashes on the FormatException trying to parse "1572Ø104582Ø186Ø9Ø" as an integer.
--
-- WHAT THIS SCRIPT DOES
--    Section 1 — DIAGNOSTICS. Run these queries first. Tell Claude the column / table
--                names so the SP rewrite (Section 2) can be finalized. Do NOT skip this.
--    Section 2 — SP REWRITE. Drops & recreates the two SPs to emit clean (typeId, valueId)
--                pairs by splitting risk_data on the Ø separator using a recursive CTE.
--                MySQL 8.0+ required for the CTE.
--    Section 3 — ROLLBACK. Provided so you can revert if the rewrite breaks something.
--
-- BACKWARDS COMPATIBILITY
--    Existing rows are never touched. Only the two read SPs are replaced. The save SP
--    (ins_transaction_risk_individual) and all save flows continue to work unchanged.
-- ════════════════════════════════════════════════════════════════════════════════


-- ──────────────────────────────────────────────────────────────────────────────
-- SECTION 1 — DIAGNOSTICS (run these first)
-- ──────────────────────────────────────────────────────────────────────────────

-- 1a. Confirm the SPs exist and capture their current bodies for rollback.
SHOW CREATE PROCEDURE get_risk_individual_by_cid;
SHOW CREATE PROCEDURE get_tran_risk_corporate_by_cid;

-- 1b. Find the column that holds the concatenated risk_data blob.
--     Look for a TEXT/VARCHAR column on the risk-detail table.
SHOW COLUMNS FROM transaction_risk_individual;
-- Expected to include: id, customercode, risk_data (or similar), final_risk_score, version, etc.

-- 1c. Look at one real concatenated row (replace 'YOUR_CIF' with a customer code that
--     reproduces the bug, e.g. the case 43 customer's customercode).
SELECT id, customercode, version,
       LENGTH(risk_data)                                   AS blob_len,
       SUBSTRING(risk_data, 1, 200)                        AS blob_preview,
       CHAR_LENGTH(risk_data) - CHAR_LENGTH(REPLACE(risk_data, CHAR(216), '')) AS o_count
  FROM transaction_risk_individual
 WHERE customercode = 'YOUR_CIF'
 ORDER BY version DESC, id DESC
 LIMIT 5;
-- (CHAR(216) is the Ø character — Latin Capital Letter O With Stroke, code 0xD8.)

-- 1d. If risk_data lives elsewhere, find candidate columns:
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE
  FROM INFORMATION_SCHEMA.COLUMNS
 WHERE TABLE_SCHEMA = DATABASE()
   AND COLUMN_NAME IN ('risk_data','riskdata','lov_risk_data','data','risk_items')
 ORDER BY TABLE_NAME;

-- → After you have the values from 1a-1d, you have what you need to verify the
-- assumptions baked into Section 2 below. The two assumptions are:
--    (i)  the column is `transaction_risk_individual.risk_data` (Individual)
--         and `transaction_risk_corporate.risk_data` (Corporate)
--    (ii) the format is exactly: "<typeId>Ø<valueId>Ø<typeId>Ø<valueId>Ø..."
-- If either assumption is wrong, edit Section 2 to match before running it.


-- ──────────────────────────────────────────────────────────────────────────────
-- SECTION 2 — REWRITTEN STORED PROCEDURES (run after diagnostics)
-- ──────────────────────────────────────────────────────────────────────────────
-- Uncomment the block below once Section 1 has confirmed table & column names.

-- DROP PROCEDURE IF EXISTS get_risk_individual_by_cid;
-- DELIMITER $$
-- CREATE PROCEDURE get_risk_individual_by_cid(IN p_id VARCHAR(64))
-- BEGIN
--   -- Header row(s): the parent risk record's metadata. Adjust column list to match
--   -- whatever GetFirstOrDefault<RiskDTO> needs (FinalRiskScore, version, DateofAssessment, etc.).
--   SELECT
--       r.id,
--       r.customercode                                   AS customercode,
--       r.customername                                   AS customername,
--       r.dateofassessment                               AS dateofassessment,
--       r.created_date                                   AS created_date,
--       r.address                                        AS address,
--       r.Customer_nationality                           AS MainNationalityTxt,
--       r.final_risk_score                               AS FinalRiskScore,
--       r.risk_score_sum                                 AS RiskScoreSum,
--       r.risk_score_count                               AS RiskScoreCount,
--       r.risk_score_before_override                     AS RiskScoreBeforeOverride,
--       r.product_reference                              AS ProductReference,
--       r.product_value                                  AS ProductValue,
--       r.comments                                       AS RiskComments,
--       r.clientId                                       AS ClientId,
--       r.created_by                                     AS CreatedBy,
--       r.version                                        AS version,
--       r.remarks                                        AS Remarks,
--       r.riskoverride                                   AS RiskOverRide,
--       r.type                                           AS Type,
--       -- ReportData rows: split risk_data on Ø into (typeId, valueId) pairs and
--       -- join LovMaster for the human-readable text + score.
--       p.lov_type_category_id                           AS lov_type_category_id,
--       p.lov_type_id                                    AS lov_type_id,
--       lov.lov_master_data                              AS lov_risk_data,
--       lov.lov_master_score                             AS lov_risk_score,
--       0                                                AS Over_ride_Score
--   FROM transaction_risk_individual r
--   LEFT JOIN (
--       -- Recursive CTE: split risk_data ("typeIdØvalueIdØtypeIdØvalueIdØ...")
--       -- into one row per pair, indexed by their position in the original blob.
--       WITH RECURSIVE pairs AS (
--           SELECT
--               r2.id                                                                              AS risk_id,
--               1                                                                                  AS pair_idx,
--               SUBSTRING_INDEX(SUBSTRING_INDEX(r2.risk_data, CHAR(216), 1), CHAR(216), -1)       AS type_id_str,
--               SUBSTRING_INDEX(SUBSTRING_INDEX(r2.risk_data, CHAR(216), 2), CHAR(216), -1)       AS value_id_str,
--               r2.risk_data                                                                       AS remainder
--           FROM transaction_risk_individual r2
--           WHERE r2.customercode = p_id
--             AND r2.risk_data IS NOT NULL
--             AND r2.risk_data <> ''
--           UNION ALL
--           SELECT
--               pairs.risk_id,
--               pairs.pair_idx + 1,
--               SUBSTRING_INDEX(SUBSTRING_INDEX(pairs.remainder, CHAR(216), pairs.pair_idx * 2 + 1), CHAR(216), -1),
--               SUBSTRING_INDEX(SUBSTRING_INDEX(pairs.remainder, CHAR(216), pairs.pair_idx * 2 + 2), CHAR(216), -1),
--               pairs.remainder
--           FROM pairs
--           WHERE pairs.pair_idx * 2 < CHAR_LENGTH(pairs.remainder) - CHAR_LENGTH(REPLACE(pairs.remainder, CHAR(216), ''))
--       )
--       SELECT
--           pairs.risk_id,
--           CAST(pairs.type_id_str  AS UNSIGNED) AS lov_type_id,
--           CAST(pairs.value_id_str AS UNSIGNED) AS value_id,
--           ltm.lov_type_category_id             AS lov_type_category_id
--       FROM pairs
--       LEFT JOIN lov_type_master ltm ON ltm.lov_type_id = CAST(pairs.type_id_str AS UNSIGNED)
--       WHERE pairs.type_id_str REGEXP '^[0-9]+$'
--         AND pairs.value_id_str REGEXP '^[0-9]+$'
--   ) p ON p.risk_id = r.id
--   LEFT JOIN lov_master lov ON lov.lov_master_id = p.value_id
--   WHERE r.customercode = p_id
--   ORDER BY r.version DESC, r.id DESC, p.lov_type_id;
-- END$$
-- DELIMITER ;

-- Mirror SP for Corporate. Same logic, different table.
-- DROP PROCEDURE IF EXISTS get_tran_risk_corporate_by_cid;
-- DELIMITER $$
-- CREATE PROCEDURE get_tran_risk_corporate_by_cid(IN p_id VARCHAR(64))
-- BEGIN
--   SELECT
--       r.id,
--       r.customercode                                   AS UniqueID,
--       r.customername                                   AS LegalNameOfEntity,
--       r.dateofassessment                               AS DateofAssessment,
--       r.created_date                                   AS created_date,
--       r.Customer_nationality                           AS CountryOfIncorporationTxt,
--       r.final_risk_score                               AS RiskAssessmentRating,
--       r.risk_score_before_override                     AS RiskAssessmentRatingWithoutOverride,
--       r.risk_sum                                       AS RiskScoreSum,
--       r.risk_count                                     AS RiskScoreCount,
--       r.product_reference                              AS ProductReference,
--       r.product_value                                  AS ProductValue,
--       r.comments                                       AS RiskComments,
--       r.clientId                                       AS ClientId,
--       r.created_by                                     AS CreatedBy,
--       r.version                                        AS version,
--       r.remarks                                        AS Remarks,
--       r.riskoverride                                   AS RiskOverRide,
--       r.type                                           AS Type,
--       p.lov_type_category_id                           AS lov_type_category_id,
--       p.lov_type_id                                    AS lov_type_id,
--       lov.lov_master_data                              AS lov_risk_data,
--       lov.lov_master_score                             AS lov_risk_score,
--       0                                                AS Over_ride_Score
--   FROM transaction_risk_corporate r
--   LEFT JOIN (
--       WITH RECURSIVE pairs AS (
--           SELECT
--               r2.id                                                                              AS risk_id,
--               1                                                                                  AS pair_idx,
--               SUBSTRING_INDEX(SUBSTRING_INDEX(r2.risk_data, CHAR(216), 1), CHAR(216), -1)       AS type_id_str,
--               SUBSTRING_INDEX(SUBSTRING_INDEX(r2.risk_data, CHAR(216), 2), CHAR(216), -1)       AS value_id_str,
--               r2.risk_data                                                                       AS remainder
--           FROM transaction_risk_corporate r2
--           WHERE r2.customercode = p_id
--             AND r2.risk_data IS NOT NULL
--             AND r2.risk_data <> ''
--           UNION ALL
--           SELECT
--               pairs.risk_id,
--               pairs.pair_idx + 1,
--               SUBSTRING_INDEX(SUBSTRING_INDEX(pairs.remainder, CHAR(216), pairs.pair_idx * 2 + 1), CHAR(216), -1),
--               SUBSTRING_INDEX(SUBSTRING_INDEX(pairs.remainder, CHAR(216), pairs.pair_idx * 2 + 2), CHAR(216), -1),
--               pairs.remainder
--           FROM pairs
--           WHERE pairs.pair_idx * 2 < CHAR_LENGTH(pairs.remainder) - CHAR_LENGTH(REPLACE(pairs.remainder, CHAR(216), ''))
--       )
--       SELECT
--           pairs.risk_id,
--           CAST(pairs.type_id_str  AS UNSIGNED) AS lov_type_id,
--           CAST(pairs.value_id_str AS UNSIGNED) AS value_id,
--           ltm.lov_type_category_id             AS lov_type_category_id
--       FROM pairs
--       LEFT JOIN lov_type_master ltm ON ltm.lov_type_id = CAST(pairs.type_id_str AS UNSIGNED)
--       WHERE pairs.type_id_str REGEXP '^[0-9]+$'
--         AND pairs.value_id_str REGEXP '^[0-9]+$'
--   ) p ON p.risk_id = r.id
--   LEFT JOIN lov_master lov ON lov.lov_master_id = p.value_id
--   WHERE r.customercode = p_id
--   ORDER BY r.version DESC, r.id DESC, p.lov_type_id;
-- END$$
-- DELIMITER ;


-- ──────────────────────────────────────────────────────────────────────────────
-- SECTION 3 — ROLLBACK
-- ──────────────────────────────────────────────────────────────────────────────
-- Before running Section 2, save the output of `SHOW CREATE PROCEDURE …` from
-- Section 1a so you can recreate the originals here. To roll back:
--    DROP PROCEDURE IF EXISTS get_risk_individual_by_cid;
--    -- then paste the original CREATE PROCEDURE body
--    DROP PROCEDURE IF EXISTS get_tran_risk_corporate_by_cid;
--    -- then paste the original CREATE PROCEDURE body


-- ──────────────────────────────────────────────────────────────────────────────
-- VERIFICATION (run after Section 2)
-- ──────────────────────────────────────────────────────────────────────────────
-- CALL get_risk_individual_by_cid('YOUR_CIF');
-- Expected: one parent row repeated once per (typeId, valueId) pair, with lov_type_id
-- as a single integer per row (no Ø characters anywhere). The .NET app's
-- ReportDataDTO.lov_type_id (int) deserialization will succeed.
