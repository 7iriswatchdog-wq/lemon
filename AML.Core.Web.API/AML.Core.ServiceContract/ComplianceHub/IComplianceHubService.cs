using AML.ViewModel.ViewModels.ComplianceHub;

namespace AML.Core.ServiceContract.ComplianceHub
{
    public interface IComplianceHubService
    {
        ComplianceHubVM         BuildHub(int clientId, System.Func<string, string> urlFor);

        KycExpiryVM             GetKycExpiry(int clientId, int horizonDays = 365);
        PeriodicReviewVM        GetPeriodicReview(int clientId);
        DataCompletenessVM      GetDataCompleteness(int clientId);
        OnboardingFunnelVM      GetOnboardingFunnel(int clientId, int monthsBack = 12);
        ScreeningGapVM          GetScreeningGap(int clientId);
        SanctionsMatrixVM       GetSanctionsMatrix(int clientId);
        PepInventoryVM          GetPepInventory(int clientId);
        AdverseMediaVM          GetAdverseMedia(int clientId);
        CrossBorderMapVM        GetCrossBorderMap(int clientId);
        ChannelProductMixVM     GetChannelProductMix(int clientId);
        WhitelistGovernanceVM   GetWhitelistGovernance(int clientId);
        ProliferationRegisterVM GetProliferationRegister(int clientId);
    }
}
