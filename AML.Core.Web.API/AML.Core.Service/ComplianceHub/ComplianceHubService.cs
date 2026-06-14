using AML.Core.RepositoryContract.ComplianceHub;
using AML.Core.ServiceContract.ComplianceHub;
using AML.ViewModel.ViewModels.ComplianceHub;
using System;

namespace AML.Core.Service.ComplianceHub
{
    public class ComplianceHubService : IComplianceHubService
    {
        private readonly IComplianceHubRepository _repo;

        public ComplianceHubService(IComplianceHubRepository repo)
        {
            _repo = repo;
        }

        public KycExpiryVM             GetKycExpiry(int clientId, int horizonDays = 365)
            => _repo.GetKycExpiry(clientId, horizonDays) ?? new KycExpiryVM();
        public PeriodicReviewVM        GetPeriodicReview(int clientId)
            => _repo.GetPeriodicReview(clientId) ?? new PeriodicReviewVM();
        public DataCompletenessVM      GetDataCompleteness(int clientId)
            => _repo.GetDataCompleteness(clientId) ?? new DataCompletenessVM();
        public OnboardingFunnelVM      GetOnboardingFunnel(int clientId, int monthsBack = 12)
            => _repo.GetOnboardingFunnel(clientId, monthsBack) ?? new OnboardingFunnelVM();
        public ScreeningGapVM          GetScreeningGap(int clientId)
            => _repo.GetScreeningGap(clientId) ?? new ScreeningGapVM();
        public SanctionsMatrixVM       GetSanctionsMatrix(int clientId)
            => _repo.GetSanctionsMatrix(clientId) ?? new SanctionsMatrixVM();
        public PepInventoryVM          GetPepInventory(int clientId)
            => _repo.GetPepInventory(clientId) ?? new PepInventoryVM();
        public AdverseMediaVM          GetAdverseMedia(int clientId)
            => _repo.GetAdverseMedia(clientId) ?? new AdverseMediaVM();
        public CrossBorderMapVM        GetCrossBorderMap(int clientId)
            => _repo.GetCrossBorderMap(clientId) ?? new CrossBorderMapVM();
        public ChannelProductMixVM     GetChannelProductMix(int clientId)
            => _repo.GetChannelProductMix(clientId) ?? new ChannelProductMixVM();
        public WhitelistGovernanceVM   GetWhitelistGovernance(int clientId)
            => _repo.GetWhitelistGovernance(clientId) ?? new WhitelistGovernanceVM();
        public ProliferationRegisterVM GetProliferationRegister(int clientId)
            => _repo.GetProliferationRegister(clientId) ?? new ProliferationRegisterVM();

        public ComplianceHubVM BuildHub(int clientId, Func<string, string> urlFor)
        {
            var stats = _repo.GetHubLiveStats(clientId) ?? new ComplianceHubLiveStats();
            var vm = new ComplianceHubVM();
            void Add(string code, string name, string desc, string theme, string action, string icon, string accent, int? stat, string statLabel)
            {
                vm.Tiles.Add(new ComplianceHubTile
                {
                    Code = code,
                    Name = name,
                    Description = desc,
                    Theme = theme,
                    Url = urlFor(action),
                    Icon = icon,
                    Accent = accent,
                    LiveStat = stat?.ToString(),
                    LiveStatLabel = statLabel
                });
            }

            // Lifecycle
            Add("CH-1",  "KYC Document Expiry Calendar", "Passport, Emirates ID and trade-licence expiry timeline.",
                "Lifecycle", "KycExpiry", "calendar-clock", "blue", stats.DocsExpiring30, "expiring in 30 days");
            Add("CH-2",  "Periodic Review Calendar", "Customers due for KYC refresh by risk-band cadence.",
                "Lifecycle", "PeriodicReview", "calendar-check", "indigo", null, null);
            Add("CH-3",  "Data Completeness Scorecard", "Per-field null-rate and per-customer completeness score.",
                "Lifecycle", "DataCompleteness", "list-checks", "emerald", null, null);
            Add("CH-4",  "Onboarding Funnel", "Monthly intake + decision lag.",
                "Lifecycle", "OnboardingFunnel", "trending-up", "sky", stats.OnboardedThisMonth, "onboarded this month");
            Add("CH-5",  "Onboarding-Without-Screening Gap", "Customers in the master with no screening record.",
                "Lifecycle", "ScreeningGap", "alert-triangle", "rose", stats.ScreeningGapCount, "unscreened customers");

            // Screening
            Add("CH-6",  "Sanctions Hit Disposition Matrix", "True/partial × PEP/sanction/adverse-media heatmap.",
                "Screening", "SanctionsMatrix", "shield-alert", "amber", stats.SanctionsHits, "cases with hits");
            Add("CH-7",  "PEP Inventory", "Domestic + foreign PEP register, jurisdictional view.",
                "Screening", "PepInventory", "user-cog", "purple", stats.PepCount, "PEP cases");
            Add("CH-8",  "Adverse Media Watch", "Media-flagged customers with time-since-review.",
                "Screening", "AdverseMedia", "newspaper", "fuchsia", stats.AdverseMediaCount, "adverse-media cases");

            // Geography
            Add("CH-9",  "Cross-Border Customer Map", "World view of nationality, residence and source-of-wealth countries.",
                "Geography", "CrossBorderMap", "globe", "teal", stats.DistinctCountries, "distinct countries");
            Add("CH-10", "Channel & Product Risk Mix", "Sankey of delivery-channel → product → risk band.",
                "Geography", "ChannelProductMix", "git-merge", "cyan", null, null);

            // Governance
            Add("CH-11", "Whitelist Governance Register", "Every whitelist event with the matching justification comment.",
                "Governance", "Whitelist", "list-check", "lime", stats.WhitelistEvents, "whitelist events");
            Add("CH-12", "Proliferation Finance Register", "PF cases with chemical / HS-code matches.",
                "Governance", "Proliferation", "atom", "orange", stats.ProliferationCases, "PF cases");

            return vm;
        }
    }
}
