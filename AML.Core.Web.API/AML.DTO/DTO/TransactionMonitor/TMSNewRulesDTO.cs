using System;
using AML.Core.Common.StaticResource;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AML.DTO.DTO.TransactionMonitor
{
    public class TMSRulesMasterDTO
    {
        [Column("tms_rm_id")]
        public int TMSRuleID { get; set; }

        [Column("tms_rm_rulename")]
        public string TMSRuleName { get; set; }
        [Column("tms_rm_rulescore")]
        public int TMSRuleScore { get; set; }
        [Column("tms_rm_detail_string")]
        public string TMSRuleDetailString { get; set; }

        [Column("tms_rd_source")]
        public string TMSRuleDetSource { get; set; }

        [Column("tms_rm_isactive")]
        public int TMSRuleIsActive { get; set; }
        [Column("tms_rm_createdby")]
        public int TMSRuleCreatedBy { get; set; }
        [Column("tms_rm_createdon")]
        public DateTime? RuleCreatedOnDB { get; set; }

        public string TMSRuleCreatedOn
        {
            get
            {
                return RuleCreatedOnDB.ToUIDDateFormat();
            }
            set
            {
                RuleCreatedOnDB = value.ParseDB();
            }
        }
        [Column("tms_rm_updatedby")]
        public int TMSRuleUpdatedBy { get; set; }
        [Column("tms_rm_updatedon")]
        public DateTime? RuleUpdatedOnDB { get; set; }

        public string TMSRuleUpdatedOn
        {
            get
            {
                return RuleUpdatedOnDB.ToUIDDateFormat();
            }
            set
            {
                RuleUpdatedOnDB = value.ParseDB();
            }
        }

        [Column("tms_rules_rand_id")]
        public int TMSRuleRandID { get; set; }

        [Column("tms_rm_descriptions")]
        public string TMSRuleDescription { get; set; }

        [Column("tms_rm_outergrpoperator")]
        public string TMSRuleOuterGrpoperator { get; set; }

        public List<TMSRulesDetailsDTO> TMSRuleParameters { get; set; }

        [Column("Client_Id")]
        public int Client_Id { get; set; }

    }


    public class TMSRulesDetailsDTO
    {
        [Column("tms_rd_id")]
        public int TMSRuleDetID { get; set; }

        [Column("tms_rd_autoid")]
        public string TMSRuleDetAutoID { get; set; }
        [Column("tms_rd_masterid")]
        public int TMSRuleDetMasterID { get; set; }
        [Column("tms_rd_paramtype")]
        public string TMSRuleDetParamType { get; set; }

        [Column("tms_rd_source")]
        public string TMSRuleDetSource { get; set; }

        [Column("tms_rd_sourceaggregate")]
        public string TMSRuleDetSourceAggr { get; set; }
        [Column("tms_rd_operator")]
        public string TMSRuleDetOperator { get; set; }

        [Column("tms_rd_target")]
        public string TMSRuleDetTarget { get; set; }
        [Column("tms_rd_targetaggregate")]
        public string TMSRuleDetTargetAggr { get; set; }
        [Column("tms_rd_value")]
        public dynamic TMSRuleDetValue { get; set; }
        [Column("tms_rd_timeframesource")]
        public string TMSRuleDetTimeFrameSource { get; set; }
        [Column("tms_rd_timeframevalue")]
        public int TMSRuleDetTimeFrameVal { get; set; }
        [Column("tms_rd_timeframetype")]
        public string TMSRuleDetTimeFrameType { get; set; }
        [Column("tms_rd_compareto")]
        public string TMSRuleDetCompareTo { get; set; }
        [Column("tms_rd_comparevalue")]
        public string TMSRuleDetCompareValue { get; set; }

        [Column("tms_rd_comparefield")]
        public string TMSRuleDetCompareField { get; set; }

        [Column("tms_rd_comparefieldaggregate")]
        public string TMSRuleDetCompareFieldAggr { get; set; }
        [Column("tms_rd_comparetimestartvalue")]
        public int TMSRuleDetCompareTimeStart { get; set; }
        [Column("tms_rd_comparetimestarttype")]
        public string TMSRuleDetCompareTimeStartType { get; set; }
        [Column("tms_rd_comparetimeendvalue")]
        public int TMSRuleDetCompareTimeEnd { get; set; }
        [Column("tms_rd_comparetimeendtype")]
        public string TMSRuleDetCompareTimeEndType { get; set; }
        [Column("tms_rd_forsame")]
        public string TMSRuleDetForSame { get; set; }

        [Column("tms_rd_paramoperatorgroup")]
        public string TMSOperatorGRP { get; set; }

       

        [Column("p_tms_rd_status")]
        public int TMSRuleDetStatus { get; set; }
        [Column("p_tms_rd_createdby")]
        public int TMSRuleDetCreatedBy { get; set; }
        [Column("p_tms_rd_createdon")]
        public DateTime? CreatedDB  { get; set; }

        public string TMSRuleDetCreatedOn
        {
            get
            {
                return CreatedDB.ToUIDDateFormat();
            }
            set
            {
                CreatedDB = value.ParseDB();
            }
        }
            [Column("p_tms_rd_updatedby")]
        public int TMSRuleDetUpdatedBy { get; set; }
        [Column("p_tms_rd_updatedon")]
        public DateTime? UpdatedDB { get; set; }

        public string TMSRuleDetUpdatedOn
        {
            get
            {
                return UpdatedDB.ToUIDDateFormat();
            }
            set
            {
                UpdatedDB = value.ParseDB();
            }
        }

        [Column("tms_rd_descriptions")]
        public string TMSRuleDetDescription { get; set; }
        public bool TMSRuleDetCompareIsPercent { get; set; }

        [Column("tms_rd_percentage")]
        public int TMSRuleDetPercentValue { get; set; }
        [Column("tms_rd_percentage")]
        public dynamic TMSRuleDetDynamicCompareValue { get; set; }
        [Column("tms_rd_if_source")]
        public string TMSRuleDetSourceIfSource { get; set; }
        [Column("tms_rd_if_source_aggregate")]
        public string TMSRuleDetSourceIfSourceAggr { get; set; }
        [Column("tms_rd_if_target")]
        public string TMSRuleDetCompareFieldIfSource { get; set; }
        [Column("tms_rd_if_target_aggregate")]
        public string TMSRuleDetCompareFieldIfSourceAggr { get; set; }
        [Column("tms_rd_if_source_value")]
        public string TMSRuleDetSourceIfValue { get; set; }
        [Column("tms_rd_if_target_value")]
        public string TMSRuleDetCompareFieldIfValue { get; set; }
        [Column("tms_rd_if_source_operator")]
        public string TMSRuleDetSourceIfOperator { get; set; }
        [Column("tms_rd_if_target_operator")]
        public string TMSRuleDetCompareFieldIfOperator { get; set; }

        [Column("Client_Id")]
        public int Client_Id { get; set; }

        [Column("tms_rd_checker_string")]
        public string TMSRuleDetCustomCheckerString { get; set; }
        [Column("tms_rd_getter_string")]
        public string TMSRuleDetCustomGetterString { get; set; }
    }

    public class TMSNewRulesNamesDTO
    {
        [Column("tms_rm_rulename")]
        public string TMSRuleName { get; set; }

        [Column("tms_rm_id")]
        public int TMSRuleID { get; set; } 

    }

    public class TMSFieldsmodelDTO
    {
        [Column("tms_header_id")]
        public int TMSHeaderId { get; set; }

        [Column("tms_header_name")]
        public string TMSHeaderName { get; set; }

        [Column("tms_header_map_screenname")]
        public string TMSHeaderMapScreenname { get; set; }


    }

    public class TMSCustomerTypeModelDTO
    {

        [Column("id")]
        public int id { get; set; }

        [Column("tms_custtypeId")]
        public string TMSCustTypeId { get; set; }

        [Column("tms_custtype")]
        public string TMSCustType { get; set; }


    }

    public class TMSTranTypeDTO 
    {
        [Column("id")]
        public int id { get; set; }

        [Column("trantype_name")]
        public string TranTypeName { get; set; }

        [Column("trantype_code")]
        public string TranTypeCode { get; set; }
    }


    public class TMSDraftLogModelDTO
    {
        [Column("id")]
        public int id { get; set; }

        [Column("tms_rule_id")]
        public int TMSRuleId { get; set; }

        [Column("tms_rule_name")]
        public string TMSRuleName { get; set; }

        [Column("tms_score")]
        public int TMSScore { get; set; }

        [Column("tms_rule_description")]
        public string TMSRuleDescription { get; set; }

        [Column("tms_rule_parameter")]
        public string TMSRuleparameter { get; set; }

        [Column("tms_ruleparameter_description")]
        public string TMSRuleParameterDescription { get; set; }

        [Column("tms_rule_operator")]
        public string TMSRuleOperator { get; set; }

        [Column("tms_result")]
        public byte[] TMSResult { get; set; }

        [Column("tms_createdby")]
        public int TMSCreatedBy { get; set; }
        [Column("tms_createdby")]
        public int client_id { get; set; }
    }


    
}
