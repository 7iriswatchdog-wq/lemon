using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AML.ViewModel.ViewModels.Department;
using AML.Core.ServiceContract.Department;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using AML.Web.Helper;
using AML.DTO.DTO.Department;
using AML.ViewModel.ViewModels.DataTable;
using NToastNotify;
using AML.Web.CustomFilters;
using AML.Core.Common.StaticResource;
using System.Reflection;

namespace AML.Web.Controllers.Department
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class DepartmentController : Controller
    {
        private IDepartmentService _departmentService;
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private IHttpClientHandler _clientHandler;
        private IConfiguration _configuration;

        public DepartmentController(IDepartmentService departmentService, IMapper mapper,
            IToastNotification toastNotification, IHttpClientHandler clientHandler, IConfiguration configuration)
        {
            _departmentService = departmentService;
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
            _configuration = configuration;
        }

        public IActionResult Index(string isActive)
        {
            return View();
        }
        [HttpPost("department/custompagination")]
        //ToDo
        public JsonResult CustomPagination(DataTableModel model)
        {
            List<DepartmentModel> abc = new List<DepartmentModel>();
            var clientId = _clientHandler.GetClientId();
            abc = _mapper.Map<List<DepartmentModel>>(_departmentService.GetAll(clientId));


            ////Sorting  
            //if (model.order!=null && model.order.Count>0)
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
                abc = abc.Where(m => m.Name.ToLower().Contains(model.search.value.ToLower())
                || m.Code.ToLower().Contains(model.search.value.ToLower())).ToList();
            }

            //var data = abc.Skip(model.start).Take(model.length).ToList();


            var data = Sort(abc, model.columns[model.order[0].column].data ?? "code", model.order[0].dir ?? "dec")

.Skip(model.start)

.Take(model.length)

.ToList();


            return Json(new
            {
                // this is what datatables wants sending backback
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


        [HttpGet("department/list")]
        public IActionResult List()
        {
            var clientId = _clientHandler.GetClientId();
            List<DepartmentModel> _DepartmentModel = _mapper.Map<List<DepartmentModel>>(_departmentService.GetAll(clientId));
            return View(_DepartmentModel);
        }


        [HttpGet("department/edit/{Id}")]
        [AML.Web.CustomFilters.TenantOwned(AML.Web.CustomFilters.TenantResource.Department, "Id")]
        public IActionResult Edit(int Id)
        {
            DepartmentModel _DepartmentModel = _mapper.Map<DepartmentModel>(_departmentService.GetDetails(Id));
            return View("Add", _DepartmentModel);
        }

        [HttpGet("department/create")]
        public IActionResult Add()
        {
            DepartmentModel _DepartmentModel = new DepartmentModel();
            return View(_DepartmentModel);
        }

        [HttpPost("department/create")]
        public IActionResult Add(DepartmentModel _DepartmentModel)
        {
            if (ModelState.IsValid)
            {
                _DepartmentModel.ClientId = _clientHandler.GetClientId();
                if (_DepartmentModel.Id > 0)
                {
                    var result = _departmentService.Update(_mapper.Map<DepartmentDTO>(_DepartmentModel));
                    if (result.Status == StaticResource.FailStatusCode)
                        _toastNotification.AddErrorToastMessage(result.Message);
                    else
                        _toastNotification.AddSuccessToastMessage("Department updated successfully");
                }
                else
                {
                    var result = _departmentService.Create(_mapper.Map<DepartmentDTO>(_DepartmentModel));
                    if (result.Status == StaticResource.FailStatusCode)
                        _toastNotification.AddErrorToastMessage(result.Message);
                    else
                        _toastNotification.AddSuccessToastMessage(result.Message);
                }
                return RedirectToAction("Index", new { isActive = 0 });
            }
            return View(_DepartmentModel);
        }

        [HttpDelete("department/delete/{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                var result = _departmentService.Delete(id);
                _toastNotification.AddSuccessToastMessage("Department deleted successfully");

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
