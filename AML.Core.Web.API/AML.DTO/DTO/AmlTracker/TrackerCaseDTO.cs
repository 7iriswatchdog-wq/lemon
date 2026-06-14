using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.AmlTracker
{
    public class TrackerCaseDTO
    {
        [Column("Id")] public int CaseId { get; set; }

        
        [Column("CustomerMasterId")] public int CustomerMasterId { get; set; }
        [Column("CustomerCode")] public string CustomerCode { get; set; }
        [Column("FirstName")] public string FirstName { get; set; }
        [Column("MiddleName")] public string MiddleName { get; set; }
        [Column("LastName")] public string LastName { get; set; }
        [Column("CustomerType")] public string CustomerType { get; set; }
        [Column("Nationality")] public string Nationality { get; set; }

        [Column("Status")] public int Status { get; set; }
        [Column("MatchScore")] public int? MatchScore { get; set; }
        [Column("RiskScore")] public int? RiskScore { get; set; }

        [Column("CreatedOn")] public DateTime? CreatedOn { get; set; }
        [Column("UpdatedOn")] public DateTime? UpdatedOn { get; set; }
        [Column("DateOfWhitelisting")] public DateTime? DateOfWhitelisting { get; set; }

        [Column("Source")] public string Source { get; set; }
        [Column("Owner")] public int Owner { get; set; }
        [Column("OwnerName")] public string OwnerName { get; set; }
        [Column("CreatedBy")] public int CreatedBy { get; set; }
        [Column("CreatedUser")] public string CreatedUser { get; set; }
        [Column("UpdatedUser")] public string UpdatedUser { get; set; }
        [Column("IsWhiteListed")] public string IsWhiteListed { get; set; }

        [Column("TrueDomesticPep")] public int? TrueDomesticPep { get; set; }
        [Column("TrueForeignPep")] public int? TrueForeignPep { get; set; }
        [Column("TrueAdverseMedia")] public int? TrueAdverseMedia { get; set; }
        [Column("PartialDomesticPep")] public int? PartialDomesticPep { get; set; }
        [Column("PartialForeignPep")] public int? PartialForeignPep { get; set; }
        [Column("PartialAdverseMedia")] public int? PartialAdverseMedia { get; set; }
        [Column("TrueUaeUnSanction")] public int? TrueUaeUnSanction { get; set; }
        [Column("TrueOtherSanction")] public int? TrueOtherSanction { get; set; }

        [Column("IndividualRiskScore")] public string IndividualRiskScore { get; set; }
        [Column("CorporateRiskScore")] public string CorporateRiskScore { get; set; }

        [Column("QaId")] public int? QaId { get; set; }
        [Column("QaStatus")] public int? QaStatus { get; set; }
        [Column("QaOutcome")] public string QaOutcome { get; set; }
        [Column("QaReasonCode")] public string QaReasonCode { get; set; }
        [Column("QaReviewerId")] public int? QaReviewerId { get; set; }
        [Column("QaReviewerName")] public string QaReviewerName { get; set; }
        [Column("QaReviewedOn")] public DateTime? QaReviewedOn { get; set; }
        [Column("QaComments")] public string QaComments { get; set; }

        // SAR/STR linkage (F8)
        [Column("SarStatus")] public string SarStatus { get; set; }
        [Column("SarReference")] public string SarReference { get; set; }
        [Column("SarFilingDeadline")] public DateTime? SarFilingDeadline { get; set; }
        [Column("SarFiledOn")] public DateTime? SarFiledOn { get; set; }

        // Compliance-export columns (sourced from customermaster)
        [Column("CustomerCreatedOn")] public DateTime? CustomerCreatedOn { get; set; }
        [Column("ClientCifNumber")] public string ClientCifNumber { get; set; }
        [Column("ProductType")] public string ProductType { get; set; }
        [Column("ProductValueAed")] public string ProductValueAed { get; set; }
        [Column("IdType")] public string IdType { get; set; }
        [Column("IdNumber")] public string IdNumber { get; set; }
        [Column("IdExpiry")] public DateTime? IdExpiry { get; set; }
        [Column("PaymentMode")] public string PaymentMode { get; set; }
        [Column("DeliveryChannel")] public string DeliveryChannel { get; set; }
        [Column("ResidentStatus")] public string ResidentStatus { get; set; }
        [Column("Profession")] public string Profession { get; set; }

        // Compliance-export columns (typed comments from case_comment, latest of each type)
        [Column("CommentsPep")] public string CommentsPep { get; set; }
        [Column("CommentsUaeUnsc")] public string CommentsUaeUnsc { get; set; }
        [Column("CommentsAdverseMedia")] public string CommentsAdverseMedia { get; set; }
        [Column("CommentsOtherSanction")] public string CommentsOtherSanction { get; set; }
        [Column("CommentsGeneral")] public string CommentsGeneral { get; set; }

        // Compliance-export columns (derived)
        [Column("RiskAssessmentDate")] public DateTime? RiskAssessmentDate { get; set; }

        // Compliance-export columns (added by 2026-05-02 migration on customercase)
        [Column("PolicyIssueDate")] public DateTime? PolicyIssueDate { get; set; }
        [Column("DateOfResponse")] public DateTime? DateOfResponse { get; set; }
        [Column("ClientProductNumber")] public string ClientProductNumber { get; set; }
        [Column("PolicyHolder")] public string PolicyHolder { get; set; }
        [Column("KycCheck")] public string KycCheck { get; set; }
        [Column("KycCheckComments")] public string KycCheckComments { get; set; }
        [Column("SeniorMgmtApprovalDate")] public DateTime? SeniorMgmtApprovalDate { get; set; }
        [Column("SanctionsScreeningDate")] public DateTime? SanctionsScreeningDate { get; set; }

        [Column("ClientStatus")] public string ClientStatus { get; set; }

        [Column("Gender")] public string Gender { get; set; }


        [Column("dob")] public string DOB { get; set; }

        [Column("DomesticPep")] public string DomesticPep { get; set; }
        [Column("ForeginPep")] public string ForeginPep { get; set; }
        [Column("RedFlags")] public string RedFlags { get; set; }
        [Column("HighNetwork")] public string HighNetwork { get; set; }
        [Column("UAEUNSanction")] public string UAEUNSanction { get; set; }
        [Column("OtherSanction")] public string OtherSanction { get; set; }
        [Column("BusinessType")] public string BusinessType { get; set; }
        [Column("LegalStatus")] public string LegalStatus { get; set; }

        [Column("ChangeStatus")] public string ChangeStatus { get; set; }

        [Column("PassportId")] public string PassportId { get; set; }
        [Column("PassportIssueDate")] public string PassportIssueDate { get; set; }
        [Column("PassportExpiryDate")]  public string PassportExpiryDate { get; set; }
        [Column("EmiratesIdNumber")]  public string EmiratesIdNumber { get; set; }
        [Column("EmiratesIdIssueDate")]  public string EmiratesIdIssueDate { get; set; }
        [Column("EmiratesIdExpiryDate")]  public string EmiratesIdExpiryDate { get; set; }

        [Column("Residence")]  public string Residence { get; set; }

        [Column("SOWSOFCountry")]  public string SOWSOFCountry { get; set; }

        [Column("Employer")]  public string Employer { get; set; }

        [Column("EmployerIndustry")]  public string EmployerIndustry { get; set; }

        [Column("EmployerSector")]  public string EmployerSector { get; set; }

        [Column("GoldenVisa")]  public string GoldenVisa { get; set; }

        [Column("CounterParty")]  public string CounterParty { get; set; }

        [Column("ProductRefNo")]  public string ProductRefNo { get; set; }

        [Column("ProductValue")]  public string ProductValue { get; set; }

        [Column("TradeLicenseAuthority")]  public string TradeLicenseAuthority { get; set; }

        [Column("TradeLicenseSector")]  public string TradeLicenseSector { get; set; }

        [Column("Share")]  public int Share { get; set; }
        [Column("Designation")]  public string Designation { get; set; }

        [Column("CounterPartyName")]  public string CounterPartyName { get; set; }

        [Column("Tradelicense")]  public string Tradelicense { get; set; }

        [Column("StatusName")] public string StatusName { get; set; }

        [Column("GroupID")] public string GroupId { get; set; }

        [Column("individual_risk_score")]  public string Individual_final_risk_score { get; set; }

        [Column("corporate_risk_score")]  public string corporate_final_risk_score { get; set; }

        [Column("QAReviewerName")] public string QAReviewerName { get; set; }


        [Column("QAReason")] public string QAReason { get; set; }


        [Column("QAComments")] public string QAComments { get; set; }

        [Column("QAStatus")] public string QAStatus { get; set; }

        [Column("QAReviewdOn")] public DateTime? QAReviewdOn { get; set; }


        //[Column("LegalStatus")] public string LegalStatus { get; set; }

        //[Column("LegalStatus")] public string LegalStatus { get; set; }


    }

    public class CaseQaDTO
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int ClientId { get; set; }
        public int QaStatus { get; set; }
        public string QaOutcome { get; set; }
        public string QaReasonCode { get; set; }
        public int? QaReviewerId { get; set; }
        public string QaReviewerName { get; set; }
        public DateTime? QaReviewedOn { get; set; }
        public string QaComments { get; set; }
        public int CreatedBy { get; set; }
    }

    public class TrackerSavedViewDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public string Name { get; set; }
        public string FilterJson { get; set; }
        public bool IsShared { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    public class TrackerSlaConfigDTO
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string CustomerType { get; set; } = "*";
        public string RiskTier { get; set; } = "*";
        public int CaseSlaOnTrackDays { get; set; } = 7;
        public int CaseSlaAtRiskDays { get; set; } = 14;
        public int QaSlaOnTrackDays { get; set; } = 7;
        public int QaSlaAtRiskDays { get; set; } = 14;
        public int StaleCaseDays { get; set; } = 30;
        public bool IsActive { get; set; } = true;
    }

    public class TrackerAuditLogDTO
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Action { get; set; }
        public int AffectedCount { get; set; }
        public string CaseIdsCsv { get; set; }
        public string Details { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    public class TrackerNotificationDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ClientId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Link { get; set; }
        public int? CaseId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? CreatedOn { get; set; }
    }

    public class TrackerSarLinkDTO
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int ClientId { get; set; }
        public string SarStatus { get; set; }
        public string SarReference { get; set; }
        public DateTime? SarFilingDeadline { get; set; }
        public DateTime? FiledOn { get; set; }
        public string Notes { get; set; }
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
    }

    public class ReviewerStatsDTO
    {
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public int CasesReviewed { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public double AvgTurnaroundDays { get; set; }
        public DateTime? LastActivity { get; set; }
    }

    public class CycleTimePointDTO
    {
        public DateTime Day { get; set; }
        public int Decisions { get; set; }
        public double MedianHours { get; set; }
    }

    public enum QaStatusCode
    {
        NotReviewed = 0,
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        Reopened = 4
    }

    public class CaseMakerInfoDTO
    {
        [Column("CaseId")]
        public int CaseId { get; set; }
        [Column("MakerId")]
        public int MakerId { get; set; }
        [Column("MakerName")]
        public string MakerName { get; set; }
        [Column("MakerEmail")]
        public string MakerEmail { get; set; }
        [Column("OwnerId")]
        public int OwnerId { get; set; }
        [Column("OwnerName")]
        public string OwnerName { get; set; }
        [Column("CustomerName")]
        public string CustomerName { get; set; }
        [Column("Status")]
        public int Status { get; set; }
    }
}
