using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.RepositoryContract.Common;
using AML.Core.ServiceContract.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using AML.Core.Common.StaticResource;
using System.Net.Mail;
using AML.ViewModel.ViewModels.Common;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.CustomerScreening;
using Newtonsoft.Json;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.RepositoryContract.CustomerCase;
using AML.Core.ServiceContract.DigiApiUser;
using AML.Core.DataContract.Authentication;
using System.Linq;
using static AML.DTO.DTO.FreeSource.CaseLogsMongoDTO;
using AML.DTO.DTO.FreeSource;
using AML.Core.RepositoryContract.FreeSource;
using AML.DTO.DTO.CorporateShareholder;
using System.Threading.Tasks;
using AML.ViewModel.ViewModels.ApiAuthentication;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using AML.Core.RepositoryContract.User;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using static AML.Core.Service.Common.CommonService;
using AML.DTO.DTO.Common;
using System.Net.Http;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.Core.Repository.FreeSource;
using AML.Core.RepositoryContract.Country;
using AML.DTO.DTO.Country;
using static AML.DTO.DTO.FreeSource.BlackListMongoDTO;
using NLog;
using System.Text.RegularExpressions;
using AML.DTO.DTO.TransactionScreening;
using AML.Core.ServiceContract.TransactionScreening;
using static AML.DTO.DTO.FreeSource.TransactionCaseLogsMongoDTO;
using AML.ViewModel.ViewModels.TransactionScreening;
using System.Reflection;
using Org.BouncyCastle.Crypto;
using AutoMapper;
using AML.ViewModel.ViewModels.CaseAssignment;
using AML.Core.Repository.CustomerCase;
using static iTextSharp.text.pdf.AcroFields;
using MySqlX.XDevAPI.Common;
using AML.Core.Common.Algorithms;
using System.Configuration;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace AML.Core.Service.Common
{
    public class CommonService : BaseService, ICommonService
    {
        private IMapper _mapper;
        private readonly ICommonRepository _commonRepository;
        private readonly IConfiguration _configuration;
        IHostingEnvironment _env;
        private string baseURL;
        private string baseapiURL;
        private string _host;
        private string _from;
        private string _alias;
        private string _c6Username;
        private string _ScreeningSource;
        private int _c6Threshold;
        private int checkThreshold;
        private IConfigurationSection _smtpSection;
        private ICustomerCaseService _customerCaseService;
        private ICustomerMasterRepository _customerMasterRepository;
        private ICountryRepository _countryRepository;
        private IDigiApiUserService _digiApiUserService;
        private IFreeSourceRepository _freeSourceRepository;
        private IUserRepository _userRepository;
        private IHttpContextAccessor _httpContextAccessor;
        private ITransactionScreeningService _transactionScreeningService;
         
        
        private string c6BaseURL = string.Empty;

        private readonly Logger log = LogManager.GetCurrentClassLogger();

        public CommonService(IMapper mapper,ICommonRepository commonRepository, IDigiApiUserService digiApiUserService, ICountryRepository countryRepository, IConfiguration configuration, IHostingEnvironment env, ICustomerCaseService customerCaseService, ICustomerMasterRepository customerMasterRepository, IFreeSourceRepository freeSourceRepository, IUserRepository userRepository, ITransactionScreeningService transactionScreeningService, IHttpContextAccessor httpContextAccessor) : base(commonRepository, configuration)
        {
            _mapper = mapper;
            _commonRepository = commonRepository;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _customerCaseService = customerCaseService;
            _customerMasterRepository = customerMasterRepository;
            _digiApiUserService = digiApiUserService;
            _freeSourceRepository = freeSourceRepository;
            _userRepository = userRepository;
            _transactionScreeningService = transactionScreeningService;
            _c6Threshold = Convert.ToInt32(_configuration.GetSection("C6BaseApiUrl:C6Threshold").Value);
            checkThreshold = Convert.ToInt32(_configuration.GetSection("C6BaseApiUrl:Threshold").Value);
            c6BaseURL = configuration.GetSection("C6BaseApiUrl").GetSection("BaseUrl").Value;
            baseapiURL = configuration.GetSection("AMLBaseApiUrl").GetSection("BaseUrl").Value;

            _countryRepository = countryRepository;
            /* client data from db instead of appseting --begin*/
            var clientId = 0;
            ClientMasterDTO clientDetails = new ClientMasterDTO();
            try
            {
                clientId = httpContextAccessor.HttpContext.Session.GetString("SessClientId").ParseInt();
                clientDetails = _customerCaseService.GetClientDetailsByID(clientId);
                //_c6Username = _configuration.GetSection("C6BaseApiUrl:Username").Value;
                if(clientDetails != null) 
                {
                    _c6Username = clientDetails.C6Username;
                    _c6Threshold = clientDetails.C6Threshold;
                    //checkThreshold = Convert.ToInt32(_configuration.GetSection("C6BaseApiUrl:Threshold").Value);
                    checkThreshold = clientDetails.Threshold;
                }

                
                baseURL = _configuration.GetSection("OHSBaseApiUrl:BaseUrl").Value;
                _ScreeningSource = _configuration.GetSection("ScreeningSource").Value;
                //_c6Threshold = Convert.ToInt32(_configuration.GetSection("C6BaseApiUrl:C6Threshold").Value);
                
            }
            catch (Exception ex)
            {

            }
            /* client data from db instead of appseting --end*/


            _env = env;
            _smtpSection = _configuration.GetSection("SMTP");
            _host = _smtpSection.GetSection("Host").Value;
            _from = _smtpSection.GetSection("From").Value;
            _alias = _smtpSection.GetSection("Alias").Value;
        }
        public string EncryptionString(string strValue)
        {
            return Core.Common.CommonClasses.EncryptionHelper.Encrypt(strValue);
        }
        public string DecryptionString(string strValue)
        {
            return Core.Common.CommonClasses.EncryptionHelper.Decrypt(strValue);
        }
        public string EncryptString(object value)
        {
            return _commonRepository.EncryptionString(value.ParseString());
        }
        public void SendEmail(EmailModel emailModel)
        {
            try
            {
                using SmtpClient client = new SmtpClient(_host);
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_from, _alias);
                mailMessage.BodyEncoding = Encoding.UTF8;
                mailMessage.To.Add(emailModel.To);
                mailMessage.Body = emailModel.Message;
                mailMessage.Subject = emailModel.Subject;
                mailMessage.IsBodyHtml = emailModel.IsBodyHtml;
                client.Send(mailMessage);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
        public async void SendEmailLog(string emailBody, int clientId,string actionDisplay)
        {
            await SendHtmlFormattedEmail("Watchdog Screening - Daily Ongoing Monitoring" + DateTime.Now.ToString() +"-"+ actionDisplay, emailBody, clientId);
        }


        public async Task<bool> sendCaseTransferedEmaiLog(string subject, string body, CaseAssignmentModel model)
        {

            try
            {
                var clientID = _httpContextAccessor.HttpContext.Session.GetString("SessClientId").ParseInt();
                var emailDetails = _commonRepository.GetConfigurationDetailsByGroup("EM", clientID);
                var from = emailDetails.Where(x => x.CategoryKey == "E_FROM").Select(_ => _.CategoryValue).FirstOrDefault();
                var alias = emailDetails.Where(x => x.CategoryKey == "E_ALIAS").Select(_ => _.CategoryValue).FirstOrDefault();
                var userid = emailDetails.Where(x => x.CategoryKey == "E_USER").Select(_ => _.CategoryValue).FirstOrDefault();
                var password = emailDetails.Where(x => x.CategoryKey == "E_PWD").Select(_ => _.CategoryValue).FirstOrDefault();
                var host = emailDetails.Where(x => x.CategoryKey == "E_HOST").Select(_ => _.CategoryValue).FirstOrDefault();
                var port = emailDetails.Where(x => x.CategoryKey == "E_PORT").Select(_ => _.CategoryValue).FirstOrDefault();
                var to = model.Email;
                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(from, alias);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;
                    //mailMessage.To.Add(new MailAddress(to));
                    mailMessage.To.Add(to); //for adding multiple email ids
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = host;
                    smtp.EnableSsl = true;
                    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                    NetworkCred.UserName = userid;
                    NetworkCred.Password = password;
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = NetworkCred;
                    smtp.Port = int.Parse(port);
                    Console.WriteLine(string.Format("Send email to {0}", to));

                    smtp.Send(mailMessage);

                    Console.WriteLine("DEBUG mode, not sending mail");

                }

                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                //throw ex;
                return false;
            }


        }

        private async Task<bool> SendHtmlFormattedEmail(string subject, string body, int clientId)
        {
            try
            {
                //var clientID = _httpContextAccessor.HttpContext.Session.GetString("SessClientId").ParseInt();
                var clientID = clientId;
                var emailDetails = _commonRepository.GetConfigurationDetailsByGroup("EM", clientID);
                var from = emailDetails.Where(x => x.CategoryKey == "E_FROM").Select(_ => _.CategoryValue).FirstOrDefault();
                var alias = emailDetails.Where(x => x.CategoryKey == "E_ALIAS").Select(_ => _.CategoryValue).FirstOrDefault();
                var userid = emailDetails.Where(x => x.CategoryKey == "E_USER").Select(_ => _.CategoryValue).FirstOrDefault();
                var password = emailDetails.Where(x => x.CategoryKey == "E_PWD").Select(_ => _.CategoryValue).FirstOrDefault();
                var host = emailDetails.Where(x => x.CategoryKey == "E_HOST").Select(_ => _.CategoryValue).FirstOrDefault();
                var port = emailDetails.Where(x => x.CategoryKey == "E_PORT").Select(_ => _.CategoryValue).FirstOrDefault();
                var to = emailDetails.Where(x => x.CategoryKey == "COMPLEM").Select(_ => _.CategoryValue).FirstOrDefault();

                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(from, alias);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;
                    //mailMessage.To.Add(new MailAddress(to));
                    mailMessage.To.Add(to); //for adding multiple email ids
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = host;
                    smtp.EnableSsl = true;
                    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                    NetworkCred.UserName = userid;
                    NetworkCred.Password = password;
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = NetworkCred;
                    smtp.Port = int.Parse(port);
                    smtp.Send(mailMessage);
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }


        public async Task<bool> SendOtpEmail(string subject,string body,string emailid)
        {
            try
            {
                var clientID = _httpContextAccessor.HttpContext.Session.GetString("SessClientId").ParseInt();
                var emailDetails = _commonRepository.GetConfigurationDetailsByGroup("EM", clientID);
                var from = emailDetails.Where(x => x.CategoryKey == "E_FROM").Select(_ => _.CategoryValue).FirstOrDefault();
                var alias = emailDetails.Where(x => x.CategoryKey == "E_ALIAS").Select(_ => _.CategoryValue).FirstOrDefault();
                var userid = emailDetails.Where(x => x.CategoryKey == "E_USER").Select(_ => _.CategoryValue).FirstOrDefault();
                var password = emailDetails.Where(x => x.CategoryKey == "E_PWD").Select(_ => _.CategoryValue).FirstOrDefault();
                var host = emailDetails.Where(x => x.CategoryKey == "E_HOST").Select(_ => _.CategoryValue).FirstOrDefault();
                var port = emailDetails.Where(x => x.CategoryKey == "E_PORT").Select(_ => _.CategoryValue).FirstOrDefault();
                //var to = emailDetails.Where(x => x.CategoryKey == "COMPLEM").Select(_ => _.CategoryValue).FirstOrDefault();
                var to = emailid;
                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(from, alias);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;
                    mailMessage.IsBodyHtml = true;
                    mailMessage.To.Add(new MailAddress(to));
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = host;
                    smtp.EnableSsl = true;
                    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                    NetworkCred.UserName = userid;
                    NetworkCred.Password = password;
                    smtp.UseDefaultCredentials = true;
                    smtp.Credentials = NetworkCred;
                    smtp.Port = int.Parse(port);
                    smtp.Send(mailMessage);
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return false;
            }
        }

        public async Task<CustomerCaseDTO> CustomerScreeningCall(CustomerCaseDTO _CustomerCaseDTO, string baseUrl, string baseC6Url, string callFrom = null, string emailBody = null, int checkThreshold = -1, int cid = -1)
        {
            if (checkThreshold >= 0)
            {
                _CustomerCaseDTO.Threshold = checkThreshold;
            }
            _ScreeningSource = _configuration.GetSection("ScreeningSource").Value;

            log.Info($"Screening source: {_ScreeningSource}");

            _CustomerCaseDTO.C6Threshold = _c6Threshold;

            if (_ScreeningSource == "C6")
            {
                if (_CustomerCaseDTO.CustomerType == "C")
                {
                    callFrom = "CORPORATE";
                }
                if (_CustomerCaseDTO.CustomerType == "I")
                {
                    callFrom = "INDIVIDUAL";
                }
                else if (_CustomerCaseDTO.CustomerType == "S")
                {
                    callFrom = "SHAREHOLDERS";
                }
                return await this.C6Screening(_CustomerCaseDTO, baseC6Url, callFrom, emailBody, cid);
            }
            else
            {
                return await this.FreeSourceScreening(_CustomerCaseDTO, baseUrl, callFrom, emailBody);
            }
        }

        public async Task<CustomerCaseDTO> CustomerScreeningCall(CustomerCaseDTO _CustomerCaseDTO, string baseUrl, string baseC6Url, string callFrom = null, string emailBody = null, int checkThreshold = -1, string cid = "")
        {
            if (checkThreshold > 0)
            {
                _CustomerCaseDTO.Threshold = checkThreshold;
            }
            _ScreeningSource = _configuration.GetSection("ScreeningSource").Value;

            log.Info($"Screening source: {_ScreeningSource}");

            _CustomerCaseDTO.C6Threshold = _c6Threshold;

            if (_ScreeningSource == "C6")
            {
                if (_CustomerCaseDTO.CustomerType == "C")
                {
                    callFrom = "CORPORATE";
                }
                if (_CustomerCaseDTO.CustomerType == "I")
                {
                    callFrom = "INDIVIDUAL";
                }
                else if (_CustomerCaseDTO.CustomerType == "S")
                {
                    callFrom = "SHAREHOLDERS";
                }
                return await this.C6ScreeningCall(_CustomerCaseDTO, baseC6Url, callFrom, emailBody, checkThreshold, cid);
            }
            else
            {
                return await this.FreeSourceScreening(_CustomerCaseDTO, baseUrl, callFrom, emailBody);
            }
        }
        public async Task<CustomerCaseDTO> CustomerScreeningCall(int _newCustMasterId, string baseUrl, string baseC6Url, string callFrom = null, string emailBody = null, int checkThreshold = -1, int cid = -1)
        {
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetCaseFullDetailsByCustId(_newCustMasterId);
            log.Info($"Screening source: {_ScreeningSource}");
            if (checkThreshold >= 0)
            {
                _CustomerCaseDTO.Threshold = checkThreshold;
            }
            if (_ScreeningSource == "C6")
            {
                if (_CustomerCaseDTO.CustomerType == "C")
                {
                    callFrom = "CORPORATE";
                }
                if (_CustomerCaseDTO.CustomerType == "I")
                {
                    callFrom = "INDIVIDUAL";
                }
                else if (_CustomerCaseDTO.CustomerType == "S")
                {
                    callFrom = "SHAREHOLDERS";
                }
                return await this.C6Screening(_CustomerCaseDTO, baseC6Url, callFrom, emailBody, cid);
            }
            else
            {
                return await this.FreeSourceScreening(_CustomerCaseDTO, baseUrl, callFrom, emailBody);
            }
        }
        public async Task<CustomerCaseDTO> CustomerScreeningCall(string CallC6Screening, int _newCustMasterId, string baseUrl, string baseC6Url, DocumentUploadModel _documentUploadModel, string callFrom = null, string emailBody = null, int checkThreshold = -1, int cid = -1)
        {
            //CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetCaseFullDetailsByCustId(_newCustMasterId);

            DocumentUploadModel _docUpload = _mapper.Map<DocumentUploadModel>(_documentUploadModel);

            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetCaseFullDetailsByCustId(_newCustMasterId);


            //for (int i = 0; i < _docUpload.CodeNames.Count; i++)
            //{
            //    if (_docUpload.CodeNames[i] == "PEP" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsPep = true; continue; }
            //    if (_docUpload.CodeNames[i] == "Sanction" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsSan = true; continue; }
            //    if (_docUpload.CodeNames[i] == "Reputational Risk Exposure" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsRre = true; continue; }
            //    if (_docUpload.CodeNames[i] == "Insolvency (UK & Ireland)" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsIns = true; continue; }
            //    if (_docUpload.CodeNames[i] == "Disqualified Director (UK Only)" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsDd = true; continue; }
            //    if (_docUpload.CodeNames[i] == "Profile of Interest" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsPoi = true; continue; }
            //    if (_docUpload.CodeNames[i] == "Regulatory Enforcement List" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsRel = true; continue; }
            //}
            //if (CallC6Screening == "Y")
            //{
            //    _CustomerCaseDTO.IsPep = true;
            //    _CustomerCaseDTO.IsSan = true;

            //}

            log.Info($"Screening source: {_ScreeningSource}");
            if (checkThreshold >= 0)
            {
                _CustomerCaseDTO.Threshold = checkThreshold;
            }
            if (_ScreeningSource == "C6")
            {
                if (_CustomerCaseDTO.CustomerType == "C")
                {
                    callFrom = "CORPORATE";
                }
                if (_CustomerCaseDTO.CustomerType == "I")
                {
                    callFrom = "INDIVIDUAL";
                }
                else if (_CustomerCaseDTO.CustomerType == "S")
                {
                    callFrom = "SHAREHOLDERS";
                }
                return await this.C6Screening(_CustomerCaseDTO, baseC6Url, callFrom, emailBody, cid);
            }
            else
            {
                return await this.FreeSourceScreening(_CustomerCaseDTO, baseUrl, callFrom, emailBody);
            }
        }

        public async Task<CustomerCaseDTO> CustomerScreeningCall(string CallC6Screening,string _newCustMasterId, string baseUrl, string baseC6Url, DocumentUploadModel _documentUploadModel, string callFrom = null, string emailBody = null,  int checkThreshold = -1, int cid = -1)
        {
            //CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetCaseFullDetailsByCustId(_newCustMasterId);
            
            DocumentUploadModel _docUpload = _mapper.Map<DocumentUploadModel>(_documentUploadModel);

            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetCaseFullDetailsByCustId(_newCustMasterId);

            
            //for (int i = 0; i < _docUpload.CodeNames.Count; i++)
            //{
            //    if (_docUpload.CodeNames[i] == "PEP" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsPep = true; continue; }
            //    if (_docUpload.CodeNames[i] == "Sanction" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsSan = true; continue; }
            //    if (_docUpload.CodeNames[i] == "Reputational Risk Exposure" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsRre = true; continue; }
            //    if (_docUpload.CodeNames[i] == "Insolvency (UK & Ireland)" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsIns = true; continue; }
            //    if (_docUpload.CodeNames[i] == "Disqualified Director (UK Only)" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsDd = true; continue; }
            //    if (_docUpload.CodeNames[i] == "Profile of Interest" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsPoi = true; continue; }
            //    if (_docUpload.CodeNames[i] == "Regulatory Enforcement List" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsRel = true; continue; }
            //}
            //if (CallC6Screening == "Y")
            //{
            //    _CustomerCaseDTO.IsPep = true;
            //    _CustomerCaseDTO.IsSan = true;

            //}

            log.Info($"Screening source: {_ScreeningSource}");
            if (checkThreshold >= 0)
            {
                _CustomerCaseDTO.Threshold = checkThreshold;
            }
            if (_ScreeningSource == "C6")
            {
                if (_CustomerCaseDTO.CustomerType == "C")
                {
                    callFrom = "CORPORATE";
                }
                if (_CustomerCaseDTO.CustomerType == "I")
                {
                    callFrom = "INDIVIDUAL";
                }
                else if (_CustomerCaseDTO.CustomerType == "S")
                {
                    callFrom = "SHAREHOLDERS";
                }
                return await this.C6Screening(_CustomerCaseDTO, baseC6Url, callFrom, emailBody, cid);
            }
            else
            {
                return await this.FreeSourceScreening(_CustomerCaseDTO, baseUrl, callFrom, emailBody);
            }
        }


        //new mthod
        public bool CustomerScreeningCallOnlySanction(List<ApiResultModel> apiResultModels,int _newCustMasterId, string baseUrl, string baseC6Url, DocumentUploadModel _documentUploadModel, string callFrom = null, string emailBody = null, int checkThreshold = -1, int cid = -1)
        {
            bool isOkToProceed = false;
            //CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetCaseFullDetailsByCustId(_newCustMasterId);

            DocumentUploadModel _docUpload = _mapper.Map<DocumentUploadModel>(_documentUploadModel);

            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetCaseFullDetailsByCustId(_newCustMasterId);


            for (int i = 0; i < _docUpload.CodeNames.Count; i++)
            {
               // if (_docUpload.CodeNames[i] == "PEP" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsPep = true; continue; }
                if (_docUpload.CodeNames[i] == "Sanction" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsSan = true; continue; }
               /* if (_docUpload.CodeNames[i] == "Reputational Risk Exposure" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsRre = true; continue; }
                if (_docUpload.CodeNames[i] == "Insolvency (UK & Ireland)" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsIns = true; continue; }
                if (_docUpload.CodeNames[i] == "Disqualified Director (UK Only)" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsDd = true; continue; }
                if (_docUpload.CodeNames[i] == "Profile of Interest" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsPoi = true; continue; }
                if (_docUpload.CodeNames[i] == "Regulatory Enforcement List" && _docUpload.IsChecked[i] == true) { _CustomerCaseDTO.IsRel = true; continue; }
               */
            }
            UpdateSanctionRecords(_CustomerCaseDTO, apiResultModels, _CustomerCaseDTO.CustomerId.ToString());
            if (_CustomerCaseDTO.IsMatched == 1 && _CustomerCaseDTO.MatchScore >= checkThreshold)
            {
                isOkToProceed = false;
               // _toastNotification.AddWarningToastMessage(string.Concat("Customer blocked, Case created for ", x.LastName, ". \n"));
                //resultString = string.Concat(resultString, "Customer blocked, Case created for ", x.FirstName, " ", x.LastName, ". \n");
            }
            else
            {
                isOkToProceed = true;
               // _toastNotification.AddInfoToastMessage(string.Concat("Customer Approved for ", x.LastName, ". \n"));
                //resultString = string.Concat(resultString, "Customer Approved for ", x.FirstName, " ", x.LastName, ". \n");
            }
            return isOkToProceed;
        }

		public async Task<CustomerCaseDTO> C6ScreeningCall(CustomerCaseDTO _CustomerCaseDTO, string baseUrl, string callFrom = null, string emailBody = null, int checkThreshold = -1, string cid = "")
		{
			string id;

			if (_CustomerCaseDTO.Id == 0 && cid != "")
			{
				id = cid;
			}
			else
			{
				id = _CustomerCaseDTO.Id.ToString();
			}

			_CustomerCaseDTO.Id = _customerCaseService.GetCaseId(id);

			return await this.C6Screening(_CustomerCaseDTO, baseUrl, callFrom, emailBody);
		}







		public async Task<CustomerCaseDTO> CustomerScreeningCall(string _newCustMasterId, string baseUrl, string baseC6Url, string callFrom = null, string emailBody = null, int checkThreshold = -1, int cid = -1)
        {
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetCaseFullDetailsByCustId(_newCustMasterId);

            log.Info($"Screening source: {_ScreeningSource}");

            if (checkThreshold >= 0)
            {
                _CustomerCaseDTO.Threshold = checkThreshold;
            }
            if (_ScreeningSource == "C6")
            {
                if (_CustomerCaseDTO.CustomerType == "C")
                {
                    callFrom = "CORPORATE";
                }
                if (_CustomerCaseDTO.CustomerType == "I")
                {
                    callFrom = "INDIVIDUAL";
                }
                else if (_CustomerCaseDTO.CustomerType == "S")
                {
                    callFrom = "SHAREHOLDERS";
                }
                return await this.C6Screening(_CustomerCaseDTO, baseC6Url, callFrom, emailBody, cid);
            }
            else
            {
                return await this.FreeSourceScreening(_CustomerCaseDTO, baseUrl, callFrom, emailBody);
            }
        }

        public async Task<CustomerCaseDTO> C6ScreeningCall(string _newCustMasterId, string baseUrl, string callFrom = null, string emailBody = null, int checkThreshold = -1)
        {
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetCaseFullDetailsByCustId(_newCustMasterId);
            if (checkThreshold >= 0) { _CustomerCaseDTO.C6Threshold = checkThreshold; }
            return await C6Screening(_CustomerCaseDTO, baseUrl, callFrom, emailBody);
        }

        


        public async Task<CustomerCaseDTO> C6ScreeningCall(CustomerCaseDTO _CustomerCaseDTO, string baseUrl, string callFrom = null, string emailBody = null, int checkThreshold = -1, int cid = -1)
        {
            if (checkThreshold >= 0) { _CustomerCaseDTO.C6Threshold = checkThreshold; }
            return await this.C6Screening(_CustomerCaseDTO, baseUrl, callFrom, emailBody, cid);

            
        }
        public CustomerCaseDTO CustomerStatusCheck(string _newCustMasterId)
        {
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetCaseStatusByCustId(_newCustMasterId);
            return _CustomerCaseDTO;
        }


        public CustomerCaseDTO CustomerRiskStatusCheck(string _newCustMasterId,string customertype)
        {
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetRiskStatusByCustId(_newCustMasterId,customertype);
            return _CustomerCaseDTO;
        }
        public ServiceResponse<string> createErrorlog(ErrorLogDTO errorlog)
        {
            return _commonRepository.createErrorlog(errorlog);
        }

        public async Task<TranScreenDTO> TransactionScreeningCall(TranScreenDTO _TranScreenDTO, string baseUrl, string baseC6Url, string callFrom = null, string emailBody = null, int cid = -1, string source = "C6")
        {
            _TranScreenDTO.Threshold = checkThreshold;

            log.Info($"Screening source: {_ScreeningSource}");


            return await this.FreesourceScreening_Transaction(_TranScreenDTO, baseUrl, callFrom, emailBody);

        }
        public TranstatusCheckDTO TransactionStatusCheck(string _newTranRefNo)
        {
            TranstatusCheckDTO _TranstatusCheckDTO = _transactionScreeningService.GetTranStatusByTranRefNo(_newTranRefNo);
            return _TranstatusCheckDTO;
        }
        private async Task<TranScreenDTO> FreesourceScreening_Transaction(TranScreenDTO _TranScreenDTO, string baseUrl, string callFrom = null, string emailBody = null)
        {

            DTO.DTO.Sanction.ScreeningSearchDTO TransactionScreeningRQ = new DTO.DTO.Sanction.ScreeningSearchDTO();

            TransactionScreeningRQ.customerdob = _TranScreenDTO.DOB.ToString();
            TransactionScreeningRQ.customerfullname = _TranScreenDTO.Name;
            TransactionScreeningRQ.customernationality = _TranScreenDTO.Country.IsNotNullOrEmpty() ? _TranScreenDTO.Country : string.Empty;
            TransactionScreeningRQ.searchtype = "F";

            checkThreshold = _TranScreenDTO.Threshold;

            _TranScreenDTO.IsMatched = 0;
            _TranScreenDTO.Status = 5;

            List<ApiResModel> apiResp = new List<ApiResModel>();
            try
            {
                if (_TranScreenDTO.IsWhiteListed != "YES")
                {
                    //Calling Screening API
                    var response = await AMLUtility.ScreeningAPICall(TransactionScreeningRQ, ScreeningService.BACKLIST_SCREENING, baseUrl);
                    //if (response.Result != null) apiResp = JsonConvert.DeserializeObject<List<ApiResModel>>(response.Result);
                    if (!string.IsNullOrEmpty(response)) apiResp = JsonConvert.DeserializeObject<List<ApiResModel>>(response);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                //If screening API call returns an error, log the error message into comments column of transaction screening table and pass the status as 10. 
                //Inside the SP, the transaction reference# will be concatenated with  '_1', so that the customer can reupload the same transaction again.

                _TranScreenDTO.Status = 10;
                _TranScreenDTO.Source = "Error occured while screening";
                _TranScreenDTO.Comments = ex.Message;
                var updateRespose = _transactionScreeningService.UpdateTransactionScreening(_TranScreenDTO);
            }


            if (apiResp.Count > 0 && apiResp[0].matchname.IsNotNullOrEmpty())
            {
                _TranScreenDTO.IsMatched = 1;

                var previousStatus = _TranScreenDTO.Status;

                _TranScreenDTO.MatchCategory = apiResp[0].matchcategory;
                _TranScreenDTO.Source = (_TranScreenDTO.IsWhiteListed != "YES") ? apiResp[0].matchtype : "WHITELIST";
                _TranScreenDTO.SourceUniqueId = apiResp[0].matchuid;
                _TranScreenDTO.MatchName = apiResp[0].matchname;
                _TranScreenDTO.MatchScore = Convert.ToInt32(apiResp[0].matchscore);
                _TranScreenDTO.MatchType = apiResp[0].matchtype;
                _TranScreenDTO.Status = _TranScreenDTO.MatchScore.IsNotNullOrEmpty() ? (_TranScreenDTO.MatchScore < checkThreshold ? 5 : 0) : 5;// matching percentage threshold taken from client settings


                var updateRespose = _transactionScreeningService.UpdateTransactionScreening(_TranScreenDTO);
                var matchrecordsList = new List<MatchTranRecordsDTO>();
                var stat = 0;

                if (_TranScreenDTO.IsMatched == 1 && _TranScreenDTO.Status == 0)
                {

                    foreach (var item in apiResp)
                    {
                        var matchrecords = new MatchTranRecordsDTO();
                        if (item.IsNotNullOrEmpty())
                        {
                            matchrecords.MATCHUID = item.matchuid;
                            matchrecords.MATCHTYPE = item.matchtype;
                            matchrecords.MATCHCATEGORY = item.matchcategory;
                            matchrecords.MATCHNAME = item.matchname;
                            matchrecords.MATCHSCORE = Convert.ToInt32(item.matchscore);
                            matchrecords.MATCHNATIONALITY = item.nationality.IsNotNullOrEmpty() ? Char.ToString(item.nationality.FirstOrDefault()) : string.Empty;
                            matchrecords.MATCHIDNO = item.matchidnumber.IsNotNullOrEmpty() ? item.matchidnumber.ToString() : string.Empty;
                            //matchrecords.MATCHDOB = item.matchdob != null ? Char.ToString(item.matchdob.FirstOrDefault()) : "";
                            matchrecords.MATCHDOB = item.matchdob;
                            matchrecordsList.Add(matchrecords);
                            stat++;

                        }
                    }
                    //insert caselog in mongodb
                    if (stat >= 1)
                    {
                        TRANSACTION_CASELOG modelCaseLog = new TRANSACTION_CASELOG
                        {
                            TRANREFNO = _TranScreenDTO.TranRefNo,
                            CASEID = _TranScreenDTO.Id.ToString(),
                            MATCHRECORDS = matchrecordsList
                        };

                        log.Debug("Add case log to MongoDB");
                        _freeSourceRepository.InsertTransactionCaseLog(modelCaseLog);
                    }

                }

                //if (_TranScreenDTO.IsMatched == 1)
                //{
                //    try
                //    {
                //        var appBaseUrl = MyHttpContext.AppBaseUrl;
                //        //Send Email notification for transaction screening matches
                //        emailBody = emailBody.Replace("{tranRefNo}", _TranScreenDTO.TranRefNo.ToString());
                //        emailBody = emailBody.Replace("{caseID}", _TranScreenDTO.Id.ToString());
                //        emailBody = emailBody.Replace("{baseUrl}", appBaseUrl);

                //        _TranScreenDTO.sendMail = 1;
                //        //var emailSent =  SendHtmlFormattedEmail("Transaction Screening Alert", emailBody);
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.Error.WriteLine(ex);
                //    }

                //}
            }

            return _TranScreenDTO;
        }
        //End Transaction Monitoring


        private async Task<CustomerCaseDTO> FreeSourceScreening(CustomerCaseDTO _CustomerCaseDTO, string baseUrl, string callFrom = null, string emailBody = null)
        {
            if (_CustomerCaseDTO.CustomerType == "C")
            {
                _CustomerCaseDTO.CustomerType = "CORPORATE";
            }
            if (_CustomerCaseDTO.CustomerType == "I")
            {
                _CustomerCaseDTO.CustomerType = "INDIVIDUAL";
            }

            if (_CustomerCaseDTO.Nationality == "0") 
            {
                _CustomerCaseDTO.Nationality = "";
            }

            bool CaseFromSchedulerStatus = false;
            CustomerScreeningRQ screeningrq = new CustomerScreeningRQ();
            screeningrq.CASEID = Convert.ToString(_CustomerCaseDTO.Id);
            screeningrq.CREATEDON = string.Empty;
            screeningrq.CUSTOMERCODE = _CustomerCaseDTO.CustomerId.IsNotNullOrEmpty() ? _CustomerCaseDTO.CustomerId : string.Empty;
            screeningrq.CUSTOMERCATEGORY = _CustomerCaseDTO.CustomerType.IsNotNullOrEmpty() ? _CustomerCaseDTO.CustomerType : string.Empty;
            screeningrq.CUSTOMERDOB = _CustomerCaseDTO.DOB.IsNotNullOrEmpty() ? _CustomerCaseDTO.DOB.ToString("dd MMM yyyy") : string.Empty;
            screeningrq.CUSTOMERCOUNTRY = (_CustomerCaseDTO.Nationality.IsNotNullOrEmpty() || _CustomerCaseDTO.Nationality == "0") ? _CustomerCaseDTO.Nationality : string.Empty;
            screeningrq.CUSTOMERNATIONALITY = (_CustomerCaseDTO.Nationality.IsNotNullOrEmpty() || _CustomerCaseDTO.Nationality == "0") ? _CustomerCaseDTO.Nationality : string.Empty;
            screeningrq.CUSTOMERFULLNAME = _CustomerCaseDTO.FirstName + " " + _CustomerCaseDTO.MiddleName + " " + _CustomerCaseDTO.LastName;
            screeningrq.CUSTOMERIDNUMBER = _CustomerCaseDTO.CustomerIdNumber.IsNotNullOrEmpty() ? _CustomerCaseDTO.CustomerIdNumber : string.Empty;
            screeningrq.CUSTOMERIDTYPE = _CustomerCaseDTO.CustomerIdType.IsNotNullOrEmpty() ? _CustomerCaseDTO.CustomerIdType : string.Empty;
            screeningrq.CUSTOMERMOBILENUMBER = _CustomerCaseDTO.Mobile.IsNotNullOrEmpty() ? _CustomerCaseDTO.Mobile : string.Empty;
            screeningrq.CUSTOMERTYPE = _CustomerCaseDTO.CustomerType.IsNotNullOrEmpty() ? _CustomerCaseDTO.CustomerType : string.Empty;
            screeningrq.UPDATEDON = string.Empty;
            checkThreshold = _CustomerCaseDTO.Threshold;
            if (_CustomerCaseDTO.Status == 2 || _CustomerCaseDTO.Status == 5)
            {
                CaseFromSchedulerStatus = true;
            }
            CustomerScreeningRS apiResp = new CustomerScreeningRS();
            try
            {
                if (_CustomerCaseDTO.IsWhiteListed != "YES")
                {
                    string cleanedString = screeningrq.CUSTOMERFULLNAME;
                    screeningrq.CUSTOMERFULLNAME = GetCleanedString(cleanedString,true);
                    //Calling Screening API
                    DateTime currentDateTime = DateTime.Now;

                    Console.WriteLine($"Starts with api scheduler calling : {currentDateTime}");
                    //Console.WriteLine("Starts with api scheduler calling : ", + currentDateTime);
                    
                    var response = await AMLUtility.ScreeningAPICall(screeningrq, ScreeningService.CUSTOMER_SCREENING, baseUrl);
                    if (!string.IsNullOrEmpty(response))
                        apiResp = JsonConvert.DeserializeObject<CustomerScreeningRS>(response);
                }
            }
            catch (Exception Ex)
            {
                Console.WriteLine(Ex.Message);
            }
            int caseId = 0;
            if (apiResp.IsNotNullOrEmpty() && apiResp.caseId.IsNotNullOrEmpty() && Convert.ToInt32(apiResp.caseId) > 0)
            {
                caseId = Convert.ToInt32(apiResp.caseId);
            }
            _CustomerCaseDTO.Status = CaseFromSchedulerStatus ? _CustomerCaseDTO.Status : 5;
            var previousStatus = _CustomerCaseDTO.Status;
            if (apiResp.IsNotNullOrEmpty() && apiResp.data.IsNotNullOrEmpty())
            {

                if (apiResp.data.matchtype != "KYC6")
                {
                    _CustomerCaseDTO.RiskScore = apiResp.riskScore;
                    _CustomerCaseDTO.MatchCategory = apiResp.data.matchcategory;
                    _CustomerCaseDTO.Source = (_CustomerCaseDTO.IsWhiteListed != "YES") ? apiResp.data.matchtype : "WHITELIST";
                    _CustomerCaseDTO.SourceUniqueId = apiResp.data.matchuid;
                    _CustomerCaseDTO.MatchScore = Convert.ToInt32(apiResp.data.matchscore);
                    _CustomerCaseDTO.MatchType = apiResp.data.matchtype;
                    _CustomerCaseDTO.Status = _CustomerCaseDTO.MatchScore.IsNotNullOrEmpty() ? (_CustomerCaseDTO.MatchScore < checkThreshold ? 5 : 0) : 5;// matching percentage threshold should form client settings
                    _CustomerCaseDTO.IsMatched = _CustomerCaseDTO.Status == 0 ? 1 : 0;
                   
                }
                else
                {
                    _CustomerCaseDTO.RiskScore = _CustomerCaseDTO.RiskScore;
                    _CustomerCaseDTO.MatchCategory = _CustomerCaseDTO.MatchCategory;
                    _CustomerCaseDTO.Source = _CustomerCaseDTO.Source;
                    _CustomerCaseDTO.SourceUniqueId = _CustomerCaseDTO.SourceUniqueId;
                    _CustomerCaseDTO.MatchScore = Convert.ToInt32(_CustomerCaseDTO.MatchScore);
                    _CustomerCaseDTO.MatchType = _CustomerCaseDTO.MatchType;
                    _CustomerCaseDTO.Status = _CustomerCaseDTO.Status;// matching percentage threshold should form client settings
                    _CustomerCaseDTO.IsMatched = _CustomerCaseDTO.IsMatched;

                }


                if (CaseFromSchedulerStatus)
                {
                    if (_CustomerCaseDTO.Status == 0)
                    {
                        _CustomerCaseDTO.Status = 6;
                    }
                    //else
                    //{
                    //    _CustomerCaseDTO.Status = previousStatus;
                    //}
                }

            }

            var updateRespose = _customerCaseService.Update(_CustomerCaseDTO);
            if (_CustomerCaseDTO.IsMatched == 1 && _CustomerCaseDTO.Status == 6)
            {
                try
                {
                    var appBaseUrl = MyHttpContext.AppBaseUrl;
                    //Send Email notification for risk creation
                    emailBody = emailBody.Replace("{customerID}", _CustomerCaseDTO.CustomerId.ToString());
                    emailBody = emailBody.Replace("{caseID}", _CustomerCaseDTO.Id.ToString());
                    emailBody = emailBody.Replace("{baseUrl}", appBaseUrl);

                   // var emailSent = await SendHtmlFormattedEmail("Risk creation Alert", emailBody, _CustomerCaseDTO.ClientId);
                }
                catch (Exception ex)
                {
                    //throw ex;
                }

            }
            return _CustomerCaseDTO;
        }
        private string GetCleanedString(string name, bool toupper = true)
        {
            if (string.IsNullOrEmpty(name)) { return ""; }

            if (toupper) { name = name.ToUpper(); }

            name = name.Trim();

            name = Regex.Replace(name, @"\s+", " ");
            name = Regex.Replace(name, @"[^A-Z\d\s]", " ");

            return name;
        }
        private Predicate<CountryDTO> CleadedEqualityChecker(string name)
        {
            return (country) =>
                Regex.Replace(country.Name.ToLower().Trim(), @"(\s)+", "$1") ==
                Regex.Replace(name.ToLower().Trim(), @"(\s)+", "$1");
        }

        private string GetTwoCharName(string name)
        {
            List<CountryDTO> countrylist = _countryRepository.GetAll(0).Result;

            CountryDTO country = countrylist.Find(CleadedEqualityChecker(name));
            if (country.IsNotNullOrEmpty())
            {
                return country.Code;
            }
            log.Warn($"Ignoring country {name.ToLower().Trim()} (Not in DB)");
            return null;
        }

        private C6ScreeningRQ GetC6ScreeningRequest(CustomerCaseDTO _CustomerCaseDTO)
        {
            C6ScreeningRQ screeningrq = new C6ScreeningRQ
            {
                Username = _c6Username,
                threshold = _CustomerCaseDTO.Threshold,
                datasets = new List<string>(),
                name = string.Concat(_CustomerCaseDTO.FirstName, " ", _CustomerCaseDTO.MiddleName, " ", _CustomerCaseDTO.LastName," ", _CustomerCaseDTO.onb_name),
            };

            if (screeningrq.Username == null)
            {
                ClientMasterDTO clientMasterDTO = _customerCaseService.GetClientDetailsByID(_CustomerCaseDTO.ClientId);
                screeningrq.Username = clientMasterDTO.C6Username;
            }

            if (_CustomerCaseDTO.DOB.Ticks > 0)
            {
                screeningrq.dob = _CustomerCaseDTO.DOB.ToString("yyyy-MM-dd");
                if (_CustomerCaseDTO.BirthYear.IsNotNullOrEmpty())
                {
                    screeningrq.dobMatching = _CustomerCaseDTO.BirthYear;
                }
            }
            if (_CustomerCaseDTO.Nationality.IsNotNullOrEmpty())
            {
                string parsedCountry = GetTwoCharName(_CustomerCaseDTO.Nationality);
                if (parsedCountry.IsNotNullOrEmpty())
                {
                    screeningrq.countries = new List<string>() { parsedCountry };
                }
            }
            if (_CustomerCaseDTO.Gender.IsNotNullOrEmpty() && _CustomerCaseDTO.CustomerType != "C")
            {
                screeningrq.gender = _CustomerCaseDTO.Gender;
            }

            if (_CustomerCaseDTO.IsPep) { screeningrq.datasets.Add("PEP"); }
            if (_CustomerCaseDTO.IsSan) { screeningrq.datasets.Add("SAN"); }
            if (_CustomerCaseDTO.IsRre) { screeningrq.datasets.Add("RRE"); }
            if (_CustomerCaseDTO.IsIns) { screeningrq.datasets.Add("INS"); }
            if (_CustomerCaseDTO.CustomerType != "C" && _CustomerCaseDTO.IsDd) { screeningrq.datasets.Add("DD"); }
            if (_CustomerCaseDTO.IsPoi) { screeningrq.datasets.Add("POI"); }
            if (_CustomerCaseDTO.IsRel) { screeningrq.datasets.Add("REL"); }

            if (screeningrq.datasets.Count == 0)
            {
                log.Warn("No dataset selected defaulting to all");
                screeningrq.datasets.Add("PEP");
                screeningrq.datasets.Add("SAN");
                screeningrq.datasets.Add("RRE");
                screeningrq.datasets.Add("INS");
                if (_CustomerCaseDTO.CustomerType != "C") { screeningrq.datasets.Add("DD"); }
                screeningrq.datasets.Add("POI");
                screeningrq.datasets.Add("REL");
            }

            return screeningrq;
        }

        private async Task<CustomerCaseDTO> C6Screening(CustomerCaseDTO _CustomerCaseDTO, string baseUrl, string callFrom = null, string emailBody = null, int cid = -1)
        {
            log.Debug($"Received customer type {_CustomerCaseDTO.CustomerType}");
            log.Debug($"Received customer id {_CustomerCaseDTO.Id} and cid {cid}");
            if (_CustomerCaseDTO.Id == 0 && cid > 0)
            {
                _CustomerCaseDTO.Id = _customerCaseService.GetCaseFullDetailsByCustId(cid).Id;
                log.Debug($"Received customer id {_CustomerCaseDTO.Id} from db");
            }

            C6ScreeningRQ screeningRequest = GetC6ScreeningRequest(_CustomerCaseDTO);
            C6ScreeningRS apiResp = new C6ScreeningRS();

            bool corporate = false;

            if (_CustomerCaseDTO.CustomerType == "C")
            {
                corporate = true;
            }
            //try
            //{
                log.Debug($"Parsed screening request:\n{JsonConvert.SerializeObject(screeningRequest, Formatting.Indented)}");
                (bool exists, List<NAMELIST> response, List<NAMELIST> response2) IsBlackListed = _freeSourceRepository.SearchNameList(screeningRequest.name.Trim(), corporate,_CustomerCaseDTO.ClientId);
            //if (IsBlackListed.response == null || IsBlackListed.response2 == null && IsBlackListed.response2.Count() == 0)
            //{
            //    var searchType = "F";
            //    IsBlackListed = await AMLUtility.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
            //    {
            //        customerdob = _CustomerCaseDTO.DOB.ParseDBDate("dd MMM yyyy", "01 jan 1970"),
            //        customerfullname = screeningRequest.name.Trim(),
            //        customernationality = _CustomerCaseDTO.Nationality,
            //        searchtype = searchType
            //    }, ScreeningService.GETFUZZYBACKLIST, baseapiURL).Result;

            //}



            checkThreshold = _CustomerCaseDTO.Threshold;
                log.Debug($"C6 Threshold: {_CustomerCaseDTO.C6Threshold}, Threshhold: {_CustomerCaseDTO.Threshold}");

                try
                {
                if (_CustomerCaseDTO.IsWhiteListed != "YES")
                {
                    log.Debug($"Send customer details to C6 ({baseUrl}) for screening");
                        string response = await AMLUtility.C6ScreeningAPICall(screeningRequest, _CustomerCaseDTO.CustomerType == "C" ? ScreeningService.C6SCREENINGCORPORATE : ScreeningService.C6SCREENING, baseUrl);
                        log.Info($"Got response {(response.Length > 100 ? response.Substring(0, 100) + "..." : response)}");

                        if (response.IsNotNullOrEmpty())
                        {
                            apiResp = JsonConvert.DeserializeObject<C6ScreeningRS>(response);
                            //var apiresults = JsonConvert.DeserializeObject<AML.DTO.DTO.CustomerCase.Root>(response);
                            log.Debug($"Got {apiResp?.users?.results?.matchCount} hits");
                        }
                        else if (response == null)
                        {
                            throw new Exception("Could not get valid response from KYC6 servers.");
                        }
                    }
                }
                catch (Exception Ex)
                {
                    log.Error(Ex);
                }
            //         }
            //         catch (Exception ex) 
            //         {
            //	log.Debug(ex);
            //}

            bool blacklistedOrKYCHit = IsBlackListed.exists ||
                (
                    apiResp.IsNotNullOrEmpty() &&
                    apiResp.users.IsNotNullOrEmpty() &&
                    apiResp.users.results.matches.IsNotNullOrEmpty() &&
                    apiResp.users.results.matchCount > 0
                );

            _CustomerCaseDTO.Status = 5;

           if (blacklistedOrKYCHit) {
            _CustomerCaseDTO.IsMatched = ((apiResp.IsNotNullOrEmpty() && IsBlackListed.exists) || apiResp.users.results.matchCount > 0) ? 1 : 0; 
        }
            if (_CustomerCaseDTO.IsWhiteListed == "YES") { _CustomerCaseDTO.Source = "WHITELIST"; }
            /*Code added by sanjana*/
            //if (apiResp.users.results.matches.Count() != 0)
            //{
                _CustomerCaseDTO.Source = "KYC6";
                _CustomerCaseDTO.MatchScore = (apiResp.users.results.matches.Count() > 0 ? apiResp.users.results.matches.FirstOrDefault().score : 0);
                _CustomerCaseDTO.SourceUniqueId = Convert.ToString(apiResp.users.results.matches.Count() > 0 ? Convert.ToString(apiResp.users.results.matches.FirstOrDefault().qrCode) : null);
                _CustomerCaseDTO.Status = _CustomerCaseDTO.MatchScore.IsNotNullOrEmpty() ? (_CustomerCaseDTO.MatchScore < checkThreshold ? 5 : 0) : 5;
            //var firstMatch = apiResp.users.results.matches?.FirstOrDefault();
            //var firstDataset = firstMatch?.datasets?.FirstOrDefault();
            //_CustomerCaseDTO.CaseChangeStatus = firstDataset?.ToString();
            var firstMatch = apiResp.users.results.matches?.FirstOrDefault();

            _CustomerCaseDTO.CaseChangeStatus =
                firstMatch?.datasets != null && firstMatch.datasets.Any()
                    ? string.Join(", ", firstMatch.datasets)
                    : "--";
            //}

            if (IsBlackListed.response != null &&  IsBlackListed.response.Count() != 0 || IsBlackListed.response2 != null && IsBlackListed.response2.Count() != 0 )
            {
                if (IsBlackListed.response != null && IsBlackListed.response.Count() != 0)
                {
                    _CustomerCaseDTO.Source = IsBlackListed.exists ? (IsBlackListed.response[0].TYPE) : "KYC6";
                    _CustomerCaseDTO.MatchScore = IsBlackListed.exists ? 100 : (apiResp.users.results.matches.Count() > 0 ? apiResp.users.results.matches.FirstOrDefault().score : 0);
                    _CustomerCaseDTO.SourceUniqueId = IsBlackListed.exists ? IsBlackListed.response[0].TYPE : Convert.ToString(apiResp.users.results.matches.Count() > 0 ? Convert.ToString(apiResp.users.results.matches.FirstOrDefault().qrCode) : null);
                    _CustomerCaseDTO.Status = _CustomerCaseDTO.MatchScore.IsNotNullOrEmpty() ? (_CustomerCaseDTO.MatchScore < checkThreshold ? 5 : 0) : 5;
                    _CustomerCaseDTO.CaseChangeStatus= IsBlackListed.exists ? (IsBlackListed.response[0].TYPE) : "KYC6";
                }
                if (IsBlackListed.response2 != null && IsBlackListed.response2.Count() != 0)
                {
                    _CustomerCaseDTO.Source = IsBlackListed.exists ? (IsBlackListed.response2[0].TYPE) : "KYC6";
                    _CustomerCaseDTO.MatchScore = IsBlackListed.exists ? 100 : (apiResp.users.results.matches.Count() > 0 ? apiResp.users.results.matches.FirstOrDefault().score : 0);
                    _CustomerCaseDTO.SourceUniqueId = IsBlackListed.exists ? IsBlackListed.response2[0].TYPE : Convert.ToString(apiResp.users.results.matches.Count() > 0 ? Convert.ToString(apiResp.users.results.matches.FirstOrDefault().qrCode) : null);
                    _CustomerCaseDTO.Status = _CustomerCaseDTO.MatchScore.IsNotNullOrEmpty() ? (_CustomerCaseDTO.MatchScore < checkThreshold ? 5 : 0) : 5;
                    _CustomerCaseDTO.CaseChangeStatus = IsBlackListed.exists ? (IsBlackListed.response2[0].TYPE) : "KYC6";

                }
            }
            
           
            



            //if (blacklistedOrKYCHit)
            //{
            //_CustomerCaseDTO.MatchScore = IsBlackListed.exists ? 100 : (apiResp.users.results.matches.Count() > 0 ? apiResp.users.results.matches.FirstOrDefault().score : 0);
            //    _CustomerCaseDTO.SourceUniqueId = IsBlackListed.exists ? IsBlackListed.response.TYPE : Convert.ToString(apiResp.users.results.matches.Count() > 0 ? Convert.ToString(apiResp.users.results.matches.FirstOrDefault().qrCode) : null);
            //    _CustomerCaseDTO.Status = _CustomerCaseDTO.MatchScore.IsNotNullOrEmpty() ? (_CustomerCaseDTO.MatchScore < checkThreshold ? 5 : 0) : 5;




            List<ApiResultModel> resultList = new List<ApiResultModel>();

            //if (!IsBlackListed.exists)
            //{
                try
                    {
                        log.Debug("Add all not null or empty C6 results to ApiResultsjson");

                        foreach (DTO.DTO.CustomerScreening.Match item in apiResp.users.results.matches)
                        {
                            var results = new ApiResultModel();
                            if (item.IsNotNullOrEmpty())
                            {
                                results.matchuid = item.qrCode.ToString();
                                results.matchtype = "KYC6";
                                results.matchcategory = callFrom;
                                results.matchname = item.name.ToUpper();
                                results.matchscore = item.score.ToString();
                                results.nationality = item.countries.IsNotNullOrEmpty() ? item.countries.FirstOrDefault() : string.Empty;
                                results.matchidnumber = item.qrCode.ToString();
                                results.matchdob = item.datesOfBirth != null ? item.datesOfBirth.FirstOrDefault() : "";

                                results.matchresourcesid = item.resourceId;
                                results.matchdatasets = item.datasets != null ? item.datasets.FirstOrDefault() : "--";
                                results.matchgender = item.gender != null ? item.gender : "";




                    }
                            resultList.Add(results);
                        }

                        log.Debug($"Added {resultList.Count} results");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            //    }

            _CustomerCaseDTO.ApiResultsjson = resultList;

                var matchrecordsList = new List<MatchRecordsDTO>();
                var stat = 1;
            var matchrecords = new MatchRecordsDTO();

            if (IsBlackListed.exists) 
            {
                
                /*Code added by sanjana*/
                if (IsBlackListed.response != null)
                {
                    log.Debug("Add single top result");

                    //stat = 1;

                    foreach (var item in IsBlackListed.response)
                    {
                        var matchrecord = new MatchRecordsDTO();
                        matchrecord.MATCHUID = item.UID.ToString();
                        matchrecord.MATCHTYPE =item.TYPE;
                        matchrecord.MATCHCATEGORY = callFrom;
                        matchrecord.MATCHNAME = item.FULLNAME;
                        matchrecord.MATCHSCORE = 100;
                        matchrecord.MATCHNATIONALITY = item.NATIONALITY;
                        matchrecord.MATCHIDNO = item.IDDETAILS.Count > 0 ? item.IDDETAILS[0].IDNUMBER : string.Empty;
                        matchrecord.MATCHDOB = item.DOB.Count > 0 ? item.DOB[0].DOB : string.Empty;
                        matchrecord.REMARKS = item.REMARKS;
                        matchrecord.MATCHDATASETS = !string.IsNullOrWhiteSpace(item.TYPE) ? item.TYPE : "--";


                        // matchrecordsList.Add(matchrecords);

                        matchrecordsList.Add(matchrecord);
                        stat++;
                    }
                }
                if (IsBlackListed.response2 != null && IsBlackListed.response2.Count() != 0)
                {
                    log.Debug("Add single top result");

                    //stat = 1;
                    //var matchrecord = new MatchRecordsDTO();
                    foreach (var item in IsBlackListed.response2)
                    {
                        var matchrecord = new MatchRecordsDTO();

                        matchrecord.MATCHUID = item.UID.ToString();
                        matchrecord.MATCHTYPE = item.TYPE;
                        matchrecord.MATCHCATEGORY = callFrom;
                        matchrecord.MATCHNAME = item.FULLNAME;
                        matchrecord.MATCHSCORE = 100;
                        matchrecord.MATCHNATIONALITY = item.NATIONALITY;
                        matchrecord.MATCHIDNO = item.IDDETAILS.Count > 0 ? item.IDDETAILS[0].IDNUMBER : string.Empty;
                        matchrecord.MATCHDOB = item.DOB.Count > 0 ? item.DOB[0].DOB : string.Empty;
                        matchrecord.MATCHDATASETS = !string.IsNullOrWhiteSpace(item.TYPE) ? item.TYPE : "--";
                        //matchrecord.REMARKS = IsBlackListed.response2.REMARKS;

                        // matchrecordsList.Add(matchrecords);

                        matchrecordsList.Add(matchrecord);
                        stat++;
                    }

                }
                    
            }

            //if (!IsBlackListed.exists)
            //{

                log.Debug($"Get C6 results below threshold ({checkThreshold}) for CaseLogUnderThreshold");

                    foreach (var item in apiResp.users.results.matches)
                    {
                    matchrecords = new MatchRecordsDTO();
                if (item.IsNotNullOrEmpty())
                        {
                            matchrecords.MATCHUID = item.qrCode.ToString();
                            matchrecords.MATCHTYPE = "KYC6";
                            matchrecords.MATCHCATEGORY = callFrom;
                            matchrecords.MATCHNAME = item.name.ToUpper();
                            matchrecords.MATCHSCORE = item.score;
                            matchrecords.MATCHNATIONALITY = item.countries.IsNotNullOrEmpty() ? item.countries.FirstOrDefault() : string.Empty;
                            matchrecords.MATCHIDNO = item.qrCode.ToString();
                            matchrecords.MATCHDOB = item.datesOfBirth != null ? item.datesOfBirth.FirstOrDefault() : "";
                            matchrecords.MATCHRESOURCESID = item.resourceId;
                            matchrecords.MATCHDATASETS = item.datasets != null && item.datasets.Any() ? string.Join(", ", item.datasets) : "--";
                            matchrecords.MATCHGENDER = item.gender != null ? item.gender : "";


                    if (item.score < checkThreshold)
                            {
                                matchrecordsList.Add(matchrecords);
                            }
                            else
                            {
                                stat = 0;
                            }
                        }
                    }

                    log.Debug($"Got {matchrecordsList.Count} results");
            //}
            //else
            //{
            //    stat = 0;
            //}

            //if (stat != 0)
            //{
            //    CASELOG modelCaseLog = new CASELOG
            //    {
            //        CASEID = _CustomerCaseDTO.Id.ToString(),
            //        MATCHRECORDS = matchrecordsList
            //    };

            //    log.Debug("Add case log under threshold to MongoDB");
            //    _freeSourceRepository.InsertCaseLogUnderThreshold(modelCaseLog);
            //}
        //}

        log.Debug("Update customer case table with details after screening");
            _customerCaseService.Update(_CustomerCaseDTO);

            if (blacklistedOrKYCHit)
            {
                //var matchrecordsList = new List<MatchRecordsDTO>();
                //var stat = 0;

                //if (!IsBlackListed.exists)
                //{
                    log.Debug($"Get C6 results above threshold ({checkThreshold}) for CaseLog");

                    foreach (var item in apiResp.users.results.matches)
                    {
                        matchrecords = new MatchRecordsDTO();
                        if (item.IsNotNullOrEmpty())
                        {
                            matchrecords.MATCHUID = item.qrCode.ToString();
                            matchrecords.MATCHTYPE = "KYC6";
                            matchrecords.MATCHCATEGORY = callFrom;
                            matchrecords.MATCHNAME = item.name.ToUpper();
                            matchrecords.MATCHSCORE = item.score;
                            matchrecords.MATCHNATIONALITY = item.countries.IsNotNullOrEmpty() ? item.countries.FirstOrDefault() : string.Empty;
                            matchrecords.MATCHIDNO = item.qrCode.ToString();
                            matchrecords.MATCHDOB = item.datesOfBirth != null ? item.datesOfBirth.FirstOrDefault() : "";
                            matchrecords.MATCHRESOURCESID = item.resourceId;
                            matchrecords.MATCHDATASETS = item.datasets != null && item.datasets.Any() ? string.Join(", ", item.datasets) : "--";
                            matchrecords.MATCHGENDER = item.gender != null ? item.gender : "";


                        // matchrecordsList.Add(matchrecords);

                        if (item.score >= checkThreshold)
                            {
                                matchrecordsList.Add(matchrecords);
                                stat++;
                            }
                        }
                    }

                    log.Debug($"Got {matchrecordsList.Count} results");
                //}
            }
                //else
                //{
                    //log.Debug("Add single top result");

                    ////stat = 1;
                    //var matchrecord = new MatchRecordsDTO();
                    
                    //matchrecord.MATCHUID = IsBlackListed.response.UID.ToString();
                    //matchrecord.MATCHTYPE = IsBlackListed.response.TYPE;
                    //matchrecord.MATCHCATEGORY = callFrom;
                    //matchrecord.MATCHNAME = IsBlackListed.response.FULLNAME;
                    //matchrecord.MATCHSCORE = 100;
                    //matchrecord.MATCHNATIONALITY = IsBlackListed.response.NATIONALITY;
                    //matchrecord.MATCHIDNO = IsBlackListed.response.IDDETAILS.Count > 0 ? IsBlackListed.response.IDDETAILS[0].IDNUMBER : string.Empty;
                    //matchrecord.MATCHDOB = IsBlackListed.response.DOB.Count > 0 ? IsBlackListed.response.DOB[0].DOB : string.Empty;

                    //// matchrecordsList.Add(matchrecords);

                    //matchrecordsList.Add(matchrecord);
                    //stat++;

                //}

                //insert caselog in mongodb
                if (stat >= 1)
                {
                    CASELOG modelCaseLog = new CASELOG
                    {
                        CASEID = _CustomerCaseDTO.Id.ToString(),
                        MATCHRECORDS = matchrecordsList
                    };

                    log.Debug("Add case log to MongoDB");
                    _freeSourceRepository.InsertCaseLog(modelCaseLog);
                }

                try
                {
                    log.Debug("Create formatted email body");

                    //Send Email notification for risk creation
                    var appBaseUrl = MyHttpContext.AppBaseUrl;
                    emailBody = emailBody.Replace("{customerID}", _CustomerCaseDTO.CustomerId.ToString());
                    emailBody = emailBody.Replace("{caseID}", _CustomerCaseDTO.Id.ToString());
                    emailBody = emailBody.Replace("{baseUrl}", appBaseUrl);
                    _CustomerCaseDTO.sendMail = 1;
                    //var emailSent = await SendHtmlFormattedEmail("Risk creation Alert", emailBody);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    //throw ex;
                }
            //}

            if (IsBlackListed.exists)
            {
                /*Code added by sanjana*/
                if (IsBlackListed.response != null && IsBlackListed.response.Count() != 0)
                {
                    _CustomerCaseDTO.Source = IsBlackListed.response[0].TYPE;
                }
                if(IsBlackListed.response2 != null && IsBlackListed.response2.Count() != 0)
                {
                    _CustomerCaseDTO.Source = IsBlackListed.response2[0].TYPE;
                }
            }

            return _CustomerCaseDTO;
        }

        public void UpdateSanctionRecords(CustomerCaseDTO _CustomerCaseDTO, List<ApiResultModel> apiResp, string cid)
        {
            string id;
            if (_CustomerCaseDTO.Id == 0 && cid != "")
            {
                id = cid;
                _CustomerCaseDTO.Id = _customerCaseService.GetCaseId(id);
            }
            else
            {
                id = _CustomerCaseDTO.Id.ToString();
            }

            

            bool isRecordCreated = false;
            string name = string.Empty;
            if(null!=_CustomerCaseDTO.FirstName)
            {
                name = _CustomerCaseDTO.FirstName;
            }
            if(null!=_CustomerCaseDTO.LastName)
            {
                name = _CustomerCaseDTO.LastName;
            }
            (bool exists, List<NAMELIST> response, List<NAMELIST> response2) IsBlackListed = _freeSourceRepository.SearchNameList(name, false,_CustomerCaseDTO.ClientId);

            _CustomerCaseDTO.Status = 5;

            if (true) { _CustomerCaseDTO.IsMatched = ((apiResp.IsNotNullOrEmpty() && IsBlackListed.exists) || apiResp.Count > 0) ? 1 : 0; }
            if (_CustomerCaseDTO.IsWhiteListed == "YES") { _CustomerCaseDTO.Source = "WHITELIST"; }



            if (true)
            {

                if (IsBlackListed.response != null && IsBlackListed.response.Count() != 0)
                {
                        _CustomerCaseDTO.MatchScore = IsBlackListed.exists ? 100 : Convert.ToInt32(apiResp.FirstOrDefault().matchscore);
                    _CustomerCaseDTO.SourceUniqueId = IsBlackListed.exists ? IsBlackListed.response[0].TYPE : Convert.ToString(apiResp.FirstOrDefault().matchuid);
                    _CustomerCaseDTO.Status = _CustomerCaseDTO.MatchScore.IsNotNullOrEmpty() ? (_CustomerCaseDTO.MatchScore < checkThreshold ? 5 : 0) : 5;
                    _CustomerCaseDTO.Source = IsBlackListed.exists ? IsBlackListed.response[0].TYPE : apiResp.FirstOrDefault().matchtype;

                }
                else
                {
                    _CustomerCaseDTO.MatchScore = IsBlackListed.exists ? 100 : Convert.ToInt32(apiResp.FirstOrDefault().matchscore);
                    _CustomerCaseDTO.SourceUniqueId = IsBlackListed.exists ? IsBlackListed.response2[0].TYPE : Convert.ToString(apiResp.FirstOrDefault().matchuid);
                    _CustomerCaseDTO.Status = _CustomerCaseDTO.MatchScore.IsNotNullOrEmpty() ? (_CustomerCaseDTO.MatchScore < checkThreshold ? 5 : 0) : 5;
                    _CustomerCaseDTO.Source = IsBlackListed.exists ? IsBlackListed.response2[0].TYPE : apiResp.FirstOrDefault().matchtype;
                }
                

                _CustomerCaseDTO.ApiResultsjson = apiResp;

                var matchrecordsList = new List<MatchRecordsDTO>();
                var stat = 0;
                if (!IsBlackListed.exists)
                {
                    log.Debug($"Get C6 results below threshold ({checkThreshold}) for CaseLogUnderThreshold");

                    foreach (var item in apiResp)
                    {

                        var matchrecords = new MatchRecordsDTO();
                        if (item.IsNotNullOrEmpty())
                        {
                            matchrecords.MATCHUID = item.matchuid.ToString();
                            matchrecords.MATCHTYPE = item.matchtype;// "KYC6";
                            matchrecords.MATCHCATEGORY = item.matchcategory;
                            matchrecords.MATCHNAME = item.matchname.ToUpper();
                            matchrecords.MATCHSCORE = Convert.ToInt32(item.matchscore);
                            matchrecords.MATCHNATIONALITY = item.nationality.IsNotNullOrEmpty() ? item.nationality.FirstOrDefault().ToString() : string.Empty;
                            matchrecords.MATCHIDNO = item.matchidnumber != null ? item.matchidnumber.ToString() : String.Empty;
                            matchrecords.MATCHDOB = item.matchdob != null ? item.matchdob.FirstOrDefault().ToString() : "";

                            if (Convert.ToInt32(item.matchscore) >= checkThreshold)
                            {
                                matchrecordsList.Add(matchrecords);
                                stat++;
                            }

                            //if (Convert.ToInt32(item.matchscore) < checkThreshold)
                            //{
                            //    matchrecordsList.Add(matchrecords);
                            //    stat = 1;
                            //}
                            //else
                            //{
                            //    stat = 0;
                            //}
                        }
                    }

                    log.Debug($"Got {matchrecordsList.Count} results");
                }
                else
                {
                    stat = 0;
                }

                if (stat >= 1)
                {
                    CASELOG modelCaseLog = new CASELOG
                    {
                        CASEID = _CustomerCaseDTO.Id.ToString(),
                        MATCHRECORDS = matchrecordsList
                    };

                    log.Debug("Add case log under threshold to MongoDB");
                    _freeSourceRepository.InsertCaseLog(modelCaseLog);
                }
                try
                {

                    _CustomerCaseDTO.sendMail = 1;
                    //var emailSent = await SendHtmlFormattedEmail("Risk creation Alert", emailBody);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    //throw ex;
                }
            }

            log.Debug("Update customer case table with details after screening");

            _customerCaseService.Update(_CustomerCaseDTO);
            /*if(_CustomerCaseDTO.ApiResultsjson.Count>0)
            {
                isRecordCreated = true;
              
            }
            return isRecordCreated;*/
            //throw new NotImplementedException();
        }

        public async Task<bool> SendHtmlFormattedEmailWithAttachment(string subject, string body, string file, byte[] bytes1)
        {
            try
            {
                var clientID = _httpContextAccessor.HttpContext.Session.GetString("SessClientId").ParseInt();
                var emailDetails = _commonRepository.GetConfigurationDetailsByGroup("EM", clientID);
                var from = emailDetails.Where(x => x.CategoryKey == "E_FROM").Select(_ => _.CategoryValue).FirstOrDefault();
                var alias = emailDetails.Where(x => x.CategoryKey == "E_ALIAS").Select(_ => _.CategoryValue).FirstOrDefault();
                var userid = emailDetails.Where(x => x.CategoryKey == "E_USER").Select(_ => _.CategoryValue).FirstOrDefault();
                var password = emailDetails.Where(x => x.CategoryKey == "E_PWD").Select(_ => _.CategoryValue).FirstOrDefault();
                var host = emailDetails.Where(x => x.CategoryKey == "E_HOST").Select(_ => _.CategoryValue).FirstOrDefault();
                var port = emailDetails.Where(x => x.CategoryKey == "E_PORT").Select(_ => _.CategoryValue).FirstOrDefault();
                var to = emailDetails.Where(x => x.CategoryKey == "COMPLEM").Select(_ => _.CategoryValue).FirstOrDefault();

                using (MailMessage mailMessage = new MailMessage())
                {
                    mailMessage.From = new MailAddress(from, alias);
                    mailMessage.Subject = subject;
                    mailMessage.Body = body;


                    mailMessage.IsBodyHtml = true;
                    //mailMessage.To.Add(new MailAddress(to));
                    mailMessage.To.Add(to); //for adding multiple email ids


                    StringReader sr = new StringReader(file);
                    Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                    HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
                        pdfDoc.Open();
                        htmlparser.Parse(sr);
                        pdfDoc.Close();
                        byte[] bytes = memoryStream.ToArray();
                        memoryStream.Close();
                        mailMessage.Attachments.Add(new Attachment(new MemoryStream(bytes1), "PotentialHits.pdf"));
                    }
                    //mailMessage.Attachments.Add(new Attachment(MemoryStream(byte)))


                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = host;
                    smtp.EnableSsl = true;
                    System.Net.NetworkCredential NetworkCred = new System.Net.NetworkCredential();
                    NetworkCred.UserName = userid;
                    NetworkCred.Password = password;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = NetworkCred;
                    smtp.Port = int.Parse(port);
                    smtp.Send(mailMessage);
//#if !DEBUG
//                    smtp.Send(mailMessage);
//#endif
                }

                return true;
            }
            catch (Exception ex)
            {
                //throw ex;
                return false;
            }
        }
        public async Task<CustomerCaseDTO> ApprovedListScreeningCall(string _newCustMasterId, string baseUrl, string baseC6Url, string callFrom = null, string emailBody = null)
        {
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetCaseFullDetailsByCustId(_newCustMasterId);
            return await this.FreeSourceScreening(_CustomerCaseDTO, baseUrl, callFrom, emailBody);
        }

        public async Task<TokenRS> CreateC6Token(string url, string baseURL)
        {
            return await AMLUtility.CreateC6Token(url, baseURL, _c6Username);
        }

        public ApiAuthResponse Authenticate(ApiAuthRequest model)
        {
            var username = model.Username;
            var password = model.Password;
            var companyName = model.CompanyName;
            bool isAuth = false;

            var response = _userRepository.GetApiUserDetails(username, password, companyName);
            // return null if user not found
            if (response.Result == null)
                return null;

            isAuth = true;
            // authentication successful so generate jwt and refresh tokens
            var jwtToken = generateJwtToken(model);

            var res = new ApiAuthResponse(isAuth, username, jwtToken);

            return res;
        }

        public string generateJwtToken(ApiAuthRequest model)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(model.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, model.Username.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        public class MyHttpContext
        {
            private static IHttpContextAccessor m_httpContextAccessor;

            public static HttpContext Current => m_httpContextAccessor.HttpContext;

            public static string AppBaseUrl => $"{Current.Request.Scheme}://{Current.Request.Host}{Current.Request.PathBase}";

            internal static void Configure(IHttpContextAccessor contextAccessor)
            {
                m_httpContextAccessor = contextAccessor;
            }
        }
        public bool UpdateCaseRemark(int caseId, List<DataListModel> model)
        {
            var matchrecordsList = new List<MatchRecordsDTO>();
            for (int i = 0; i < model.Count; i++)
            {


                var matchrecords = new MatchRecordsDTO();
                matchrecords.MATCHUID = model[i].matchidno;
                matchrecords.MATCHTYPE = model[i].matchtype;
                matchrecords.MATCHCATEGORY = model[i].matchcategory;
                matchrecords.MATCHNAME = model[i].matchname;
                matchrecords.MATCHSCORE = model[i].matchscore.ParseInt();
                matchrecords.MATCHNATIONALITY = model[i].matchnationality;
                matchrecords.REMARKS = model[i].remarks;
                matchrecords.SEARCHTYPES = model[i].searchTypes;
                matchrecordsList.Add(matchrecords);
            }
            CASELOG modelCaseLog = new CASELOG
            {
                CASEID = caseId.ToString(),
                MATCHRECORDS = matchrecordsList
            };
            //   var test =_freeSourceRepository.GetDetailsByCaseId(modelCaseLog.CASEID.ParseInt());
            var res = _freeSourceRepository.UpdateCaseLogRemarks(modelCaseLog);

            return res;
        }

        public async Task<int> ScreenCustomersFromDB(string baseURL)
        {
            var unscreenedCustomers = _customerCaseService.GetUnscreenedCustomers();

            foreach (var unscreenedCustomer in unscreenedCustomers)
            {
                await C6Screening(new CustomerCaseDTO
                {
                    CustomerType = unscreenedCustomer.CustomerType,
                    Threshold = unscreenedCustomer.Threshold > 0 ? unscreenedCustomer.Threshold : _c6Threshold,
                    C6Threshold = unscreenedCustomer.C6Threshold,
                    IsWhiteListed = "NO",
                    IsMatched = 0,
                    Source = "KYC6",
                    MatchScore = 0,
                    Status = 5,
                    SourceUniqueId = "",
                    ApiResultsjson = new List<ApiResultModel>(),
                    CustomerId = unscreenedCustomer.CustomerId,
                    sendMail = 1,
                    FirstName = unscreenedCustomer.FirstName,
                    MiddleName = unscreenedCustomer.MiddleName,
                    LastName = unscreenedCustomer.LastName,
                    DOB = unscreenedCustomer.DOB,
                    BirthYear = null,
                    Nationality = unscreenedCustomer.Nationality,
                    Gender = unscreenedCustomer.Gender,
                    IsPep = true,
                    IsSan = true,
                    IsRre = true,
                    IsIns = true,
                    IsDd = true,
                    IsPoi = true,
                    IsRel = true
                }, baseURL, "DB", "");
            }

            return unscreenedCustomers.Count;
        }

        private TranScreenDTO GetTranScreenDTO(dynamic TmsObject, string CustomerId = null, string TranRefNo = null, string customerSegment = null)
        {

            Type TmsObjectType = TmsObject.GetType();
            //PropertyInfo TmsObjectPropertyRemitter = TmsObjectType.GetProperty("Remitter");
            PropertyInfo TmsObjectPropertyBName = TmsObjectType.GetProperty("Bankname");
            PropertyInfo TmsObjectPropertyCOI = TmsObjectType.GetProperty("CountryOfIncorporation");
            PropertyInfo TmsObjectPropertyDOB = TmsObjectType.GetProperty("Dob");
            try
            {


                TranScreenDTO tranScreenDTO = new TranScreenDTO
                {
                    TranRefNo = TranRefNo,
                    CustRefNo = CustomerId ?? TmsObject.CustomerId,
                    CustType = CustomerId == null ? "I" : "C",
                    Name = TmsObjectPropertyBName == null ? TmsObject.Name : TmsObject.Bankname,
                    Country = TmsObjectPropertyCOI == null ? TmsObject.Nationality : TmsObject.CountryOfIncorporation,
                    //CustomerSegment = customerSegment
                };



                if (TmsObjectPropertyDOB != null && TmsObject.Dob != "")
                {
                    tranScreenDTO.DOB = Convert.ToDateTime(TmsObject.Dob);
                }
                //tranScreenDTO.CreatedBy  = _clientHandler.GetUserId();
                //tranScreenDTO.UpdatedBy = _clientHandler.GetUserId();
                return tranScreenDTO;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return null;
            }

        }

        public async Task<int> ScreenTMSFromDB(string c6BaseURL)
        {
            List<TranScreenDTO> screeningModel = _transactionScreeningService.GetAllUnScreenedTransactions();

            try
            {
                foreach (var model in screeningModel)
                {
                    //Check if the Customer Id provided in API is an alreday whitelisted onboarded customer
                    IsWhiteListedCheckDTO IsWhiteListed = _transactionScreeningService.checkIfCusWhiteListed(model.CustRefNo);
                    if (IsWhiteListed.IsNotNullOrEmpty()) { model.IsWhiteListed = IsWhiteListed.IsWhiteListed; }

                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(@"Views/TransactionScreening/TranRiskEmailBody.html")) { body = reader.ReadToEnd(); };

                    await TransactionScreeningCall(model, baseURL, c6BaseURL, "API", body, model.Id, "Free");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("There was an error while screening: {0}", ex.Message));
            }

            return screeningModel.Count;
        }

        //Transaction MOnitoring 
        public bool UpdateTransactionCaseRemark(int caseId, List<ApiRespModel> model)
        {
            var matchrecordsList = new List<MatchTranRecordsDTO>();
            for (int i = 0; i < model.Count; i++)
            {


                var matchrecords = new MatchTranRecordsDTO();
                matchrecords.MATCHUID = model[i].matchidnumber;
                matchrecords.MATCHTYPE = model[i].matchtype;
                matchrecords.MATCHCATEGORY = model[i].matchcategory;
                matchrecords.MATCHNAME = model[i].matchname;
                matchrecords.MATCHSCORE = model[i].matchscore.ParseInt();
                matchrecords.MATCHNATIONALITY = model[i].nationality;
                matchrecords.REMARKS = model[i].remarks;
                matchrecordsList.Add(matchrecords);
            }
            TRANSACTION_CASELOG modelCaseLog = new TRANSACTION_CASELOG
            {
                CASEID = caseId.ToString(),
                MATCHRECORDS = matchrecordsList
            };
            //   var test =_freeSourceRepository.GetDetailsByCaseId(modelCaseLog.CASEID.ParseInt());
            var res = _freeSourceRepository.UpdateTranCaseLogRemarks(modelCaseLog);

            return res;
        }

        //End Transaction Screening

        
        //    [JwtAuthorize]
        public async Task<bool> ScreenApprovedListByClientId(int client_id)
        {
            
            string respData = "null";
            string potentialhits = "<p>The Potential hits are :</p>";
            string html_table = "<table><thead><tr><th>S.No</th><th>Case ID</th><th>Customer Name</th><th>Date of Initial Screening</th></tr></thead><tbody>";
           
                ClientMasterDTO clientMasterDTO = _customerCaseService.GetAllClients().Find(val => val.ClientId == client_id);
                var result = _customerCaseService.GetCasebyApprovedStatus(client_id);
                CustomerCaseDTO apiResp = new CustomerCaseDTO();
                var count = 0;
                string body = string.Empty;
                try
                {

                    foreach (var item in result)
                    {
                        apiResp = null;
                        using (StreamReader reader = new StreamReader(@"Views/Risk/ApprovedRiskEmailBody.html"))
                        {
                            body = reader.ReadToEnd();
                        };

                        apiResp = await ApprovedListScreeningCall(item.CustomerId, baseURL, c6BaseURL, "API", body);
                        if (apiResp.IsMatched == 1)
                        {
                            count++;
                            html_table += "<tr><td>" + count + "</td><td>" + item.CustomerId + "</td><td>" + item.LastName + "</td><td>" + item.CreatedOn + "</td></tr>";
                        }
                    }
                    html_table += "</tbody></table>";
                    potentialhits += html_table;
                    if (count == 0)
                    {

                        potentialhits = "";
                        html_table = "";
                    }
                }
                catch { }
                try
                {
                    var loginfo = count.ToString() + "/" + result.Count().ToString() + "cases added to approved list on " + DateTime.Now;
                    var msg = new LogEventInfo(LogLevel.Info, "", "Approved List Screening Scheduler Log from  digi scheduler\n" + loginfo);
                    msg.Properties.Add("User", "KYCDigi");
                    log.Info(msg);
                    _customerCaseService.InsertDigiSchedulerLogs(count, result.Count(), client_id);
                    using (StreamReader reader = new StreamReader(@"Views/Risk/ApprovedScreenLogEmailBody.html"))
                    {
                        body = reader.ReadToEnd();
                    }

                    string emailBody = body;
                    emailBody = emailBody.Replace("{Date}", DateTime.Now.ToString());
                    emailBody = emailBody.Replace("{count}", count.ToString());
                    emailBody = emailBody.Replace("{TotalCount}", result.Count().ToString());
                    string actionDisplay = (count == 0) ? "No Action Required" : "Action Required";
                    string alertDisplay = (count == 0) ? "No further action is required from your end" : "Please clear the alerts from the case management section.";
                    emailBody = emailBody.Replace("{alertDisplay}", alertDisplay);
                    emailBody = emailBody.Replace("{ActionDisplay}", actionDisplay);
                    emailBody = emailBody.Replace("{Html_Table}", potentialhits);
                    emailBody = emailBody.Replace("{CompanyName}", clientMasterDTO.ClientName);
                    SendEmailLog(emailBody, client_id, actionDisplay);
                }
                catch { }
                if (result != null)
                {
                    respData += count.ToString() + "/" + result.Count().ToString() + " hits";
                }
            
            string resp = "null";
            resp = respData;

            return true;
        }




        public CreatedAndUpdatedByNames GetCreatedByAndUpdateByNameFromId(int createdById, int updatedById)
        {

            CreatedAndUpdatedByNames createdAndUpdatedNames = _customerCaseService.GetCreatedByAndUpdateByNameFromId(createdById, updatedById);

            return createdAndUpdatedNames;
        }

    }

    public static class HttpContextExtensions
    {
        public static void AddHttpContextAccessor(this IServiceCollection services)
        {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public static IApplicationBuilder UseHttpContext(this IApplicationBuilder app)
        {
            MyHttpContext.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
            return app;
        }

        
    }

}
