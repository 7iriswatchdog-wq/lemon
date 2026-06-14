-- =====================================================================
-- STM - Industry-standard rules for Insurance and Real Estate sectors
-- Based on FATF/UAE CB AML guidance and the RuleSchemas xlsx
-- =====================================================================

-- Helpers (variables)
SET @INS = (SELECT id FROM stm_sector WHERE sector_code = 'INS');
SET @RE  = (SELECT id FROM stm_sector WHERE sector_code = 'RE');

-- =====================================================================
-- INSURANCE RULES
-- =====================================================================

-- INS-R001: High-Value Single Premium Payment
INSERT INTO stm_rule (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                      logical_operator, action_on_hit, is_active, is_system_rule, client_id)
VALUES ('INS-R001','High-Value Single Premium',
        'Single premium payment greater than the threshold amount',
        @INS,'High',80,'AND','CREATE_CASE',1,1,0);
SET @r := LAST_INSERT_ID();
INSERT INTO stm_rule_condition (rule_id, sequence_no, field_name, operator, compare_value, conjunction, description) VALUES
  (@r,1,'TranType','equals','Premium','AND','Transaction is a premium payment'),
  (@r,2,'Amount','gte','100000','AND','Amount >= 100,000');

-- INS-R002: Early Surrender (Premium paid then surrendered within short period)
INSERT INTO stm_rule (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                      logical_operator, action_on_hit, is_active, is_system_rule, client_id)
VALUES ('INS-R002','Early Policy Surrender',
        'Policy surrendered within 12 months of inception (high indicator of money laundering)',
        @INS,'High',90,'AND','CREATE_CASE',1,1,0);
SET @r := LAST_INSERT_ID();
INSERT INTO stm_rule_condition (rule_id, sequence_no, field_name, operator, compare_value, timeframe_value, timeframe_unit, conjunction, description) VALUES
  (@r,1,'TranType','equals','Surrender',NULL,NULL,'AND','Policy surrendered'),
  (@r,2,'PolicyAgeMonths','lte','12',NULL,NULL,'AND','Policy age <= 12 months');

-- INS-R003: Cash Premium Payment
INSERT INTO stm_rule (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                      logical_operator, action_on_hit, is_active, is_system_rule, client_id)
VALUES ('INS-R003','Cash Premium Payment Above Threshold',
        'Premium paid in cash above the regulator-defined cash limit',
        @INS,'High',75,'AND','CREATE_CASE',1,1,0);
SET @r := LAST_INSERT_ID();
INSERT INTO stm_rule_condition (rule_id, sequence_no, field_name, operator, compare_value, conjunction, description) VALUES
  (@r,1,'TranMode','equals','Cash','AND','Payment in cash'),
  (@r,2,'Amount','gte','55000','AND','Amount >= 55,000 (AED)');

-- INS-R004: Third-Party Premium Payment
INSERT INTO stm_rule (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                      logical_operator, action_on_hit, is_active, is_system_rule, client_id)
VALUES ('INS-R004','Third-Party Premium Payment',
        'Premium paid by a person other than the policy holder (potential ML)',
        @INS,'High',70,'AND','CREATE_CASE',1,1,0);
SET @r := LAST_INSERT_ID();
INSERT INTO stm_rule_condition (rule_id, sequence_no, field_name, operator, compare_field, conjunction, description) VALUES
  (@r,1,'RemitterId','not_equals_field','CustomerId','AND','Payer is not the policy holder');

-- INS-R005: Frequent Policy Top-ups
INSERT INTO stm_rule (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                      logical_operator, action_on_hit, is_active, is_system_rule, client_id)
VALUES ('INS-R005','Frequent Top-ups in 90 Days',
        'Customer making 3 or more premium top-ups in 90 days',
        @INS,'Medium',60,'AND','CREATE_CASE',1,1,0);
SET @r := LAST_INSERT_ID();
INSERT INTO stm_rule_condition (rule_id, sequence_no, field_name, field_aggregation, operator, compare_value, timeframe_value, timeframe_unit, conjunction, description) VALUES
  (@r,1,'TranType','Count','gte','3',90,'Day','AND','>= 3 top-up transactions in 90 days');

-- INS-R006: High Risk Country Counter-party
INSERT INTO stm_rule (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                      logical_operator, action_on_hit, is_active, is_system_rule, client_id)
VALUES ('INS-R006','High-Risk Country Beneficiary',
        'Claim/payout to a beneficiary in a high-risk country',
        @INS,'High',85,'AND','CREATE_CASE',1,1,0);
SET @r := LAST_INSERT_ID();
INSERT INTO stm_rule_condition (rule_id, sequence_no, field_name, operator, compare_value, conjunction, description) VALUES
  (@r,1,'BeneficiaryCountry','in_high_risk_list',NULL,'AND','Beneficiary country in high-risk list'),
  (@r,2,'TranType','equals','Claim','AND','Transaction is a claim payout');

-- INS-R007: Single Premium followed by partial withdrawal
INSERT INTO stm_rule (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                      logical_operator, action_on_hit, is_active, is_system_rule, client_id)
VALUES ('INS-R007','Premium then Withdrawal Pattern',
        'Withdrawal preceded by a single premium within the prior 30 days',
        @INS,'High',85,'AND','CREATE_CASE',1,1,0);
SET @r := LAST_INSERT_ID();
INSERT INTO stm_rule_condition (rule_id, sequence_no, field_name, operator, compare_value, timeframe_value, timeframe_unit, conjunction, description) VALUES
  (@r,1,'TranType','equals','Withdrawal',NULL,NULL,'AND','Withdrawal transaction'),
  (@r,2,'PriorTranType','equals','Premium',30,'Day','AND','Preceded by a premium within 30 days');

-- INS-R008: Nominee in different nationality
INSERT INTO stm_rule (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                      logical_operator, action_on_hit, is_active, is_system_rule, client_id)
VALUES ('INS-R008','Nominee Different Nationality',
        'Policy nominee has a different nationality than the policy holder',
        @INS,'Medium',55,'AND','CREATE_CASE',1,1,0);
SET @r := LAST_INSERT_ID();
INSERT INTO stm_rule_condition (rule_id, sequence_no, field_name, operator, compare_field, conjunction, description) VALUES
  (@r,1,'NomineeNationality','not_equals_field','CustomerNationality','AND','Nominee nationality != Customer nationality');

-- =====================================================================
-- REAL ESTATE RULES
-- =====================================================================

-- RE-R001: High-Value Cash Property Purchase
INSERT INTO stm_rule (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                      logical_operator, action_on_hit, is_active, is_system_rule, client_id)
VALUES ('RE-R001','High-Value Cash Property Purchase',
        'Property purchase paid in cash above the threshold',
        @RE,'High',90,'AND','CREATE_CASE',1,1,0);
SET @r := LAST_INSERT_ID();
INSERT INTO stm_rule_condition (rule_id, sequence_no, field_name, operator, compare_value, conjunction, description) VALUES
  (@r,1,'TranType','equals','Purchase','AND','Purchase transaction'),
  (@r,2,'TranMode','equals','Cash','AND','Paid in cash'),
  (@r,3,'Amount','gte','55000','AND','Amount >= 55,000 (AED) regulatory threshold');

-- RE-R002: Quick Resale (Property flipping)
INSERT INTO stm_rule (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                      logical_operator, action_on_hit, is_active, is_system_rule, client_id)
VALUES ('RE-R002','Quick Resale within 90 Days',
        'Property purchased and resold within 90 days - potential layering',
        @RE,'High',85,'AND','CREATE_CASE',1,1,0);
SET @r := LAST_INSERT_ID();
INSERT INTO stm_rule_condition (rule_id, sequence_no, field_name, operator, compare_value, timeframe_value, timeframe_unit, conjunction, description) VALUES
  (@r,1,'TranType','equals','Sale',90,'Day','AND','Sale transaction'),
  (@r,2,'PreviousOwnershipDays','lte','90',NULL,NULL,'AND','Owned <= 90 days');

-- RE-R003: Sale Price Significantly Below/Above Market
INSERT INTO stm_rule (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                      logical_operator, action_on_hit, is_active, is_system_rule, client_id)
VALUES ('RE-R003','Price Deviation From Market Value',
        'Sale price differs more than 30% from market value',
        @RE,'High',80,'OR','CREATE_CASE',1,1,0);
SET @r := LAST_INSERT_ID();
INSERT INTO stm_rule_condition (rule_id, sequence_no, field_name, operator, compare_value, conjunction, description) VALUES
  (@r,1,'PriceDeviationPct','gte','30','OR','Sale price >= 30% above market'),
  (@r,2,'PriceDeviationPct','lte','-30','OR','Sale price <= 30% below market');

-- RE-R004: Multiple Properties Purchased in Short Window
INSERT INTO stm_rule (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                      logical_operator, action_on_hit, is_active, is_system_rule, client_id)
VALUES ('RE-R004','Multiple Property Purchases',
        'Same buyer making 3 or more property purchases within 60 days',
        @RE,'High',75,'AND','CREATE_CASE',1,1,0);
SET @r := LAST_INSERT_ID();
INSERT INTO stm_rule_condition (rule_id, sequence_no, field_name, field_aggregation, operator, compare_value, timeframe_value, timeframe_unit, conjunction, description) VALUES
  (@r,1,'TranType','Count','gte','3',60,'Day','AND','>= 3 purchase transactions in 60 days');

-- RE-R005: Buyer from High-Risk Country
INSERT INTO stm_rule (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                      logical_operator, action_on_hit, is_active, is_system_rule, client_id)
VALUES ('RE-R005','Foreign Buyer High-Risk Country',
        'Property purchased by buyer from a high-risk country',
        @RE,'High',85,'AND','CREATE_CASE',1,1,0);
SET @r := LAST_INSERT_ID();
INSERT INTO stm_rule_condition (rule_id, sequence_no, field_name, operator, compare_value, conjunction, description) VALUES
  (@r,1,'CustomerNationality','in_high_risk_list',NULL,'AND','Buyer nationality high-risk'),
  (@r,2,'TranType','equals','Purchase','AND','Purchase transaction');

-- RE-R006: Third-Party Funded Property Purchase
INSERT INTO stm_rule (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                      logical_operator, action_on_hit, is_active, is_system_rule, client_id)
VALUES ('RE-R006','Third-Party Payment for Property',
        'Property purchase funds remitted by a third party (not the buyer)',
        @RE,'High',80,'AND','CREATE_CASE',1,1,0);
SET @r := LAST_INSERT_ID();
INSERT INTO stm_rule_condition (rule_id, sequence_no, field_name, operator, compare_field, conjunction, description) VALUES
  (@r,1,'RemitterId','not_equals_field','CustomerId','AND','Remitter != Buyer');

-- RE-R007: Corporate Buyer with PEP Director
INSERT INTO stm_rule (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                      logical_operator, action_on_hit, is_active, is_system_rule, client_id)
VALUES ('RE-R007','Corporate Buyer with PEP',
        'Corporate buyer with politically exposed person as director/shareholder',
        @RE,'High',90,'AND','CREATE_CASE',1,1,0);
SET @r := LAST_INSERT_ID();
INSERT INTO stm_rule_condition (rule_id, sequence_no, field_name, operator, compare_value, conjunction, description) VALUES
  (@r,1,'CustomerType','equals','C','AND','Corporate'),
  (@r,2,'HasPEP','equals','true','AND','Has PEP related party'),
  (@r,3,'TranType','equals','Purchase','AND','Purchase');

-- RE-R008: Rental Yielding Cash Above Threshold
INSERT INTO stm_rule (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                      logical_operator, action_on_hit, is_active, is_system_rule, client_id)
VALUES ('RE-R008','Cash Rental Income High',
        'Rental income paid in cash exceeding threshold per month',
        @RE,'Medium',60,'AND','CREATE_CASE',1,1,0);
SET @r := LAST_INSERT_ID();
INSERT INTO stm_rule_condition (rule_id, sequence_no, field_name, operator, compare_value, conjunction, description) VALUES
  (@r,1,'TranType','equals','Rental','AND','Rental transaction'),
  (@r,2,'TranMode','equals','Cash','AND','Cash payment'),
  (@r,3,'Amount','gte','50000','AND','Amount >= 50,000 monthly');
