using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPTMGrabber.DNB
{
    public class CompanyInformationForCookie
    {
        public string companyAddress { get; set; }
        public string companyCity { get; set; }
        public string companyCountry { get; set; }
        public string companyName { get; set; }
        public string companyState { get; set; }
        public string companyZip { get; set; }
        public string encryptedDuns { get; set; }
        public bool completedCreditSignalFlow { get; set; }
    }

    public class CompanyInfo
    {
        public CompanyInformationForCookie companyInformationForCookie { get; set; }
        public string companyAddress { get; set; }
        public string companyCity { get; set; }
        public string companyCountry { get; set; }
        public string companyProfileLink { get; set; }
        public string companyRegion { get; set; }
        public string companyZipCode { get; set; }
        public string countryRegion { get; set; }
        public string duns { get; set; }
        public string primaryName { get; set; }
        public string urlSelector { get; set; }
    }

    public class SearchResult
    {
        public List<CompanyInfo> companies { get; set; }
        public string businessCreditLabel { get; set; }
        public string businessCreditLink { get; set; }
        public string companyProfileLinkLabel { get; set; }
        public string showMoreLabel { get; set; }
        public int returnResults { get; set; }
        public int totalMatchedResults { get; set; }
    }
}
