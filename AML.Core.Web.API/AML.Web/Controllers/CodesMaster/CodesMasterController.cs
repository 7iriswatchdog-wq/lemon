using AML.Core.ServiceContract.CodeMaster;
using AML.DTO.DTO.Branch;
using AML.DTO.DTO.CodesMaster;
using AML.ViewModel.ViewModels.Branch;
using AML.ViewModel.ViewModels.CodesMaster;
using AML.ViewModel.ViewModels.CustomerMaster;
using AML.ViewModel.ViewModels.DataTable;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AML.Web.Controllers.CodesMaster
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class CodesMasterController : Controller
    {
        private ICodeMasterService _codesMasterService;
        private IMapper _mapper;
        private IHttpClientHandler _clientHandler;
        private IConfiguration _configuration;
        private readonly IToastNotification _toastNotification;
        public CodesMasterController(ICodeMasterService codeMasterService, IMapper mapper, IHttpClientHandler clientHandler, IConfiguration configuration, IToastNotification toastNotification)
        {
            _codesMasterService = codeMasterService;
            _mapper = mapper;
            _clientHandler = clientHandler;
            _configuration = configuration;
            _toastNotification = toastNotification;
        }
        //public IActionResult CodeList()
        //{
        //    return View();
        //}
        public IActionResult Index(string CcActiveYn)
        
        {
            return View();
        }
        [HttpPost("codesmaster/custompagination")]
        public JsonResult CustomPagination(DataTableModel model)
        {
            List<CodesTableModel> abc = new List<CodesTableModel>();
            var CcClientId = _clientHandler.GetClientId();
            abc = _mapper.Map<List<CodesTableModel>>(_codesMasterService.GetAllCodes(CcClientId));


            ////Sorting  
            //if (model.order != null && model.order.Count > 0)
            //{
            //    abc = ListOrdering.OrderByDynamic(abc, model.order[0].column, false);
            //    //abc = abc.OrderBy( model.order[0].column) ;
            //}
            var param = model.order[0].column;
            var propertyInfo = typeof(Column).GetProperty(model.columns[model.order[0].column].data);
            var orderByAddress = abc.OrderBy(x => propertyInfo.GetValue(x, null));

            //Search  
            if (!string.IsNullOrEmpty(model.search.value))
            {
                abc = abc.Where(m => m.ccName.ToLower().Contains(model.search.value.ToLower())
                || m.ccCode.ToLower().Contains(model.search.value.ToLower())).ToList();
            }

            //var data = abc.Skip(model.start).Take(model.length).ToList();


            var data = Sort(abc, model.columns[model.order[0].column].data ?? "ccCode", model.order[0].dir ?? "dec")

.Skip(model.start)

.Take(model.length)

.ToList();


            return Json(new
            {
                // this is what datatables wants sending backback
                model.draw,
                recordsTotal = abc.Count,//totalResultsCount,
                recordsFiltered = abc.Count,//filteredResultsCount,
                data = data,
            });

        }

        public List<T> Sort<T>(List<T> input, string property, string dir)

        {

            var type = typeof(T);

            var sortProperty = type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            return dir == "asc"

                ? input.OrderBy(p => sortProperty?.GetValue(p, null)).ToList()

                : input.OrderByDescending(p => sortProperty?.GetValue(p, null)).ToList();

        }

        [HttpGet("codesmaster/edit/{id}/{code}/{Type}")]
        public ActionResult Edit(int id, string Code, string Type)
        {
            var x= Request.RouteValues.Values;
            TempData["Code"] = Code;
            TempData["Type"] = Type;
            CodesTableModel _CodesTableModel = _mapper.Map<CodesTableModel>(_codesMasterService.GetDetails(id, Code, Type));
            return View("View", _CodesTableModel);
        }
        // GET: codesmaster/Create
        [HttpGet("codesmaster/create")]
        public ActionResult Create()
        {
            CodesTableModel _CodesTableModel = new CodesTableModel();
            return View("View", _CodesTableModel);
        }

        // POST: codesmaster/Create
         [HttpPost("CodesMaster/Create")]
        //[ValidateAntiForgeryToken]
       // [HttpPost]
        public ActionResult Create(CodesTableModel model)
        {
            try
            {
                var clientId = _clientHandler.GetClientId();
                model.ccClientId = clientId;
                if (ModelState.IsValid)
                {
                        var result = _codesMasterService.Create(_mapper.Map<CodesTableDTO>(model));
                        TempData["Message"] = result.Message;
                        TempData["Status"] = result.Status;
                       _toastNotification.AddSuccessToastMessage("Codes added successfully");
                   
                    return RedirectToAction("Index", new { isActive = 0 });
                }
                return View("Index");

            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["Status"] = 500;
                return View(model);
            }
        }

        [HttpPost("CodesMaster/Update")]
        //[ValidateAntiForgeryToken]
      //  [HttpPost]
        public ActionResult Update(CodesTableModel model)
        {
            try
            {
                if(null!=TempData)
                {
                    model.ccCode= TempData["code"].ToString();
                    model.ccType = TempData["Type"].ToString();
                }
                TempData.Keep();
                var clientId = _clientHandler.GetClientId();
                model.ccClientId = clientId;
                if (ModelState.IsValid)
                {


                        var result = _codesMasterService.Update(_mapper.Map<CodesTableDTO>(model));
                        TempData["Message"] = result.Message;
                        TempData["Status"] = result.Status;
                         _toastNotification.AddSuccessToastMessage("Codes updated successfully");

                    return RedirectToAction("Index", new { isActive = 0 });
                }
                return View("Index");

            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["Status"] = 500;
                return View(model);
            }
        }

        [HttpDelete("codesmaster/delete/{id}/{code}/{type}")]
        public ActionResult Delete(int id, string code,string type)
        {
            try
            {
                _codesMasterService.Delete(id,code,type);
                _toastNotification.AddSuccessToastMessage("Code deleted successfully");

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
