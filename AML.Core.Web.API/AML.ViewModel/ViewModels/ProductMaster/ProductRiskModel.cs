using AML.DTO.DTO.ProdMaster;
using AML.DTO.DTO.Risk;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.ProductMaster
{
    public class ProductRiskModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
       
        public DateTime DateofAssessment { get; set; }

        public string FinalRiskScore { get; set; }
        public int ProdRiskScoreSum { get; set; }
        public int ProdRiskScoreCount { get; set; }
        public string RiskScoreBeforeOverride { get; set; }


        public List<ProdRiskTypeCategoryDTO> ProdRiskTypeCategoryDTO { get; set; }

        public List<ProdReportDataDTO> ProdReportDataDTO { get; set; }


        public bool userAuthorised { get; set; }
        public int ClientId { get; set; }
        public int CreatedBy { get; set; }
        public int version { get; set; }
    }
}
