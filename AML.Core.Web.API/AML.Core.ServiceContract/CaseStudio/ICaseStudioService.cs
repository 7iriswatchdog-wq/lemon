using AML.DTO.DTO.CustomerCase;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AML.Core.ServiceContract.CaseStudio
{
    public interface ICaseStudioService : IBaseService
    {
        Task<CaseStudioResult> ProcessHierarchyAsync(CaseStudioPayload payload, int clientId, int userId, string baseURL, string baseC6URL);
        Task PerformDetailedRiskAssessmentAsync(CaseStudioNode node, int clientId, int userId, string backendId);

        Task<string> SaveCaseDraftAsync(int clientId, int userId, CaseStudioPayload payload);
        Task<IEnumerable<CaseStudioDraftSummaryDTO>> GetCaseDraftSummariesAsync(int clientId, int userId);
        Task<CaseStudioPayload> GetCaseDraftAsync(int clientId, int userId, string draftId);
        Task<bool> DeleteCaseDraftAsync(int clientId, int userId, string draftId);
        Task<bool> RenameCaseDraftAsync(int clientId, int userId, string draftId, string newDraftName);
    }

    public class CaseStudioPayload
    {
        public string DraftId { get; set; }
        public string DraftName { get; set; }
        public List<CaseStudioNode> Nodes { get; set; }
        public List<CaseStudioEdge> Edges { get; set; }
    }

    public class CaseStudioNode
    {
        public string Id { get; set; }
        public string StudioId { get; set; }
        public string CaseId { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public string ShareholderType { get; set; }
        public string FlagType { get; set; }
        public string Name { get; set; }
        public bool IsRoot { get; set; }
        
        // Identity
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Nationality { get; set; }
        public string Dob { get; set; }
        public string Gender { get; set; }
        public string Cif { get; set; }
        
        // IDs
        public string PassportId { get; set; }
        public string PassportIssueDate { get; set; }
        public string PassportExpiryDate { get; set; }
        public string EmiratesIdNumber { get; set; }
        public string EmiratesIdIssueDate { get; set; }
        public string EmiratesIdExpiryDate { get; set; }
        public List<string> SelectedIdTypes { get; set; }
        public string TradeLicence { get; set; }
        public string TradeLicenseAuthority { get; set; }
        
        // Business/Profile
        public string EntityTypeTxt { get; set; }
        public string BusinessType { get; set; }
        public string RegistrationDate { get; set; }
        public string Profession { get; set; }
        public string Employer { get; set; }
        public string SourceOfFunds { get; set; }
        public decimal? EstimatedIncome { get; set; }
        
        // Compliance
        public string Relationship { get; set; }
        public decimal? Share { get; set; }
        public int MatchThreshold { get; set; }
        public bool IsScreened { get; set; }
        public bool IsFetched { get; set; }
        public bool IsMainEntity { get; set; }
        public bool IsJointParty { get; set; }
        public string RiskCategory { get; set; }
        public List<string> ScreeningSources { get; set; }
        
        // Product
        public string ProductName { get; set; }
        public decimal? ProductValue { get; set; }
        public string DeliveryChannel { get; set; }
        public string Modeofpayment { get; set; }
        public string Residence { get; set; }
        public string PlaceOfBirth { get; set; }

        public string EmployerIndustry { get; set; }
        public string EmployerSector { get; set; }
        public string GoldenVisa { get; set; }
        public string CounterParty { get; set; }
        public string CounterPartyName { get; set; }
        public string ProductRefNo { get; set; }
        public string Designation { get; set; }
        public string Sowsofcountry { get; set; }
        public bool IsDuplicate { get; set; }
        public string GroupId { get; set; }
        public string GroupRisk { get; set; }
        public string GroupEntityof { get; set; }
        
        // Detailed Risk Assessment IDs
        public string EntityId { get; set; }
        public string BusinessId { get; set; }
        public string ProductId { get; set; }
        public string DeliveryChannelId { get; set; }
        public string ModeOfPaymentId { get; set; }

        public List<CaseStudioAttachment> Attachments { get; set; }
    }

    public class CaseStudioAttachment
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public string FullPath { get; set; }
        public string DocumentType { get; set; }
        public string IssuedDate { get; set; }
        public string ExpiryDate { get; set; }
    }

    public class CaseStudioEdge
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Relationship { get; set; }
        public decimal? Share { get; set; }
        public string Designation { get; set; }
        public string Remarks { get; set; }
        public string Label { get; set; }
    }

    public class CaseStudioResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<NodeResult> NodeResults { get; set; } = new List<NodeResult>();
    }

    public class NodeResult
    {
        public string StudioId { get; set; }
        public string BackendId { get; set; }
        public int NumericId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public bool Screened { get; set; }
    }
}
