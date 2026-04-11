using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Kyc;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.Kyc;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AML.Core.Repository.Kyc
{
    public class KycRepository : BaseRepository, IKycRepository
    {
        public KycRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        {

        }
        public ServiceResponse<int> CreateCorporate(CorporateKycDTO kycCorporate)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_customer_id", kycCorporate.CustomerId);
                //       parameters.Add("p_fullname", kycCorporate.FullName);
                //   parameters.Add("p_date_of_incorporation", kycCorporate.DateofIncorporation.ToMysqlDateFormat());
                //     parameters.Add("p_place_of_incorporation", kycCorporate.PlaceofIncorporation);


                parameters.Add("p_entity_type", kycCorporate.EntityTypeTxt);
                parameters.Add("p_entity_city", kycCorporate.EntityAddress.City);
                parameters.Add("p_entity_emirate", kycCorporate.EntityAddress.Emirate);
                parameters.Add("p_enitity_country", kycCorporate.EntityAddress.Country);
                parameters.Add("p_po_box", kycCorporate.EntityAddress.POBox);
                parameters.Add("p_corporate_website", kycCorporate.CorporateWebsite);
                parameters.Add("p_telephone", kycCorporate.Telephone);
                parameters.Add("p_email", kycCorporate.Email);
                parameters.Add("p_license_issue_date", kycCorporate.CommercialLicense.LicenseIssueDate);
                parameters.Add("p_license_authority", kycCorporate.CommercialLicense.LicenseIssuingAuthority);
                parameters.Add("p_license_expiry_date", kycCorporate.CommercialLicense.LicenseExpiryDate);
                parameters.Add("p_license_type", kycCorporate.CommercialLicense.LicenseTypeTxt);
                parameters.Add("p_place_of_issue", kycCorporate.CommercialLicense.PlaceofIssue);
                parameters.Add("p_vat_number", kycCorporate.VATRegistrationNumber);
                parameters.Add("p_fund_source", kycCorporate.FundSourceTxt);
                parameters.Add("p_fund_source_other", kycCorporate.FundSourceOther);
                parameters.Add("p_business_type", kycCorporate.BusinessType);
                parameters.Add("p_pep_status", kycCorporate.PEPStatus);
                //parameters.Add("p_pep_name", kycCorporate.PEPDetails.Name);
                //parameters.Add("p_pep_title", kycCorporate.PEPDetails.Title);
                //parameters.Add("p_pep_type_status", kycCorporate.PEPDetails.PEPTypeStatus);
                parameters.Add("p_product_type", kycCorporate.ProductName);
                parameters.Add("p_delivery_channel", kycCorporate.DeliveryChannelName);
                parameters.Add("p_modeofpayment", kycCorporate.Modeofpayment);
                parameters.Add("p_remark", kycCorporate.Remarks);
                parameters.Add("p_address", kycCorporate.Address);
                serviceResponse.Result = ExecuteScalar("ins_corporate_kyc", parameters, commandType: CommandType.StoredProcedure).ParseInt();

                serviceResponse.Message = "Corporate KYC inserted successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occured while executing stored procedure 'ins_corporate_kyc'\n{ex.Message}");
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> CreateIndividual(KycIndividualDTO kycIndividual)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_customer_id", kycIndividual.CustomerId);
                parameters.Add("p_residence_status", kycIndividual.ResidenceStatus);
                parameters.Add("p_occupation_type", kycIndividual.OccupatinTypeTxt);
                parameters.Add("p_employer_name", kycIndividual.EmployerName);
                parameters.Add("p_employer_address", kycIndividual.EmployerAddress);
                parameters.Add("p_residence_address", kycIndividual.ResidenceAddress);
                parameters.Add("p_email", kycIndividual.Email);
                parameters.Add("p_pep_status", kycIndividual.PEPStatus);
                parameters.Add("p_guardian_name", kycIndividual.GuardianName);
                parameters.Add("p_guardian_relation", kycIndividual.GuardianRelation);
                parameters.Add("p_remark", kycIndividual.Remarks);
                parameters.Add("p_maritalStatus", kycIndividual.MaritalStatus);
                parameters.Add("p_placeofbirth", kycIndividual.PlaceOfBirth);
                parameters.Add("p_bankaccountbranch", kycIndividual.BankAccountBranch);
                parameters.Add("p_bankaccountname", kycIndividual.BankAccountName);
                parameters.Add("p_bankaccountno", kycIndividual.BankAccountNo);
                parameters.Add("p_idexpirydate", kycIndividual.IdExpdate);
                parameters.Add("p_sourceofincome", kycIndividual.Sourceofincome);
                parameters.Add("p_productname", kycIndividual.ProductName);
                parameters.Add("p_deliverychannelname", kycIndividual.DeliveryChannelName);
                parameters.Add("p_modeofpayment", kycIndividual.ModeOfPayment);
                parameters.Add("p_address", kycIndividual.Address);
                serviceResponse.Result = ExecuteScalar("ins_individual_kyc", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Message = "Individual KYC inserted successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)    
            {
                Console.Error.WriteLine($"Error occured while executing stored procedure 'ins_individual_kyc'\n{ex.Message}");
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<string> GetRiskTypeId(KycIndividualDTO kycIndividual, CorporateKycDTO kycCorporate, string Type,string culture,int ClientIds)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                //kycCorporate.Partners = new List<PersonDetailsDTO>();
                
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_culture", culture);
                parameters.Add("p_clientId", ClientIds);
                parameters.Add("p_profession", kycIndividual.OccupatinTypeTxt != null && kycIndividual.OccupatinTypeTxt != "0"  ? kycIndividual.OccupatinTypeTxt : "");
                parameters.Add("p_nationality", kycIndividual.Nationality != null && kycIndividual.Nationality != "0" ? kycIndividual.Nationality : "");
               // if(kycIndividual.IsPeP != null)
               // {
               //     parameters.Add("p_ind_pep", kycIndividual.IsPeP != null ? kycIndividual.IsPeP : "");
               // }
               //else
               // {
               //     parameters.Add("p_ind_pep", kycIndividual.IsPep != null ? kycIndividual.IsPep : "");
               // }

                //parameters.Add("p_corp_pep", kycCorporate.IsPeP != null ? kycCorporate.IsPeP : "");

                parameters.Add("p_entityType", kycCorporate.EntityTypeTxt != null && kycCorporate.EntityTypeTxt != "0" ? kycCorporate.EntityTypeTxt : "");
                parameters.Add("p_incorporationPlace", kycCorporate.PlaceofIncorporation != null && kycCorporate.PlaceofIncorporation != "0"  ? kycCorporate.PlaceofIncorporation : "");
                parameters.Add("p_businessNature", kycCorporate.BusinessType != null && kycCorporate.BusinessType != "0"  ? kycCorporate.BusinessType : "");
                parameters.Add("p_residence", kycIndividual.ResidenceStatus != null && kycIndividual.ResidenceStatus != "0" ? kycIndividual.ResidenceStatus : "");
                
                if (kycCorporate.Partners != null && kycCorporate.Partners.Count >= 1)
                {
                    if (kycCorporate.Partners.Count >= 1)
                    {
                        parameters.Add("p_partner1", kycCorporate.Partners[0].Nationality != null && kycCorporate.Partners[0].Nationality != "0"   ? kycCorporate.Partners[0].Nationality : "");
                    }
                    else
                    {
                        parameters.Add("p_partner1", "");
                    }
                    if (kycCorporate.Partners.Count >= 2)
                    {
                        parameters.Add("p_partner2", kycCorporate.Partners[1].Nationality != null && kycCorporate.Partners[1].Nationality != "0"  ? kycCorporate.Partners[1].Nationality : "");
                    }
                    else
                    {
                        parameters.Add("p_partner2", "");
                    }
                    if (kycCorporate.Partners.Count >= 3)
                    {
                        parameters.Add("p_partner3", kycCorporate.Partners[2].Nationality != null && kycCorporate.Partners[2].Nationality != "0" ? kycCorporate.Partners[2].Nationality : "");
                    }
                    else
                    {
                        parameters.Add("p_partner3", "");
                    }
                    if (kycCorporate.Partners.Count >= 4)
                    {
                        parameters.Add("p_partner4", kycCorporate.Partners[3].Nationality != null && kycCorporate.Partners[3].Nationality != "0"  ? kycCorporate.Partners[3].Nationality : "");
                    }
                    else
                    {
                        parameters.Add("p_partner4", "");
                    }
                    if (kycCorporate.Partners.Count >= 5)
                    {
                        parameters.Add("p_partner5", kycCorporate.Partners[4].Nationality != null && kycCorporate.Partners[4].Nationality != "0" ? kycCorporate.Partners[4].Nationality : "");
                    }
                    else
                    {
                        parameters.Add("p_partner5", "");
                    }
                }
                else
                {
                    parameters.Add("p_partner1", "");
                    parameters.Add("p_partner2", "");
                    parameters.Add("p_partner3", "");
                    parameters.Add("p_partner4", "");
                    parameters.Add("p_partner5", "");
                }
                if (kycCorporate.ProductName =="0")
                {
                    kycCorporate.ProductName = null;
                }   
                if(kycCorporate.DeliveryChannelName =="0")
                {
                    kycCorporate.DeliveryChannelName = null;
                }
                parameters.Add("p_product", kycCorporate.ProductName != null && kycCorporate.ProductName != "0" ? kycCorporate.ProductName : "");
                parameters.Add("p_delivery", kycCorporate.DeliveryChannelName != null && kycCorporate.DeliveryChannelName != "0"  ? kycCorporate.DeliveryChannelName : "");
                parameters.Add("p_indproduct", kycIndividual.ProductName != null && kycIndividual.ProductName != "0" ? kycIndividual.ProductName : "");
                parameters.Add("p_inddelivery", kycIndividual.DeliveryChannelName != null && kycIndividual.DeliveryChannelName != "0"  ? kycIndividual.DeliveryChannelName : "");
                parameters.Add("p_indmodeofpayment", kycIndividual.ModeOfPayment != null && kycIndividual.ModeOfPayment != "0" ? kycIndividual.ModeOfPayment : "");
                parameters.Add("p_corpmodeofpayment", kycCorporate.Modeofpayment != null && kycCorporate.Modeofpayment != "0"  ? kycCorporate.Modeofpayment : "");
                parameters.Add("p_domesticpep", kycIndividual.Domesticpep != null && kycIndividual.Domesticpep != "0" ? kycIndividual.Domesticpep : "");
                parameters.Add("p_corpdomesticpep", kycCorporate.Domesticpep != null && kycCorporate.Domesticpep != "0" ? kycCorporate.Domesticpep : "");
                parameters.Add("p_indforeignpep", kycIndividual.ForeignPep != null && kycIndividual.ForeignPep != "0" ? kycIndividual.ForeignPep : "");
                parameters.Add("p_corpforeignpep", kycCorporate.ForeignPep != null && kycCorporate.ForeignPep != "0" ? kycCorporate.ForeignPep : "");
                parameters.Add("p_indredflags", kycIndividual.RedFlags != null && kycIndividual.RedFlags != "0" ? kycIndividual.RedFlags : "");
                parameters.Add("p_corpredflags", kycCorporate.RedFlags != null && kycCorporate.RedFlags != "0" ? kycCorporate.RedFlags : "");
                parameters.Add("p_indsanctionMatch", kycIndividual.SanctionMatch != null && kycIndividual.SanctionMatch != "0" ? kycIndividual.SanctionMatch : "");
                parameters.Add("p_corpsanctionMatch", kycCorporate.SanctionMatch != null && kycCorporate.SanctionMatch != "0" ? kycCorporate.SanctionMatch : "");
                parameters.Add("p_induaeorunsc", kycIndividual.UAEORUNSC != null && kycIndividual.UAEORUNSC != "0" ? kycIndividual.UAEORUNSC : "");
                parameters.Add("p_corpuaeorunsc", kycCorporate.UAEORUNSC != null && kycCorporate.UAEORUNSC != "0" ? kycCorporate.UAEORUNSC : "");
                parameters.Add("p_corpfatf", kycCorporate.FATF != null && kycCorporate.FATF != "0" ? kycCorporate.FATF : "");
                parameters.Add("p_indhighestriskproduct", kycIndividual.HighestRiskProduct != null && kycIndividual.HighestRiskProduct != "0" ? kycIndividual.HighestRiskProduct : "");
                parameters.Add("p_corphighestriskproduct", kycCorporate.HighestRiskProduct != null && kycCorporate.HighestRiskProduct != "0" ? kycCorporate.HighestRiskProduct : "");
                parameters.Add("p_indhighnetworkindividual", kycIndividual.HighNetworkIndividual != null && kycIndividual.HighNetworkIndividual != "0" ? kycIndividual.HighNetworkIndividual : "");
                parameters.Add("p_corphighnetworkindividual", kycCorporate.HighNetworkIndividual != null && kycCorporate.HighNetworkIndividual != "0" ? kycCorporate.HighNetworkIndividual : "");
                parameters.Add("p_dualusegoods", kycCorporate.DualUseGoods != null && kycCorporate.DualUseGoods != "0" ? kycCorporate.DualUseGoods : "");
                parameters.Add("p_inddualusegoods", kycIndividual.DualUseGoods != null && kycIndividual.DualUseGoods != "0" ? kycIndividual.DualUseGoods : "");
                parameters.Add("p_moredualusegoods", kycCorporate.MoreDualUseGoods != null && kycCorporate.MoreDualUseGoods != "0" ? kycCorporate.MoreDualUseGoods : "");
                parameters.Add("p_indmoredualusegoods", kycIndividual.MoreDualUseGoods != null && kycIndividual.MoreDualUseGoods != "0" ? kycIndividual.MoreDualUseGoods : "");
                parameters.Add("p_type", Type);
                serviceResponse.Result = (string)ExecuteScalar("get_risk_type_id_kyc", parameters, commandType: CommandType.StoredProcedure) ?? null;
                serviceResponse.Message = "Individual KYC risk type id fetched successfully";
                 serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occured while executing stored procedure 'get_risk_type_id_kyc'\n{ex.Message}");
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }


        public ServiceResponse<string> GetRiskLovId(KycIndividualDTO kycIndividual, CorporateKycDTO kycCorporate, string Type, string culture, int ClientIds)
        {
            ServiceResponse<string> serviceResponse = new ServiceResponse<string>();
            try
            {
                //kycCorporate.Partners = new List<PersonDetailsDTO>();
                if (kycCorporate.ProductName == "0")
                {
                    kycCorporate.ProductName = null;
                }
                if (kycCorporate.DeliveryChannelName == "0")
                {
                    kycCorporate.DeliveryChannelName = null;
                }
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_culture", culture);
                parameters.Add("p_clientId", ClientIds);
                parameters.Add("p_customertype", Type);
                parameters.Add("p_profession", kycIndividual.OccupatinTypeTxt != null ? kycIndividual.OccupatinTypeTxt : "");
                //if (kycIndividual.IsPeP != null)
                //{
                //    parameters.Add("p_ind_pep", kycIndividual.IsPeP != null ? kycIndividual.IsPeP : "");
                //}
                //else
                //{
                //    parameters.Add("p_ind_pep", kycIndividual.IsPep != null ? kycIndividual.IsPep : "");
                //}

               // parameters.Add("p_corp_pep", kycCorporate.IsPeP != null ? kycCorporate.IsPeP : "");
                
                parameters.Add("p_nationality", kycIndividual.Nationality != null && kycIndividual.Nationality != "0" ? kycIndividual.Nationality : "");
                parameters.Add("p_entityType", kycCorporate.EntityTypeTxt != null && kycCorporate.EntityTypeTxt != "0" ? kycCorporate.EntityTypeTxt : "");
                parameters.Add("p_incorporationPlace", kycCorporate.PlaceofIncorporation != null && kycCorporate.PlaceofIncorporation != "0" ? kycCorporate.PlaceofIncorporation : "");
                parameters.Add("p_businessNature", kycCorporate.BusinessType != null && kycCorporate.BusinessType != "0" ? kycCorporate.BusinessType : "");
                parameters.Add("p_residence", kycIndividual.ResidenceStatus != null && kycIndividual.ResidenceStatus != "0" ? kycIndividual.ResidenceStatus : "");
                parameters.Add("p_product", kycCorporate.ProductName != null && kycCorporate.ProductName != "0" ? kycCorporate.ProductName : "");
                parameters.Add("p_delivery", kycCorporate.DeliveryChannelName != null && kycCorporate.DeliveryChannelName != "0" ? kycCorporate.DeliveryChannelName : "");
                parameters.Add("p_indproduct", kycIndividual.ProductName != null && kycIndividual.ProductName != "0" ? kycIndividual.ProductName : "");
                parameters.Add("p_inddelivery", kycIndividual.DeliveryChannelName != null && kycIndividual.DeliveryChannelName != "0" ? kycIndividual.DeliveryChannelName : "");
                parameters.Add("p_indmodeofpayment",kycIndividual.ModeOfPayment != null && kycIndividual.ModeOfPayment != "0" ? kycIndividual.ModeOfPayment : "");
                parameters.Add("p_corpmodeofpayment", kycCorporate.Modeofpayment != null && kycCorporate.Modeofpayment != "0" ?  kycCorporate.Modeofpayment : "");

                if (kycCorporate.Partners != null && kycCorporate.Partners.Count >= 1)
                {
                    if (kycCorporate.Partners.Count >= 1)
                    {
                        parameters.Add("p_partner1", kycCorporate.Partners[0].Nationality != null && kycCorporate.Partners[0].Nationality != "0"  ? kycCorporate.Partners[0].Nationality : "");
                    }
                    else
                    {
                        parameters.Add("p_partner1", "");
                    }
                    if (kycCorporate.Partners.Count >= 2)
                    {
                        parameters.Add("p_partner2", kycCorporate.Partners[1].Nationality != null && kycCorporate.Partners[1].Nationality != "0"  ? kycCorporate.Partners[1].Nationality : "");
                    }
                    else
                    {
                        parameters.Add("p_partner2", "");
                    }
                    if (kycCorporate.Partners.Count >= 3)
                    {
                        parameters.Add("p_partner3", kycCorporate.Partners[2].Nationality != null && kycCorporate.Partners[2].Nationality != "0"  ? kycCorporate.Partners[2].Nationality : "");
                    }
                    else
                    {
                        parameters.Add("p_partner3", "");
                    }
                    if (kycCorporate.Partners.Count >= 4)
                    {
                        parameters.Add("p_partner4", kycCorporate.Partners[3].Nationality != null && kycCorporate.Partners[3].Nationality != "0"  ? kycCorporate.Partners[3].Nationality : "");
                    }
                    else
                    {
                        parameters.Add("p_partner4", "");
                    }
                    if (kycCorporate.Partners.Count >= 5)
                    {
                        parameters.Add("p_partner5", kycCorporate.Partners[4].Nationality != null && kycCorporate.Partners[4].Nationality != "0"  ? kycCorporate.Partners[4].Nationality : "");
                    }
                    else
                    {
                        parameters.Add("p_partner5", "");
                    }
                }
                else
                {
                    parameters.Add("p_partner1", "");
                    parameters.Add("p_partner2", "");
                    parameters.Add("p_partner3", "");
                    parameters.Add("p_partner4", "");
                    parameters.Add("p_partner5", "");
                }
                parameters.Add("p_domesticpep", kycIndividual.Domesticpep != null && kycIndividual.Domesticpep != "0" ? kycIndividual.Domesticpep : "");
                parameters.Add("p_corpdomesticpep", kycCorporate.Domesticpep != null && kycCorporate.Domesticpep != "0" ? kycCorporate.Domesticpep : "");
                parameters.Add("p_indforeignpep", kycIndividual.ForeignPep != null && kycIndividual.ForeignPep != "0" ? kycIndividual.ForeignPep : "");
                parameters.Add("p_corpforeignpep", kycCorporate.ForeignPep != null && kycCorporate.ForeignPep != "0" ? kycCorporate.ForeignPep : "");
                parameters.Add("p_indredflags", kycIndividual.RedFlags != null && kycIndividual.RedFlags != "0" ? kycIndividual.RedFlags : "");
                parameters.Add("p_corpredflags", kycCorporate.RedFlags != null && kycCorporate.RedFlags != "0" ? kycCorporate.RedFlags : "");
                parameters.Add("p_indsanctionMatch", kycIndividual.SanctionMatch != null && kycIndividual.SanctionMatch != "0" ? kycIndividual.SanctionMatch : "");
                parameters.Add("p_corpsanctionMatch", kycCorporate.SanctionMatch != null && kycCorporate.SanctionMatch != "0" ? kycCorporate.SanctionMatch : "");
                parameters.Add("p_induaeorunsc", kycIndividual.UAEORUNSC != null && kycIndividual.UAEORUNSC != "0" ? kycIndividual.UAEORUNSC : "");
                parameters.Add("p_corpuaeorunsc", kycCorporate.UAEORUNSC != null && kycCorporate.UAEORUNSC != "0" ? kycCorporate.UAEORUNSC : "");
                parameters.Add("p_corpfatf", kycCorporate.FATF != null && kycCorporate.FATF != "0" ? kycCorporate.FATF : "");
                parameters.Add("p_indhighestriskproduct", kycIndividual.HighestRiskProduct != null && kycIndividual.HighestRiskProduct != "0" ? kycIndividual.HighestRiskProduct : "");
                parameters.Add("p_corphighestriskproduct", kycCorporate.HighestRiskProduct != null && kycCorporate.HighestRiskProduct != "0" ? kycCorporate.HighestRiskProduct : "");
                parameters.Add("p_indhighnetworkindividual", kycIndividual.HighNetworkIndividual != null && kycIndividual.HighNetworkIndividual != "0" ? kycIndividual.HighNetworkIndividual : "");
                parameters.Add("p_corphighnetworkindividual", kycCorporate.HighNetworkIndividual != null && kycCorporate.HighNetworkIndividual != "0" ? kycCorporate.HighNetworkIndividual : "");
                parameters.Add("p_dualusegoods", kycCorporate.DualUseGoods != null && kycCorporate.DualUseGoods != "0" ? kycCorporate.DualUseGoods : "");
                parameters.Add("p_inddualusegoods", kycIndividual.DualUseGoods != null && kycIndividual.DualUseGoods != "0" ? kycIndividual.DualUseGoods : "");
                parameters.Add("p_moredualusegoods", kycCorporate.MoreDualUseGoods != null && kycCorporate.MoreDualUseGoods != "0" ? kycCorporate.MoreDualUseGoods : "");
                parameters.Add("p_indmoredualusegoods", kycIndividual.MoreDualUseGoods != null && kycIndividual.MoreDualUseGoods != "0" ? kycIndividual.MoreDualUseGoods : "");

                parameters.Add("p_type", Type);
                serviceResponse.Result = (string)ExecuteScalar("get_risk_lov_id_kyc", parameters, commandType: CommandType.StoredProcedure) ?? null;
                serviceResponse.Message = "Individual KYC risk type id fetched successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occured while executing stored procedure 'get_risk_type_id_kyc'\n{ex.Message}");
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<BusinessNatureDTO>> GetBusinessType(string culture, string CustomerType, int ClientId)
        {
            ServiceResponse<List<BusinessNatureDTO>> ServiceResponse = new ServiceResponse<List<BusinessNatureDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_culture", culture);
                parameters.Add("p_customertype", CustomerType);
                parameters.Add("p_clientId", ClientId);
                ServiceResponse.Result = Get<BusinessNatureDTO>("get_all_business_type", parameters, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occured while executing stored procedure 'get_all_business_type'\n{ex.Message}");
                ServiceResponse.Message = ex.Message;
                ServiceResponse.Status = StaticResource.FailStatusCode;
            }
            return ServiceResponse;
        }
        public ServiceResponse<List<ProductTypeDTO>> GetAllProduct(string culture,string CustomerType, int ClientId)
        {
            ServiceResponse<List<ProductTypeDTO>> ServiceResponse = new ServiceResponse<List<ProductTypeDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_culture", culture);
                parameters.Add("p_customertype", CustomerType);
                parameters.Add("p_clientId", ClientId);
                ServiceResponse.Result = Get<ProductTypeDTO>("get_all_product", parameters, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occured while executing stored procedure 'get_all_product'\n{ex.Message}");
                ServiceResponse.Message = ex.Message;
                ServiceResponse.Status = StaticResource.FailStatusCode;
            }
            return ServiceResponse;
        }

        public ServiceResponse<List<DeliveryChannelDTO>> GetAllDeliveryChannel(string culture, string CustomerType, int ClientId)
        {
            ServiceResponse<List<DeliveryChannelDTO>> ServiceResponse = new ServiceResponse<List<DeliveryChannelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_culture", culture);
                parameters.Add("p_customertype", CustomerType);
                parameters.Add("p_clientId", ClientId);
                ServiceResponse.Result = Get<DeliveryChannelDTO>("get_all_delivery_channel", parameters, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occured while executing stored procedure 'get_all_delivery_channel'\n{ex.Message}");

                ServiceResponse.Message = ex.Message;
                ServiceResponse.Status = StaticResource.FailStatusCode;
            }
            return ServiceResponse;
        }

        public ServiceResponse<List<DeliveryChannelDTO>> get_all_mode_of_payment(string culture, int ClientId, string CategoryType)
        {
            ServiceResponse<List<DeliveryChannelDTO>> ServiceResponse = new ServiceResponse<List<DeliveryChannelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_culture", culture);
                parameters.Add("p_customertype", CategoryType);
                parameters.Add("p_clientId", ClientId);
                ServiceResponse.Result = Get<DeliveryChannelDTO>("get_all_mode_of_payment", parameters, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occured while executing stored procedure 'get_all_mode_of_payment'\n{ex.Message}");
                ServiceResponse.Message = ex.Message;
                ServiceResponse.Status = StaticResource.FailStatusCode;
            }
            return ServiceResponse;
        }


        public ServiceResponse<List<DeliveryChannelDTO>> get_all_residence_status(string culture, int ClientId, string CategoryType)
        {
            ServiceResponse<List<DeliveryChannelDTO>> ServiceResponse = new ServiceResponse<List<DeliveryChannelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_culture", culture);
                parameters.Add("p_customertype", CategoryType);
                parameters.Add("p_clientId", ClientId);
                ServiceResponse.Result = Get<DeliveryChannelDTO>("get_all_residence_status", parameters, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occured while executing stored procedure 'get_all_mode_of_payment'\n{ex.Message}");
                ServiceResponse.Message = ex.Message;
                ServiceResponse.Status = StaticResource.FailStatusCode;
            }
            return ServiceResponse;
        }

        public ServiceResponse<List<DeliveryChannelDTO>> GetProfessionalStatus(string culture, string CustomerType, int ClientId)
        {
            ServiceResponse<List<DeliveryChannelDTO>> ServiceResponse = new ServiceResponse<List<DeliveryChannelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_culture", culture);
                parameters.Add("p_customertype", CustomerType);
                parameters.Add("p_clientId", ClientId);
                ServiceResponse.Result = Get<DeliveryChannelDTO>("get_all_Profession_Type", parameters, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occured while executing stored procedure 'get_all_delivery_channel'\n{ex.Message}");

                ServiceResponse.Message = ex.Message;
                ServiceResponse.Status = StaticResource.FailStatusCode;
            }
            return ServiceResponse;
        }

        public ServiceResponse<List<LegalStatusDTO>> GetLegalStatus(string culture, string CustomerType, int ClientId)
        {
            ServiceResponse<List<LegalStatusDTO>> serviceResponse = new ServiceResponse<List<LegalStatusDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_culture", culture);
                parameters.Add("p_customertype", CustomerType);
                parameters.Add("p_clientId", ClientId);
                serviceResponse.Result = Get<LegalStatusDTO>("get_all_legal_status_entity", parameters, commandType: CommandType.StoredProcedure).ToList();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error occured while executing stored procedure 'get_all_legal_status_entity'\n{ex.Message}");

                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<ClientMenuRightsModelDTO> GetMenuRightsByClientId(int ClientIds)
        {
            ServiceResponse<ClientMenuRightsModelDTO> serviceResponse = new ServiceResponse<ClientMenuRightsModelDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_ClientId", ClientIds);
                serviceResponse.Result = GetFirstOrDefault<ClientMenuRightsModelDTO>("get_menu_rights_by_clientId", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "User Group details fetched successfully.";
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
