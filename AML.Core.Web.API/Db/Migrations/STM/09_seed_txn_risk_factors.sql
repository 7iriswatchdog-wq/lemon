-- =====================================================================
-- Seed industry-standard transaction risk factors for Insurance and Real
-- Estate sectors. Derived from:
--   * FATF "Risk Based Approach Guidance for the Life Insurance Sector"
--   * FATF "Money Laundering & Terrorist Financing through the Real
--     Estate Sector"
--   * UAE CB AML/CFT and Sanctions Compliance Guidelines for FIs
--
-- Re-runnable: clears existing factors/bands before reseeding.
-- Bands use numeric_min/numeric_max for ranges, match_value for exact
-- string match, match_in_list for membership tests (comma-separated).
-- Rating per band feeds the rule "any High band => High overall"; the
-- engine still computes a weighted score for finer ranking.
-- =====================================================================

SET @INS = (SELECT id FROM stm_sector WHERE sector_code = 'INS');
SET @RE  = (SELECT id FROM stm_sector WHERE sector_code = 'RE');

DELETE FROM stm_txn_risk_band   WHERE factor_id IN (SELECT id FROM stm_txn_risk_factor WHERE is_active = 1);
DELETE FROM stm_txn_risk_factor WHERE is_active = 1;

-- ---------------------------------------------------------------------
-- INSURANCE FACTORS
-- ---------------------------------------------------------------------

-- 1. Premium Amount
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('INS-F01','Premium Amount','Higher single premium values carry higher ML risk',@INS,'Amount','NUMERIC',3,1);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, numeric_min, numeric_max, score, rating, sequence_no) VALUES
  (@f,'< 50,000',                NULL,    50000, 1,'Low',1),
  (@f,'50,000 – 200,000',       50000,  200000, 2,'Medium',2),
  (@f,'200,000 – 1,000,000',   200000, 1000000, 3,'High',3),
  (@f,'>= 1,000,000',         1000000,    NULL, 3,'High',4);

-- 2. Payment Mode
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('INS-F02','Payment Mode','Cash and crypto are highest risk; bank transfer lowest',@INS,'TranMode','ENUM',3,2);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, match_value, score, rating) VALUES
  (@f,'Bank Transfer / Cheque','Bank',   1,'Low'),
  (@f,'Cheque',                  'Cheque', 1,'Low'),
  (@f,'Card',                    'Card',   1,'Low'),
  (@f,'Wire',                    'Wire',   2,'Medium'),
  (@f,'Cash',                    'Cash',   3,'High'),
  (@f,'Crypto',                  'Crypto', 3,'High');

-- 3. Third-party premium payer
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('INS-F03','Third-Party Payer','Premium paid by a person other than the policy holder',@INS,'PayerIsThirdParty','FLAG',3,3);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, match_value, score, rating) VALUES
  (@f,'Policy holder pays','false', 1,'Low'),
  (@f,'Third party pays','true',    3,'High');

-- 4. Delivery channel
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('INS-F04','Delivery Channel','Non face-to-face and agent channels are higher risk',@INS,'DeliveryChannel','ENUM',2,4);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, match_value, score, rating) VALUES
  (@f,'Branch','Branch', 1,'Low'),
  (@f,'Online','Online', 2,'Medium'),
  (@f,'Broker','Broker', 2,'Medium'),
  (@f,'Agent','Agent',   2,'Medium');

-- 5. Customer / Beneficiary country
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('INS-F05','Beneficiary Country Risk','Beneficiary located in FATF high-risk jurisdiction',@INS,'BeneficiaryCountry','LIST',3,5);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, match_in_list, score, rating) VALUES
  (@f,'FATF high-risk countries','IR,KP,SY,CU,MM,AF,YE,SO,SD,IQ,LY,VE,NI,BY', 3,'High'),
  (@f,'Other countries','*ELSE*', 1,'Low');

-- 6. Customer high-risk flag
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('INS-F06','High-Risk Customer','Customer flagged as high-risk in KYC',@INS,'IsHighRiskCustomer','FLAG',3,6);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, match_value, score, rating) VALUES
  (@f,'No',  '0', 1,'Low'),
  (@f,'Yes', '1', 3,'High');

-- 7. Product type (Single premium / Endowment carry higher risk than Term)
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('INS-F07','Product Type','Single premium and investment-linked policies carry higher ML risk',@INS,'Product','ENUM',2,7);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, match_value, score, rating) VALUES
  (@f,'Term Insurance','Term',                 1,'Low'),
  (@f,'Health Insurance','Health',             1,'Low'),
  (@f,'Endowment','Endowment',                 2,'Medium'),
  (@f,'Unit Linked','UnitLinked',              2,'Medium'),
  (@f,'Single Premium','SinglePremium',        3,'High'),
  (@f,'Investment Plan','InvestmentPlan',      3,'High');

-- 8. Transaction Type (Surrender / Withdrawal early are high)
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('INS-F08','Transaction Type','Surrenders and partial withdrawals are higher risk than premiums',@INS,'TranType','ENUM',2,8);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, match_value, score, rating) VALUES
  (@f,'Premium',     'Premium',     1,'Low'),
  (@f,'Claim',       'Claim',       2,'Medium'),
  (@f,'Surrender',   'Surrender',   3,'High'),
  (@f,'Withdrawal',  'Withdrawal',  3,'High');

-- ---------------------------------------------------------------------
-- REAL ESTATE FACTORS
-- ---------------------------------------------------------------------

-- 1. Property Value (purchase amount)
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('RE-F01','Property Value','Higher property values carry higher ML risk',@RE,'Amount','NUMERIC',3,1);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, numeric_min, numeric_max, score, rating) VALUES
  (@f,'< 500,000',              NULL,    500000, 1,'Low'),
  (@f,'500,000 – 2,000,000',  500000,  2000000, 2,'Medium'),
  (@f,'2,000,000 – 10,000,000', 2000000, 10000000, 3,'High'),
  (@f,'>= 10,000,000',       10000000,     NULL, 3,'High');

-- 2. Payment Mode
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('RE-F02','Payment Mode','Cash purchases are the highest ML/TF risk in real estate',@RE,'TranMode','ENUM',3,2);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, match_value, score, rating) VALUES
  (@f,'Bank Transfer','Bank',   1,'Low'),
  (@f,'Cheque',       'Cheque', 1,'Low'),
  (@f,'Wire',         'Wire',   2,'Medium'),
  (@f,'Card',         'Card',   2,'Medium'),
  (@f,'Cash',         'Cash',   3,'High'),
  (@f,'Crypto',       'Crypto', 3,'High');

-- 3. Third-party payer
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('RE-F03','Third-Party Payer','Property funds remitted by a person other than the buyer',@RE,'PayerIsThirdParty','FLAG',3,3);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, match_value, score, rating) VALUES
  (@f,'Buyer pays',       'false', 1,'Low'),
  (@f,'Third party pays', 'true',  3,'High');

-- 4. Buyer nationality risk
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('RE-F04','Buyer Country Risk','Buyer from FATF high-risk jurisdiction',@RE,'RemitterCountry','LIST',3,4);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, match_in_list, score, rating) VALUES
  (@f,'FATF high-risk countries','IR,KP,SY,CU,MM,AF,YE,SO,SD,IQ,LY,VE,NI,BY', 3,'High'),
  (@f,'Other countries','*ELSE*', 1,'Low');

-- 5. Holding period (quick resale = high)
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('RE-F05','Holding Period','Quick resale within 90 days indicates potential layering',@RE,'HoldingDays','NUMERIC',3,5);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, numeric_min, numeric_max, score, rating) VALUES
  (@f,'< 90 days',     NULL,   90, 3,'High'),
  (@f,'90 – 365 days',   90,  365, 2,'Medium'),
  (@f,'> 365 days',     365, NULL, 1,'Low');

-- 6. Price deviation from market
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('RE-F06','Price Deviation From Market','Sale price significantly above/below market is a red flag',@RE,'PriceDeviationPct','NUMERIC',3,6);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, numeric_min, numeric_max, score, rating) VALUES
  (@f,'<= -30% (under-priced)', NULL,  -30, 3,'High'),
  (@f,'-30% to -10%',             -30,  -10, 2,'Medium'),
  (@f,'-10% to +10% (in range)',  -10,   10, 1,'Low'),
  (@f,'+10% to +30%',              10,   30, 2,'Medium'),
  (@f,'>= +30% (over-priced)',     30, NULL, 3,'High');

-- 7. Customer Type + PEP flag (corporate with PEP = high)
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('RE-F07','High-Risk Buyer','Buyer flagged high-risk (PEP, sanctions, etc.) in KYC',@RE,'IsHighRiskCustomer','FLAG',3,7);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, match_value, score, rating) VALUES
  (@f,'No',  '0', 1,'Low'),
  (@f,'Yes', '1', 3,'High');

-- 8. Delivery channel
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('RE-F08','Delivery / Intermediary','Broker/agent channels need extra diligence vs direct deals',@RE,'DeliveryChannel','ENUM',2,8);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, match_value, score, rating) VALUES
  (@f,'Direct (Branch)','Branch', 1,'Low'),
  (@f,'Broker',         'Broker', 2,'Medium'),
  (@f,'Agent',          'Agent',  2,'Medium'),
  (@f,'Online',         'Online', 2,'Medium');

-- 9. Multi-party / Joint purchase
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('RE-F09','Multi-Party Purchase','Multiple buyers / nominee structures need extra scrutiny',@RE,'IsMultiParty','FLAG',2,9);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, match_value, score, rating) VALUES
  (@f,'Single party','0', 1,'Low'),
  (@f,'Multi party', '1', 2,'Medium');

-- 10. Transaction type
INSERT INTO stm_txn_risk_factor (factor_code, factor_name, description, sector_id, field_name, factor_type, weight, sequence_no)
VALUES ('RE-F10','Transaction Type','Cash rental income carries different risk than sales/purchases',@RE,'TranType','ENUM',1,10);
SET @f := LAST_INSERT_ID();
INSERT INTO stm_txn_risk_band (factor_id, band_label, match_value, score, rating) VALUES
  (@f,'Purchase', 'Purchase', 2,'Medium'),
  (@f,'Sale',     'Sale',     2,'Medium'),
  (@f,'Rental',   'Rental',   1,'Low'),
  (@f,'Lease',    'Lease',    1,'Low');

SELECT
    s.sector_code,
    COUNT(DISTINCT f.id) AS factors,
    COUNT(b.id) AS bands
FROM stm_sector s
LEFT JOIN stm_txn_risk_factor f ON f.sector_id = s.id AND f.is_active = 1
LEFT JOIN stm_txn_risk_band   b ON b.factor_id = f.id
WHERE s.sector_code IN ('INS','RE')
GROUP BY s.sector_code;
