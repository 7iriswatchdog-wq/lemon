using AML.Core.Repository.AmlTracker;
using AML.Core.Repository.Branch;
using AML.Core.Repository.CodesMaster;
using AML.Core.Repository.Common;
using AML.Core.Repository.Company;
using AML.Core.Repository.CorporateShareholder;
using AML.Core.Repository.Country;
using AML.Core.Repository.CustomerCase;
using AML.Core.Repository.CustomerCategory;
using AML.Core.Repository.CustomerScreening;
using AML.Core.Repository.Department;
using AML.Core.Repository.Designation;
using AML.Core.Repository.DigiApiUser;
using AML.Core.Repository.EtlBatch;
using AML.Core.Repository.EWRA;
using AML.Core.Repository.FreeSource;
using AML.Core.Repository.IdentityType;
using AML.Core.Repository.InternalWatchList;
using AML.Core.Repository.Kyc;
using AML.Core.Repository.LegalType;
using AML.Core.Repository.LovMaster;
using AML.Core.Repository.LovMasterV2;
using AML.Core.Repository.Notepad;
using AML.Core.Repository.ProductMaster;
using AML.Core.Repository.ProliferationFinance;
using AML.Core.Repository.Report;
using AML.Core.Repository.Risk;
using AML.Core.Repository.RiskV2;
using AML.Core.Repository.Sanction;
using AML.Core.Repository.TransactionMonitor;
using AML.Core.Repository.SectoralTMS;
using AML.Core.RepositoryContract.SectoralTMS;
using AML.Core.Service.SectoralTMS;
using AML.Core.ServiceContract.SectoralTMS;
using AML.Core.Repository.TransactionScreening;
using AML.Core.Repository.User;
using AML.Core.Repository.UserAccess;
using AML.Core.Repository.UserGroup;
using AML.Core.Repository.VisaType;
using AML.Core.RepositoryContract;
using AML.Core.RepositoryContract.AmlTracker;
using AML.Core.RepositoryContract.Branch;
using AML.Core.RepositoryContract.CodeMaster;
using AML.Core.RepositoryContract.Common;
using AML.Core.RepositoryContract.Company;
using AML.Core.RepositoryContract.CorporateShareholder;
using AML.Core.RepositoryContract.Country;
using AML.Core.RepositoryContract.CustomerCase;
using AML.Core.RepositoryContract.CustomerCategory;
using AML.Core.RepositoryContract.CustomerScreening;
using AML.Core.RepositoryContract.Department;
using AML.Core.RepositoryContract.Designation;
using AML.Core.RepositoryContract.DigiApiUser;
using AML.Core.RepositoryContract.EtlBatch;
using AML.Core.RepositoryContract.EWRA;
using AML.Core.RepositoryContract.FreeSource;
using AML.Core.RepositoryContract.IdentityType;
using AML.Core.RepositoryContract.InternalWatchList;
using AML.Core.RepositoryContract.Kyc;
using AML.Core.RepositoryContract.LegalType;
using AML.Core.RepositoryContract.LovMaster;
using AML.Core.RepositoryContract.LovMasterV2;
using AML.Core.RepositoryContract.Notepad;
using AML.Core.RepositoryContract.ProductMaster;
using AML.Core.RepositoryContract.ProliferationFinance;
using AML.Core.RepositoryContract.Report;
using AML.Core.RepositoryContract.Risk;
using AML.Core.RepositoryContract.RiskV2;
using AML.Core.RepositoryContract.Sanction;
using AML.Core.RepositoryContract.TransactionMonitor;
using AML.Core.RepositoryContract.TransactionScreening;
using AML.Core.RepositoryContract.User;
using AML.Core.RepositoryContract.UserAccess;
using AML.Core.RepositoryContract.UserGroup;
using AML.Core.RepositoryContract.VisaType;
using AML.Core.Service.AI;
using AML.Core.Service.CaseStudio;
using AML.Core.Service.AmlTracker;
using AML.Core.Service.Branch;
using AML.Core.Service.CaseAssignment;
using AML.Core.Service.CaseComment;
using AML.Core.Service.CaseDocument;
using AML.Core.Service.CodesMaster;
using AML.Core.Service.Common;
using AML.Core.Service.Company;
using AML.Core.Service.Country;
using AML.Core.Service.CustomerCase;
using AML.Core.Service.CustomerCategory;
using AML.Core.Service.CustomerMaster;
using AML.Core.Service.CustomerScreening;
using AML.Core.Service.Department;
using AML.Core.Service.Designation;
using AML.Core.Service.DigiApiUser;
using AML.Core.Service.EtlBatch;
using AML.Core.Service.EWRA;
using AML.Core.Service.FreeSource;
using AML.Core.Service.IdentityType;
using AML.Core.Service.InternalWatchList;
using AML.Core.Service.Kyc;
using AML.Core.Service.LegalType;
using AML.Core.Service.LovMaster;
using AML.Core.Service.LovMasterV2;
using AML.Core.Service.Notepad;
using AML.Core.Service.ProductMaster;
using AML.Core.Service.ProliferationFinance;
using AML.Core.Service.Report;
using AML.Core.Service.Risk;
using AML.Core.Service.RiskV2;
using AML.Core.Service.Sanction;
using AML.Core.Service.TransactionMonitor;
using AML.Core.Service.TransactionScreening;
using AML.Core.Service.User;
using AML.Core.Service.UserAccess;
using AML.Core.Service.UserGroup;
using AML.Core.Service.VisaType;
using AML.Core.ServiceContract.AI;

using AML.Core.ServiceContract.AmlTracker;
using AML.Core.ServiceContract.Branch;
using AML.Core.ServiceContract.CaseAssignment;
using AML.Core.ServiceContract.CaseComment;
using AML.Core.ServiceContract.CaseDocument;
using AML.Core.ServiceContract.CodeMaster;
using AML.Core.ServiceContract.Common;
using AML.Core.ServiceContract.Company;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.CustomerCatogory;
using AML.Core.ServiceContract.CustomerMaster;
using AML.Core.ServiceContract.CustomerScreening;
using AML.Core.ServiceContract.Department;
using AML.Core.ServiceContract.Designation;
using AML.Core.ServiceContract.DigiApiUser;
using AML.Core.ServiceContract.EtlBatch;
using AML.Core.ServiceContract.EWRA;
using AML.Core.ServiceContract.FreeSource;
using AML.Core.ServiceContract.IdentityType;
using AML.Core.ServiceContract.InternalWatchList;
using AML.Core.ServiceContract.Kyc;
using AML.Core.ServiceContract.LegalType;
using AML.Core.ServiceContract.LovMaster;
using AML.Core.ServiceContract.LovMasterV2;
using AML.Core.ServiceContract.Notepad;
using AML.Core.ServiceContract.ProductMaster;
using AML.Core.ServiceContract.ProliferationFinance;
using AML.Core.ServiceContract.Report;
using AML.Core.ServiceContract.Risk;
using AML.Core.ServiceContract.RiskV2;
using AML.Core.ServiceContract.Sanction;
using AML.Core.ServiceContract.TransactionMonitor;
using AML.Core.ServiceContract.TransactionScreening;
using AML.Core.ServiceContract.User;
using AML.Core.ServiceContract.UserAccess;
using AML.Core.ServiceContract.UserGroup;
using AML.Core.ServiceContract.VisaType;
using AML.Web.CustomFilters;
using AML.Core.ServiceContract.CaseStudio;
using AML.Web.Helper;
using AML.Web.Helpers;
using AML.Web.Services;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NLog;
using NLog.Extensions.Logging;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace AML.Web
{
    public class Startup
    {
        private string _amlConnectionString = string.Empty;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _amlConnectionString = configuration
                                .GetSection("Data:Aml")
                                .GetSection("DbConnectionString").Value;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Localizatioj
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddMvc()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var cultures = new List<CultureInfo> {
        new CultureInfo("en"),
        new CultureInfo("ar")
        { DateTimeFormat = { Calendar = new GregorianCalendar() }}
                };
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en");
                options.SupportedCultures = cultures;
                options.SupportedUICultures = cultures;
            });
            ///

            services.AddMvc().AddNToastNotifyToastr(new ToastrOptions()
            {
                ProgressBar = true,
                PositionClass = ToastPositions.BottomRight,
                // Auto-dismiss after 4s to stop the toast-stacking problem reported on
                // 2026-05-03; ExtendedTimeOut keeps it visible while the user hovers, and
                // CloseButton lets them snap it shut. For genuinely critical errors that
                // must persist, the inline call site can override TimeOut: 0 per-call.
                TimeOut = 4000,
                ExtendedTimeOut = 2000,
                CloseButton = true,
                TapToDismiss = true,
                PreventDuplicates = true,
                NewestOnTop = true
            }).AddSessionStateTempDataProvider();
            // Session cookie hardening — closes the session-hijack risk surfaced in the
            // 2026-05-02 audit (HttpOnly stops JS access if any XSS slips through; Secure
            // forces HTTPS; SameSite=Lax blocks the cross-site CSRF leg without breaking
            // top-level navigation; IdleTimeout caps how long a stolen cookie stays valid).
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.IsEssential = true;
            });

            //services.AddMvc(o =>
            //{
            //    var policy = new AuthorizationPolicyBuilder()
            //        .RequireAuthenticatedUser()
            //        .Build();
            //    o.Filters.Add(new AuthorizeFilter(policy));
            //}).AddNToastNotifyToastr(new ToastrOptions()
            //{
            //    ProgressBar = false,
            //    PositionClass = ToastPositions.BottomCenter
            //});
            services.AddMvc()
            .AddSessionStateTempDataProvider();

            services.AddControllersWithViews();
            services.AddControllersWithViews(options =>
                {
                    // Closes the IDOR class identified in the 2026-05-02 security audit:
                    // any action carrying [TenantOwned] is checked against the caller's
                    // session ClientId before the action body runs. See AML.Web/CustomFilters/TenantOwnedFilter.cs.
                    options.Filters.Add<AML.Web.CustomFilters.TenantOwnedFilter>();
                })
                .AddRazorRuntimeCompilation();

            // Filter is created per-request so it can resolve the right scoped services.
            services.AddScoped<AML.Web.CustomFilters.TenantOwnedFilter>();

            services.AddAutoMapper(typeof(Startup));
            ConfigureDependencyInjection(services);

            services.AddHttpClient();

            // Rate limiting — protect screening endpoints + bulk uploads from abuse
            // and from accidentally burning through C6 quota. Buckets are per-user
            // (session-id fallback to remote IP if missing).
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status429TooManyRequests;

                options.AddPolicy("screening-tight", httpContext =>
                {
                    var key = httpContext.Session?.GetString("SessUserId")
                              ?? httpContext.Connection.RemoteIpAddress?.ToString()
                              ?? "anon";
                    return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: key,
                        factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 6,
                            Window = System.TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        });
                });

                options.AddPolicy("standard", httpContext =>
                {
                    var key = httpContext.Session?.GetString("SessUserId")
                              ?? httpContext.Connection.RemoteIpAddress?.ToString()
                              ?? "anon";
                    return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: key,
                        factory: _ => new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 120,
                            Window = System.TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        });
                });
            });

            // For IIS out-of-process hosting: trust X-Forwarded-Proto from the loopback
            // proxy only. In-process hosting (IIS default) does not need this — ANCM sets
            // the scheme directly — but it is harmless to leave it configured.
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            // Anti-forgery service registered with hardened cookie defaults. NOT globally
            // enforced (would 400 every existing form that doesn't include the token).
            // Opt-in per endpoint via [ValidateAntiForgeryToken] / [AutoValidateAntiforgeryToken].
            // Forms must add @Html.AntiForgeryToken() and AJAX must send the
            // RequestVerificationToken header. Tracked as follow-up after the IDOR fix lands.
            services.AddAntiforgery(options =>
            {
                options.HeaderName = "RequestVerificationToken";
                options.Cookie.HttpOnly = true;
                // SameAsRequest: cookie is Secure when the connection is HTTPS (directly or via
                // X-Forwarded-Proto), but won't throw when behind a proxy that doesn't forward the
                // proto header. Keeps the antiforgery CheckSSLConfig check happy on UAT/HTTP.
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            services.AddCors();
            services.AddControllers();
            services.AddControllers().AddControllersAsServices();

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 52428800; // 50MB
            });
            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("JwtAppSettings");
            services.Configure<Helpers.JwtAppSettings>(appSettingsSection);



            var config = new ConfigurationBuilder()
   .SetBasePath(System.IO.Directory.GetCurrentDirectory())
   .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
   .Build();

            LogManager.Configuration = new NLogLoggingConfiguration(config.GetSection("NLog"));


            services.Configure<FormOptions>(options => options.ValueCountLimit = 5000);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Must be first — reads X-Forwarded-Proto from nginx so Request.IsHttps is correct
            app.UseForwardedHeaders();

            app.UseHttpContext();
            app.UseNToastNotify();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            if (!env.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }
            app.UseStaticFiles();

            app.UseRequestLocalization(app.ApplicationServices.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);


            // CORS — restricted to an allowlist driven by config (Cors:AllowedOrigins).
            // Closes the cross-tenant data-exfiltration risk surfaced in the 2026-05-02
            // audit (AllowAnyOrigin lets any third-party site fetch case data via the
            // user's session cookie). For production, set the allowed origins in the
            // hosting environment (env var Cors__AllowedOrigins__0=https://prod.example.com).
            var corsAllowed = Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                              ?? new[] { "https://localhost:5007", "http://localhost:5008" };
            app.UseCors(x => x
                .WithOrigins(corsAllowed)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            app.UseRouting();
            app.UseAuthorization();
            app.UseSession();
            app.UseRateLimiter();

            // custom jwt auth middleware
            app.UseMiddleware<JwtMiddleware>();


            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllerRoute(
                //    name: "login",
                //    pattern: "/{action}",
                //    defaults: new { action= "login" });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=UserAccess}/{action=Login}/{id?}");
            });

            //            var cultures = new List<CultureInfo> {
            //    new CultureInfo("en"),
            //    new CultureInfo("ar")
            //};
            //            app.UseRequestLocalization(options => {
            //                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("en");
            //                options.SupportedCultures = cultures;
            //                options.SupportedUICultures = cultures;
            //      });
            //var requestOpt = new RequestLocalizationOptions();
            //requestOpt.SupportedCultures = new List<CultureInfo>
            //{
            //    new CultureInfo("en-US")
            //};
            //    requestOpt.SupportedUICultures = new List<CultureInfo>
            //{
            //    new CultureInfo("en-US")
            //};
            //requestOpt.RequestCultureProviders.Clear();
            //requestOpt.RequestCultureProviders.Add(new SingleCultureProvider());

            //app.UseRequestLocalization(requestOpt);


        }

        public class SingleCultureProvider : IRequestCultureProvider
        {
            public Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
            {
                return Task.Run(() => new ProviderCultureResult("en-US", "en-US"));
            }
        }

        private void ConfigureDependencyInjection(IServiceCollection services)
        {
            // Add application services.
            services.AddResponseCaching();
            //services.AddTransient<IBaseService, BaseService>();
            services.AddTransient<IUserGroupRightService, UserGroupRightService>();
            services.AddTransient<ICaseStudioService, CaseStudioService>();
            services.AddTransient<IModuleService, ModuleService>();
            services.AddTransient<IFunctionalityService, FunctionalityService>();
            services.AddTransient<IAuthenticationService, AuthenticationService>();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IDepartmentService, DepartmentService>();
            services.AddTransient<IDesignationService, DesignationService>();
            services.AddTransient<ICodeMasterService, CodeMasterService>();
            services.AddTransient<IBranchService, BranchService>();
            services.AddTransient<IUserGroupService, UserGroupService>();
            services.AddTransient<ICountryService, CountryService>();
            services.AddTransient<IVisaTypeService, VisaTypeService>();
            services.AddTransient<IIdentityTypeService, IdentityTypeService>();
            services.AddTransient<ICommonRepository, CommonRepository>();
            services.AddTransient<ICustomerCaseService, CustomerCaseService>();
            services.AddTransient<ICustomerScreeningService, CustomerScreeningService>();
            services.AddTransient<IEtlBatchService, EtlBatchService>();
            services.AddTransient<IFreeSourceService, FreeSourceService>();
            services.AddTransient<ISanctionService, SanctionService>();
            services.AddScoped<ILegalTypeService, LegalTypeService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddTransient<ICustomerCategoryService, CustomerCategoryService>();
            services.AddTransient<ICaseAssignmentService, CaseAssignmentService>();
            services.AddTransient<ICaseCommentService, CaseCommentService>();
            services.AddTransient<ICaseDocumentService, CaseDocumentService>();
            services.AddTransient<IDigiApiUserService, DigiApiUSerService>();
            services.AddTransient<IInternalWatchListService, InternalWatchListService>();
            services.AddTransient<ICommonService, CommonService>();
            services.AddTransient<IHttpClientHandler, HttpClientHandler>();
            services.AddTransient<IScreeningService, ScreeningService>();
            services.AddTransient<IReportService, ReportService>();
            services.AddTransient<ILovMasterService, LovMasterService>();
            services.AddTransient<ILovMasterV2Service, LovMasterV2Service>();
            services.AddTransient<IProdMasterService, ProdMasterService>();
            services.AddTransient<IRiskService, RiskService>();
            services.AddTransient<IRiskV2Service, RiskV2Service>();
            services.AddTransient<ICustomerMasterService, CustomerMasterService>();
            services.AddTransient<ITransactionMonitorService, TransactionMonitorService>();
            services.AddTransient<ITransactionScreeningService, TransactionScreeningService>();
            services.AddTransient<IEWRAService, EWRAService>();
            services.AddTransient<IAIService, MockAIService>();
            services.AddTransient<ChatDataService, ChatDataService>();
            services.AddTransient<ChatHistoryService, ChatHistoryService>();
            services.AddTransient<INotepadService, NotepadService>();
            services.AddTransient<IAmlTrackerService, AmlTrackerService>();
            services.AddScoped<IAmlTrackerRepository, AmlTrackerRepository>();
            services.AddTransient<AML.Core.ServiceContract.ComplianceHub.IComplianceHubService, AML.Core.Service.ComplianceHub.ComplianceHubService>();
            services.AddScoped<AML.Core.RepositoryContract.ComplianceHub.IComplianceHubRepository, AML.Core.Repository.ComplianceHub.ComplianceHubRepository>();

            //services.AddScoped<IBaseRepository, BaseRepository>();
            services.AddScoped<IUserGroupRightRepository, UserGroupRightRepository>();
            services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            services.AddScoped<IFunctionalityRepository, FunctionalityRepository>();
            services.AddScoped<IModuleRepository, ModuleRepository>();
            services.AddScoped<ICodesMasterRepository, CodesMasterRepository>();
            services.AddScoped<IUserGroupRightRepository, UserGroupRightRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserDetailRepository, UserDetailRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IDesignationRepository, DesignationRepository>();
            services.AddScoped<IUserGroupRepository, UserGroupRepository>();
            services.AddScoped<IBranchRepository, BranchRepository>();
            services.AddScoped<IVisaTypeRepository, VisaTypeRepository>();
            services.AddScoped<IIdentityTypeRepository, IdentityTypeRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<ICustomerCaseRepository, CustomerCaseRepository>();
            services.AddScoped<IFreeSourceRepository, FreeSourceRepository>();
            services.AddScoped<ICustomerScreeningRepository, CustomerScreeningRepository>();
            services.AddScoped<ICustomerCategoryRepository, CustomerCategoryRepository>();
            services.AddScoped<IEtlLogRepository, EtlLogRepository>();
            services.AddScoped<IViewRenderService, ViewRenderService>();
            services.AddScoped<IExportDataService, ExportDataService>();
            services.AddScoped<IWatchListRepository, WatchListRepository>();
            services.AddScoped<ICommonRepository, CommonRepository>();
            services.AddScoped<ILegalTypeRepository, LegalTypeRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IDigiApiUserRepository, DigiApiUserRepository>();
            services.AddScoped<ICaseAssignmentRepository, CaseAssignmentRepository>();
            services.AddScoped<ICaseCommentRepository, CaseCommentRepository>();
            services.AddScoped<ICaseDocumentRepository, CaseDocumentRepository>();
            services.AddScoped<ICommonRepository, CommonRepository>();
            services.AddTransient<IInternalWatchListRepository, InternalWatchListRepository>();
            services.AddScoped<IFileUploader, FileUploader>();
            services.AddScoped<PageAuthorizeAttribute>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<ICustomerMasterRepository, CustomerMasterRepository>();
            services.AddScoped<ICorporateShareholderRepository, CorporateShareholderRepository>();
            services.AddScoped<IRiskRepository, RiskRepository>();
            services.AddScoped<IRiskV2Repository, RiskV2Repository>();
            services.AddScoped<ILovMasterRepository, LovMasterRepository>();
            services.AddScoped<ILovMasterV2Repository, LovMasterV2Repository>();
            services.AddScoped<IProdMasterRepository, ProdMasterRepository>();
            services.AddScoped<IEWRARepository, EWRARepository>();
            services.AddScoped<ITransactionScreeningRepository, TransactionScreeningRepository>();
            services.AddScoped<IAmlTrackerRepository, AmlTrackerRepository>();


            services.AddTransient<IProliferationFinanceService, ProliferationFinanceService>();
            services.AddScoped<IProliferationFinanceRepository, ProliferationFinanceRepository>();
            services.AddScoped<IProliferationFinanceMongoRepository, ProliferationFinanceMongoRepository>();
            services.AddScoped<IInternalWatchListMongoRepository, InternalWatchListMongoRepository>();
            services.AddScoped<INotepadRepository, NotepadRepository>();

            services.AddControllersWithViews();

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<ITransactionMonitorService, TransactionMonitorService>();
            services.AddScoped<ITransactionMonitorRepository, TransactionMonitorRepository>();

            services.AddTransient<IKycService, KycService>();
            services.AddScoped<IKycRepository, KycRepository>();

            // ---- Sectoral Transaction Monitoring (STM) ----
            services.AddTransient<IStmService, StmService>();
            services.AddScoped<IStmRepository, StmRepository>();

            services.AddControllersWithViews().AddRazorRuntimeCompilation();

        }


    }
}
