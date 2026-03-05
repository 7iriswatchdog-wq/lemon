using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AML.Core.ServiceContract.IdentityType;
using AML.DTO.DTO.IdentityType;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.IdentityType;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NToastNotify;

namespace AML.Web.Controllers.IdentityType
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class IdentityTypeController : Controller
    {
        private IIdentityTypeService _IdentityTypeService;
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private IHttpClientHandler _clientHandler;

        public IdentityTypeController(IIdentityTypeService IdentityTypeService, IToastNotification toastNotification,
            IMapper mapper, IHttpClientHandler clientHandler, IConfiguration _configuration)
        {
            _IdentityTypeService = IdentityTypeService;
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
        }
        // GET: IdentityType
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost("identitytype/custompagination")]
        //ToDo
        public JsonResult CustomPagination(DataTableModel model)
        {
            var clientId = _clientHandler.GetClientId();
            List<IdentityTypeModel> abc = _mapper.Map<List<IdentityTypeModel>>(_IdentityTypeService.GetAll(clientId));
            if (!string.IsNullOrEmpty(model.search.value))
            {
                abc = abc.Where(m => m.Name.ToLower().Contains(model.search.value.ToLower())
                || m.Description.ToLower().Contains(model.search.value.ToLower()) || m.Code.ToLower().Contains(model.search.value.ToLower())).ToList();
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


        [HttpGet]
        // GET: IdentityType/Create
        public ActionResult Create()
        {
            IdentityTypeModel _IdentityTypeModel = new IdentityTypeModel();
            return View(_IdentityTypeModel);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(IdentityTypeModel model)
        {
            try
            {
                model.ClientId = _clientHandler.GetClientId();
                if (ModelState.IsValid)
                {
                    if (model.Id > 0)
                    {
                        var result = _IdentityTypeService.Update(_mapper.Map<IdentityTypeDTO>(model));
                        _toastNotification.AddSuccessToastMessage("ID Type updated successfully");

                    }
                    else
                    {
                        var result = _IdentityTypeService.Create(_mapper.Map<IdentityTypeDTO>(model));
                        _toastNotification.AddSuccessToastMessage("ID Type added successfully");
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

        // GET: IdentityType/Edit/5
        [HttpGet]
        public ActionResult Edit(int id)
        {
            IdentityTypeModel _IdentityTypeModel = _mapper.Map<IdentityTypeModel>(_IdentityTypeService.GetDetails(id));
            return View("Create", _IdentityTypeModel);
        }



        [HttpDelete]
        // POST: IdentityType/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                var result = _IdentityTypeService.Delete(id);
                _toastNotification.AddSuccessToastMessage("ID Type deleted successfully");
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
