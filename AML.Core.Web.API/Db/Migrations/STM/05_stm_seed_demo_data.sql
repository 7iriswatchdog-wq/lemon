-- =====================================================================
-- STM - Demo transactions + cases for the National Compliance client (id=1)
-- Mix of Insurance and Real Estate transactions; some hit rules and get cases.
-- Idempotent: deletes existing demo rows first (identified by `DEMO-` prefix).
-- =====================================================================

-- Clean up previous demo rows so this script can be re-run safely
DELETE FROM stm_case_comment WHERE case_id IN (SELECT id FROM stm_case WHERE case_ref_no LIKE 'DEMO-%');
DELETE FROM stm_case WHERE case_ref_no LIKE 'DEMO-%';
DELETE FROM stm_rule_exec_log WHERE transaction_id IN (SELECT id FROM stm_transaction WHERE tran_ref_no LIKE 'DEMO-%');
DELETE FROM stm_transaction_party WHERE transaction_id IN (SELECT id FROM stm_transaction WHERE tran_ref_no LIKE 'DEMO-%');
DELETE FROM stm_transaction WHERE tran_ref_no LIKE 'DEMO-%';

SET @INS = (SELECT id FROM stm_sector WHERE sector_code = 'INS');
SET @RE  = (SELECT id FROM stm_sector WHERE sector_code = 'RE');
SET @CLIENT = 1;
SET @USR = 1;

-- ---------------------------------------------------------------------
-- TRANSACTION 1 - Insurance: high-value single premium (hits INS-R001)
-- ---------------------------------------------------------------------
INSERT INTO stm_transaction
  (tran_ref_no, sector_id, tran_date, tran_type, tran_mode, delivery_channel, product,
   amount, currency, customer_id, customer_name, customer_type,
   remitter_name, remitter_country, beneficiary_name, beneficiary_country,
   policy_no, purpose, branch_code, is_high_risk_country, is_high_risk_customer,
   is_multi_party, rule_hit_status, client_id, created_by)
VALUES
  ('DEMO-INS-001', @INS, DATE_SUB(NOW(), INTERVAL 5 DAY), 'Premium', 'Bank', 'Branch', 'LifePolicy-PlanA',
   250000, 'AED', 'NAT57', 'Narendra Modi', 'I',
   'Narendra Modi', 'AE', 'Acme Insurance Ltd', 'AE',
   'POL-2026-0001', 'Single premium policy', 'BR-DXB-01', 0, 0,
   0, 'HIT', @CLIENT, @USR);
SET @T1 = LAST_INSERT_ID();

-- ---------------------------------------------------------------------
-- TRANSACTION 2 - Real Estate: cash purchase above threshold (hits RE-R001)
-- ---------------------------------------------------------------------
INSERT INTO stm_transaction
  (tran_ref_no, sector_id, tran_date, tran_type, tran_mode, delivery_channel,
   amount, currency, customer_id, customer_name, customer_type,
   remitter_name, remitter_country, beneficiary_name, beneficiary_country,
   property_ref, property_value, purpose, branch_code,
   is_high_risk_country, is_high_risk_customer, is_multi_party,
   rule_hit_status, client_id, created_by)
VALUES
  ('DEMO-RE-001', @RE, DATE_SUB(NOW(), INTERVAL 3 DAY), 'Purchase', 'Cash', 'Branch',
   850000, 'AED', 'NAT57', 'Narendra Modi', 'I',
   'Narendra Modi', 'AE', 'Sunrise Developers LLC', 'AE',
   'DXB-MARINA-1204', 850000, 'Property purchase - Marina', 'BR-DXB-01',
   0, 0, 0, 'HIT', @CLIENT, @USR);
SET @T2 = LAST_INSERT_ID();

-- ---------------------------------------------------------------------
-- TRANSACTION 3 - Insurance: cash premium above threshold (hits INS-R003)
-- ---------------------------------------------------------------------
INSERT INTO stm_transaction
  (tran_ref_no, sector_id, tran_date, tran_type, tran_mode, delivery_channel, product,
   amount, currency, customer_id, customer_name, customer_type,
   remitter_name, remitter_country, beneficiary_name, beneficiary_country,
   policy_no, purpose, branch_code, is_high_risk_country, is_high_risk_customer,
   is_multi_party, rule_hit_status, client_id, created_by)
VALUES
  ('DEMO-INS-002', @INS, DATE_SUB(NOW(), INTERVAL 2 DAY), 'Premium', 'Cash', 'Agent', 'GeneralPolicy',
   75000, 'AED', 'NAT58', 'Aisha Al-Mansouri', 'I',
   'Aisha Al-Mansouri', 'AE', 'Gulf Insurance', 'AE',
   'POL-2026-0002', 'Cash premium', 'BR-AUH-02', 0, 0,
   0, 'HIT', @CLIENT, @USR);
SET @T3 = LAST_INSERT_ID();

-- ---------------------------------------------------------------------
-- TRANSACTION 4 - Real Estate: high-risk country buyer (hits RE-R005)
-- ---------------------------------------------------------------------
INSERT INTO stm_transaction
  (tran_ref_no, sector_id, tran_date, tran_type, tran_mode, delivery_channel,
   amount, currency, customer_id, customer_name, customer_type,
   remitter_name, remitter_country, beneficiary_name, beneficiary_country,
   property_ref, property_value, purpose, branch_code,
   is_high_risk_country, is_high_risk_customer, is_multi_party,
   rule_hit_status, client_id, created_by)
VALUES
  ('DEMO-RE-002', @RE, DATE_SUB(NOW(), INTERVAL 1 DAY), 'Purchase', 'Bank', 'Broker',
   1200000, 'AED', 'NAT59', 'Hassan Ahmadi', 'I',
   'Hassan Ahmadi', 'IR', 'Emaar Properties', 'AE',
   'DXB-DOWNTOWN-2401', 1200000, 'Downtown apartment', 'BR-DXB-01',
   1, 1, 0, 'HIT', @CLIENT, @USR);
SET @T4 = LAST_INSERT_ID();

-- ---------------------------------------------------------------------
-- TRANSACTION 5 - Insurance: clean (no rule hit) - regular bank premium
-- ---------------------------------------------------------------------
INSERT INTO stm_transaction
  (tran_ref_no, sector_id, tran_date, tran_type, tran_mode, delivery_channel, product,
   amount, currency, customer_id, customer_name, customer_type,
   remitter_country, beneficiary_country, policy_no,
   is_high_risk_country, is_high_risk_customer, is_multi_party,
   rule_hit_status, client_id, created_by)
VALUES
  ('DEMO-INS-003', @INS, DATE_SUB(NOW(), INTERVAL 6 DAY), 'Premium', 'Bank', 'Online', 'HealthPolicy',
   8000, 'AED', 'NAT57', 'Narendra Modi', 'I',
   'AE', 'AE', 'POL-2026-0003',
   0, 0, 0, 'NO_HIT', @CLIENT, @USR);

-- ---------------------------------------------------------------------
-- TRANSACTION 6 - Real Estate multi-party (joint purchase, no rule hit)
-- ---------------------------------------------------------------------
INSERT INTO stm_transaction
  (tran_ref_no, sector_id, tran_date, tran_type, tran_mode, delivery_channel,
   amount, currency, customer_id, customer_name, customer_type,
   remitter_country, beneficiary_country, property_ref, property_value,
   is_high_risk_country, is_high_risk_customer, is_multi_party,
   rule_hit_status, client_id, created_by)
VALUES
  ('DEMO-RE-003', @RE, DATE_SUB(NOW(), INTERVAL 10 DAY), 'Purchase', 'Bank', 'Branch',
   2200000, 'AED', 'NAT60', 'Mohammed & Fatima Al-Hashimi', 'I',
   'AE', 'AE', 'DXB-JBR-501', 2200000,
   0, 0, 1, 'NO_HIT', @CLIENT, @USR);
SET @T6 = LAST_INSERT_ID();

-- Multi-party rows for T6
INSERT INTO stm_transaction_party (transaction_id, party_role, customer_id, customer_name, nationality, customer_type, share_percentage)
VALUES
  (@T6, 'Buyer',    'NAT60', 'Mohammed Al-Hashimi', 'AE', 'I', 50.00),
  (@T6, 'Buyer',    'NAT61', 'Fatima Al-Hashimi',   'AE', 'I', 50.00);

-- ---------------------------------------------------------------------
-- CASES - corresponding to the 4 HIT transactions
-- ---------------------------------------------------------------------
SET @R001 = (SELECT id FROM stm_rule WHERE rule_code = 'INS-R001');
SET @R003 = (SELECT id FROM stm_rule WHERE rule_code = 'INS-R003');
SET @R_RE001 = (SELECT id FROM stm_rule WHERE rule_code = 'RE-R001');
SET @R_RE005 = (SELECT id FROM stm_rule WHERE rule_code = 'RE-R005');

-- Case 1 - OPEN
INSERT INTO stm_case (case_ref_no, transaction_id, sector_id, rules_violated, rule_names,
                      total_risk_score, risk_rating, status, client_id, created_by)
VALUES
  ('DEMO-CASE-001', @T1, @INS, JSON_ARRAY(@R001), 'High-Value Single Premium',
   80, 'High', 'OPEN', @CLIENT, @USR);
SET @C1 = LAST_INSERT_ID();
INSERT INTO stm_case_comment (case_id, comment_text, action_type, created_by, created_user)
VALUES (@C1, 'Case auto-created from rule hit. Rules: INS-R001', 'AUTO_CREATED', @USR, 'System');

-- Case 2 - OPEN
INSERT INTO stm_case (case_ref_no, transaction_id, sector_id, rules_violated, rule_names,
                      total_risk_score, risk_rating, status, client_id, created_by)
VALUES
  ('DEMO-CASE-002', @T2, @RE, JSON_ARRAY(@R_RE001), 'High-Value Cash Property Purchase',
   90, 'High', 'OPEN', @CLIENT, @USR);
SET @C2 = LAST_INSERT_ID();
INSERT INTO stm_case_comment (case_id, comment_text, action_type, created_by, created_user)
VALUES (@C2, 'Case auto-created from rule hit. Rules: RE-R001', 'AUTO_CREATED', @USR, 'System');

-- Case 3 - APPROVED (reviewed - false positive)
INSERT INTO stm_case (case_ref_no, transaction_id, sector_id, rules_violated, rule_names,
                      total_risk_score, risk_rating, status, reviewed_by, reviewed_on,
                      review_decision, review_remarks, client_id, created_by)
VALUES
  ('DEMO-CASE-003', @T3, @INS, JSON_ARRAY(@R003), 'Cash Premium Payment Above Threshold',
   75, 'High', 'APPROVED', @USR, DATE_SUB(NOW(), INTERVAL 1 DAY),
   'FALSE_POSITIVE', 'Customer is a long-time client with verified income source. Cash deposit consistent with documented salary. Cleared.',
   @CLIENT, @USR);
SET @C3 = LAST_INSERT_ID();
INSERT INTO stm_case_comment (case_id, comment_text, action_type, created_by, created_user)
VALUES (@C3, 'Case auto-created from rule hit. Rules: INS-R003', 'AUTO_CREATED', @USR, 'System');
INSERT INTO stm_case_comment (case_id, comment_text, action_type, created_by, created_user)
VALUES (@C3, 'Case reviewed. Decision: FALSE_POSITIVE. Status: APPROVED. Remarks: Long-time client, verified income.', 'DECISION', @USR, 'compliance.officer');

-- Case 4 - REJECTED (reviewed - true positive, blocked)
INSERT INTO stm_case (case_ref_no, transaction_id, sector_id, rules_violated, rule_names,
                      total_risk_score, risk_rating, status, reviewed_by, reviewed_on,
                      review_decision, review_remarks, client_id, created_by)
VALUES
  ('DEMO-CASE-004', @T4, @RE, JSON_ARRAY(@R_RE005), 'Foreign Buyer High-Risk Country',
   85, 'High', 'REJECTED', @USR, DATE_SUB(NOW(), INTERVAL 12 HOUR),
   'TRUE_POSITIVE', 'Buyer is from a sanctioned country. Source of funds not verifiable. STR filed with FIU. Transaction blocked.',
   @CLIENT, @USR);
SET @C4 = LAST_INSERT_ID();
INSERT INTO stm_case_comment (case_id, comment_text, action_type, created_by, created_user)
VALUES (@C4, 'Case auto-created from rule hit. Rules: RE-R005', 'AUTO_CREATED', @USR, 'System');
INSERT INTO stm_case_comment (case_id, comment_text, action_type, created_by, created_user)
VALUES (@C4, 'Case reviewed. Decision: TRUE_POSITIVE. Status: REJECTED. Remarks: STR filed.', 'DECISION', @USR, 'compliance.officer');

-- Quick verification
SELECT 'Demo data inserted' AS info;
SELECT COUNT(*) AS demo_transactions FROM stm_transaction WHERE tran_ref_no LIKE 'DEMO-%';
SELECT COUNT(*) AS demo_cases FROM stm_case WHERE case_ref_no LIKE 'DEMO-%';
