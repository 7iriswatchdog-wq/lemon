using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.SectoralTMS;
using AML.DTO.DTO.SectoralTMS;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AML.Core.Repository.SectoralTMS
{
    public class StmRepository : BaseRepository, IStmRepository
    {
        public StmRepository(IConfiguration configuration, IHttpContextAccessor context)
            : base(configuration, context) { }

        // ==================================================================
        // SECTOR
        // ==================================================================
        public ServiceResponse<List<StmSectorDTO>> GetSectors(int clientId)
        {
            var response = new ServiceResponse<List<StmSectorDTO>>();
            try
            {
                // Restrict to sectors the client has been granted access to via stm_client_sector_access.
                // A "master" client (e.g. National Compliance, is_master = 1) sees every sector.
                var sql = @"SELECT s.* FROM stm_sector s
                            WHERE s.is_active = 1
                              AND (
                                   EXISTS (SELECT 1 FROM stm_client_sector_access csa
                                           WHERE csa.client_id = @ClientId AND csa.is_master = 1)
                                OR EXISTS (SELECT 1 FROM stm_client_sector_access csa
                                           WHERE csa.client_id = @ClientId AND csa.sector_id = s.id)
                              )
                            ORDER BY s.sector_name";
                response.Result = Get<StmSectorDTO>(sql, new { ClientId = clientId }).ToList();
                response.Status = StaticResource.SuccessStatusCode;
                response.Message = "Sectors fetched";
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<StmSectorDTO> GetSectorByCode(string sectorCode, int clientId)
        {
            var response = new ServiceResponse<StmSectorDTO>();
            try
            {
                var sql = @"SELECT * FROM stm_sector WHERE sector_code = @SectorCode
                            AND (client_id = @ClientId OR client_id = 0) LIMIT 1";
                response.Result = GetFirstOrDefault<StmSectorDTO>(sql, new { SectorCode = sectorCode, ClientId = clientId });
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        // ==================================================================
        // RULES
        // ==================================================================
        public ServiceResponse<int> InsertRule(StmRuleDTO rule)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var sql = @"INSERT INTO stm_rule
                            (rule_code, rule_name, rule_description, sector_id, risk_rating, rule_score,
                             logical_operator, action_on_hit, is_active, is_system_rule, client_id,
                             created_on, created_by)
                            VALUES (@RuleCode, @RuleName, @RuleDescription, @SectorId, @RiskRating, @RuleScore,
                             @LogicalOperator, @ActionOnHit, @IsActive, @IsSystemRule, @ClientId,
                             NOW(), @CreatedBy);
                            SELECT LAST_INSERT_ID();";
                using (var conn = GetConnections())
                {
                    response.Result = conn.ExecuteScalar<int>(sql, rule);
                }
                response.Status = StaticResource.SuccessStatusCode;
                response.Message = "Rule created";
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<int> InsertRuleCondition(StmRuleConditionDTO condition)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var sql = @"INSERT INTO stm_rule_condition
                            (rule_id, sequence_no, field_name, field_aggregation, `operator`,
                             compare_value, compare_field, timeframe_value, timeframe_unit,
                             conjunction, description, created_on)
                            VALUES (@RuleId, @SequenceNo, @FieldName, @FieldAggregation, @Operator,
                             @CompareValue, @CompareField, @TimeframeValue, @TimeframeUnit,
                             @Conjunction, @Description, NOW());
                            SELECT LAST_INSERT_ID();";
                using (var conn = GetConnections())
                {
                    response.Result = conn.ExecuteScalar<int>(sql, condition);
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<int> UpdateRule(StmRuleDTO rule)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var sql = @"UPDATE stm_rule SET
                              rule_name        = @RuleName,
                              rule_description = @RuleDescription,
                              sector_id        = @SectorId,
                              risk_rating      = @RiskRating,
                              rule_score       = @RuleScore,
                              logical_operator = @LogicalOperator,
                              action_on_hit    = @ActionOnHit,
                              is_active        = @IsActive,
                              updated_on       = NOW(),
                              updated_by       = @UpdatedBy
                            WHERE id = @Id";
                response.Result = Execute(sql, rule);
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<int> DeleteRuleConditions(int ruleId)
        {
            var response = new ServiceResponse<int>();
            try
            {
                response.Result = Execute("DELETE FROM stm_rule_condition WHERE rule_id = @RuleId", new { RuleId = ruleId });
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<int> ToggleRuleStatus(int ruleId, int isActive, int updatedBy)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var sql = "UPDATE stm_rule SET is_active = @IsActive, updated_by = @UpdatedBy, updated_on = NOW() WHERE id = @Id";
                response.Result = Execute(sql, new { IsActive = isActive, UpdatedBy = updatedBy, Id = ruleId });
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<List<StmRuleDTO>> GetRules(int clientId, string sectorCode = null)
        {
            var response = new ServiceResponse<List<StmRuleDTO>>();
            try
            {
                // Rules are visible if:
                //  - they belong to a sector the client has access to, AND
                //  - they are either a system rule (client_id = 0) or owned by this client.
                var sql = @"SELECT r.*, s.sector_name AS sector_name, s.sector_code AS sector_code
                            FROM stm_rule r
                            INNER JOIN stm_sector s ON s.id = r.sector_id
                            WHERE (r.client_id = @ClientId OR r.client_id = 0)
                              AND (@SectorCode IS NULL OR s.sector_code = @SectorCode)
                              AND (
                                   EXISTS (SELECT 1 FROM stm_client_sector_access csa
                                           WHERE csa.client_id = @ClientId AND csa.is_master = 1)
                                OR EXISTS (SELECT 1 FROM stm_client_sector_access csa
                                           WHERE csa.client_id = @ClientId AND csa.sector_id = r.sector_id)
                              )
                            ORDER BY r.created_on DESC";
                response.Result = Get<StmRuleDTO>(sql, new { ClientId = clientId, SectorCode = sectorCode }).ToList();
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<StmRuleDTO> GetRuleById(int ruleId)
        {
            var response = new ServiceResponse<StmRuleDTO>();
            try
            {
                var sql = @"SELECT r.*, s.sector_name AS sector_name, s.sector_code AS sector_code
                            FROM stm_rule r
                            INNER JOIN stm_sector s ON s.id = r.sector_id
                            WHERE r.id = @Id LIMIT 1";
                response.Result = GetFirstOrDefault<StmRuleDTO>(sql, new { Id = ruleId });
                if (response.Result != null)
                {
                    response.Result.Conditions = GetRuleConditions(ruleId).Result;
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<List<StmRuleConditionDTO>> GetRuleConditions(int ruleId)
        {
            var response = new ServiceResponse<List<StmRuleConditionDTO>>();
            try
            {
                var sql = "SELECT * FROM stm_rule_condition WHERE rule_id = @RuleId ORDER BY sequence_no";
                response.Result = Get<StmRuleConditionDTO>(sql, new { RuleId = ruleId }).ToList();
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        // ==================================================================
        // TRANSACTIONS
        // ==================================================================
        public ServiceResponse<int> InsertTransaction(StmTransactionDTO tran)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var sql = @"INSERT INTO stm_transaction
                            (tran_ref_no, sector_id, tran_date, tran_type, tran_mode, delivery_channel,
                             product, amount, currency, customer_id, customer_name, customer_type, customer_nationality,
                             remitter_id, remitter_name, remitter_country, beneficiary_id, beneficiary_name, beneficiary_country,
                             policy_no, policy_inception_date, property_ref, property_value, property_purchase_date, purpose, branch_code,
                             is_high_risk_country, is_high_risk_customer, has_pep, is_multi_party,
                             rule_hit_status, client_id, created_on, created_by)
                            VALUES (@TranRefNo, @SectorId, @TranDate, @TranType, @TranMode, @DeliveryChannel,
                             @Product, @Amount, @Currency, @CustomerId, @CustomerName, @CustomerType, @CustomerNationality,
                             @RemitterId, @RemitterName, @RemitterCountry, @BeneficiaryId, @BeneficiaryName, @BeneficiaryCountry,
                             @PolicyNo, @PolicyInceptionDate, @PropertyRef, @PropertyValue, @PropertyPurchaseDate, @Purpose, @BranchCode,
                             @IsHighRiskCountry, @IsHighRiskCustomer, @HasPep, @IsMultiParty,
                             @RuleHitStatus, @ClientId, NOW(), @CreatedBy);
                            SELECT LAST_INSERT_ID();";
                using (var conn = GetConnections())
                {
                    response.Result = conn.ExecuteScalar<int>(sql, tran);
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<int> InsertTransactionParty(StmTransactionPartyDTO party)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var sql = @"INSERT INTO stm_transaction_party
                            (transaction_id, party_role, customer_id, customer_master_id, customer_name,
                             nationality, customer_type, id_type, id_number, mobile, address,
                             relation_to_primary, share_percentage, created_on)
                            VALUES (@TransactionId, @PartyRole, @CustomerId, @CustomerMasterId, @CustomerName,
                             @Nationality, @CustomerType, @IdType, @IdNumber, @Mobile, @Address,
                             @RelationToPrimary, @SharePercentage, NOW());
                            SELECT LAST_INSERT_ID();";
                using (var conn = GetConnections())
                {
                    response.Result = conn.ExecuteScalar<int>(sql, party);
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<int> UpdateTransactionRuleStatus(int transactionId, string status, int updatedBy)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var sql = "UPDATE stm_transaction SET rule_hit_status = @Status, updated_by = @UpdatedBy, updated_on = NOW() WHERE id = @Id";
                response.Result = Execute(sql, new { Status = status, UpdatedBy = updatedBy, Id = transactionId });
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<List<StmTransactionDTO>> SearchTransactions(StmTransactionSearchDTO search)
        {
            var response = new ServiceResponse<List<StmTransactionDTO>>();
            try
            {
                // Tenant filter: client must own the transaction AND have access to its sector.
                var sql = @"SELECT t.*, s.sector_name AS sector_name
                            FROM stm_transaction t
                            INNER JOIN stm_sector s ON s.id = t.sector_id
                            WHERE t.client_id = @ClientId
                              AND (@SectorCode IS NULL OR s.sector_code = @SectorCode)
                              AND (@CustomerId IS NULL OR t.customer_id = @CustomerId)
                              AND (@TranRefNo IS NULL OR t.tran_ref_no LIKE CONCAT('%', @TranRefNo, '%'))
                              AND (@FromDate IS NULL OR t.tran_date >= @FromDate)
                              AND (@ToDate IS NULL OR t.tran_date <= @ToDate)
                              AND (@TranType IS NULL OR t.tran_type = @TranType)
                              AND (@RuleHitStatus IS NULL OR t.rule_hit_status = @RuleHitStatus)
                              AND (
                                   EXISTS (SELECT 1 FROM stm_client_sector_access csa
                                           WHERE csa.client_id = @ClientId AND csa.is_master = 1)
                                OR EXISTS (SELECT 1 FROM stm_client_sector_access csa
                                           WHERE csa.client_id = @ClientId AND csa.sector_id = t.sector_id)
                              )
                            ORDER BY t.tran_date DESC";
                response.Result = Get<StmTransactionDTO>(sql, search).ToList();
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<StmTransactionDTO> GetTransactionById(int id)
        {
            var response = new ServiceResponse<StmTransactionDTO>();
            try
            {
                var sql = @"SELECT t.*, s.sector_name AS sector_name
                            FROM stm_transaction t
                            INNER JOIN stm_sector s ON s.id = t.sector_id
                            WHERE t.id = @Id LIMIT 1";
                response.Result = GetFirstOrDefault<StmTransactionDTO>(sql, new { Id = id });
                if (response.Result != null)
                {
                    response.Result.Parties = GetTransactionParties(id).Result;
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<StmTransactionDTO> GetTransactionByRefNo(string refNo, int clientId)
        {
            var response = new ServiceResponse<StmTransactionDTO>();
            try
            {
                var sql = @"SELECT t.*, s.sector_name AS sector_name
                            FROM stm_transaction t
                            INNER JOIN stm_sector s ON s.id = t.sector_id
                            WHERE t.tran_ref_no = @RefNo AND t.client_id = @ClientId LIMIT 1";
                response.Result = GetFirstOrDefault<StmTransactionDTO>(sql, new { RefNo = refNo, ClientId = clientId });
                if (response.Result != null)
                {
                    response.Result.Parties = GetTransactionParties(response.Result.Id).Result;
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<List<StmTransactionPartyDTO>> GetTransactionParties(int transactionId)
        {
            var response = new ServiceResponse<List<StmTransactionPartyDTO>>();
            try
            {
                var sql = "SELECT * FROM stm_transaction_party WHERE transaction_id = @TranId";
                response.Result = Get<StmTransactionPartyDTO>(sql, new { TranId = transactionId }).ToList();
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<List<StmTransactionDTO>> GetCustomerTransactions(string customerId, int clientId)
        {
            var response = new ServiceResponse<List<StmTransactionDTO>>();
            try
            {
                var sql = @"SELECT t.*, s.sector_name AS sector_name
                            FROM stm_transaction t
                            INNER JOIN stm_sector s ON s.id = t.sector_id
                            WHERE t.client_id = @ClientId AND
                            (t.customer_id = @CustomerId OR t.id IN
                             (SELECT transaction_id FROM stm_transaction_party WHERE customer_id = @CustomerId))
                              AND (
                                   EXISTS (SELECT 1 FROM stm_client_sector_access csa
                                           WHERE csa.client_id = @ClientId AND csa.is_master = 1)
                                OR EXISTS (SELECT 1 FROM stm_client_sector_access csa
                                           WHERE csa.client_id = @ClientId AND csa.sector_id = t.sector_id)
                              )
                            ORDER BY t.tran_date DESC";
                response.Result = Get<StmTransactionDTO>(sql, new { CustomerId = customerId, ClientId = clientId }).ToList();
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        // ==================================================================
        // CUSTOMER LOOKUP (search existing onboarded customers)
        // ==================================================================
        public ServiceResponse<List<dynamic>> SearchCustomers(string query, int clientId)
        {
            var response = new ServiceResponse<List<dynamic>>();
            try
            {
                // customermaster uses `cust_ref_id` as the business key and `Client_Id` (capital).
                var sql = @"SELECT
                              cust_ref_id        AS CustomerId,
                              id                 AS CustomerMasterId,
                              CONCAT_WS(' ', fname, mname, lname) AS CustomerName,
                              cust_type          AS CustomerType,
                              nationality        AS Nationality,
                              mobile             AS Mobile,
                              cust_id_type       AS IdType,
                              cust_id_number     AS IdNumber
                            FROM customermaster
                            WHERE Client_Id = @ClientId
                              AND (is_deleted IS NULL OR is_deleted = 0)
                              AND (@Q IS NULL OR @Q = ''
                                   OR cust_ref_id    LIKE CONCAT('%', @Q, '%')
                                   OR fname          LIKE CONCAT('%', @Q, '%')
                                   OR mname          LIKE CONCAT('%', @Q, '%')
                                   OR lname          LIKE CONCAT('%', @Q, '%')
                                   OR mobile         LIKE CONCAT('%', @Q, '%')
                                   OR cust_id_number LIKE CONCAT('%', @Q, '%'))
                            ORDER BY fname LIMIT 50";
                using (var conn = GetConnections())
                {
                    response.Result = conn.Query(sql, new { ClientId = clientId, Q = query ?? string.Empty })
                                          .Select(r => (dynamic)r).ToList();
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<dynamic> GetCustomerDetails(string customerId, int clientId)
        {
            var response = new ServiceResponse<dynamic>();
            try
            {
                var sql = @"SELECT
                              cust_ref_id          AS CustomerId,
                              cust_ref_id          AS CustomerRefId,
                              id                   AS CustomerMasterId,
                              CONCAT_WS(' ', fname, mname, lname) AS CustomerName,
                              fname                AS FirstName,
                              mname                AS MiddleName,
                              lname                AS LastName,
                              cust_type            AS CustomerType,
                              nationality          AS Nationality,
                              dob                  AS DOB,
                              cust_id_type         AS IdType,
                              cust_id_number       AS IdNumber,
                              PassportId           AS PassportId,
                              EmiratesIdNumber     AS EmiratesIdNumber,
                              mobile               AS Mobile,
                              residence_status     AS ResidenceStatus,
                              profession           AS Profession,
                              employer             AS Employer,
                              employerindustry     AS EmployerIndustry,
                              employersector       AS EmployerSector,
                              delivery_channel     AS DeliveryChannel,
                              mode_of_payment      AS PaymentMode,
                              customer_final_risk_score AS RiskScore,
                              tradelicense         AS TradeLicense,
                              cif_number           AS CIFNumber
                            FROM customermaster
                            WHERE cust_ref_id = @CustomerId AND Client_Id = @ClientId
                            LIMIT 1";
                using (var conn = GetConnections())
                {
                    response.Result = conn.QueryFirstOrDefault(sql, new { CustomerId = customerId, ClientId = clientId });
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        // ==================================================================
        // CASES
        // ==================================================================
        public ServiceResponse<int> InsertCase(StmCaseDTO @case)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var sql = @"INSERT INTO stm_case
                            (case_ref_no, transaction_id, sector_id, rules_violated, rule_names,
                             total_risk_score, risk_rating, status, assigned_to, client_id, created_on, created_by)
                            VALUES (@CaseRefNo, @TransactionId, @SectorId, @RulesViolated, @RuleNames,
                             @TotalRiskScore, @RiskRating, @Status, @AssignedTo, @ClientId, NOW(), @CreatedBy);
                            SELECT LAST_INSERT_ID();";
                using (var conn = GetConnections())
                {
                    response.Result = conn.ExecuteScalar<int>(sql, @case);
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<int> UpdateCase(StmCaseDTO @case)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var sql = @"UPDATE stm_case SET
                              status          = @Status,
                              assigned_to     = @AssignedTo,
                              reviewed_by     = @ReviewedBy,
                              reviewed_on     = @ReviewedOn,
                              review_decision = @ReviewDecision,
                              review_remarks  = @ReviewRemarks,
                              updated_on      = NOW(),
                              updated_by      = @UpdatedBy
                            WHERE id = @Id";
                response.Result = Execute(sql, @case);
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<int> InsertCaseComment(StmCaseCommentDTO comment)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var sql = @"INSERT INTO stm_case_comment (case_id, comment_text, action_type, created_on, created_by, created_user)
                            VALUES (@CaseId, @CommentText, @ActionType, NOW(), @CreatedBy, @CreatedUser);
                            SELECT LAST_INSERT_ID();";
                using (var conn = GetConnections())
                {
                    response.Result = conn.ExecuteScalar<int>(sql, comment);
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<List<StmCaseDTO>> SearchCases(StmCaseSearchDTO search)
        {
            var response = new ServiceResponse<List<StmCaseDTO>>();
            try
            {
                var sql = @"SELECT c.*, t.tran_ref_no AS tran_ref_no, t.tran_date AS tran_date,
                                   t.customer_id AS customer_id, t.customer_name AS customer_name,
                                   t.amount AS amount, t.currency AS currency,
                                   s.sector_name AS sector_name, s.sector_code AS sector_code
                            FROM stm_case c
                            INNER JOIN stm_transaction t ON t.id = c.transaction_id
                            INNER JOIN stm_sector s ON s.id = c.sector_id
                            WHERE c.client_id = @ClientId
                              AND (@SectorCode IS NULL OR s.sector_code = @SectorCode)
                              AND (@Status IS NULL OR c.status = @Status)
                              AND (@CustomerId IS NULL OR t.customer_id = @CustomerId)
                              AND (@CustomerName IS NULL OR t.customer_name LIKE CONCAT('%', @CustomerName, '%'))
                              AND (@FromDate IS NULL OR c.created_on >= @FromDate)
                              AND (@ToDate IS NULL OR c.created_on <= @ToDate)
                              AND (@RiskRating IS NULL OR c.risk_rating = @RiskRating)
                              AND (
                                   EXISTS (SELECT 1 FROM stm_client_sector_access csa
                                           WHERE csa.client_id = @ClientId AND csa.is_master = 1)
                                OR EXISTS (SELECT 1 FROM stm_client_sector_access csa
                                           WHERE csa.client_id = @ClientId AND csa.sector_id = c.sector_id)
                              )
                            ORDER BY c.created_on DESC";
                response.Result = Get<StmCaseDTO>(sql, search).ToList();
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<StmCaseDTO> GetCaseById(int id)
        {
            var response = new ServiceResponse<StmCaseDTO>();
            try
            {
                var sql = @"SELECT c.*, t.tran_ref_no AS tran_ref_no, t.tran_date AS tran_date,
                                   t.customer_id AS customer_id, t.customer_name AS customer_name,
                                   t.amount AS amount, t.currency AS currency,
                                   s.sector_name AS sector_name, s.sector_code AS sector_code
                            FROM stm_case c
                            INNER JOIN stm_transaction t ON t.id = c.transaction_id
                            INNER JOIN stm_sector s ON s.id = c.sector_id
                            WHERE c.id = @Id LIMIT 1";
                response.Result = GetFirstOrDefault<StmCaseDTO>(sql, new { Id = id });
                if (response.Result != null)
                {
                    response.Result.Comments = GetCaseComments(id).Result;
                    response.Result.Transaction = GetTransactionById(response.Result.TransactionId).Result;
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<List<StmCaseCommentDTO>> GetCaseComments(int caseId)
        {
            var response = new ServiceResponse<List<StmCaseCommentDTO>>();
            try
            {
                var sql = "SELECT * FROM stm_case_comment WHERE case_id = @CaseId ORDER BY created_on DESC";
                response.Result = Get<StmCaseCommentDTO>(sql, new { CaseId = caseId }).ToList();
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<bool> IsStmModuleEnabled(int clientId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                // Menu_Id = 12 is "Transaction Monitoring" (see menu_master).
                // Active row in client_right_master means the module is granted.
                var sql = @"SELECT COUNT(*) FROM client_right_master
                            WHERE Client_Id = @ClientId AND Menu_Id = 12 AND is_Active = 1";
                int count;
                using (var conn = GetConnections())
                {
                    count = conn.ExecuteScalar<int>(sql, new { ClientId = clientId });
                }
                response.Result = count > 0;
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<List<string>> GetAllowedSectorCodes(int clientId)
        {
            var response = new ServiceResponse<List<string>>();
            try
            {
                // Use the same logic as GetSectors: master client sees every active sector,
                // otherwise filter by stm_client_sector_access rows.
                var sql = @"SELECT s.sector_code FROM stm_sector s
                            WHERE s.is_active = 1
                              AND (
                                   EXISTS (SELECT 1 FROM stm_client_sector_access csa
                                           WHERE csa.client_id = @ClientId AND csa.is_master = 1)
                                OR EXISTS (SELECT 1 FROM stm_client_sector_access csa
                                           WHERE csa.client_id = @ClientId AND csa.sector_id = s.id)
                              )";
                response.Result = Get<string>(sql, new { ClientId = clientId }).ToList();
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
                response.Result = new List<string>();
            }
            return response;
        }

        public ServiceResponse<int> InsertRuleExecLog(StmRuleExecLogDTO log)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var sql = @"INSERT INTO stm_rule_exec_log (transaction_id, rule_id, is_hit, hit_details, executed_on)
                            VALUES (@TransactionId, @RuleId, @IsHit, @HitDetails, NOW()); SELECT LAST_INSERT_ID();";
                using (var conn = GetConnections())
                {
                    response.Result = conn.ExecuteScalar<int>(sql, log);
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        // ==================================================================
        // TRANSACTION RISK
        // ==================================================================
        public ServiceResponse<List<StmTxnRiskFactorDTO>> GetActiveRiskFactors(int sectorId, int clientId)
        {
            var response = new ServiceResponse<List<StmTxnRiskFactorDTO>>();
            try
            {
                // System factors (client_id = 0) are available to every tenant;
                // client-specific overrides also returned if present.
                var sql = @"SELECT f.*, s.sector_code AS sector_code, s.sector_name AS sector_name
                            FROM stm_txn_risk_factor f
                            INNER JOIN stm_sector s ON s.id = f.sector_id
                            WHERE f.sector_id = @SectorId
                              AND f.is_active = 1
                              AND (f.client_id = @ClientId OR f.client_id = 0)
                            ORDER BY f.sequence_no, f.id";
                response.Result = Get<StmTxnRiskFactorDTO>(sql, new { SectorId = sectorId, ClientId = clientId }).ToList();

                foreach (var f in response.Result)
                {
                    f.Bands = GetBandsForFactor(f.Id).Result;
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<List<StmTxnRiskBandDTO>> GetBandsForFactor(int factorId)
        {
            var response = new ServiceResponse<List<StmTxnRiskBandDTO>>();
            try
            {
                var sql = "SELECT * FROM stm_txn_risk_band WHERE factor_id = @FactorId ORDER BY sequence_no, id";
                response.Result = Get<StmTxnRiskBandDTO>(sql, new { FactorId = factorId }).ToList();
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<List<StmTxnRiskFactorDTO>> GetAllRiskFactors(int sectorId, int clientId)
        {
            // Same as GetActiveRiskFactors but includes inactive (for edit screens)
            var response = new ServiceResponse<List<StmTxnRiskFactorDTO>>();
            try
            {
                var sql = @"SELECT f.*, s.sector_code AS sector_code, s.sector_name AS sector_name
                            FROM stm_txn_risk_factor f
                            INNER JOIN stm_sector s ON s.id = f.sector_id
                            WHERE f.sector_id = @SectorId
                              AND (f.client_id = @ClientId OR f.client_id = 0)
                            ORDER BY f.sequence_no, f.id";
                response.Result = Get<StmTxnRiskFactorDTO>(sql, new { SectorId = sectorId, ClientId = clientId }).ToList();
                foreach (var f in response.Result)
                {
                    f.Bands = GetBandsForFactor(f.Id).Result;
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<StmTxnRiskFactorDTO> GetRiskFactorById(int id)
        {
            var response = new ServiceResponse<StmTxnRiskFactorDTO>();
            try
            {
                var sql = @"SELECT f.*, s.sector_code AS sector_code, s.sector_name AS sector_name
                            FROM stm_txn_risk_factor f
                            INNER JOIN stm_sector s ON s.id = f.sector_id
                            WHERE f.id = @Id LIMIT 1";
                response.Result = GetFirstOrDefault<StmTxnRiskFactorDTO>(sql, new { Id = id });
                if (response.Result != null)
                {
                    response.Result.Bands = GetBandsForFactor(id).Result;
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<int> InsertRiskFactor(StmTxnRiskFactorDTO factor)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var sql = @"INSERT INTO stm_txn_risk_factor
                              (factor_code, factor_name, description, sector_id, field_name,
                               factor_type, weight, is_active, sequence_no, client_id, created_on, created_by)
                            VALUES
                              (@FactorCode, @FactorName, @Description, @SectorId, @FieldName,
                               @FactorType, @Weight, @IsActive, @SequenceNo, @ClientId, NOW(), @CreatedBy);
                            SELECT LAST_INSERT_ID();";
                using (var conn = GetConnections())
                {
                    response.Result = conn.ExecuteScalar<int>(sql, factor);
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<int> UpdateRiskFactor(StmTxnRiskFactorDTO factor)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var sql = @"UPDATE stm_txn_risk_factor SET
                              factor_code = @FactorCode,
                              factor_name = @FactorName,
                              description = @Description,
                              sector_id   = @SectorId,
                              field_name  = @FieldName,
                              factor_type = @FactorType,
                              weight      = @Weight,
                              is_active   = @IsActive,
                              sequence_no = @SequenceNo
                            WHERE id = @Id";
                response.Result = Execute(sql, factor);
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<int> DeleteRiskFactor(int factorId)
        {
            var response = new ServiceResponse<int>();
            try
            {
                // ON DELETE CASCADE on stm_txn_risk_band will also clear bands.
                response.Result = Execute("DELETE FROM stm_txn_risk_factor WHERE id = @Id", new { Id = factorId });
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<int> InsertRiskBand(StmTxnRiskBandDTO band)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var sql = @"INSERT INTO stm_txn_risk_band
                              (factor_id, band_label, numeric_min, numeric_max, match_value,
                               match_in_list, score, rating, sequence_no)
                            VALUES
                              (@FactorId, @BandLabel, @NumericMin, @NumericMax, @MatchValue,
                               @MatchInList, @Score, @Rating, @SequenceNo);
                            SELECT LAST_INSERT_ID();";
                using (var conn = GetConnections())
                {
                    response.Result = conn.ExecuteScalar<int>(sql, band);
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<int> DeleteBandsForFactor(int factorId)
        {
            var response = new ServiceResponse<int>();
            try
            {
                response.Result = Execute("DELETE FROM stm_txn_risk_band WHERE factor_id = @Id", new { Id = factorId });
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<int> SaveRiskResult(StmTxnRiskResultDTO result)
        {
            var response = new ServiceResponse<int>();
            try
            {
                // One result per transaction. Replace on re-evaluation.
                var sql = @"INSERT INTO stm_txn_risk_result
                              (transaction_id, sector_id, total_score, risk_rating, factor_count, factor_breakdown, client_id, computed_on)
                            VALUES
                              (@TransactionId, @SectorId, @TotalScore, @RiskRating, @FactorCount, @FactorBreakdown, @ClientId, NOW())
                            ON DUPLICATE KEY UPDATE
                              total_score = VALUES(total_score),
                              risk_rating = VALUES(risk_rating),
                              factor_count = VALUES(factor_count),
                              factor_breakdown = VALUES(factor_breakdown),
                              computed_on = NOW();
                            SELECT LAST_INSERT_ID();";
                using (var conn = GetConnections())
                {
                    response.Result = conn.ExecuteScalar<int>(sql, result);
                }
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }

        public ServiceResponse<StmTxnRiskResultDTO> GetRiskResultForTransaction(int transactionId)
        {
            var response = new ServiceResponse<StmTxnRiskResultDTO>();
            try
            {
                var sql = "SELECT * FROM stm_txn_risk_result WHERE transaction_id = @Id LIMIT 1";
                response.Result = GetFirstOrDefault<StmTxnRiskResultDTO>(sql, new { Id = transactionId });
                response.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                response.Status = StaticResource.FailStatusCode;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
