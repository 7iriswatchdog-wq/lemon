using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AML.Core.ServiceContract.Designation;
using AML.DTO.DTO.Designation;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.Designation;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NToastNotify;

namespace AML.Web.Controllers.Designation
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class DesignationController : Controller
    {
        private IDesignationService _DesignationService;
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private IHttpClientHandler _clientHandler;

        public DesignationController(IDesignationService DesignationService, IToastNotification toastNotification,
            IMapper mapper, IHttpClientHandler clientHandler, IConfiguration _configuration)
        {
            _DesignationService = DesignationService;
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
        }
        // GET: Designation
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost("designation/custompagination")]
        //ToDo
        public JsonResult CustomPagination(DataTableModel model)
        {
            var clientId = _clientHandler.GetClientId();
            List<DesignationModel> abc = _mapper.Map<List<DesignationModel>>(_DesignationService.GetAll(clientId));
            if (!string.IsNullOrEmpty(model.search.value))
            {
                //abc = abc.Where(m => m.Name.ToLower().Contains(model.search.value.ToLower())
                //|| m.Description.ToLower().Contains(model.search.value.ToLower()) || m.Code.ToLower().Contains(model.search.value.ToLower())).ToList();
                abc = abc.Where(m =>
                (m.Name?.ToLower().Contains(model.search.value.ToLower()) ?? false) ||
                (m.Description?.ToLower().Contains(model.search.value.ToLower()) ?? false) ||
                (m.Code?.ToLower().Contains(model.search.value.ToLower()) ?? false)
            ).ToList();

            }


            var data = Sort(abc, model.columns[model.order[0].column].data ?? "code", model.order[0].dir ?? "dec")

.Skip(model.start)

.Take(model.length)

.ToList();

            //var data = abc.Skip(model.start).Take(model.length).ToList();
            return Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = abc.Count,//totalResultsCount,
                recordsFiltered = abc.Count,//filteredResultsCount,
                data = data.Where(x => x.IsActive == 1),
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


        [HttpGet("designation/details")]
        // GET: Designation/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Designation/Create
        [HttpGet("designation/create")]
        public ActionResult Create()
        {
            DesignationModel _DesignationModel = new DesignationModel();
            return View(_DesignationModel);
        }

        // POST: Designation/Create
        [HttpPost("designation/create")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DesignationModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.ClientId = _clientHandler.GetClientId();
                    if (model.Id > 0)
                    {
                        var result = _DesignationService.Update(_mapper.Map<DesignationDTO>(model));
                        _toastNotification.AddSuccessToastMessage("Designation updated successfully");

                    }
                    else
                    {
                        var result = _DesignationService.Create(_mapper.Map<DesignationDTO>(model));
                        _toastNotification.AddSuccessToastMessage("Designation added successfully");
                    }
                    return RedirectToAction("Index", new { isActive = 0 });
                }
                return View(model);

            }
            catch
            {
                return View(model);
            }
        }

        [HttpGet("designation/edit/{id}")]
        // GET: Designation/Edit/5
        public ActionResult Edit(int id)
        {
            DesignationModel _DesignationModel = _mapper.Map<DesignationModel>(_DesignationService.GetDetails(id));
            return View("Create", _DesignationModel);
        }



        [HttpDelete("designation/delete/{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                var result = _DesignationService.Delete(id);
                _toastNotification.AddSuccessToastMessage("Designation deleted successfully");

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
