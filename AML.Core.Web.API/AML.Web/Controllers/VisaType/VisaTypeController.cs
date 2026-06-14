using AML.Core.Common.StaticResource;
using AML.Core.ServiceContract.VisaType;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.VisaType;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.VisaType;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AML.Web.Controllers.VisaType
{

    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class VisaTypeController : Controller
    {

        private IVisaTypeService _UserGroupService;
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private IHttpClientHandler _clientHandler;

        public VisaTypeController(IVisaTypeService UserGroupService, 
            IToastNotification toastNotification, IMapper mapper,
            IHttpClientHandler clientHandler, IConfiguration _configuration)
        {
            _UserGroupService = UserGroupService;
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
        }
        // GET: UserGroup
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost("visatype/custompagination")]
        public JsonResult CustomPagination(DataTableModel model,
    int orderColumn = 0, string orderDirection = "asc") // Added sorting parameters
        {
                var clientId = _clientHandler.GetClientId();

                // Get all active records
                var data = _mapper.Map<List<VisaTypeModel>>(
                    _UserGroupService.GetAll(clientId).Where(x => x.IsActive == 1)
                );

                // Apply search filter if any
                if (!string.IsNullOrEmpty(model.search?.value))
                {
                    var searchValue = model.search.value.ToLower();
                    data = data.Where(m =>
                        (m.Name?.ToLower()?.Contains(searchValue) ?? false) ||
                        (m.Description?.ToLower()?.Contains(searchValue) ?? false) ||
                        (m.Code?.ToLower()?.Contains(searchValue) ?? false)
                    ).ToList();
                }

                // Apply sorting
                IOrderedEnumerable<VisaTypeModel> sortedData;
                switch (orderColumn)
                {
                    case 0: // Code
                        sortedData = orderDirection == "asc" ?
                            data.OrderBy(x => x.Code ?? string.Empty) :
                            data.OrderByDescending(x => x.Code ?? string.Empty);
                        break;
                    case 1: // Name
                        sortedData = orderDirection == "asc" ?
                            data.OrderBy(x => x.Name ?? string.Empty) :
                            data.OrderByDescending(x => x.Name ?? string.Empty);
                        break;
                    case 2: // Description
                        sortedData = orderDirection == "asc" ?
                            data.OrderBy(x => x.Description ?? string.Empty) :
                            data.OrderByDescending(x => x.Description ?? string.Empty);
                        break;
                    default: // Default sort by Code
                        sortedData = orderDirection == "asc" ?
                            data.OrderBy(x => x.Code ?? string.Empty) :
                            data.OrderByDescending(x => x.Code ?? string.Empty);
                        break;
                }

                // Get counts after filtering but before pagination
                int recordsTotal = data.Count;
                int recordsFiltered = sortedData.Count();

                // Apply pagination
                var pagedData = sortedData
                    .Skip(model.start)
                    .Take(model.length)
                    .ToList();

                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsFiltered,
                    data = pagedData,
                    allRows = sortedData.ToList() // For client-side operations
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
        // GET: UserGroup/UserGroup-add
        public ActionResult Create()
        {
            VisaTypeModel _UserGroupModel = new VisaTypeModel();
            return View(_UserGroupModel);
        }

        [HttpPost]
        // POST: UserGroup/UserGroup-add
        [HttpPost]
        public ActionResult Create(VisaTypeModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.ClientId = _clientHandler.GetClientId();
                    if (model.Id > 0)
                    {
                        var result = _UserGroupService.Update(_mapper.Map<VisaTypeDTO>(model));
                        if (result.Status == StaticResource.FailStatusCode)
                        {
                            _toastNotification.AddErrorToastMessage(result.Message);
                        }
                        else
                        {
                            _toastNotification.AddSuccessToastMessage(result.Message);
                        }

                    }
                    else
                    {
                        var result = _UserGroupService.Create(_mapper.Map<VisaTypeDTO>(model));
                        if (result.Status == StaticResource.FailStatusCode)
                        {
                            _toastNotification.AddErrorToastMessage(result.Message);
                        }
                        else
                        {
                            _toastNotification.AddSuccessToastMessage(result.Message);
                        }
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

        [HttpGet]
        //[AML.Web.CustomFilters.TenantOwned(AML.Web.CustomFilters.TenantResource.VisaType, "id")]
        public ActionResult Edit(int id)
        {
            VisaTypeModel _UserGroupModel = _mapper.Map<VisaTypeModel>(_UserGroupService.GetDetails(id));
            return View("Create", _UserGroupModel);
        }



        [HttpDelete]
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                var result = _UserGroupService.Delete(id);
                _toastNotification.AddSuccessToastMessage("Visa Type deleted successfully");

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
