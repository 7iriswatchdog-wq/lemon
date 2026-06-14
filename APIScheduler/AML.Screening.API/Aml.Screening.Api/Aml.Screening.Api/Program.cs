using Aml.Screening.ServiceContracts.ServiceContracts;
using Aml.Screening.Services.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Dependency Injection
builder.Services.AddTransient<ICustomerServices, CustomerServices>();
builder.Services.AddTransient<IScreeningServices, ScreeningServices>();
builder.Services.AddTransient<IBlackListServices, BlackListServices>();
builder.Services.AddTransient<ICaseLogServices, CaseLogServices>();
builder.Services.AddTransient<ITransactionCaseLogServices, TransactionCaseLogServices>();

// Register Configuration directly if needed (though builder.Configuration is already available in DI)
// but Startup had: services.AddSingleton(Configuration);
builder.Services.AddSingleton(builder.Configuration);

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AML Screening API", Version = "v1" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AML Screening API V1");
});

app.Run();
