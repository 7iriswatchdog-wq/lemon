using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Risk;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.Risk;
using AML.ViewModel.ViewModels.Risk;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AML.Core.Repository.Risk
{
    public class RiskRepository : BaseRepository, IRiskRepository
    {
        public RiskRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }


        public ServiceResponse<int> Create(RiskDTO _riskDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_customercode", _riskDTO.CustomerCode);
                parameters.Add("@p_customername", _riskDTO.CustomerName);
                parameters.Add("@p_dateofassessment", _riskDTO.DateofAssessment);
                parameters.Add("@p_address", _riskDTO.Address);
                parameters.Add("@p_Customer_nationality", _riskDTO.MainNationalityTxt);
                parameters.Add("@p_final_risk_score", _riskDTO.FinalRiskScore);
                parameters.Add("@p_risk_score_sum", _riskDTO.RiskScoreSum);
                parameters.Add("@p_risk_score_count", _riskDTO.RiskScoreCount);
                parameters.Add("@p_risk_score_before_override", _riskDTO.RiskScoreBeforeOverride);
                parameters.Add("@p_product_reference", _riskDTO.ProductReference);
                parameters.Add("@p_product_value", _riskDTO.ProductValue);
                parameters.Add("@p_comments", _riskDTO.RiskComments);
                string riskItemlist = "";
                int index2 = 0;
                for (int i= 0;i < _riskDTO.RiskTypeCategoryDTO.Count;i++)
                {
                   
                    for (int j = 0; j < _riskDTO.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        if (_riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].SelectedItemId != 0)
                        {
                            riskItemlist +=_riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].Id + "Ø" + _riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].SelectedItemId + "¥";
                            index2++;
                        }
                       
                    }
                }
                parameters.Add("@p_risk_data", riskItemlist);
                parameters.Add("@p_risk_category_count", index2);
                parameters.Add("@p_clientId", _riskDTO.ClientId);
                parameters.Add("@p_created_by", _riskDTO.CreatedBy);
                parameters.Add("@p_version", _riskDTO.version);
                parameters.Add("@p_remarks", _riskDTO.Remarks);
                parameters.Add("@p_riskoverride", _riskDTO.RiskOverRide);
                parameters.Add("@p_type", _riskDTO.Type);
                var response = ExecuteScalar("ins_transaction_risk_individual", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Risk Assessment for Individual added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> CreateCorpCustomerRisk(RiskCorpCustomerDTO _riskDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_customercode", _riskDTO.UniqueID);
                parameters.Add("@p_customername", _riskDTO.LegalNameOfEntity);
                parameters.Add("@p_dateofassessment", _riskDTO.DateofAssessment);
                parameters.Add("@p_created_date", DateTime.Now);
                parameters.Add("@p_address", string.Empty);
                parameters.Add("@p_Customer_nationality", _riskDTO.CountryOfIncorporationTxt);
                parameters.Add("@p_final_risk_score", _riskDTO.RiskAssessmentRating);
                parameters.Add("@p_risk_score_before_override", _riskDTO.RiskAssessmentRatingWithoutOverride);
                parameters.Add("@p_risk_sum", _riskDTO.RiskScoreSum);
                parameters.Add("@p_risk_count", _riskDTO.RiskScoreCount);
                parameters.Add("@p_product_reference", _riskDTO.ProductReference);
                parameters.Add("@p_product_value", _riskDTO.ProductValue);
                parameters.Add("@p_comments", _riskDTO.RiskComments);
                var rowData = "";
                var index = 0;
                for (var i = 0; i < _riskDTO.RiskTypeCategoryDTO.Count; i++)
                {
                    for (var j = 0; j < _riskDTO.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        if (_riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].SelectedItemId != 0)
                        {
                            rowData += _riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].Id + "Ø" + _riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].SelectedItemId + "¥";
                            index++;
                        }
                    }
                }
                parameters.Add("@p_risk_data", rowData);
                parameters.Add("@p_risk_category_count", index);
                parameters.Add("@p_clientId", _riskDTO.ClientId);
                parameters.Add("@p_created_by", _riskDTO.CreatedBy);
                parameters.Add("@p_version", _riskDTO.version);
                parameters.Add("@p_remarks", _riskDTO.Remarks);
                parameters.Add("@p_riskoverride", _riskDTO.RiskOverRide);
                parameters.Add("@p_type", _riskDTO.Type);
                var response = ExecuteScalar("ins_transaction_risk_Corporate", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Risk Assesment for Corporate Customer added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;

        }

        public ServiceResponse<int> CreateBankRisk(RiskAssessmentBankDTO _riskDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_customercode", _riskDTO.UniqueIDNumber);
                parameters.Add("@p_customername", _riskDTO.LegalNameOfTheEntity);
                parameters.Add("@p_dateofassessment", _riskDTO.EntityDate);
                parameters.Add("@p_nationality", _riskDTO.TxtCountryOfIncorporation);
                parameters.Add("@p_risk_sum", _riskDTO.RiskScoreSum);
                parameters.Add("@p_risk_count", _riskDTO.RiskScoreCount);
                parameters.Add("@p_risk_score_initial", _riskDTO.RiskAssessmentRatingWithoutOverride);
                parameters.Add("@p_final_risk_score", _riskDTO.RiskAssessmentRating); var rowData = "";
                var index = 0;
                for (var i = 0; i < _riskDTO.RiskTypeCategoryDTO.Count; i++)
                {
                    for (var j = 0; j < _riskDTO.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        if (_riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].SelectedItemId != 0)
                        {
                            rowData += _riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].Id + "Ø" + _riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].SelectedItemId + "¥";
                            index++;
                        }
                    }
                }
                parameters.Add("@p_risk_data", rowData);
                parameters.Add("@p_risk_category_count", index);
                parameters.Add("@p_clientId", _riskDTO.ClientId);
                parameters.Add("@p_created_by", _riskDTO.CreatedBy);
                var response = ExecuteScalar("ins_transaction_risk_bank", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Risk Assesment for Bank added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;

        }

        public ServiceResponse<int> CreateVendorRisk(RiskAssessmentVendorDTO _riskDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_customercode", _riskDTO.RegistrationNumber);
                parameters.Add("@p_customername", _riskDTO.VendorName);
                parameters.Add("@p_dateofassessment", _riskDTO.RegisteredDate);
                parameters.Add("@p_nationality", _riskDTO.Country);
               

                parameters.Add("@p_risk_sum", _riskDTO.RiskScoreSum);
                parameters.Add("@p_risk_count", _riskDTO.RiskScoreCount);
                parameters.Add("@p_risk_score_initial", _riskDTO.RiskAssessmentRatingWithoutOverride);
                parameters.Add("@p_final_risk_score", _riskDTO.RiskAssessmentRating);
                parameters.Add("@p_product_reference", _riskDTO.ProductReference);
                parameters.Add("@p_product_value", _riskDTO.ProductValue);
                parameters.Add("@p_comments", _riskDTO.RiskComments);

                string riskItemlist = "";
                int index2 = 0;
                for (int i = 0; i < _riskDTO.RiskTypeCategoryDTO.Count; i++)
                {

                    for (int j = 0; j < _riskDTO.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        if (_riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].SelectedItemId != 0)
                        {
                            riskItemlist += _riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].Id + "Ø" + _riskDTO.RiskTypeCategoryDTO[i].RiskTypes[j].SelectedItemId + "¥";
                            index2++;
                        }

                    }
                }
                parameters.Add("@p_risk_data", riskItemlist);
                parameters.Add("@p_risk_category_count", index2);
                parameters.Add("@p_clientId", _riskDTO.ClientId);

                parameters.Add("@p_created_by", _riskDTO.CreatedBy);

                var response = ExecuteScalar("ins_transaction_risk_vendor", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Risk Assesment for Vendor added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;

        }


        public ServiceResponse<List<RiskSummaryDTO>> GetRiskSummary(DateTime date,int clientId)
        {
            ServiceResponse<List<RiskSummaryDTO>> serviceResponse = new ServiceResponse<List<RiskSummaryDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_as_on_date", date);
                parameters.Add("@c_clientId", clientId);
                serviceResponse.Result = Get<RiskSummaryDTO>("get_risk_score_all_cust_types_summary", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<RiskReportDTO>> GetRiskReportBetweenDateAndType(int clientId, string createdByUserID, DateTime fromDate, DateTime toDate, string riskType, int riskLevel)
        {
            ServiceResponse<List<RiskReportDTO>> serviceResponse = new ServiceResponse<List<RiskReportDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_from_date", fromDate);
                parameters.Add("@c_to_date", toDate);
                parameters.Add("@C_Cust_Type", riskType);
                parameters.Add("@c_clientId", clientId);
                parameters.Add("@c_risk_level", riskLevel);
                parameters.Add("@p_created_by", createdByUserID);
                serviceResponse.Result = Get<RiskReportDTO>("get_risk_score_customerwise_all_new", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<RiskReportDTO>> GetRiskReportBetweenDateAndTypeWithDuplicate(int clientId, string createdByUserID,DateTime fromDate, DateTime toDate, string riskType, int riskLevel)
        {
            ServiceResponse<List<RiskReportDTO>> serviceResponse = new ServiceResponse<List<RiskReportDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_from_date", fromDate);
                parameters.Add("@c_to_date", toDate);
                parameters.Add("@C_Cust_Type", riskType);
                parameters.Add("@c_clientId", clientId);
                parameters.Add("@c_risk_level", riskLevel);
                parameters.Add("@p_created_by", createdByUserID);
                serviceResponse.Result = Get<RiskReportDTO>("get_risk_score_customerwise_all_with_dupli", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<RiskReportDTO>> GetLastestRiskVersion(string customercode,string customertype)
        {
            ServiceResponse<List<RiskReportDTO>> serviceResponse = new ServiceResponse<List<RiskReportDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_customercode",customercode);
                parameters.Add("@p_customertype", customertype);
                serviceResponse.Result = Get<RiskReportDTO>("get_lastes_risk_version_details", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<RiskReportDTO>> GetAllRiskVersion(string customercode, string customertype)
        {
            ServiceResponse<List<RiskReportDTO>> serviceResponse = new ServiceResponse<List<RiskReportDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_customercode", customercode);
                parameters.Add("@p_customertype", customertype);
                serviceResponse.Result = Get<RiskReportDTO>("get_all_risk_version_details", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<RiskReportDTO>> GetAllVersionRiskReportByid(string customerCode, string riskType, int clientId)
        {
            ServiceResponse<List<RiskReportDTO>> serviceResponse = new ServiceResponse<List<RiskReportDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_customercode", customerCode);
                parameters.Add("@p_risktype", riskType);
                parameters.Add("@c_clientId", clientId);
                serviceResponse.Result = Get<RiskReportDTO>("get_all_riskVerison_By_customercode", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<RiskDTO> GetRiskDetailsOfIndividual(int id)
        {
            ServiceResponse<RiskDTO> serviceResponse = new ServiceResponse<RiskDTO>();
            ServiceResponse<List<ReportDataDTO>> serviceResponse1 = new ServiceResponse<List<ReportDataDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                serviceResponse.Result = GetFirstOrDefault<RiskDTO>("get_risk_individual_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse1.Result = (List<ReportDataDTO>)Get<ReportDataDTO>("get_risk_individual_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Result.ReportDataDTO = serviceResponse1.Result;
                serviceResponse.Message = "Risk Details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<RiskExcelReportDTO>> GetRiskReportForExcel(DateTime fromDate, DateTime toDate, string createdByUserID, string riskType, int riskLevel, int clientId)
        {
            ServiceResponse<List<RiskExcelReportDTO>> serviceResponse = new ServiceResponse<List<RiskExcelReportDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_from_date", fromDate);
                parameters.Add("@c_to_date", toDate);
                parameters.Add("@clientId", clientId);
                parameters.Add("@c_risk_level", riskLevel);
                parameters.Add("@p_created_by", createdByUserID);
                parameters.Add("@C_Cust_Type", riskType);
                serviceResponse.Result = Get<RiskExcelReportDTO>("get_risk_verbose", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<RiskCorpCustomerDTO> GetRiskDetailsOfCorporate(int id)
        {
            ServiceResponse<RiskCorpCustomerDTO> serviceResponse = new ServiceResponse<RiskCorpCustomerDTO>();
            ServiceResponse<List<ReportDataDTO>> serviceResponse1 = new ServiceResponse<List<ReportDataDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                serviceResponse.Result = GetFirstOrDefault<RiskCorpCustomerDTO>("get_tran_risk_corporate_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse1.Result = (List<ReportDataDTO>)Get<ReportDataDTO>("get_tran_risk_corporate_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Result.ReportDataDTO = serviceResponse1.Result;
                serviceResponse.Message = "Risk Details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<RiskAssessmentBankDTO> GetRiskDetailsOfBank(int id)
        {
            ServiceResponse<RiskAssessmentBankDTO> serviceResponse = new ServiceResponse<RiskAssessmentBankDTO>();
            ServiceResponse<List<ReportDataDTO>> serviceResponse1 = new ServiceResponse<List<ReportDataDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                serviceResponse.Result = GetFirstOrDefault<RiskAssessmentBankDTO>("get_tran_risk_bank_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse1.Result = (List<ReportDataDTO>)Get<ReportDataDTO>("get_tran_risk_bank_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Result.ReportDataDTO = serviceResponse1.Result;
                serviceResponse.Message = "Risk Details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<RiskAssessmentVendorDTO> GetRiskDetailsOfVendor(int id)
        {
            ServiceResponse<RiskAssessmentVendorDTO> serviceResponse = new ServiceResponse<RiskAssessmentVendorDTO>();
            ServiceResponse<List<ReportDataDTO>> serviceResponse1 = new ServiceResponse<List<ReportDataDTO>>();
            try
            {
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@p_id", id);
                    serviceResponse.Result = GetFirstOrDefault<RiskAssessmentVendorDTO>("get_tran_risk_vendor_by_id", parameters, commandType: CommandType.StoredProcedure);
                    serviceResponse1.Result = (List<ReportDataDTO>)Get<ReportDataDTO>("get_tran_risk_vendor_by_id", parameters, commandType: CommandType.StoredProcedure);
                    serviceResponse.Result.ReportDataDTO = serviceResponse1.Result;
                    serviceResponse.Message = "Risk Details fetched successfully.";
                    serviceResponse.Status = StaticResource.SuccessStatusCode;
                    serviceResponse.Message = "Risk Details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        //SoftDelete
        public ServiceResponse<int> DeleteRiskIndiviual(int id)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@p_id", id);
                serviceResponse.Result = Execute("del_risk", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Risk Deleted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> DeleteRiskCorporate(int id)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                // Console.WriteLine(string.Format("cmr Id : {0}", id));
                parameters.Add("@p_id", id);
                int response = Execute("del_risk_corporate", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Result = serviceResponse.Result;
                serviceResponse.Message = "Risk Deleted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> DeleteRiskBank(int id)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                //Console.WriteLine(string.Format("cmr Id : {0}", id));
                parameters.Add("@p_id", id);
                int response = Execute("del_risk_bank", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Result = serviceResponse.Result;
                serviceResponse.Message = "Risk Deleted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> DeleteRiskVendor(int id)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                //Console.WriteLine(string.Format("cmr Id : {0}", id));
                parameters.Add("@p_id", id);
                int response = Execute("del_risk_vendor", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Result = serviceResponse.Result;
                serviceResponse.Message = "Risk Deleted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());

                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        //DNIRC


        public ServiceResponse<RiskDTO> GetRiskDetailsOfIndividualByCID(string id)
        {
            ServiceResponse<RiskDTO> serviceResponse = new ServiceResponse<RiskDTO>();
            ServiceResponse<List<ReportDataDTO>> serviceResponse1 = new ServiceResponse<List<ReportDataDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                serviceResponse.Result = GetFirstOrDefault<RiskDTO>("get_risk_individual_by_cid", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse1.Result = (List<ReportDataDTO>)Get<ReportDataDTO>("get_risk_individual_by_cid", parameters, commandType: CommandType.StoredProcedure);
                if(serviceResponse1.Result.Count>0)
                {
                    serviceResponse.Result.ReportDataDTO = serviceResponse1.Result;
                    serviceResponse.Message = "Risk Details fetched successfully.";
                    serviceResponse.Status = StaticResource.SuccessStatusCode;
                }
                
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<RiskCorpCustomerDTO> GetRiskDetailsOfCorporateByCID(string id)
        {
            ServiceResponse<RiskCorpCustomerDTO> serviceResponse = new ServiceResponse<RiskCorpCustomerDTO>();
            ServiceResponse<List<ReportDataDTO>> serviceResponse1 = new ServiceResponse<List<ReportDataDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                serviceResponse.Result = GetFirstOrDefault<RiskCorpCustomerDTO>("get_tran_risk_corporate_by_cid", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse1.Result = (List<ReportDataDTO>)Get<ReportDataDTO>("get_tran_risk_corporate_by_cid", parameters, commandType: CommandType.StoredProcedure);
                if (serviceResponse1.Result.Count > 0)
                {
                    serviceResponse.Result.ReportDataDTO = serviceResponse1.Result;
                    serviceResponse.Message = "Risk Details fetched successfully.";
                    serviceResponse.Status = StaticResource.SuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }


        public ServiceResponse<RiskAssessmentVendorDTO> GetRiskDetailsOfVendorByCID(string id)
        {
            ServiceResponse<RiskAssessmentVendorDTO> serviceResponse = new ServiceResponse<RiskAssessmentVendorDTO>();
            ServiceResponse<List<ReportDataDTO>> serviceResponse1 = new ServiceResponse<List<ReportDataDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                serviceResponse.Result = GetFirstOrDefault<RiskAssessmentVendorDTO>("get_tran_risk_vendor_by_cid", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse1.Result = (List<ReportDataDTO>)Get<ReportDataDTO>("get_tran_risk_vendor_by_cid", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Result.ReportDataDTO = serviceResponse1.Result;
                serviceResponse.Message = "Risk Details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }


        public ServiceResponse<int> GetRiskId(int riskTypeId, string riskData)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_type_id", riskTypeId);
                parameters.Add("@p_risk_data", riskData);
                serviceResponse.Result = Get<int>("get_riskid_by_riskdata", parameters, commandType: CommandType.StoredProcedure).FirstOrDefault();
                serviceResponse.Message = "Customer details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> GetRiskIdByCustomercode(string customercode, string type)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_customercode", customercode);
                parameters.Add("@p_type", type);
                serviceResponse.Result = GetFirstOrDefault<int>("get_risk_id_by_customercode", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Risk Details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }


    }
}
