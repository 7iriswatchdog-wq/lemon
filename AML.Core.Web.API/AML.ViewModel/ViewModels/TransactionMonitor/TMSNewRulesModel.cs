using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.ViewModel.ViewModels.TransactionMonitor
{
    public class TMSNewRulesModel
    {
        public int TMSRuleID { get; set; }
        public string TMSRuleName { get; set; }
        public int TMSRuleScore { get; set; }
        public string TMSRuleDetailString { get; set; }
        public string TMSRuleDetSource { get; set; }
        public int TMSRuleIsActive { get; set; }
        public int TMSRuleCreatedBy { get; set; }
        public DateTime? TMSRuleCreatedOn { get; set; }

        
        public int TMSRuleUpdatedBy { get; set; }
        public string TMSRuleUpdatedOn { get; set; }

        public int TMSRuleRandID { get; set; }

        public string TMSRuleDescription {get;set;}

        public string TMSRuleOuterGrpoperator { get; set; }

        public List<TMSRulesDetailsModel> TMSRuleParameters { get; set; }
        public SelectList TMSRuleNames { get; set; }

        public SelectList TMSRuleIds { get; set; }


        public SelectList TMSHeaderIds { get; set; }

        public SelectList TMSHeaderMapScreennames { get; set; }

        public SelectList TMSHeaderName { get; set; }

        public SelectList TMSCustType { get; set; }

        public SelectList TMSTranType { get; set; }

        public int Client_Id { get; set; }

    }

    public class TMSRulesDetailsModel
    {
        public int TMSRuleDetID { get; set; }
        public string TMSRuleDetAutoID { get; set; }
        public int TMSRuleDetMasterID { get; set; }
        public string TMSRuleDetParamType { get; set; }
        public string TMSRuleDetSource { get; set; }
        public string TMSRuleDetSourceAggr { get; set; }
        public string TMSRuleDetOperator { get; set; }
        public string TMSRuleDetTarget { get; set; }
        public string TMSRuleDetTargetAggr { get; set; }
        public string TMSRuleDetValue { get; set; }
        public string TMSRuleDetTimeFrameSource { get; set; }
        public int TMSRuleDetTimeFrameVal { get; set; }
        public string TMSRuleDetTimeFrameType { get; set; }
        public string TMSRuleDetCompareTo { get; set; }
        public string TMSRuleDetCompareValue { get; set; }
        public dynamic TMSRuleDetDynamicCompareValue { get; set; }
        public string TMSRuleDetCompareField { get; set; }
        public string TMSRuleDetCompareFieldAggr { get; set; }
        public int TMSRuleDetCompareTimeStart { get; set; }
        public string TMSRuleDetCompareTimeStartType { get; set; }
        public int TMSRuleDetCompareTimeEnd { get; set; }
        public string TMSRuleDetCompareTimeEndType { get; set; }
        public string TMSRuleDetForSame { get; set; }

        public string TMSOperatorGRP { get; set; }
        public int TMSRuleDetStatus { get; set; }
        public int TMSRuleDetCreatedBy { get; set; }
        public string TMSRuleDetCreatedOn { get; set; }
        public int TMSRuleDetUpdatedBy { get; set; }
        public string TMSRuleDetUpdatedOn { get; set; }

        public string TMSRuleDetDescription { get; set; }
        public bool TMSRuleDetCompareIsPercent { get; set; }
        public int TMSRuleDetPercentValue { get; set; }
        public string TMSRuleDetSourceIfSource { get; set; }
        public string TMSRuleDetSourceIfSourceAggr { get; set; }
        public string TMSRuleDetCompareFieldIfSource { get; set; }
        public string TMSRuleDetCompareFieldIfSourceAggr { get; set; }
        public string TMSRuleDetSourceIfValue { get; set; }
        public string TMSRuleDetCompareFieldIfValue { get; set; }
        public string TMSRuleDetSourceIfOperator { get; set; }
        public string TMSRuleDetCompareFieldIfOperator { get; set; }

        public int Client_Id { get; set; }

    }

    //public class TMSNewRulesNames
    //{
    //    public SelectList TMSRuleName { get; set; }

    //}
    public class TMSNewRulesNamesModel
    {
        public int TMSRuleID { get; set; }
        public string TMSRuleName { get; set; }


    }


    public class TMSFieldsNamesmodel
    {

        public int TmsHeaderId { get; set; }

        public string TMSHeaderName { get; set; }

        public string TMSHeaderMapScreenname { get; set; }


    }

    public class TMSCustomerTypeModel
    {
        public int id { get;set;}
        public string TMSCustTypeId { get; set; }
        
        public string TMSCustType { get; set; }


    }

    public class TMSTranType 
    {
     public int id { get; set; }
        
     public string TranTypeName { get; set; }

     public string TranTypeCode { get; set; }
    }

    public class TMSDraftLogModel 
    {
        public int id { get; set; }

        public int TMSRuleId{ get; set; }

        public string TMSRuleName  { get; set; }

        public int TMSScore  { get; set; }

        public string TMSRuleDescription  { get; set; }

        public string TMSRuleparameter  { get; set; }

        public string TMSRuleParameterDescription  { get; set; }

        public string TMSRuleOperator { get; set; }

        public byte[] TMSResult { get; set; }

        public int TMSCreatedBy { get; set; }

        public int client_id { get; set; }

        public Dictionary<int, List<TMSDraftLogModel>> valuePairs { get; set; }
        public List<DataListModel> DataList { get; set; }
    }

    public class DataListModel
    {
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
        public byte[] TMSResults { get; set; }

    }

  








}
