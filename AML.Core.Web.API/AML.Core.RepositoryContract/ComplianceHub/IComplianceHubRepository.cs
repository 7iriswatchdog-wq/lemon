using AML.ViewModel.ViewModels.ComplianceHub;

namespace AML.Core.RepositoryContract.ComplianceHub
{
    public interface IComplianceHubRepository
    {
        KycExpiryVM             GetKycExpiry(int clientId, int horizonDays);
        PeriodicReviewVM        GetPeriodicReview(int clientId);
        DataCompletenessVM      GetDataCompleteness(int clientId);
        OnboardingFunnelVM      GetOnboardingFunnel(int clientId, int monthsBack);
        ScreeningGapVM          GetScreeningGap(int clientId);
        SanctionsMatrixVM       GetSanctionsMatrix(int clientId);
        PepInventoryVM          GetPepInventory(int clientId);
        AdverseMediaVM          GetAdverseMedia(int clientId);
        CrossBorderMapVM        GetCrossBorderMap(int clientId);
        ChannelProductMixVM     GetChannelProductMix(int clientId);
        WhitelistGovernanceVM   GetWhitelistGovernance(int clientId);
        ProliferationRegisterVM GetProliferationRegister(int clientId);

        // Hub landing live-stat (cheap counts only)
        ComplianceHubLiveStats  GetHubLiveStats(int clientId);
    }

    public class ComplianceHubLiveStats
    {
        public int DocsExpiring30 { get; set; }
        public int ReviewsOverdue { get; set; }
        public int IncompleteCustomers { get; set; }
        public int OnboardedThisMonth { get; set; }
        public int ScreeningGapCount { get; set; }
        public int SanctionsHits { get; set; }
        public int PepCount { get; set; }
        public int AdverseMediaCount { get; set; }
        public int DistinctCountries { get; set; }
        public int HighRiskMixCount { get; set; }
        public int WhitelistEvents { get; set; }
        public int ProliferationCases { get; set; }
    }
}
