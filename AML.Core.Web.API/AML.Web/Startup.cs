using AML.Core.Repository.Branch;
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
using AML.Web.Services;

using AML.Core.Repository.FreeSource;
using AML.Core.Repository.IdentityType;
using AML.Core.Repository.InternalWatchList;
using AML.Core.Repository.Kyc;
using AML.Core.Repository.LegalType;
using AML.Core.Repository.LovMaster;
using AML.Core.Repository.Report;
using AML.Core.Repository.Risk;
using AML.Core.Repository.Sanction;

using AML.Core.Repository.User;
using AML.Core.Repository.UserAccess;
using AML.Core.Repository.UserGroup;
using AML.Core.Repository.VisaType;
using AML.Core.RepositoryContract.Branch;
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

using AML.Core.RepositoryContract.FreeSource;
using AML.Core.RepositoryContract.IdentityType;
using AML.Core.RepositoryContract.InternalWatchList;
using AML.Core.RepositoryContract.Kyc;
using AML.Core.RepositoryContract.LegalType;
using AML.Core.RepositoryContract.LovMaster;
using AML.Core.RepositoryContract.Report;
using AML.Core.RepositoryContract.Risk;
using AML.Core.RepositoryContract.Sanction;

using AML.Core.RepositoryContract.User;
using AML.Core.RepositoryContract.UserAccess;
using AML.Core.RepositoryContract.UserGroup;
using AML.Core.RepositoryContract.VisaType;
using AML.Core.Service.Branch;
using AML.Core.Service.CaseAssignment;
using AML.Core.Service.CaseComment;
using AML.Core.Service.CaseDocument;
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


using AML.Core.Service.FreeSource;
using AML.Core.Service.IdentityType;
using AML.Core.Service.InternalWatchList;
using AML.Core.Service.Kyc;
using AML.Core.Service.LegalType;
using AML.Core.Service.LovMaster;
using AML.Core.Service.Report;
using AML.Core.Service.Risk;
using AML.Core.Service.Sanction;

using AML.Core.Service.User;
using AML.Core.Service.UserAccess;
using AML.Core.Service.UserGroup;
using AML.Core.Service.VisaType;

using AML.Core.ServiceContract.Branch;
using AML.Core.ServiceContract.CaseAssignment;
using AML.Core.ServiceContract.CaseComment;
using AML.Core.ServiceContract.CaseDocument;
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

using AML.Core.ServiceContract.FreeSource;
using AML.Core.ServiceContract.IdentityType;
using AML.Core.ServiceContract.InternalWatchList;
using AML.Core.ServiceContract.Kyc;
using AML.Core.ServiceContract.LegalType;
using AML.Core.ServiceContract.LovMaster;
using AML.Core.ServiceContract.Report;
using AML.Core.ServiceContract.Risk;
using AML.Core.ServiceContract.Sanction;
using AML.Core.ServiceContract.TransactionMonitor;
using AML.Core.ServiceContract.User;
using AML.Core.ServiceContract.UserAccess;
using AML.Core.ServiceContract.UserGroup;
using AML.Core.ServiceContract.VisaType;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AML.Web.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
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
using AML.Core.Service.TransactionMonitor;
using AML.Core.Repository.TransactionMonitor;
using AML.Core.RepositoryContract.TransactionMonitor;
using NToastNotify;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AML.Core.ServiceContract.TransactionScreening;
using AML.Core.Service.TransactionScreening;
using AML.Core.RepositoryContract.TransactionScreening;
using AML.Core.Repository.TransactionScreening;
using AML.Core.ServiceContract.CodeMaster;
using AML.Core.Service.CodesMaster;
using AML.Core.RepositoryContract;
using AML.Core.Repository.CodesMaster;
using AML.Core.RepositoryContract.CodeMaster;
using AML.Core.ServiceContract.EWRA;
using AML.Core.Service.EWRA;
using AML.Core.RepositoryContract.EWRA;
using AML.Core.Repository.EWRA;
using AML.Core.RepositoryContract.ProductMaster;
using AML.Core.ServiceContract.ProductMaster;
using AML.Core.Service.ProductMaster;
using AML.Core.Repository.ProductMaster;
using AML.Core.ServiceContract.RiskV2;
using AML.Core.Service.RiskV2;
using AML.Core.ServiceContract.LovMasterV2;
using AML.Core.Service.LovMasterV2;
using AML.Core.RepositoryContract.RiskV2;
using AML.Core.Repository.RiskV2;
using AML.Core.RepositoryContract.LovMasterV2;
using AML.Core.Repository.LovMasterV2;
using AML.Core.RepositoryContract.ProliferationFinance;
using AML.Core.ServiceContract.ProliferationFinance;
using AML.Core.Repository.ProliferationFinance;
using AML.Core.Service.ProliferationFinance;
using AML.Core.ServiceContract.AI;
using AML.Core.Service.AI;

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
                ProgressBar = false,
                PositionClass = ToastPositions.BottomCenter
            }).AddSessionStateTempDataProvider();
            //services.AddSession(options =>
            //{
            //    options.IdleTimeout = TimeSpan.FromMinutes(1);//Session Timeout.  
            //});
            services.AddSession();

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
            services.AddControllersWithViews()

        .AddRazorRuntimeCompilation();
            services.AddAutoMapper(typeof(Startup));
            ConfigureDependencyInjection(services);

            services.AddHttpClient();
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


            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseRouting();
            app.UseAuthorization();
            app.UseSession();

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
            services.AddTransient<IAIService, OllamaAIService>();
            services.AddTransient<ChatDataService, ChatDataService>();
            services.AddTransient<ChatHistoryService, ChatHistoryService>();

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
            
            services.AddTransient<IProliferationFinanceService, ProliferationFinanceService>();
            services.AddScoped<IProliferationFinanceRepository, ProliferationFinanceRepository>();
            services.AddScoped<IProliferationFinanceMongoRepository, ProliferationFinanceMongoRepository>();

            services.AddControllersWithViews();

            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<ITransactionMonitorService, TransactionMonitorService>();
            services.AddScoped<ITransactionMonitorRepository, TransactionMonitorRepository>();

            services.AddTransient<IKycService, KycService>();
            services.AddScoped<IKycRepository, KycRepository>();

            services.AddControllersWithViews().AddRazorRuntimeCompilation();

        }


    }
}
