
namespace Aml.Screening.DataContracts.Dtos
{
    public class CustomerScreenDto
    {
        public string CaseId { get; set; }
        /// <summary>
        /// Gets or sets the custome r_ iddetails.
        /// </summary>
        /// <value>
        /// The custome r_ iddetails.
        /// </value>
        public string CustomerIdType { get; set; }
        /// <summary>
        /// Gets or sets the identifier number.
        /// </summary>
        /// <value>
        /// The identifier number.
        /// </value>
        public string CustomerIdNumber { get; set; }
        /// <summary>
        /// Gets or sets the custome r_ dob.
        /// </summary>
        /// <value>
        /// The custome r_ dob.
        /// </value>
        public string CustomerDob { get; set; }
        /// <summary>
        /// Gets or sets the custome r_ nationality.
        /// </summary>
        /// <value>
        /// The custome r_ nationality.
        /// </value>
        public string CustomerNationality { get; set; }
        /// <summary>
        /// Gets or sets the name of the custome r_.
        /// </summary>
        /// <value>
        /// The name of the custome r_.
        /// </value>
        public string CustomerFullName { get; set; }
        /// <summary>
        /// Gets or sets the customercode.
        /// </summary>
        /// <value>
        /// The customercode.
        /// </value>
        public string CustomerCode { get; set; }
        /// <summary>
        /// Gets or sets the account number.
        /// </summary>
        /// <value>
        /// The account number.
        /// </value>
        /// <author>Anand</author>
        /// <datetime>1/31/2018.12:55 PM</datetime>
        public string AccountNumber { get; set; }
        public string CustomerType { get; set; }
        public int Threshold { get; set; }
        public int ClientId { get; set; }

        public string WhitelistingDate { get; set; }

        public string WWhitelisting { get; set; }

        
    }

   
}
