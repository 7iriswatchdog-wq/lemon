-- ===================================================================
-- Migration: 14_military_goods_risk
-- Seeds LOV tables for Military Goods Match and If More Military Goods.
-- Recreates get_risk_lov_id_kyc and get_risk_type_id_kyc to return 40 parameters.
-- Redefines get_proliferation_status and creates get_military_goods_status.
-- ===================================================================

-- 1. Seed lovtypecategory
INSERT INTO lovtypecategory (id, lov_type_category, lov_risk_category_code, lov_risk_category, active_yn, Client_Id)
SELECT 1149, 'Military Goods', 'I', 'Individual', 1, 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM lovtypecategory WHERE id = 1149);

INSERT INTO lovtypecategory (id, lov_type_category, lov_risk_category_code, lov_risk_category, active_yn, Client_Id)
SELECT 1150, 'Military Goods', 'C', NULL, 1, 1 FROM DUAL WHERE NOT EXISTS (SELECT 1 FROM lovtypecategory WHERE id = 1150);

-- 2. Seed lovtypemaster_backup
INSERT INTO lovtypemaster_backup (id, lov_risk_category, lov_risk_category_code, lov_type_id, lov_type_category_id, lov_type_name, active_yn, date_created, Client_Id, lov_country_duplicate)
SELECT 2544, 'Individual', 'I', 2544, '1149', 'Military Goods Match', 1, NOW(), 1, 0 FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM lovtypemaster_backup WHERE id = 2544);

INSERT INTO lovtypemaster_backup (id, lov_risk_category, lov_risk_category_code, lov_type_id, lov_type_category_id, lov_type_name, active_yn, date_created, Client_Id, lov_country_duplicate)
SELECT 2545, 'Corporate', 'C', 2545, '1150', 'Military Goods Match', 1, NOW(), 1, 0 FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM lovtypemaster_backup WHERE id = 2545);

INSERT INTO lovtypemaster_backup (id, lov_risk_category, lov_risk_category_code, lov_type_id, lov_type_category_id, lov_type_name, active_yn, date_created, Client_Id, lov_country_duplicate)
SELECT 2546, 'Individual', 'I', 2546, '1149', 'If More Military Goods', 1, NOW(), 1, 0 FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM lovtypemaster_backup WHERE id = 2546);

INSERT INTO lovtypemaster_backup (id, lov_risk_category, lov_risk_category_code, lov_type_id, lov_type_category_id, lov_type_name, active_yn, date_created, Client_Id, lov_country_duplicate)
SELECT 2547, 'Corporate', 'C', 2547, '1150', 'If More Military Goods', 1, NOW(), 1, 0 FROM DUAL
WHERE NOT EXISTS (SELECT 1 FROM lovtypemaster_backup WHERE id = 2547);

-- 3. Seed lovmaster for all client IDs
INSERT INTO lovmaster (lov_risk_catogory, lov_risk_catogory_code, lov_type_id, lov_type_name, lov_risk_data, lov_risk_score, Over_ride_Score, active_yn, date_created, date_updated, Client_Id, Created_By)
SELECT 'Individual', 'I', 2544, 'Military Goods Match', 'Yes', 3, 3, 1, NOW(), NOW(), Client_Id, NULL FROM client_master cm
WHERE NOT EXISTS (SELECT 1 FROM lovmaster lm WHERE lm.lov_type_id = 2544 AND lm.lov_risk_data = 'Yes' AND lm.Client_Id = cm.Client_Id);

INSERT INTO lovmaster (lov_risk_catogory, lov_risk_catogory_code, lov_type_id, lov_type_name, lov_risk_data, lov_risk_score, Over_ride_Score, active_yn, date_created, date_updated, Client_Id, Created_By)
SELECT 'Individual', 'I', 2544, 'Military Goods Match', 'No', 1, 0, 1, NOW(), NOW(), Client_Id, NULL FROM client_master cm
WHERE NOT EXISTS (SELECT 1 FROM lovmaster lm WHERE lm.lov_type_id = 2544 AND lm.lov_risk_data = 'No' AND lm.Client_Id = cm.Client_Id);

INSERT INTO lovmaster (lov_risk_catogory, lov_risk_catogory_code, lov_type_id, lov_type_name, lov_risk_data, lov_risk_score, Over_ride_Score, active_yn, date_created, date_updated, Client_Id, Created_By)
SELECT 'Corporate', 'C', 2545, 'Military Goods Match', 'Yes', 3, 3, 1, NOW(), NOW(), Client_Id, NULL FROM client_master cm
WHERE NOT EXISTS (SELECT 1 FROM lovmaster lm WHERE lm.lov_type_id = 2545 AND lm.lov_risk_data = 'Yes' AND lm.Client_Id = cm.Client_Id);

INSERT INTO lovmaster (lov_risk_catogory, lov_risk_catogory_code, lov_type_id, lov_type_name, lov_risk_data, lov_risk_score, Over_ride_Score, active_yn, date_created, date_updated, Client_Id, Created_By)
SELECT 'Corporate', 'C', 2545, 'Military Goods Match', 'No', 1, 0, 1, NOW(), NOW(), Client_Id, NULL FROM client_master cm
WHERE NOT EXISTS (SELECT 1 FROM lovmaster lm WHERE lm.lov_type_id = 2545 AND lm.lov_risk_data = 'No' AND lm.Client_Id = cm.Client_Id);

INSERT INTO lovmaster (lov_risk_catogory, lov_risk_catogory_code, lov_type_id, lov_type_name, lov_risk_data, lov_risk_score, Over_ride_Score, active_yn, date_created, date_updated, Client_Id, Created_By)
SELECT 'Individual', 'I', 2546, 'If More Military Goods', 'Yes', 3, 3, 1, NOW(), NOW(), Client_Id, NULL FROM client_master cm
WHERE NOT EXISTS (SELECT 1 FROM lovmaster lm WHERE lm.lov_type_id = 2546 AND lm.lov_risk_data = 'Yes' AND lm.Client_Id = cm.Client_Id);

INSERT INTO lovmaster (lov_risk_catogory, lov_risk_catogory_code, lov_type_id, lov_type_name, lov_risk_data, lov_risk_score, Over_ride_Score, active_yn, date_created, date_updated, Client_Id, Created_By)
SELECT 'Individual', 'I', 2546, 'If More Military Goods', 'No', 1, 0, 1, NOW(), NOW(), Client_Id, NULL FROM client_master cm
WHERE NOT EXISTS (SELECT 1 FROM lovmaster lm WHERE lm.lov_type_id = 2546 AND lm.lov_risk_data = 'No' AND lm.Client_Id = cm.Client_Id);

INSERT INTO lovmaster (lov_risk_catogory, lov_risk_catogory_code, lov_type_id, lov_type_name, lov_risk_data, lov_risk_score, Over_ride_Score, active_yn, date_created, date_updated, Client_Id, Created_By)
SELECT 'Corporate', 'C', 2547, 'If More Military Goods', 'Yes', 3, 3, 1, NOW(), NOW(), Client_Id, NULL FROM client_master cm
WHERE NOT EXISTS (SELECT 1 FROM lovmaster lm WHERE lm.lov_type_id = 2547 AND lm.lov_risk_data = 'Yes' AND lm.Client_Id = cm.Client_Id);

INSERT INTO lovmaster (lov_risk_catogory, lov_risk_catogory_code, lov_type_id, lov_type_name, lov_risk_data, lov_risk_score, Over_ride_Score, active_yn, date_created, date_updated, Client_Id, Created_By)
SELECT 'Corporate', 'C', 2547, 'If More Military Goods', 'No', 1, 0, 1, NOW(), NOW(), Client_Id, NULL FROM client_master cm
WHERE NOT EXISTS (SELECT 1 FROM lovmaster lm WHERE lm.lov_type_id = 2547 AND lm.lov_risk_data = 'No' AND lm.Client_Id = cm.Client_Id);

-- 4. Recreate Stored Procedures

DROP PROCEDURE IF EXISTS get_risk_lov_id_kyc;
DROP PROCEDURE IF EXISTS get_risk_type_id_kyc;
DROP PROCEDURE IF EXISTS get_proliferation_status;
DROP PROCEDURE IF EXISTS get_military_goods_status;

DELIMITER $$

CREATE PROCEDURE get_risk_lov_id_kyc(
IN p_profession VARCHAR(50),
IN p_nationality VARCHAR(50),
IN p_entityType VARCHAR(200),
IN p_incorporationPlace VARCHAR(100),
IN p_businessNature VARCHAR(500),
IN p_type VARCHAR(5),
IN p_residence VARCHAR(100),
IN p_partner1 VARCHAR(100),
IN p_partner2 VARCHAR(100),
IN p_partner3 VARCHAR(100),
IN p_partner4 VARCHAR(100),
IN p_partner5 VARCHAR(100),
IN p_product VARCHAR(200),
IN p_delivery VARCHAR(100),
IN p_culture VARCHAR(5),
IN p_clientId INT,
IN p_customertype VARCHAR(200),

IN p_indproduct VARCHAR(100),
IN p_inddelivery VARCHAR(100),
IN p_indmodeofpayment VARCHAR(100),
IN p_corpmodeofpayment VARCHAR(100),
IN p_domesticpep VARCHAR(100),
IN p_corpdomesticpep VARCHAR(100),
IN p_indforeignpep VARCHAR(100),
IN p_corpforeignpep VARCHAR(100),
IN p_indredflags VARCHAR(100),
IN p_corpredflags VARCHAR(100),
IN p_indsanctionMatch VARCHAR(100),
IN p_corpsanctionMatch VARCHAR(100),
IN p_induaeorunsc VARCHAR(100),
IN p_corpuaeorunsc VARCHAR(100),
IN p_corpfatf VARCHAR(100),
IN p_indhighestriskproduct VARCHAR(100),
IN p_corphighestriskproduct VARCHAR(100),
IN p_indhighnetworkindividual VARCHAR(100),
IN p_corphighnetworkindividual VARCHAR(100),
IN p_dualusegoods VARCHAR(100),
IN p_inddualusegoods VARCHAR(100),
IN p_moredualusegoods VARCHAR(100),
IN p_indmoredualusegoods VARCHAR(100),
IN p_militarygoods VARCHAR(100),
IN p_indmilitarygoods VARCHAR(100),
IN p_moremilitarygoods VARCHAR(100),
IN p_indmoremilitarygoods VARCHAR(100)
)
BEGIN
DECLARE profId INT;
DECLARE natId INT;
DECLARE entId INT;
DECLARE incorpId INT;
DECLARE busId INT;
DECLARE resId INT;
DECLARE part1 INT;
DECLARE part2 INT;
DECLARE part3 INT;
DECLARE part4 INT;
DECLARE part5 INT;
DECLARE prodId INT;
DECLARE deliveryId INT;

DECLARE IndprodId INT;
DECLARE InddeliveryId INT;
DECLARE IndmodeofpaymentId INT;
DECLARE CorpmodeofpaymentId INT;
DECLARE InddomesticId INT;
DECLARE CorpdomesticId INT;
DECLARE IndforeignId INT;
DECLARE CorpforeignId INT;
DECLARE IndredflagsId INT;
DECLARE corpredflagsId INT;
DECLARE IndsanctionMatchId INT;
DECLARE CorpsanctionMatchId INT;
DECLARE InduaeorunscId INT;
DECLARE CorpuaeorunscId INT;
DECLARE CorpfatfId INT;
DECLARE IndhighestriskproductId INT;
DECLARE corphighestriskproductId INT;
DECLARE IndhighnetworkindividualId INT;
DECLARE corphighnetworkindividualId INT;
DECLARE dualusegoodsId INT;
DECLARE inddualusegoodsId INT;
DECLARE moredualusegoodsId INT;
DECLARE indmoredualusegoodsId INT;
DECLARE militarygoodsId INT;
DECLARE indmilitarygoodsId INT;
DECLARE moremilitarygoodsId INT;
DECLARE indmoremilitarygoodsId INT;

IF p_profession != '' THEN
	SET profId = (SELECT lov_type_id FROM lovmaster WHERE lov_risk_data=p_profession AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET profId = 0;
END IF;

IF p_nationality != '' THEN
	SET natId = (SELECT id FROM lovtypemaster_backup WHERE lov_type_name='Nationality' AND active_yn = 1 AND lov_risk_category_code = p_type AND (Client_Id IS NULL OR Client_Id = 1 OR Client_Id = p_clientId) LIMIT 1);
ELSE
	SET natId = 0;
END IF;

IF p_entityType != '' THEN
	SET entId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Legal Status of the Enitiy' AND lov_risk_data = p_entityType AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId AND active_yn = 1 LIMIT 1);
ELSE
	SET entId = 0;
END IF;

IF p_incorporationPlace != '' THEN
	SET incorpId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name = 'Country of Incorporation' AND lov_risk_catogory_code = p_type AND (Client_Id IS NULL OR Client_Id = 1 OR Client_Id = p_clientId) AND active_yn = 1 LIMIT 1);
ELSE
	SET incorpId = 0;
END IF;

IF p_businessNature != '' THEN
	SET busId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name = 'Nature Of Business' AND lov_risk_data = p_businessNature AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET busId = 0;
END IF;

IF p_residence != '' THEN
	SET resId = (SELECT id FROM lovtypemaster_backup WHERE lov_type_name='Residence Country' AND active_yn = 1 AND lov_risk_category_code = p_type AND (Client_Id IS NULL OR Client_Id = 1 OR Client_Id = p_clientId) LIMIT 1);
ELSE
	SET resId = 0;
END IF;

IF p_partner1 != '' THEN
	SET part1 = (SELECT id FROM lovtypemaster_backup WHERE lov_type_name='Nationality Partner 1' AND active_yn = 1 AND lov_risk_category_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET part1 = 0;
END IF;

IF p_partner2 != '' THEN
	SET part2 = (SELECT id FROM lovtypemaster_backup WHERE lov_type_name = 'Nationality Partner 2' AND active_yn = 1 AND lov_risk_category_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET part2 = 0;
END IF;

IF p_partner3 != '' THEN
	SET part3 = (SELECT id FROM lovtypemaster_backup WHERE lov_type_name = 'Nationality Partner 3' AND active_yn = 1 AND lov_risk_category_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET part3 = 0;
END IF;

IF p_partner4 != '' THEN
	SET part4 = (SELECT id FROM lovtypemaster_backup WHERE lov_type_name = 'Nationality- Partner 4' AND active_yn = 1 AND lov_risk_category_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET part4 = 0;
END IF;

IF p_partner5 != '' THEN
	SET part5 = (SELECT id FROM lovtypemaster_backup WHERE lov_type_name = 'Nationality- Partner 5' AND active_yn = 1 AND lov_risk_category_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET part5 = 0;
END IF;

IF p_product != '' THEN
	SET prodId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name = 'Product' AND lov_risk_data = p_product AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId AND active_yn = 1 LIMIT 1);
ELSE
	SET prodId = 0;
END IF;

IF p_delivery != '' THEN
	SET deliveryId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name = 'Delivery Channel' AND lov_risk_data = p_delivery AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId AND active_yn = 1 LIMIT 1);
ELSE
	SET deliveryId = 0;
END IF;

IF p_indproduct != '' THEN
	SET IndprodId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Product, Service & Activity' AND lov_risk_data = p_indproduct AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET IndprodId = 0;
END IF;

IF p_inddelivery != '' THEN
	SET InddeliveryId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Delivery Channel' AND lov_risk_data = p_inddelivery AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET InddeliveryId = 0;
END IF;

IF p_indmodeofpayment != '' THEN
	SET IndmodeofpaymentId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Mode of Payment' AND lov_risk_data = p_indmodeofpayment AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET IndmodeofpaymentId = 0;
END IF;

IF p_corpmodeofpayment != '' THEN
	SET CorpmodeofpaymentId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Mode of Payment' AND lov_risk_data = p_corpmodeofpayment AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET CorpmodeofpaymentId = 0;
END IF;

IF p_domesticpep != '' THEN
	SET InddomesticId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Is the Customer a Domestic PEP or Related Close Associate of Domestic PEP?' AND lov_risk_data = p_domesticpep AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET InddomesticId = 0;
END IF;

IF p_corpdomesticpep != '' THEN
	SET CorpdomesticId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Is there any Domestic PEP match on the Owners / BOD/Senior Management / related parties names?' AND lov_risk_data = p_corpdomesticpep AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET CorpdomesticId = 0;
END IF;

IF p_indforeignpep != '' THEN
	SET IndforeignId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Is the Customer a Foreign PEP or Related Close Associate of Foreign PEP?' AND lov_risk_data = p_indforeignpep AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET IndforeignId = 0;
END IF;

IF p_corpforeignpep != '' THEN
	SET CorpforeignId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Is there any Foreign PEP match on the Owners / BOD/Senior Management/ related parties names?' AND lov_risk_data = p_corpforeignpep AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET CorpforeignId = 0;
END IF;

IF p_indredflags != '' THEN
	SET IndredflagsId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Are there any Red Flags noticed against the customer or related close associate?' AND lov_risk_data = p_indredflags AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET IndredflagsId = 0;
END IF;

IF p_corpredflags != '' THEN
	SET corpredflagsId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Are there any Red Flags noticed against the company/ Owners / BOD/Senior Management/ related parties names?' AND lov_risk_data = p_corpredflags AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET corpredflagsId = 0;
END IF;

IF p_indsanctionMatch != '' THEN
	SET IndsanctionMatchId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Is the Customer a Sanction match against other than UAE local list or UNSC consolidated?' AND lov_risk_data = p_indsanctionMatch AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET IndsanctionMatchId = 0;
END IF;

IF p_corpsanctionMatch != '' THEN
	SET CorpsanctionMatchId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Is there a sanction match against other than UAE local list or UNSC consolidated list on the company, Owner/Partners/BOD, Senior Management / related parties names?' AND lov_risk_data = p_corpsanctionMatch AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET CorpsanctionMatchId = 0;
END IF;

IF p_induaeorunsc != '' THEN
	SET InduaeorunscId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Is the Customer a Sanction match against UAE local list or UNSC consolidated?' AND lov_risk_data = p_induaeorunsc AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET InduaeorunscId = 0;
END IF;

IF p_corpuaeorunsc != '' THEN
	SET CorpuaeorunscId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Is there a sanction match against UAE local list or UNSC consolidated list on the company, Owner/Partners/BOD, Senior Management / related parties names?' AND lov_risk_data = p_corpuaeorunsc AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET CorpuaeorunscId = 0;
END IF;

IF p_corpfatf != '' THEN
	SET CorpfatfId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Does the company have any subsidiary, affiliate, branch or group/holding company in FATF listed high risk monitored jurisdiction?' AND lov_risk_data = p_corpfatf AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET CorpfatfId = 0;
END IF;

IF p_indhighestriskproduct != '' THEN
	SET IndhighestriskproductId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='If More Than One Product(Put The Riskiest Product)' AND lov_risk_data = p_indhighestriskproduct AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET IndhighestriskproductId = 0;
END IF;

IF p_corphighestriskproduct != '' THEN
	SET corphighestriskproductId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='If more than one Product(Put the Highest risk product)' AND lov_risk_data = p_corphighestriskproduct AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET corphighestriskproductId = 0;
END IF;

IF p_indhighnetworkindividual != '' THEN
	SET IndhighnetworkindividualId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Is the Customer Categorized as Very High Networth Individual?' AND lov_risk_data = p_indhighnetworkindividual AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET IndhighnetworkindividualId = 0;
END IF;

IF p_corphighnetworkindividual != '' THEN
	SET corphighnetworkindividualId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Is there any Owners / BOD/Senior Management names categorized as Very High Networth Individual?' AND lov_risk_data = p_corphighnetworkindividual AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET corphighnetworkindividualId = 0;
END IF;

IF p_dualusegoods != '' THEN
	SET dualusegoodsId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Dual Use Goods Match' AND lov_risk_data = p_dualusegoods AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET dualusegoodsId = 0;
END IF;

IF p_inddualusegoods != '' THEN
	SET inddualusegoodsId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Dual Use Goods Match' AND lov_risk_data = p_inddualusegoods AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET inddualusegoodsId = 0;
END IF;

IF p_moredualusegoods != '' THEN
	SET moredualusegoodsId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='If More Dual Use Goods' AND lov_risk_data = p_moredualusegoods AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET moredualusegoodsId = 0;
END IF;

IF p_indmoredualusegoods != '' THEN
	SET indmoredualusegoodsId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='If More Dual Use Goods' AND lov_risk_data = p_indmoredualusegoods AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET indmoredualusegoodsId = 0;
END IF;

IF p_militarygoods != '' THEN
	SET militarygoodsId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Military Goods Match' AND lov_risk_data = p_militarygoods AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET militarygoodsId = 0;
END IF;

IF p_indmilitarygoods != '' THEN
	SET indmilitarygoodsId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='Military Goods Match' AND lov_risk_data = p_indmilitarygoods AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET indmilitarygoodsId = 0;
END IF;

IF p_moremilitarygoods != '' THEN
	SET moremilitarygoodsId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='If More Military Goods' AND lov_risk_data = p_moremilitarygoods AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET moremilitarygoodsId = 0;
END IF;

IF p_indmoremilitarygoods != '' THEN
	SET indmoremilitarygoodsId = (SELECT lov_type_id FROM lovmaster WHERE lov_type_name='If More Military Goods' AND lov_risk_data = p_indmoremilitarygoods AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET indmoremilitarygoodsId = 0;
END IF;

SELECT CONCAT(
	IFNULL(profId,0),'Ø',IFNULL(natId,0),'Ø',IFNULL(entId,0),'Ø',IFNULL(incorpId,0),'Ø',IFNULL(busId,0),'Ø',IFNULL(resId,0),'Ø',IFNULL(part1,0),'Ø',
	IFNULL(part2,0),'Ø',IFNULL(part3,0),'Ø',IFNULL(part4,0),'Ø',IFNULL(part5,0),'Ø',IFNULL(prodId,0),'Ø',IFNULL(deliveryId,0),'Ø',
	IFNULL(IndprodId,0),'Ø',IFNULL(InddeliveryId,0),'Ø',IFNULL(IndmodeofpaymentId,0),'Ø',IFNULL(CorpmodeofpaymentId,0),'Ø',
	IFNULL(InddomesticId,0),'Ø',IFNULL(CorpdomesticId,0),'Ø',IFNULL(IndforeignId,0),'Ø',IFNULL(CorpforeignId,0),'Ø',
	IFNULL(IndredflagsId,0),'Ø',IFNULL(corpredflagsId,0),'Ø',IFNULL(IndsanctionMatchId,0),'Ø',IFNULL(CorpsanctionMatchId,0),'Ø',
	IFNULL(InduaeorunscId,0),'Ø',IFNULL(CorpuaeorunscId,0),'Ø',IFNULL(CorpfatfId,0),'Ø',IFNULL(IndhighestriskproductId,0),'Ø',
	IFNULL(corphighestriskproductId,0),'Ø',IFNULL(IndhighnetworkindividualId,0),'Ø',IFNULL(corphighnetworkindividualId,0),'Ø',
	IFNULL(dualusegoodsId,0),'Ø',IFNULL(inddualusegoodsId,0),'Ø',IFNULL(moredualusegoodsId,0),'Ø',IFNULL(indmoredualusegoodsId,0),'Ø',
	IFNULL(militarygoodsId,0),'Ø',IFNULL(indmilitarygoodsId,0),'Ø',IFNULL(moremilitarygoodsId,0),'Ø',IFNULL(indmoremilitarygoodsId,0)
);

END$$

CREATE PROCEDURE get_risk_type_id_kyc(
IN p_profession VARCHAR(50),
IN p_nationality VARCHAR(50),
IN p_entityType VARCHAR(200),
IN p_incorporationPlace VARCHAR(100),
IN p_businessNature VARCHAR(500),
IN p_type VARCHAR(5),
IN p_residence VARCHAR(100),
IN p_partner1 VARCHAR(100),
IN p_partner2 VARCHAR(100),
IN p_partner3 VARCHAR(100),
IN p_partner4 VARCHAR(100),
IN p_partner5 VARCHAR(100),
IN p_product VARCHAR(200),
IN p_delivery VARCHAR(100),
IN p_culture VARCHAR(5),
IN p_clientId INT,

IN p_indproduct VARCHAR(100),
IN p_inddelivery VARCHAR(100),
IN p_indmodeofpayment VARCHAR(100),
IN p_corpmodeofpayment VARCHAR(100),
IN p_domesticpep VARCHAR(100),
IN p_corpdomesticpep VARCHAR(100),
IN p_indforeignpep VARCHAR(100),
IN p_corpforeignpep VARCHAR(100),
IN p_indredflags VARCHAR(100),
IN p_corpredflags VARCHAR(100),
IN p_indsanctionMatch VARCHAR(100),
IN p_corpsanctionMatch VARCHAR(100),
IN p_induaeorunsc VARCHAR(100),
IN p_corpuaeorunsc VARCHAR(100),
IN p_corpfatf VARCHAR(100),
IN p_indhighestriskproduct VARCHAR(100),
IN p_corphighestriskproduct VARCHAR(100),
IN p_indhighnetworkindividual VARCHAR(100),
IN p_corphighnetworkindividual VARCHAR(100),
IN p_dualusegoods VARCHAR(100),
IN p_inddualusegoods VARCHAR(100),
IN p_moredualusegoods VARCHAR(100),
IN p_indmoredualusegoods VARCHAR(100),
IN p_militarygoods VARCHAR(100),
IN p_indmilitarygoods VARCHAR(100),
IN p_moremilitarygoods VARCHAR(100),
IN p_indmoremilitarygoods VARCHAR(100)
)
BEGIN
DECLARE profId INT;
DECLARE natId INT;
DECLARE entId INT;
DECLARE incorpId INT;
DECLARE busId INT;
DECLARE resId INT;
DECLARE part1 INT;
DECLARE part2 INT;
DECLARE part3 INT;
DECLARE part4 INT;
DECLARE part5 INT;
DECLARE prodId INT;
DECLARE deliveryId INT;

DECLARE IndprodId INT;
DECLARE InddeliveryId INT;
DECLARE IndmodeofpaymentId INT;
DECLARE CorpmodeofpaymentId INT;
DECLARE InddomesticId INT;
DECLARE CorpdomesticId INT;
DECLARE IndforeignId INT;
DECLARE CorpforeignId INT;
DECLARE IndredflagsId INT;
DECLARE corpredflagsId INT;
DECLARE IndsanctionMatchId INT;
DECLARE CorpsanctionMatchId INT;
DECLARE InduaeorunscId INT;
DECLARE CorpuaeorunscId INT;
DECLARE CorpfatfId INT;
DECLARE IndhighestriskproductId INT;
DECLARE corphighestriskproductId INT;
DECLARE IndhighnetworkindividualId INT;
DECLARE corphighnetworkindividualId INT;
DECLARE dualusegoodsId INT;
DECLARE inddualusegoodsId INT;
DECLARE moredualusegoodsId INT;
DECLARE indmoredualusegoodsId INT;
DECLARE militarygoodsId INT;
DECLARE indmilitarygoodsId INT;
DECLARE moremilitarygoodsId INT;
DECLARE indmoremilitarygoodsId INT;

IF p_profession != '' THEN
	SET profId = (SELECT id FROM lovmaster WHERE lov_risk_data=p_profession AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET profId = 0;
END IF;

IF p_nationality != '' THEN
	SET natId = (SELECT id FROM country WHERE NAME = p_nationality AND Is_active = 1 AND (Client_Id IS NULL OR Client_Id = 1 OR Client_Id = p_clientId) LIMIT 1);
ELSE
	SET natId = 0;
END IF;

IF p_entityType != '' THEN
	SET entId = (SELECT id FROM lovmaster WHERE lov_type_name='Legal Status of the Enitiy' AND lov_risk_data = p_entityType AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId AND active_yn = 1 LIMIT 1);
ELSE
	SET entId = 0;
END IF;

IF p_incorporationPlace != '' THEN
	SET incorpId = (SELECT id FROM country WHERE NAME = p_incorporationPlace AND Is_active = 1 AND (Client_Id IS NULL OR Client_Id = 1 OR Client_Id = p_clientId) LIMIT 1);
ELSE
	SET incorpId = 0;
END IF;

IF p_businessNature != '' THEN
	SET busId = (SELECT id FROM lovmaster WHERE lov_type_name = 'Nature Of Business' AND lov_risk_data = p_businessNature AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET busId = 0;
END IF;

IF p_residence != '' THEN
	SET resId = (SELECT id FROM country WHERE NAME = p_residence AND Is_active = 1 AND (Client_Id IS NULL OR Client_Id = 1 OR Client_Id = p_clientId) LIMIT 1);
ELSE
	SET resId = 0;
END IF;

IF p_partner1 != '' THEN
	SET part1 = (SELECT id FROM country WHERE NAME = p_partner1 AND Is_active = 1 AND (Client_Id IS NULL OR Client_Id = 1 OR Client_Id = p_clientId) LIMIT 1);
ELSE
	SET part1 = 0;
END IF;

IF p_partner2 != '' THEN
	SET part2 = (SELECT id FROM country WHERE NAME = p_partner2 AND Is_active = 1 AND (Client_Id IS NULL OR Client_Id = 1 OR Client_Id = p_clientId) LIMIT 1);
ELSE
	SET part2 = 0;
END IF;

IF p_partner3 != '' THEN
	SET part3 = (SELECT id FROM country WHERE NAME = p_partner3 AND Is_active = 1 AND (Client_Id IS NULL OR Client_Id = 1 OR Client_Id = p_clientId) LIMIT 1);
ELSE
	SET part3 = 0;
END IF;

IF p_partner4 != '' THEN
	SET part4 = (SELECT id FROM country WHERE NAME = p_partner4 AND Is_active = 1 AND (Client_Id IS NULL OR Client_Id = 1 OR Client_Id = p_clientId) LIMIT 1);
ELSE
	SET part4 = 0;
END IF;

IF p_partner5 != '' THEN
	SET part5 = (SELECT id FROM country WHERE NAME = p_partner5 AND Is_active = 1 AND (Client_Id IS NULL OR Client_Id = 1 OR Client_Id = p_clientId) LIMIT 1);
ELSE
	SET part5 = 0;
END IF;

IF p_product != '' THEN
	SET prodId = (SELECT id FROM lovmaster WHERE lov_type_name = 'Product' AND lov_risk_data = p_product AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId AND active_yn = 1 LIMIT 1);
ELSE
	SET prodId = 0;
END IF;

IF p_delivery != '' THEN
	SET deliveryId = (SELECT id FROM lovmaster WHERE lov_type_name = 'Delivery Channel' AND lov_risk_data = p_delivery AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId AND active_yn = 1 LIMIT 1);
ELSE
	SET deliveryId = 0;
END IF;

IF p_indproduct != '' THEN
	SET IndprodId = (SELECT id FROM lovmaster WHERE lov_type_name='Product, Service & Activity' AND lov_risk_data = p_indproduct AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET IndprodId = 0;
END IF;

IF p_inddelivery != '' THEN
	SET InddeliveryId = (SELECT id FROM lovmaster WHERE lov_type_name='Delivery Channel' AND lov_risk_data = p_inddelivery AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET InddeliveryId = 0;
END IF;

IF p_indmodeofpayment != '' THEN
	SET IndmodeofpaymentId = (SELECT id FROM lovmaster WHERE lov_type_name='Mode of Payment' AND lov_risk_data = p_indmodeofpayment AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET IndmodeofpaymentId = 0;
END IF;

IF p_corpmodeofpayment != '' THEN
	SET CorpmodeofpaymentId = (SELECT id FROM lovmaster WHERE lov_type_name='Mode of Payment' AND lov_risk_data = p_corpmodeofpayment AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET CorpmodeofpaymentId = 0;
END IF;

IF p_domesticpep != '' THEN
	SET InddomesticId = (SELECT id FROM lovmaster WHERE lov_type_name='Is the Customer a Domestic PEP or Related Close Associate of Domestic PEP?' AND lov_risk_data = p_domesticpep AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET InddomesticId = 0;
END IF;

IF p_corpdomesticpep != '' THEN
	SET CorpdomesticId = (SELECT id FROM lovmaster WHERE lov_type_name='Is there any Domestic PEP match on the Owners / BOD/Senior Management / related parties names?' AND lov_risk_data = p_corpdomesticpep AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET CorpdomesticId = 0;
END IF;

IF p_indforeignpep != '' THEN
	SET IndforeignId = (SELECT id FROM lovmaster WHERE lov_type_name='Is the Customer a Foreign PEP or Related Close Associate of Foreign PEP?' AND lov_risk_data = p_indforeignpep AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET IndforeignId = 0;
END IF;

IF p_corpforeignpep != '' THEN
	SET CorpforeignId = (SELECT id FROM lovmaster WHERE lov_type_name='Is there any Foreign PEP match on the Owners / BOD/Senior Management/ related parties names?' AND lov_risk_data = p_corpforeignpep AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET CorpforeignId = 0;
END IF;

IF p_indredflags != '' THEN
	SET IndredflagsId = (SELECT id FROM lovmaster WHERE lov_type_name='Are there any Red Flags noticed against the customer or related close associate?' AND lov_risk_data = p_indredflags AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET IndredflagsId = 0;
END IF;

IF p_corpredflags != '' THEN
	SET corpredflagsId = (SELECT id FROM lovmaster WHERE lov_type_name='Are there any Red Flags noticed against the company/ Owners / BOD/Senior Management/ related parties names?' AND lov_risk_data = p_corpredflags AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET corpredflagsId = 0;
END IF;

IF p_indsanctionMatch != '' THEN
	SET IndsanctionMatchId = (SELECT id FROM lovmaster WHERE lov_type_name='Is the Customer a Sanction match against other than UAE local list or UNSC consolidated?' AND lov_risk_data = p_indsanctionMatch AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET IndsanctionMatchId = 0;
END IF;

IF p_corpsanctionMatch != '' THEN
	SET CorpsanctionMatchId = (SELECT id FROM lovmaster WHERE lov_type_name='Is there a sanction match against other than UAE local list or UNSC consolidated list on the company, Owner/Partners/BOD, Senior Management / related parties names?' AND lov_risk_data = p_corpsanctionMatch AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET CorpsanctionMatchId = 0;
END IF;

IF p_induaeorunsc != '' THEN
	SET InduaeorunscId = (SELECT id FROM lovmaster WHERE lov_type_name='Is the Customer a Sanction match against UAE local list or UNSC consolidated?' AND lov_risk_data = p_induaeorunsc AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET InduaeorunscId = 0;
END IF;

IF p_corpuaeorunsc != '' THEN
	SET CorpuaeorunscId = (SELECT id FROM lovmaster WHERE lov_type_name='Is there a sanction match against UAE local list or UNSC consolidated list on the company, Owner/Partners/BOD, Senior Management / related parties names?' AND lov_risk_data = p_corpuaeorunsc AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET CorpuaeorunscId = 0;
END IF;

IF p_corpfatf != '' THEN
	SET CorpfatfId = (SELECT id FROM lovmaster WHERE lov_type_name='Does the company have any subsidiary, affiliate, branch or group/holding company in FATF listed high risk monitored jurisdiction?' AND lov_risk_data = p_corpfatf AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET CorpfatfId = 0;
END IF;

IF p_indhighestriskproduct != '' THEN
	SET IndhighestriskproductId = (SELECT id FROM lovmaster WHERE lov_type_name='If More Than One Product(Put The Riskiest Product)' AND lov_risk_data = p_indhighestriskproduct AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET IndhighestriskproductId = 0;
END IF;

IF p_corphighestriskproduct != '' THEN
	SET corphighestriskproductId = (SELECT id FROM lovmaster WHERE lov_type_name='If more than one Product(Put the Highest risk product)' AND lov_risk_data = p_corphighestriskproduct AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET corphighestriskproductId = 0;
END IF;

IF p_indhighnetworkindividual != '' THEN
	SET IndhighnetworkindividualId = (SELECT id FROM lovmaster WHERE lov_type_name='Is the Customer Categorized as Very High Networth Individual?' AND lov_risk_data = p_indhighnetworkindividual AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET IndhighnetworkindividualId = 0;
END IF;

IF p_corphighnetworkindividual != '' THEN
	SET corphighnetworkindividualId = (SELECT id FROM lovmaster WHERE lov_type_name='Is there any Owners / BOD/Senior Management names categorized as Very High Networth Individual?' AND lov_risk_data = p_corphighnetworkindividual AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET corphighnetworkindividualId = 0;
END IF;

IF p_dualusegoods != '' THEN
	SET dualusegoodsId = (SELECT id FROM lovmaster WHERE lov_type_name='Dual Use Goods Match' AND lov_risk_data = p_dualusegoods AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET dualusegoodsId = 0;
END IF;

IF p_inddualusegoods != '' THEN
	SET inddualusegoodsId = (SELECT id FROM lovmaster WHERE lov_type_name='Dual Use Goods Match' AND lov_risk_data = p_inddualusegoods AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET inddualusegoodsId = 0;
END IF;

IF p_moredualusegoods != '' THEN
	SET moredualusegoodsId = (SELECT id FROM lovmaster WHERE lov_type_name='If More Dual Use Goods' AND lov_risk_data = p_moredualusegoods AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET moredualusegoodsId = 0;
END IF;

IF p_indmoredualusegoods != '' THEN
	SET indmoredualusegoodsId = (SELECT id FROM lovmaster WHERE lov_type_name='If More Dual Use Goods' AND lov_risk_data = p_indmoredualusegoods AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET indmoredualusegoodsId = 0;
END IF;

IF p_militarygoods != '' THEN
	SET militarygoodsId = (SELECT id FROM lovmaster WHERE lov_type_name='Military Goods Match' AND lov_risk_data = p_militarygoods AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET militarygoodsId = 0;
END IF;

IF p_indmilitarygoods != '' THEN
	SET indmilitarygoodsId = (SELECT id FROM lovmaster WHERE lov_type_name='Military Goods Match' AND lov_risk_data = p_indmilitarygoods AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET indmilitarygoodsId = 0;
END IF;

IF p_moremilitarygoods != '' THEN
	SET moremilitarygoodsId = (SELECT id FROM lovmaster WHERE lov_type_name='If More Military Goods' AND lov_risk_data = p_moremilitarygoods AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET moremilitarygoodsId = 0;
END IF;

IF p_indmoremilitarygoods != '' THEN
	SET indmoremilitarygoodsId = (SELECT id FROM lovmaster WHERE lov_type_name='If More Military Goods' AND lov_risk_data = p_indmoremilitarygoods AND active_yn = 1 AND lov_risk_catogory_code = p_type AND Client_Id = p_clientId LIMIT 1);
ELSE
	SET indmoremilitarygoodsId = 0;
END IF;

SELECT CONCAT(
	IFNULL(profId,0),'Ø',IFNULL(natId,0),'Ø',IFNULL(entId,0),'Ø',IFNULL(incorpId,0),'Ø',IFNULL(busId,0),'Ø',IFNULL(resId,0),'Ø',IFNULL(part1,0),'Ø',
	IFNULL(part2,0),'Ø',IFNULL(part3,0),'Ø',IFNULL(part4,0),'Ø',IFNULL(part5,0),'Ø',IFNULL(prodId,0),'Ø',IFNULL(deliveryId,0),'Ø',
	IFNULL(IndprodId,0),'Ø',IFNULL(InddeliveryId,0),'Ø',IFNULL(IndmodeofpaymentId,0),'Ø',IFNULL(CorpmodeofpaymentId,0),'Ø',
	IFNULL(InddomesticId,0),'Ø',IFNULL(CorpdomesticId,0),'Ø',IFNULL(IndforeignId,0),'Ø',IFNULL(CorpforeignId,0),'Ø',
	IFNULL(IndredflagsId,0),'Ø',IFNULL(corpredflagsId,0),'Ø',IFNULL(IndsanctionMatchId,0),'Ø',IFNULL(CorpsanctionMatchId,0),'Ø',
	IFNULL(InduaeorunscId,0),'Ø',IFNULL(CorpuaeorunscId,0),'Ø',IFNULL(CorpfatfId,0),'Ø',IFNULL(IndhighestriskproductId,0),'Ø',
	IFNULL(corphighestriskproductId,0),'Ø',IFNULL(IndhighnetworkindividualId,0),'Ø',IFNULL(corphighnetworkindividualId,0),'Ø',
	IFNULL(dualusegoodsId,0),'Ø',IFNULL(inddualusegoodsId,0),'Ø',IFNULL(moredualusegoodsId,0),'Ø',IFNULL(indmoredualusegoodsId,0),'Ø',
	IFNULL(militarygoodsId,0),'Ø',IFNULL(indmilitarygoodsId,0),'Ø',IFNULL(moremilitarygoodsId,0),'Ø',IFNULL(indmoremilitarygoodsId,0)
);

END$$

CREATE PROCEDURE get_proliferation_status(IN p_id VARCHAR(200))
BEGIN
	SELECT Status FROM proliferationfinancecase WHERE CorporateId = p_id AND CustomerType IN ('Goods', 'Chemical') ORDER BY Id DESC LIMIT 1;
END$$

CREATE PROCEDURE get_military_goods_status(IN p_id VARCHAR(200))
BEGIN
	SELECT Status FROM proliferationfinancecase WHERE CorporateId = p_id AND CustomerType = 'Military' ORDER BY Id DESC LIMIT 1;
END$$

DELIMITER ;
