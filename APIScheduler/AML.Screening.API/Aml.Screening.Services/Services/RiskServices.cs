using Aml.Screening.DataContracts.Dtos;
using Aml.Screening.Services.seeds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aml.Screening.Services.Services
{
    public class RiskServices
    {
        int countryRisk = 0;
        int idRisk = 0;
        int typeRisk = 0;
        
        public double calculateCustomerRisk(CustomerScreenDto dto)
        {

            RiskSamples risk = new RiskSamples();
            var countryResult = risk.GetRiskCountries().Where(x => x.Name.Equals(dto.CustomerNationality.ToUpper())).FirstOrDefault();
            countryRisk = countryResult != null ? countryResult.Score : 1;

            var idResult = risk.GeRiskIDTypes().Where(x => x.Name.Equals(dto.CustomerIdType.ToUpper())).FirstOrDefault();
            idRisk = idResult != null ? idResult.Score : 1;

            var typeResult = risk.GeRiskCustomerTypes().Where(x => x.Name.Equals(dto.CustomerType.ToUpper())).FirstOrDefault();
            typeRisk = typeResult != null ? typeResult.Score : 1;

            double wtavg = (countryRisk + idRisk + typeRisk) / 3;
            double finalscore = Math.Round(3 * wtavg, 2);         

            return finalscore;
        }
    }
}
