
namespace Aml.Screening.DataContracts.Dtos
{
    public  class ScreeningResponse
    {
        /// <summary>
        /// Gets or sets the is positive.
        /// </summary>
        /// <value>
        /// The is positive.
        /// </value>
        public bool IsPositive { get; set; }
        /// <summary>
        /// Gets or sets the match uid.
        /// </summary>
        /// <value>
        /// The match uid.
        /// </value>
        public string MatchUID { get; set; }
        /// <summary>
        /// Gets or sets the name of the match.
        /// </summary>
        /// <value>
        /// The name of the match.
        /// </value>
        public string MatchName { get; set; }
        /// <summary>
        /// Gets or sets the match percentage.
        /// </summary>
        /// <value>
        /// The match percentage.
        /// </value>
        public string MatchPercentage { get; set; }
        /// <summary>
        /// Gets or sets the match risk.
        /// </summary>
        /// <value>
        /// The match risk.
        /// </value>
        public string MatchRisk { get; set; }
        /// <summary>
        /// Gets or sets the match category.
        /// </summary>
        /// <value>
        /// The match category.
        /// </value>
        public string MatchCategory { get; set; }
        /// <summary>
        /// Gets or sets the match category.
        /// </summary>
        /// <value>
        /// The match category.
        /// </value>
        public string MatchType { get; set; }        
        /// <summary>
        /// Gets or sets the match count.
        /// </summary>
        /// <value>
        /// The match count.
        /// </value>
        /// <author>Anand</author>
        /// <datetime>1/29/2018.02:40 PM</datetime>
        public string MatchCount { get; set; }
        /// <summary>
        /// Gets or sets the authentication flag.
        /// </summary>
        /// <value>
        /// The authentication flag.
        /// </value>
        /// <author>Anand</author>
        /// <datetime>1/29/2018.02:41 PM</datetime>
        public string AuthFlag { get; set; }
        /// <summary>
        /// Gets or sets the match remarks.
        /// </summary>
        /// <value>
        /// The match remarks.
        /// </value>
        /// <author>Anand</author>
        /// <datetime>1/30/2018.07:34 PM</datetime>
        public string MatchRemarks { get; set; }        
    }
}
