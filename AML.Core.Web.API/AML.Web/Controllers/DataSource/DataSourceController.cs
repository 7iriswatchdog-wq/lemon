using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AML.Web.Controllers.DataSource
{
    public class DataSourceController : Controller
    {
        private IConfiguration _configuration;

        public DataSourceController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("DataSource/ExecuteSchedule")]
        public IActionResult ExecuteSchedule()
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = $"{_configuration.GetSection("SchedulerExePath").Value}"; // relative path or absolute path.
                //process.StartInfo.FileName = @"cmd.exe";
                //process.StartInfo.Arguments = @"/c dir";      // print the current working directory information
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.OutputDataReceived += (sender, data) => Console.WriteLine(data.Data);
                process.ErrorDataReceived += (sender, data) => Console.WriteLine(data.Data);
                Console.WriteLine("starting");
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                var exited = process.WaitForExit(1000 * 10);     // (optional) wait up to 10 seconds
                Console.WriteLine($"exit {exited}");
            }
            return View("Index");
        }
    }
}
