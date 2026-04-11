using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AML.Core.Service.EtlBatch;
using AML.Core.ServiceContract.EtlBatch;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.EtlBatch;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace AML.Web.Controllers.EtlBatch
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class EtlController : Controller
    {
        private IEtlBatchService _etlBatchService;
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        public EtlController(IEtlBatchService etlBatchService, IMapper mapper,IToastNotification toastNotification)
        {
            _etlBatchService = etlBatchService;
            _mapper = mapper;
            _toastNotification = toastNotification;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("etl/custompagination")]
        public JsonResult CustomPagination(DataTableModel model)
        {
            List<EtlBatchModel> etllogs = _mapper.Map<List<EtlBatchModel>>(_etlBatchService.GetAll(model.fromdate, model.todate).Result);
            if (!string.IsNullOrEmpty(model.search.value))
            {
                etllogs = etllogs.Where(m => m.AddedUser.ToLower().ToString().Contains(model.search.value)
                ).ToList();
            }
            var data = etllogs.Skip(model.start).Take(model.length).ToList();
            return Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = etllogs.Count,//totalResultsCount,
                recordsFiltered = etllogs.Count,//filteredResultsCount,
                data = data,
            });
        }

        [HttpGet("etl/BulkUploadLogs_PDF")]
        public async Task<IActionResult> BulkUploadLogs_PDF(string fromdate, string todate, string searchValue)
        {
            List<EtlBatchModel> etllogs = _mapper.Map<List<EtlBatchModel>>(_etlBatchService.GetAll(fromdate, todate).Result);
            if (!string.IsNullOrEmpty(searchValue))
            {
                etllogs = etllogs.Where(m => m.AddedUser != null && m.AddedUser.ToLower().Contains(searchValue.ToLower())).ToList();
            }
            return View("BulkUploadLogs_PDF", etllogs);
        }

        [HttpGet("etl/BulkUploadLogs_CSV")]
        public async Task<IActionResult> BulkUploadLogs_CSV(string fromdate, string todate, string searchValue)
        {
            List<EtlBatchModel> etllogs = _mapper.Map<List<EtlBatchModel>>(_etlBatchService.GetAll(fromdate, todate).Result);
            if (!string.IsNullOrEmpty(searchValue))
            {
                etllogs = etllogs.Where(m => m.AddedUser != null && m.AddedUser.ToLower().Contains(searchValue.ToLower())).ToList();
            }

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Total Rows,Rows Recorded,File Name,Upload Time,User");
            foreach (var item in etllogs)
            {
                builder.AppendLine($"{item.TotalRows},{item.RowsRecorded},{item.FileName},{item.AddedOn},{item.AddedUser}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"BulkUploadLogs_{DateTime.Now:yyyyMMddHHmmss}.csv");
        }
    }
}
