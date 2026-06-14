-- =====================================================================
-- Fix: get_risk_id_by_customercode was returning EVERY risk record for a
-- customer with no ORDER BY. The C# caller (RiskRepository.GetRiskIdByCustomercode)
-- uses GetFirstOrDefault<int>, which picks the first row returned -
-- effectively the OLDEST record, often left over from earlier flows that
-- stored corrupt LOV blobs (e.g. "1572¥104585¥185¥..." inside
-- transaction_risk_category_type_score.trans_risk_type_id).
--
-- Result: the on-screen detail and the PDF download both pulled an
-- ancient risk record whose ReportDataDTO rows could not be matched
-- against the current LOV config, so every dropdown rendered as "-".
--
-- Patch: keep the SP signature, only change the SELECT to return the
-- LATEST risk record for the given customer + type. All existing
-- callers benefit immediately - they get the active assessment.
-- =====================================================================

DROP PROCEDURE IF EXISTS get_risk_id_by_customercode;

DELIMITER $$

CREATE PROCEDURE get_risk_id_by_customercode(
    IN p_customercode VARCHAR(500),
    IN p_type         VARCHAR(200)
)
BEGIN
    IF p_type = 'I' THEN
        SELECT id
        FROM transaction_risk_individual
        WHERE customercode = p_customercode
          AND IsDeleted = 0
        ORDER BY id DESC
        LIMIT 1;
    ELSE
        SELECT id
        FROM transaction_risk_corporate
        WHERE customercode = p_customercode
          AND IsDeleted = 0
        ORDER BY id DESC
        LIMIT 1;
    END IF;
END$$

DELIMITER ;

-- Sanity check
CALL get_risk_id_by_customercode('NAT59', 'I');
