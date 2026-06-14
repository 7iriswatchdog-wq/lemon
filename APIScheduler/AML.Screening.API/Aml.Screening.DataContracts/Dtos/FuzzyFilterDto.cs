using System;
using System.Collections.Generic;
using System.Text;

namespace Aml.Screening.DataContracts.Dtos
{
    public class FuzzyFilterDto
    {
        public string NAME { get; set; }
        public double SCORE { get; set; }
        public string UID { get; set; }
        public string CATEGORY { get; set; }
        public string TYPE { get; set; }
        public bool ISPOSITIVE { get; set; }
    }
    public class MatchDto
    {
        public string MatchRange { get; set; }
        public string MatchRisk { get; set; }
    }
    public class IdDto
    {
        public string Id { get; set; }
    }
}
