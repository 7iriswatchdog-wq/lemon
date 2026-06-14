DROP PROCEDURE IF EXISTS `get_all_customercase_by_companycode`;
DELIMITER ;;
CREATE PROCEDURE `get_all_customercase_by_companycode`(IN p_id varchar(200))
BEGIN
select
    cc.`id`,
    cm.`fname`,
    cm.`mname`,
    cm.`lname`,
    cm.`cust_type`,
    cm.`dob`,
    cc.`cust_id`,
    cm.`nationality`,
    cc.`comments`,
    cc.`risk_score`,
    cc.`match_score`,
    cm.`batch_id`,
    cm.`mobile`,
    cm.`cust_id_type` AS `customer_id_type`,
    cm.`cust_id_number` AS `customer_id_number`,
    cc.`status`,
    CASE cc.status
       WHEN 0 THEN CONCAT('Pending')
        WHEN 1 THEN CONCAT('White List')
        WHEN 2 THEN CONCAT('Approved')
        WHEN 3 THEN CONCAT('Rejected')
        WHEN 4 THEN CONCAT('Pending with Senior Management')
        WHEN 5 THEN CONCAT('Auto')
        WHEN 6 THEN CONCAT('Pending Case Created from Daily Scheduler')
        WHEN 7 THEN CONCAT('On Hold')
        ELSE '-'
        END
         AS 'CaseStatus',
    cc.`source`,
    cc.`match_category`,
    cc.`match_type`,
    cc.`created_by`,
    cc.`created_on`,
    concat(IFNULL(u.fname, ''), ' ', IFNULL(u.lname,'')) as `updated_user`,us.user_name as `created_user`,
    cc.`updated_on`,
    cc.source_unique_id,
    cc.`is_deleted`,
    cc.`cust_master_id`,
    cm.whitelisted_for_screening,
       tri.final_risk_score AS 'individual_risk_score',
    trc.final_risk_score AS 'corporate_risk_score',
    cc.no_match,
    cc.true_dometic_pep,
    cc.true_foreign_pep,
    cc.true_adversemedia,
    cc.partial_domestic_pep,
    cc.partial_foreign_pep,
    cc.partial_adversemedia,
    cc.true_uae_un_sanction,
    cc.true_other_sanction,
    cc.gender,
    cm.type,
    cm.parent_id,
    cm.flagtype,
    cm.relationship,
    cm.is_duplicate,
    cm.version,
    cm.replaceflag,
       cm.updatereason
from
    customercase cc
    JOIN customermaster cm ON `cc`.`cust_id` = `cm`.`cust_ref_id`
    LEFT JOIN user u ON cc.updated_by = u.id
    left join user us on cm.created_by = us.id LEFT JOIN transaction_risk_individual tri ON cc.cust_id=tri.customercode
    LEFT JOIN transaction_risk_corporate trc ON cc.cust_id=trc.customercode
where (
        cm.`company_code` = p_id
        OR FIND_IN_SET(p_id, cm.`company_code`) > 0
    )
    AND cm.version = (
        SELECT MAX(cu2.version)
        FROM customermaster cu2
        WHERE cu2.cust_ref_id = cm.cust_ref_id
    );
END ;;
DELIMITER ;

DROP PROCEDURE IF EXISTS `get_all_customercase_by_parentcode`;
DELIMITER ;;
CREATE PROCEDURE `get_all_customercase_by_parentcode`(IN p_id varchar(200))
BEGIN
select
    cc.`id`,
    cm.`fname`,
    cm.`mname`,
    cm.`lname`,
    cm.`cust_type`,
    cm.`dob`,
    cc.`cust_id`,
    cm.`nationality`,
    cc.`comments`,
    cc.`risk_score`,
    cc.`match_score`,
    cm.`batch_id`,
    cm.`mobile`,
    cm.`cust_id_type` AS `customer_id_type`,
    cm.`cust_id_number` AS `customer_id_number`,
    cc.`status`,
    CASE cc.status
        WHEN 0 THEN CONCAT('Pending')
        WHEN 1 THEN CONCAT('Assigned')
        WHEN 2 THEN CONCAT('Approved')
        WHEN 3 THEN CONCAT('Rejected')
        WHEN 4 THEN CONCAT('Pending with Senior Management')
        WHEN 5 THEN CONCAT('Auto')
        WHEN 6 THEN CONCAT('Pending Daily Scheduler')
        ELSE '-'
        END
         AS 'CaseStatus',
    cc.`source`,
    cc.`match_category`,
    cc.`match_type`,
    cc.`created_by`,
    cc.`created_on`,
    concat(IFNULL(u.fname, ''), ' ', IFNULL(u.lname,'')) as `updated_user`,us.user_name as `created_user`,
    cc.`updated_on`,
    cc.source_unique_id,
    cc.`is_deleted`,
    cc.`cust_master_id`,
    cm.whitelisted_for_screening,
       tri.final_risk_score AS 'individual_risk_score',
    trc.final_risk_score AS 'corporate_risk_score',
    cc.no_match,
    cc.true_dometic_pep,
    cc.true_foreign_pep,
    cc.true_adversemedia,
    cc.partial_domestic_pep,
    cc.partial_foreign_pep,
    cc.partial_adversemedia,
    cc.true_uae_un_sanction,
    cc.true_other_sanction,
    cc.gender,
    cm.type,
    cm.parent_id,
    cm.flagtype,
    cm.relationship,
    cm.is_duplicate,
    cm.replaceflag,
    cm.updatereason
from
    customercase cc
    JOIN customermaster cm ON `cc`.`cust_id` = `cm`.`cust_ref_id`
    LEFT JOIN user u ON cc.updated_by = u.id
    left join user us on cm.created_by = us.id LEFT JOIN transaction_risk_individual tri ON cc.cust_id=tri.customercode
    LEFT JOIN transaction_risk_corporate trc ON cc.cust_id=trc.customercode
where (
        cm.`parent_id` = p_id
        OR FIND_IN_SET(p_id, cm.`parent_id`) > 0
    )
    AND cm.version = (
        SELECT MAX(cu2.version)
        FROM customermaster cu2
        WHERE cu2.cust_ref_id = cm.cust_ref_id
    );
END ;;
DELIMITER ;

DROP PROCEDURE IF EXISTS `get_all_CaseCreated_shareholders`;
DELIMITER ;;
CREATE PROCEDURE `get_all_CaseCreated_shareholders`(In p_clientid int,In p_companyCode varchar(200),in p_userid int)
BEGIN
Select `id`,
`cust_ref_id` AS `cust_id`,
    `company_code`,
    `fname`,
    `mname`,
    `lname`,
    `share`,
    `designation`,
    `type`,
    `tradelicense`,
    `client_id`,
    `employer`,
    `goldenvisa`,
    `residence`,
    `cust_type`,
    `nationality`,`employerindustry`,`employersector`,`sowsofcountry`,`tradelicenseauthority`,`tradelicensesector`,`relationship`,`flagtype`,`is_duplicate`
from customermaster
where client_id=p_clientid
    and (
        company_code=p_companyCode
        OR FIND_IN_SET(p_companyCode, company_code) > 0
    );
END ;;
DELIMITER ;
