-- =====================================================================
-- STM - Fix INS-R007 "Premium then Withdrawal" semantics
--
-- The original conditions were:
--   TranType         equals Premium
--   SubsequentTranType equals Withdrawal (30 days)
-- A single transaction can never be both a Premium and a Withdrawal, and
-- the engine had no concept of a "subsequent" transaction, so this rule
-- could never hit correctly.
--
-- Reworked to trigger on the WITHDRAWAL and look back for a Premium in the
-- preceding 30 days (PriorTranType is evaluated against the customer's
-- transaction history by the engine). This is the realistic real-time
-- trigger point.
--
-- Applies to all clients that have the rule (system rule is client_id = 0).
-- =====================================================================

DELETE rc FROM stm_rule_condition rc
    INNER JOIN stm_rule r ON r.id = rc.rule_id
    WHERE r.rule_code = 'INS-R007';

INSERT INTO stm_rule_condition
    (rule_id, sequence_no, field_name, operator, compare_value, timeframe_value, timeframe_unit, conjunction, description)
SELECT r.id, 1, 'TranType', 'equals', 'Withdrawal', NULL, NULL, 'AND', 'Withdrawal transaction'
    FROM stm_rule r WHERE r.rule_code = 'INS-R007'
UNION ALL
SELECT r.id, 2, 'PriorTranType', 'equals', 'Premium', 30, 'Day', 'AND', 'Preceded by a premium within 30 days'
    FROM stm_rule r WHERE r.rule_code = 'INS-R007';
