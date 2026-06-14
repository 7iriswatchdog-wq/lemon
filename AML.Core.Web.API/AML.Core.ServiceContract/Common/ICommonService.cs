using AML.Core.DataContract.Authentication;
using AML.DTO.DTO.CustomerCase;
using AML.ViewModel.ViewModels.ApiAuthentication;
using AML.ViewModel.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AML.DTO.DTO.Common;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.DTO.DTO.TransactionScreening;
using AML.ViewModel.ViewModels.TransactionScreening;
using AML.Core.Common.StaticResource;
using AML.ViewModel.ViewModels.CaseAssignment;

namespace AML.Core.ServiceContract.Common
{
    public interface ICommonService : IBaseService
    {
        string EncryptionString(string strValue);
        string DecryptionString(string strValue);
        string EncryptString(object value);
        void SendEmail(EmailModel emailModel);
        Task<bool> SendOtpEmail(string subject,string body, string email);
        Task<TokenRS> CreateC6Token(string url, string baseURL);
        //Task<CustomerCaseDTO> CaseScreeningCall(int _newCustId, string baseUrl);
        Task<CustomerCaseDTO> CustomerScreeningCall(string CallC6Screening,int _newCustMasterId, string baseUrl, string baseC6Url, DocumentUploadModel _documentUploadModel, string callFrom = null, string emailBody = null, int checkThreshold = -1, int cid = -1);

        Task<CustomerCaseDTO> CustomerScreeningCall(string CallC6Screening, string _newCustMasterId, string baseUrl, string baseC6Url, DocumentUploadModel _documentUploadModel, string callFrom = null, string emailBody = null, int checkThreshold = -1, int cid = -1);

        bool CustomerScreeningCallOnlySanction(List<ApiResultModel> apiResultModels,int _newCustMasterId, string baseUrl, string baseC6Url, DocumentUploadModel _documentUploadModel, string callFrom = null, string emailBody = null, int checkThreshold = -1, int cid = -1);
        Task<CustomerCaseDTO> CustomerScreeningCall(int _newCustMasterId, string baseUrl, string baseC6Url, string callFrom = null, string emailBody = null, int checkThreshold = -1, int cid = -1);
        Task<CustomerCaseDTO> CustomerScreeningCall(string _newCustMasterId, string baseUrl, string baseC6Url, string callFrom = null, string emailBody = null, int checkThreshold = -1, int cid = -1);
        Task<CustomerCaseDTO> CustomerScreeningCall(CustomerCaseDTO customerCaseDTO, string baseUrl, string baseC6Url, string callFrom = null, string emailBody = null, int checkThreshold = -1, string cid = "");
        Task<CustomerCaseDTO> CustomerScreeningCall(CustomerCaseDTO customerCaseDTO, string baseUrl, string baseC6Url, string callFrom = null, string emailBody = null, int checkThreshold = -1, int cid = -1);
        Task<CustomerCaseDTO> C6ScreeningCall(string _newCustMasterId, string baseUrl, string callFrom = null, string emailBody = null, int checkThreshold = 0);
        Task<CustomerCaseDTO> C6ScreeningCall(CustomerCaseDTO customerCaseDTO, string baseUrl, string callFrom = null, string emailBody = null, int checkThreshold = 0, int cid = -1);

        

        Task<int> ScreenCustomersFromDB(string baseUrl);
        Task<int> ScreenTMSFromDB(string baseUrl);
        CustomerCaseDTO CustomerStatusCheck(string resultL);
        CustomerCaseDTO CustomerRiskStatusCheck(string resultL,string customertype);
        
         ApiAuthResponse Authenticate(ApiAuthRequest model);
        Task<CustomerCaseDTO> ApprovedListScreeningCall(string _newCustMasterId, string baseUrl, string baseC6Url, string schedulerRunId, string callFrom = null, string emailBody = null);
        void SendEmailLog(string emailBody, int clientId,string actionDisplay);
        bool UpdateCaseRemark(int caseId, List<DataListModel> model);
        Task<bool> SendHtmlFormattedEmailWithAttachment(string v, string body, string file, byte[] ms);

        Task<bool> sendCaseTransferedEmaiLog(string subject, string body, CaseAssignmentModel model);
        CreatedAndUpdatedByNames GetCreatedByAndUpdateByNameFromId(int createdById, int updatedById);
        ServiceResponse<string> createErrorlog(ErrorLogDTO errorlog);

        //Transaction Screening
        bool UpdateTransactionCaseRemark(int caseId, List<ApiRespModel> model);
        TranstatusCheckDTO TransactionStatusCheck(string resultL);
        Task<TranScreenDTO> TransactionScreeningCall(TranScreenDTO TranScreenDTO, string baseUrl, string baseC6Url, string callFrom = null, string emailBody = null, int cid = -1, string source = "Free");

        void UpdateSanctionRecords(CustomerCaseDTO _CustomerCaseDTO, List<ApiResultModel> apiResp, string cid);

        Task<bool> ScreenApprovedListByClientId(int client_id);





    }
}
