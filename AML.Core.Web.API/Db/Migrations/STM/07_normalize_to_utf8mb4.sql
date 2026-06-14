-- =====================================================================
-- Normalize the lemon_uat schema to utf8mb4 / utf8mb4_unicode_ci.
-- See the explanation header in this migration's commit message.
-- Lossless: utf8mb4 is a superset of latin1 and utf8mb3.
-- =====================================================================

ALTER DATABASE lemon_uat CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

ALTER TABLE `customermaster` MODIFY COLUMN `cust_ref_id` VARCHAR(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `nationality` VARCHAR(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `cust_type` ENUM('I','C','S','B','V') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'I';
ALTER TABLE `customermaster` MODIFY COLUMN `cust_id_type` VARCHAR(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `cust_id_number` VARCHAR(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `mobile` VARCHAR(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `customer_final_risk_score` VARCHAR(15) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `whitelisted_for_screening` ENUM('NO','YES') CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT 'NO';
ALTER TABLE `customermaster` MODIFY COLUMN `passport_expiry` VARCHAR(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `customer_type` VARCHAR(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `group_entity_of` VARCHAR(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `designation` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `emirates_id` VARCHAR(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `emirates_id_expiry` VARCHAR(45) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `company_code` VARCHAR(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `tradelicense` VARCHAR(250) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `type` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `profession` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `mode_of_payment` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `delivery_channel` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `residence_status` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `product_name` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `bussiness_type` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `legal_status` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `cif_number` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `parent_id` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `screeningoption` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `residence` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `employer` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `customermaster` MODIFY COLUMN `goldenvisa` VARCHAR(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `screening_database_logs` MODIFY COLUMN `updateddate` VARCHAR(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `tempshareholdersdata` MODIFY COLUMN `companycode` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL;
ALTER TABLE `tempshareholdersdata` MODIFY COLUMN `fullname` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL;
ALTER TABLE `tempshareholdersdata` MODIFY COLUMN `designation` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `tempshareholdersdata` MODIFY COLUMN `idnumber` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `tempshareholdersdata` MODIFY COLUMN `idtype` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `tempshareholdersdata` MODIFY COLUMN `cif` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `tempshareholdersdata` MODIFY COLUMN `type` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `tempshareholdersdata` MODIFY COLUMN `tradelicense` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `tempshareholdersdata` MODIFY COLUMN `nationality` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;
ALTER TABLE `tempshareholdersdata` MODIFY COLUMN `companyname` VARCHAR(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL;

-- Reaffirm table defaults so future ADD COLUMN inherits utf8mb4
ALTER TABLE customermaster          CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE tempshareholdersdata    CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
ALTER TABLE screening_database_logs CONVERT TO CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Sanity check
SELECT 'Remaining non-utf8mb4 columns' AS info, COUNT(*) AS cnt
FROM information_schema.columns
WHERE table_schema='lemon_uat'
  AND character_set_name IS NOT NULL
  AND character_set_name NOT LIKE 'utf8mb4%';
