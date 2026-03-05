using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AML.Core.ServiceContract.Branch;
using AML.DTO.DTO.Branch;
using AML.ViewModel.ViewModels.Branch;
using AML.ViewModel.ViewModels.DataTable;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NToastNotify;

namespace AML.Web.Controllers.Branch
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class BranchController : Controller
    {
        private IBranchService _branchService;
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;

        private IHttpClientHandler _clientHandler;

        public BranchController(IBranchService branchService, IMapper mapper, 
             IConfiguration _configuration, IToastNotification toastNotification,IHttpClientHandler clientHandler)
        {
            _branchService = branchService;
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
        }
        // GET: Branch
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost("branch/custompagination")]
        //ToDo
        public JsonResult CustomPagination(DataTableModel model)
        {
            var clientId = _clientHandler.GetClientId();
            List<BranchModel> abc = _mapper.Map<List<BranchModel>>(_branchService.GetAll(clientId));

            //Search  
            if (!string.IsNullOrEmpty(model.search.value))
            {
                abc = abc.Where(m => m.Name.ToLower().Contains(model.search.value.ToLower())
                || m.Code.ToLower().Contains(model.search.value.ToLower())).ToList();
            }

            var sorted = Sort(abc, model.columns[model.order[0].column].data ?? "code", model.order[0].dir ?? "dec");
            var paged = sorted
                .Skip(model.start)
                .Take(model.length)
                .Where(x => x.IsActive == 1)
                .ToList();

            return Json(new
            {
                draw = model.draw,
                recordsTotal = abc.Count,
                recordsFiltered = abc.Count,
                data = paged
            });
        }

        public List<T> Sort<T>(List<T> input, string property, string dir)

        {

            var type = typeof(T);

            var sortProperty = type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            return dir == "asc"

                ? input.OrderBy(p => sortProperty.GetValue(p, null)).ToList()

                : input.OrderByDescending(p => sortProperty.GetValue(p, null)).ToList();

        }


        // GET: Branch/Create
        [HttpGet("branch/create")]
        public ActionResult Create()
        {
            BranchModel _BranchModel = new BranchModel();
            return View(_BranchModel);
        }

        // POST: Branch/Create
        [HttpPost("branch/create")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(BranchModel model)
        {
            try
            {
                var clientId = _clientHandler.GetClientId();
                model.ClientId = clientId;
                model.CreatedBy = _clientHandler.GetUserId();
                if (ModelState.IsValid)
                {
                    if (model.Id > 0)
                    {
                        var result = _branchService.Update(_mapper.Map<BranchDTO>(model));
                        TempData["Message"] = result.Message;
                        TempData["Status"] = result.Status;
                        _toastNotification.AddSuccessToastMessage("Branch updated successfully");

                    }
                    else
                    {
                        var result = _branchService.Create(_mapper.Map<BranchDTO>(model));
                        TempData["Message"] = result.Message;
                        TempData["Status"] = result.Status;
                        _toastNotification.AddSuccessToastMessage("Branch added successfully");     
                    }
                    return RedirectToAction("Index", new { isActive = 0 });
                }
                return View(model);

            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["Status"] = 500;
                return View(model);
            }
        }

        [HttpGet("branch/edit/{id}")]
        public ActionResult Edit(int id)
        {
            BranchModel _BranchModel = _mapper.Map<BranchModel>(_branchService.GetDetails(id));
            return View("Create", _BranchModel);
        }



        [HttpDelete("branch/delete/{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                _branchService.Delete(id);
                _toastNotification.AddSuccessToastMessage("Branch deleted successfully");

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
