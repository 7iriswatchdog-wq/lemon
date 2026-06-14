using AML.Core.ServiceContract.TransactionMonitor;
using AML.DTO.DTO.TransactionMonitor;
using AML.Web.Helper;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AML.Web.Schedulers
{
    public class TMSTimedHostedService : IHostedService, IDisposable
    {
        private Timer _timer = null!;
        private readonly ILogger<TMSTimedHostedService> _logger;
        private readonly IServiceScopeFactory scopeFactory;

        public TMSTimedHostedService(ILogger<TMSTimedHostedService> logger, IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");



            //_timer = new Timer(DoWork, null, TimeSpan.FromDays(5), TimeSpan.FromDays(5));

            //_timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));


           _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(0));




            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            using var scope = scopeFactory.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<ITransactionMonitorService>();

            //var TMSRule = dbContext.GetAllTMSRules();
                    
            //var Transactions = dbContext.GetAllTransactionsForTMS();

            //if (Transactions?.Count() == 0 || Transactions ==null || TMSRule?.Count() ==0 || TMSRule ==null)
            //{
            //    return;
            //}

            //Console.WriteLine($"Got {TMSRule.Count} rules from DB, and {Transactions.Count} transactions from DB.");

            //foreach (var rule in TMSRule)
            //{
            //    bool isMasterRuleHit = true;

            //    if (rule.TMSRuleParameters[0].TMSOperatorGRP== "AND")
            //    {
            //        isMasterRuleHit = true;
            //    }
            //    else
            //    {
            //        isMasterRuleHit = false;
            //    }

            //    List<TMSNewMasterDTO> MasterHits = new List<TMSNewMasterDTO>();

            //    foreach (var ruleParam in rule.TMSRuleParameters)
            //    {
            //        int intCompareValue;

            //        bool success = int.TryParse(ruleParam.TMSRuleDetCompareValue, out intCompareValue);
            //        if (success)
            //        {
            //            ruleParam.TMSRuleDetDynamicCompareValue = intCompareValue;
            //        }
            //        else
            //        {
            //            ruleParam.TMSRuleDetDynamicCompareValue = ruleParam.TMSRuleDetCompareValue;
            //        }
                   
            //        (bool isParamRuleHit, List<TMSNewMasterDTO> hits) = Builder.CheckRule(ruleParam, MasterHits.Count() > 0 ? MasterHits : Transactions);

            //        Console.WriteLine($"isRuleHit: {isParamRuleHit}");
            //        Console.WriteLine($"Got {hits.Count()} hit transactions");

            //        if (isParamRuleHit)
            //        {
            //            MasterHits = hits;
            //        }

            //        if (ruleParam.TMSOperatorGRP == "AND")
            //        {
            //            isMasterRuleHit = isMasterRuleHit && isParamRuleHit;
            //        }
            //        else
            //        {
            //            isMasterRuleHit = isMasterRuleHit || isParamRuleHit;
            //        }
            //    }

            //    if (isMasterRuleHit)
            //    {
            //        dbContext.InsertRuleViolatedTransactions(MasterHits, rule);
            //    }
            //}

            //dbContext.SetMonitoredStatus(Transactions.Where(val => val.tmsstatus == 0).ToList());
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
