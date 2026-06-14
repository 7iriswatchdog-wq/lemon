using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Aml.Screening.MongoModal.Config
{
    public static class AppSettings
    {
        //public static string GetConnectionString()
        //{
        //    var configuration = new ConfigurationBuilder()
        //        .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: false)
        //        .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.Development.json"), optional: false)
        //        .Build();
        //    return configuration.GetSection("AppSettings").GetSection("ConnectionString").Value;
        //}
    }
}
