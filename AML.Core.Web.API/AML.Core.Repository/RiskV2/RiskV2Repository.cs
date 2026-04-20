using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Risk;
using AML.Core.RepositoryContract.RiskV2;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.RiskV2;
using AML.ViewModel.ViewModels.RiskV2;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AML.Core.Repository.RiskV2
{
    public class RiskV2Repository : BaseRepository, IRiskV2Repository
    {
        public RiskV2Repository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }


        public ServiceResponse<int> Create(RiskDTOV2 _riskDTO)
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
                var response = ExecuteScalar("ins_transaction_risk_individual_v2", parameters, commandType: CommandType.StoredProcedure).ParseInt();
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

        public ServiceResponse<int> CreateCorpCustomerRisk(RiskCorpCustomerDTOV2 _riskDTO)
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
                var response = ExecuteScalar("ins_transaction_risk_Corporate_v2", parameters, commandType: CommandType.StoredProcedure).ParseInt();
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

        public ServiceResponse<int> CreateBankRisk(RiskAssessmentBankDTOV2 _riskDTO)
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
                parameters.Add("@p_remarks", _riskDTO.Remarks);
                parameters.Add("@p_product_reference", _riskDTO.ProductReference);
                parameters.Add("@p_product_value", _riskDTO.ProductValue);
                parameters.Add("@p_comments", _riskDTO.Comments);
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

        public ServiceResponse<int> CreateVendorRisk(RiskAssessmentVendorDTOV2 _riskDTO)
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
                parameters.Add("@p_remarks", _riskDTO.Remarks);
                parameters.Add("@p_product_reference", _riskDTO.ProductReference);
                parameters.Add("@p_product_value", _riskDTO.ProductValue);
                parameters.Add("@p_comments", _riskDTO.Comments);
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


        public ServiceResponse<List<RiskSummaryDTOV2>> GetRiskSummary(DateTime date,int clientId)
        {
            ServiceResponse<List<RiskSummaryDTOV2>> serviceResponse = new ServiceResponse<List<RiskSummaryDTOV2>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_as_on_date", date);
                parameters.Add("@c_clientId", clientId);
                serviceResponse.Result = Get<RiskSummaryDTOV2>("get_risk_score_all_cust_types_summary", parameters, commandType: CommandType.StoredProcedure).ToList();
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

        public ServiceResponse<List<RiskReportDTOV2>> GetRiskReportBetweenDateAndType(int clientId, string createdByUserID, DateTime fromDate, DateTime toDate, string riskType, int riskLevel)
        {
            ServiceResponse<List<RiskReportDTOV2>> serviceResponse = new ServiceResponse<List<RiskReportDTOV2>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_from_date", fromDate);
                parameters.Add("@c_to_date", toDate);
                parameters.Add("@C_Cust_Type", riskType);
                parameters.Add("@c_clientId", clientId);
                parameters.Add("@c_risk_level", riskLevel);
                parameters.Add("@p_created_by", createdByUserID);
                serviceResponse.Result = Get<RiskReportDTOV2>("get_risk_score_customerwise_all_new_v2", parameters, commandType: CommandType.StoredProcedure).ToList();
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

        public ServiceResponse<List<RiskReportDTOV2>> GetRiskReportBetweenDateAndTypeWithDuplicate(int clientId, string createdByUserID,DateTime fromDate, DateTime toDate, string riskType, int riskLevel)
        {
            ServiceResponse<List<RiskReportDTOV2>> serviceResponse = new ServiceResponse<List<RiskReportDTOV2>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_from_date", fromDate);
                parameters.Add("@c_to_date", toDate);
                parameters.Add("@C_Cust_Type", riskType);
                parameters.Add("@c_clientId", clientId);
                parameters.Add("@c_risk_level", riskLevel);
                parameters.Add("@p_created_by", createdByUserID);
                serviceResponse.Result = Get<RiskReportDTOV2>("get_risk_score_customerwise_all_with_dupli", parameters, commandType: CommandType.StoredProcedure).ToList();
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
        public ServiceResponse<List<RiskReportDTOV2>> GetAllVersionRiskReportByid(string customerCode, string riskType, int clientId)
        {
            ServiceResponse<List<RiskReportDTOV2>> serviceResponse = new ServiceResponse<List<RiskReportDTOV2>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_customercode", customerCode);
                parameters.Add("@p_risktype", riskType);
                parameters.Add("@c_clientId", clientId);
                serviceResponse.Result = Get<RiskReportDTOV2>("get_all_riskVerison_By_customercode", parameters, commandType: CommandType.StoredProcedure).ToList();
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

        public ServiceResponse<RiskDTOV2> GetRiskDetailsOfIndividual(int id)
        {
            ServiceResponse<RiskDTOV2> serviceResponse = new ServiceResponse<RiskDTOV2>();
            ServiceResponse<List<ReportDataDTOV2>> serviceResponse1 = new ServiceResponse<List<ReportDataDTOV2>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                serviceResponse.Result = GetFirstOrDefault<RiskDTOV2>("get_risk_individual_by_id_v2", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse1.Result = (List<ReportDataDTOV2>)Get<ReportDataDTOV2>("get_risk_individual_by_id_v2", parameters, commandType: CommandType.StoredProcedure);
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

        public ServiceResponse<List<RiskExcelReportDTOV2>> GetRiskReportForExcel(DateTime fromDate, DateTime toDate, string createdByUserID, string searchvalue, string riskType, int riskLevel, int clientId)
        {
            ServiceResponse<List<RiskExcelReportDTOV2>> serviceResponse = new ServiceResponse<List<RiskExcelReportDTOV2>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@From_Date", fromDate);
                parameters.Add("@To_Date", toDate);
                parameters.Add("@clientId", clientId);
                parameters.Add("@c_risk_level", riskLevel);
                serviceResponse.Result = Get<RiskExcelReportDTOV2>("get_risk_verbose", parameters, commandType: CommandType.StoredProcedure).ToList();
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

        public ServiceResponse<RiskCorpCustomerDTOV2> GetRiskDetailsOfCorporate(int id)
        {
            ServiceResponse<RiskCorpCustomerDTOV2> serviceResponse = new ServiceResponse<RiskCorpCustomerDTOV2>();
            ServiceResponse<List<ReportDataDTOV2>> serviceResponse1 = new ServiceResponse<List<ReportDataDTOV2>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                serviceResponse.Result = GetFirstOrDefault<RiskCorpCustomerDTOV2>("get_tran_risk_corporate_by_id_v2", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse1.Result = (List<ReportDataDTOV2>)Get<ReportDataDTOV2>("get_tran_risk_corporate_by_id_v2", parameters, commandType: CommandType.StoredProcedure);
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

        public ServiceResponse<RiskAssessmentBankDTOV2> GetRiskDetailsOfBank(int id)
        {
            ServiceResponse<RiskAssessmentBankDTOV2> serviceResponse = new ServiceResponse<RiskAssessmentBankDTOV2>();
            ServiceResponse<List<ReportDataDTOV2>> serviceResponse1 = new ServiceResponse<List<ReportDataDTOV2>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                serviceResponse.Result = GetFirstOrDefault<RiskAssessmentBankDTOV2>("get_tran_risk_bank_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse1.Result = (List<ReportDataDTOV2>)Get<ReportDataDTOV2>("get_tran_risk_bank_by_id", parameters, commandType: CommandType.StoredProcedure);
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

        public ServiceResponse<RiskAssessmentVendorDTOV2> GetRiskDetailsOfVendor(int id)
        {
            ServiceResponse<RiskAssessmentVendorDTOV2> serviceResponse = new ServiceResponse<RiskAssessmentVendorDTOV2>();
            ServiceResponse<List<ReportDataDTOV2>> serviceResponse1 = new ServiceResponse<List<ReportDataDTOV2>>();
            try
            {
                    DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@p_id", id);
                    serviceResponse.Result = GetFirstOrDefault<RiskAssessmentVendorDTOV2>("get_tran_risk_vendor_by_id", parameters, commandType: CommandType.StoredProcedure);
                    serviceResponse1.Result = (List<ReportDataDTOV2>)Get<ReportDataDTOV2>("get_tran_risk_vendor_by_id", parameters, commandType: CommandType.StoredProcedure);
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


        public ServiceResponse<RiskDTOV2> GetRiskDetailsOfIndividualByCID(string id)
        {
            ServiceResponse<RiskDTOV2> serviceResponse = new ServiceResponse<RiskDTOV2>();
            ServiceResponse<List<ReportDataDTOV2>> serviceResponse1 = new ServiceResponse<List<ReportDataDTOV2>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                serviceResponse.Result = GetFirstOrDefault<RiskDTOV2>("get_risk_individual_by_cid", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse1.Result = (List<ReportDataDTOV2>)Get<ReportDataDTOV2>("get_risk_individual_by_cid", parameters, commandType: CommandType.StoredProcedure);
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
        public ServiceResponse<RiskCorpCustomerDTOV2> GetRiskDetailsOfCorporateByCID(string id)
        {
            ServiceResponse<RiskCorpCustomerDTOV2> serviceResponse = new ServiceResponse<RiskCorpCustomerDTOV2>();
            ServiceResponse<List<ReportDataDTOV2>> serviceResponse1 = new ServiceResponse<List<ReportDataDTOV2>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                serviceResponse.Result = GetFirstOrDefault<RiskCorpCustomerDTOV2>("get_tran_risk_corporate_by_cid", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse1.Result = (List<ReportDataDTOV2>)Get<ReportDataDTOV2>("get_tran_risk_corporate_by_cid", parameters, commandType: CommandType.StoredProcedure);
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


        public ServiceResponse<RiskAssessmentVendorDTOV2> GetRiskDetailsOfVendorByCID(string id)
        {
            ServiceResponse<RiskAssessmentVendorDTOV2> serviceResponse = new ServiceResponse<RiskAssessmentVendorDTOV2>();
            ServiceResponse<List<ReportDataDTOV2>> serviceResponse1 = new ServiceResponse<List<ReportDataDTOV2>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                serviceResponse.Result = GetFirstOrDefault<RiskAssessmentVendorDTOV2>("get_tran_risk_vendor_by_cid", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse1.Result = (List<ReportDataDTOV2>)Get<ReportDataDTOV2>("get_tran_risk_vendor_by_cid", parameters, commandType: CommandType.StoredProcedure);
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


    }
}
