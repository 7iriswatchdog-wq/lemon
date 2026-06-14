using System;
using System.Collections.Generic;
using System.Linq;
using AML.Core.RepositoryContract.ComplianceHub;
using AML.Core.Service.ComplianceHub;
using AML.ViewModel.ViewModels.ComplianceHub;
using Moq;
using Xunit;

namespace AML.Web.Tests
{
    public class ComplianceHubServiceTests
    {
        private static (ComplianceHubService svc, Mock<IComplianceHubRepository> repo) Build()
        {
            var repo = new Mock<IComplianceHubRepository>(MockBehavior.Strict);
            return (new ComplianceHubService(repo.Object), repo);
        }

        // ── BuildHub ────────────────────────────────────────────────

        [Fact]
        public void BuildHub_returns_twelve_tiles_grouped_into_four_themes()
        {
            var (svc, repo) = Build();
            repo.Setup(r => r.GetHubLiveStats(1)).Returns(new ComplianceHubLiveStats());

            var vm = svc.BuildHub(1, action => "/Compliance/" + action);

            Assert.Equal(12, vm.Tiles.Count);
            var themes = vm.Tiles.Select(t => t.Theme).Distinct().OrderBy(t => t).ToList();
            Assert.Equal(new[] { "Geography", "Governance", "Lifecycle", "Screening" }, themes);
        }

        [Fact]
        public void BuildHub_uses_url_factory_for_each_tile()
        {
            var (svc, repo) = Build();
            repo.Setup(r => r.GetHubLiveStats(7)).Returns(new ComplianceHubLiveStats());

            var vm = svc.BuildHub(7, a => $"/x/{a}");

            Assert.All(vm.Tiles, t => Assert.StartsWith("/x/", t.Url));
            Assert.Contains(vm.Tiles, t => t.Url == "/x/KycExpiry");
            Assert.Contains(vm.Tiles, t => t.Url == "/x/Proliferation");
        }

        [Fact]
        public void BuildHub_propagates_live_stats_into_tile_subset()
        {
            var (svc, repo) = Build();
            repo.Setup(r => r.GetHubLiveStats(1)).Returns(new ComplianceHubLiveStats
            {
                DocsExpiring30 = 5, ScreeningGapCount = 2, PepCount = 3,
                ProliferationCases = 9, OnboardedThisMonth = 4
            });

            var vm = svc.BuildHub(1, a => a);
            string Stat(string code) => vm.Tiles.Single(t => t.Code == code).LiveStat;

            Assert.Equal("5", Stat("CH-1"));
            Assert.Equal("4", Stat("CH-4"));
            Assert.Equal("2", Stat("CH-5"));
            Assert.Equal("3", Stat("CH-7"));
            Assert.Equal("9", Stat("CH-12"));
        }

        [Fact]
        public void BuildHub_handles_null_live_stats_without_throwing()
        {
            var (svc, repo) = Build();
            repo.Setup(r => r.GetHubLiveStats(1)).Returns((ComplianceHubLiveStats)null);

            var vm = svc.BuildHub(1, a => a);

            Assert.Equal(12, vm.Tiles.Count);
        }

        // ── Pass-through methods (12) ───────────────────────────────

        [Fact]
        public void GetKycExpiry_passes_through_repo_result()
        {
            var (svc, repo) = Build();
            var expected = new KycExpiryVM { Rows = new List<KycExpiryRow> { new KycExpiryRow { CustomerCode = "X1" } } };
            repo.Setup(r => r.GetKycExpiry(1, 365)).Returns(expected);

            Assert.Same(expected, svc.GetKycExpiry(1));
        }

        [Fact]
        public void GetKycExpiry_returns_empty_VM_when_repo_returns_null()
        {
            var (svc, repo) = Build();
            repo.Setup(r => r.GetKycExpiry(1, 365)).Returns((KycExpiryVM)null);

            var vm = svc.GetKycExpiry(1);

            Assert.NotNull(vm);
            Assert.Empty(vm.Rows);
        }

        [Fact]
        public void GetPeriodicReview_returns_empty_VM_when_repo_returns_null()
        {
            var (svc, repo) = Build();
            repo.Setup(r => r.GetPeriodicReview(2)).Returns((PeriodicReviewVM)null);

            Assert.NotNull(svc.GetPeriodicReview(2));
        }

        [Fact]
        public void GetDataCompleteness_returns_empty_VM_when_repo_returns_null()
        {
            var (svc, repo) = Build();
            repo.Setup(r => r.GetDataCompleteness(2)).Returns((DataCompletenessVM)null);

            Assert.NotNull(svc.GetDataCompleteness(2));
        }

        [Fact]
        public void GetOnboardingFunnel_default_is_twelve_months()
        {
            var (svc, repo) = Build();
            repo.Setup(r => r.GetOnboardingFunnel(3, 12)).Returns(new OnboardingFunnelVM());

            svc.GetOnboardingFunnel(3);

            repo.Verify(r => r.GetOnboardingFunnel(3, 12), Times.Once);
        }

        [Fact]
        public void GetScreeningGap_returns_empty_VM_when_repo_returns_null()
        {
            var (svc, repo) = Build();
            repo.Setup(r => r.GetScreeningGap(2)).Returns((ScreeningGapVM)null);

            Assert.NotNull(svc.GetScreeningGap(2));
        }

        [Fact]
        public void GetSanctionsMatrix_returns_empty_VM_when_repo_returns_null()
        {
            var (svc, repo) = Build();
            repo.Setup(r => r.GetSanctionsMatrix(2)).Returns((SanctionsMatrixVM)null);

            Assert.NotNull(svc.GetSanctionsMatrix(2));
        }

        [Fact]
        public void GetPepInventory_returns_empty_VM_when_repo_returns_null()
        {
            var (svc, repo) = Build();
            repo.Setup(r => r.GetPepInventory(2)).Returns((PepInventoryVM)null);

            Assert.NotNull(svc.GetPepInventory(2));
        }

        [Fact]
        public void GetAdverseMedia_returns_empty_VM_when_repo_returns_null()
        {
            var (svc, repo) = Build();
            repo.Setup(r => r.GetAdverseMedia(2)).Returns((AdverseMediaVM)null);

            Assert.NotNull(svc.GetAdverseMedia(2));
        }

        [Fact]
        public void GetCrossBorderMap_returns_empty_VM_when_repo_returns_null()
        {
            var (svc, repo) = Build();
            repo.Setup(r => r.GetCrossBorderMap(2)).Returns((CrossBorderMapVM)null);

            Assert.NotNull(svc.GetCrossBorderMap(2));
        }

        [Fact]
        public void GetChannelProductMix_returns_empty_VM_when_repo_returns_null()
        {
            var (svc, repo) = Build();
            repo.Setup(r => r.GetChannelProductMix(2)).Returns((ChannelProductMixVM)null);

            Assert.NotNull(svc.GetChannelProductMix(2));
        }

        [Fact]
        public void GetWhitelistGovernance_returns_empty_VM_when_repo_returns_null()
        {
            var (svc, repo) = Build();
            repo.Setup(r => r.GetWhitelistGovernance(2)).Returns((WhitelistGovernanceVM)null);

            Assert.NotNull(svc.GetWhitelistGovernance(2));
        }

        [Fact]
        public void GetProliferationRegister_returns_empty_VM_when_repo_returns_null()
        {
            var (svc, repo) = Build();
            repo.Setup(r => r.GetProliferationRegister(2)).Returns((ProliferationRegisterVM)null);

            Assert.NotNull(svc.GetProliferationRegister(2));
        }

        // ── VM-derived helpers ──────────────────────────────────────

        [Fact]
        public void KycExpiryVM_buckets_count_correctly()
        {
            var vm = new KycExpiryVM
            {
                Rows = new List<KycExpiryRow>
                {
                    new() { DaysUntil = -5 }, new() { DaysUntil = -1 },                 // 2 expired
                    new() { DaysUntil = 0 }, new() { DaysUntil = 30 },                   // 2 within 30
                    new() { DaysUntil = 31 }, new() { DaysUntil = 60 },                  // 2 within 60
                    new() { DaysUntil = 61 }, new() { DaysUntil = 90 },                  // 2 within 90
                    new() { DaysUntil = 200 }                                            // future
                }
            };
            Assert.Equal(2, vm.Expired);
            Assert.Equal(2, vm.Within30);
            Assert.Equal(2, vm.Within60);
            Assert.Equal(2, vm.Within90);
        }

        [Fact]
        public void PeriodicReviewVM_overdue_and_due_in_30_split_correctly()
        {
            var vm = new PeriodicReviewVM
            {
                Rows = new List<PeriodicReviewRow>
                {
                    new() { DaysUntilReview = -10 }, new() { DaysUntilReview = -1 },
                    new() { DaysUntilReview = 0 }, new() { DaysUntilReview = 15 }, new() { DaysUntilReview = 30 },
                    new() { DaysUntilReview = 31 }
                }
            };
            Assert.Equal(2, vm.Overdue);
            Assert.Equal(3, vm.DueIn30);
        }

        [Fact]
        public void ScreeningGapVM_pct_calculated_correctly_and_handles_zero_total()
        {
            var vm = new ScreeningGapVM
            {
                TotalCustomers = 100,
                Rows = Enumerable.Range(0, 7).Select(_ => new ScreeningGapRow()).ToList()
            };
            Assert.Equal(7, vm.Unscreened);
            Assert.Equal(7.0, vm.UnscreenedPct);

            var empty = new ScreeningGapVM { TotalCustomers = 0 };
            Assert.Equal(0, empty.UnscreenedPct);
        }

        [Fact]
        public void DataCompletenessVM_overall_score_is_average_of_field_scores()
        {
            var vm = new DataCompletenessVM
            {
                Fields = new List<FieldCompleteness>
                {
                    new() { Populated = 10, Total = 10 }, // 100%
                    new() { Populated = 5,  Total = 10 }, //  50%
                    new() { Populated = 0,  Total = 10 }  //   0%
                }
            };
            Assert.Equal(50.0, vm.OverallScore);

            var empty = new DataCompletenessVM();
            Assert.Equal(0, empty.OverallScore);
        }

        [Fact]
        public void PepInventoryVM_jurisdiction_aggregation_orders_by_count_desc()
        {
            var vm = new PepInventoryVM
            {
                Rows = new List<PepRow>
                {
                    new() { Nationality = "UAE",   IsDomestic = true },
                    new() { Nationality = "UAE",   IsForeign = true },
                    new() { Nationality = "UAE",   IsDomestic = true },
                    new() { Nationality = "India", IsForeign = true },
                    new() { Nationality = null,    IsDomestic = true }
                }
            };
            Assert.Equal(2, vm.Domestic + 0); // 2 IsDomestic out of 5
            Assert.Equal(2, vm.Foreign);
            Assert.Equal(2, vm.Domestic);
            Assert.Equal("UAE", vm.ByJurisdiction.First().Key);
            Assert.Equal(3, vm.ByJurisdiction.First().Value);
            Assert.Contains(vm.ByJurisdiction, kv => kv.Key == "Unknown" && kv.Value == 1);
        }

        [Fact]
        public void AdverseMediaVM_overdue_review_is_only_above_90_days()
        {
            var vm = new AdverseMediaVM
            {
                Rows = new List<AdverseMediaRow>
                {
                    new() { DaysSinceReview = 10, IsConfirmed = true },
                    new() { DaysSinceReview = 90, IsConfirmed = true },
                    new() { DaysSinceReview = 91, IsConfirmed = false },
                    new() { DaysSinceReview = 365, IsConfirmed = true }
                }
            };
            Assert.Equal(2, vm.OverdueReview);
            Assert.Equal(3, vm.Confirmed);
            Assert.Equal(1, vm.Partial);
        }

        [Fact]
        public void CrossBorderMapVM_high_risk_exposure_sums_grey_and_black()
        {
            var vm = new CrossBorderMapVM
            {
                Countries = new List<CountryExposure>
                {
                    new() { Name = "Iran",       NationalityCount = 5, FatfStatus = "Black" },
                    new() { Name = "Yemen",      NationalityCount = 2, FatfStatus = "Grey"  },
                    new() { Name = "UAE",        NationalityCount = 9, FatfStatus = null     },
                    new() { Name = "Mozambique", NationalityCount = 1, FatfStatus = "Grey"  }
                }
            };
            Assert.Equal(8, vm.HighRiskExposure);
            Assert.Equal(17, vm.TotalCustomers);
            Assert.Equal(4, vm.DistinctCountries);
        }

        [Fact]
        public void OnboardingFunnelVM_totals_aggregate_across_months()
        {
            var vm = new OnboardingFunnelVM
            {
                Months = new List<OnboardingMonth>
                {
                    new() { Onboarded = 5, Approved = 3, Rejected = 1, Pending = 1 },
                    new() { Onboarded = 7, Approved = 5, Rejected = 0, Pending = 2 }
                }
            };
            Assert.Equal(12, vm.TotalOnboarded);
            Assert.Equal(8, vm.TotalApproved);
            Assert.Equal(1, vm.TotalRejected);
            Assert.Equal(3, vm.TotalPending);
        }

        [Fact]
        public void WhitelistGovernanceVM_unjustified_counts_blank_and_whitespace()
        {
            var vm = new WhitelistGovernanceVM
            {
                Events = new List<WhitelistEvent>
                {
                    new() { Justification = "valid reason", WhitelistedOn = DateTime.UtcNow.AddDays(-10) },
                    new() { Justification = "",             WhitelistedOn = DateTime.UtcNow.AddDays(-10) },
                    new() { Justification = "   ",          WhitelistedOn = DateTime.UtcNow.AddDays(-10) },
                    new() { Justification = null,           WhitelistedOn = DateTime.UtcNow.AddDays(-10) }
                }
            };
            Assert.Equal(3, vm.Unjustified);
            Assert.Equal(4, vm.TotalEvents);
        }

        [Fact]
        public void WhitelistGovernanceVM_older_than_one_year_uses_strict_threshold()
        {
            var vm = new WhitelistGovernanceVM
            {
                Events = new List<WhitelistEvent>
                {
                    new() { WhitelistedOn = DateTime.UtcNow.AddDays(-100) },
                    new() { WhitelistedOn = DateTime.UtcNow.AddDays(-365) },
                    new() { WhitelistedOn = DateTime.UtcNow.AddDays(-400) }
                }
            };
            Assert.Equal(1, vm.OlderThan1Year);
        }

        [Fact]
        public void ProliferationRegisterVM_status_buckets_are_case_insensitive()
        {
            var vm = new ProliferationRegisterVM
            {
                Rows = new List<ProliferationRow>
                {
                    new() { Status = "Pending"  }, new() { Status = "pending" },
                    new() { Status = "APPROVED" }, new() { Status = "Rejected" },
                    new() { Status = null }
                }
            };
            Assert.Equal(5, vm.Total);
            Assert.Equal(2, vm.Pending);
            Assert.Equal(1, vm.Approved);
            Assert.Equal(1, vm.Rejected);
        }

        [Fact]
        public void ProliferationRegisterVM_top_chemicals_groups_and_orders()
        {
            var vm = new ProliferationRegisterVM
            {
                Rows = new List<ProliferationRow>
                {
                    new() { ChemicalName = "Acetone" }, new() { ChemicalName = "Acetone" },
                    new() { ChemicalName = "Toluene" }, new() { ChemicalName = "Acetone" },
                    new() { ChemicalName = null },     new() { ChemicalName = "" }
                }
            };
            Assert.Equal(2, vm.TopChemicals.Count);
            Assert.Equal("Acetone", vm.TopChemicals.First().Key);
            Assert.Equal(3, vm.TopChemicals.First().Value);
        }
    }
}
