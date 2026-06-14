using System;
using System.Collections.Generic;
using System.Text;

namespace Aml.Screening.Services.seeds
{
    public class RiskSamples
    {
     //   public List<CountryRisk> HighRiskCountry { get; set; }
        public List<Risk> GetRiskCountries()
        {
            List<Risk> HighRiskCountry = new List<Risk>();
            HighRiskCountry.Add(new Risk() { Name = "IRAN", Score = 3 });
            HighRiskCountry.Add(new Risk() { Name = "PAKISTAN", Score = 2 });
            HighRiskCountry.Add(new Risk() { Name = "IRAQ", Score = 3 });
            HighRiskCountry.Add(new Risk() { Name = "INDIA", Score = 1 });
            HighRiskCountry.Add(new Risk() { Name = "SYRIA", Score = 3 });
            HighRiskCountry.Add(new Risk() { Name = "SRILANKA", Score = 2 });
            return HighRiskCountry;
        }

        public List<Risk> GeRiskCustomerTypes()
        {
            List<Risk> lst = new List<Risk>();
            lst.Add(new Risk() { Name = "INDIVIDUAL", Score = 1 });
            lst.Add(new Risk() { Name = "CORPORATE", Score = 3 });
            return lst;
        }

        public List<Risk> GeRiskIDTypes()
        {
            List<Risk> lst = new List<Risk>();
            lst.Add(new Risk() { Name = "NATIONALID", Score = 1 });
            lst.Add(new Risk() { Name = "PASSPORT", Score = 2 });
            lst.Add(new Risk() { Name = "DIPLOMATICID", Score = 3 });
            return lst;
        }
    }
    public class Risk
    {
        public int Score { get; set; }
        public string Name { get; set; }
    }

    public class CustomerTypeRisk
    {
        public int Risk { get; set; }
        public string Name { get; set; }
    }
}
