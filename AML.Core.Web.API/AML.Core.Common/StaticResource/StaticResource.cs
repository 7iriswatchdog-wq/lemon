using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Common.StaticResource
{
    public static class StaticResource
    {
        public const int SuccessStatusCode = 200;
        public const string SuccessMessage = "Success";

        public const int NotFoundStatusCode = 404;
        public const string NotFoundMessage = "No record found";

        public const int UnauthorizedStatusCode = 401;
        public const string UnauthorizedMessage = "Unauthorized";

        public const int DuplicateStatusCode = 409;
        public const string DuplicateMessage = "Duplicate record";

        public const int FailStatusCode = 500;
        public const string FailMessage = "Error occurred";

        public const int InvalidCardCode = 402;
        public const string InvalidCardMessage = "Invalid card detail";
        public const string InvalidAchDetail = "Invalid ach detail";

        public const int TransferredFailedCode = 403;
        public const string TransferredFailedMessage = "Vehicle transferred failed";

        public const int SessionTimeoutCode = 440;
        public const string SessionTimeoutMessage = "Your session has expired.";


        #region Email Messages
        public const string EmailSent = "Email has been sent.";
        public const string EmailNotSent = "Failed to send email.";
        public const string EmailNotAvailable = "Email is not available.";
        #endregion

        #region Session 
        public const string sessCultureInfo = "sessCultureInfo";
        public const string sessUserId = "SessUserId";
        public const string sessRoleId = "SessGroupId";
        public const string sessBranchId = "SessBranchId";
        public const string sessUsername = "SessUsername";
        public const string sessClientId = "SessClientId";
        public const string sessModules = "SessModules";
        public const string sessFunctionalities = "SessFunctionalities";
        public const string SessCompanyName = "SessCompanyName";
        public const string SessLogoUrl = "SessLogoUrl";
        public const string SessEmail = "SessEmail";
        public const string SessDescription = "SessDescription";
        #endregion

    }
    #region Free Sources
    public static class FreeResouce
    {
        public const string OFAC = "OFAC";
        public const string ALL = "ALL";
        public const string EURO = "EURO";
        public const string UN = "UN";
        public const string CONSOLIDATE = "CONSOLIDATE";
    }
    #endregion
    #region Free Sources
    public static class ScreeningService
    {
        public const string CUSTOMER_SCREENING = "Customer/ScreenCustomer";
        public const string BACKLIST_SCREENING = "BlackList/BlackListSearch";
        public const string GETBYCASEID = "BlackList/GetByCaseId";
        public const string ADDTOBLACKLIST = "BlackList/AddBlackList";
        public const string UPDATETOBLACKLIST = "BlackList/UpdateBlackList";
        public const string GETINTERNALWATCHLISTBYUID = "BlackList/GetWatchlistbyuid";
        public const string INTERNALWATCHLISTREPORT = "BlackList/GetWatchlist";
        public const string C6AUTHENTICATION = "users/authenticate";
        public const string C6SCREENING = "personal/newIndividual";
        public const string C6SCREENINGCORPORATE = "business/ScreenBusiness";
        public const string C6_PERSONAL_BULKID = "personal/bulkid";
        public const string C6_BUSINESS_BULKID = "business/bulkid";
        public const string C6WEBSEARCH = "api/search";

        public const string GETBYTRANCASEID = "BlackList/GetByTranCaseId";
        public const string GETBYTRANREFNO = "BlackList/GetByTranRefNo";
        public const string GETTERRORISLLIST = "uaelist/json";
        public const string GETFUZZYBACKLIST = "BlackList/BlackFuzzySearch";
        public const string GETPendingCasesScheduler = "Customer/PendingCases";
        public const string UserCreation = "users/create";
        public const string UserUpdation = "users/reset";
    }
    #endregion
}
