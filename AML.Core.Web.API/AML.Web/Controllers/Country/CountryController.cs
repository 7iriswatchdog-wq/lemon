using AML.Core.ServiceContract.Country;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.Country;
using AML.ViewModel.ViewModels.Country;
using AML.ViewModel.ViewModels.DataTable;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AML.Web.Controllers.Country
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class CountryController : Controller
    {
        private ICountryService _CountryService;
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private IHttpClientHandler _clientHandler;

        public CountryController(ICountryService CountryService, IToastNotification toastNotification,
            IMapper mapper, IHttpClientHandler clientHandler, IConfiguration _configuration)
        {
            _CountryService = CountryService;
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
        }
        // GET: Country
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost("country/custompagination")]
        public JsonResult CustomPagination(DataTableModel model,
    int orderColumn = 0, string orderDirection = "asc") // Added sorting parameters
        {
                var clientId = _clientHandler.GetClientId();
                var data = _mapper.Map<List<CountryModel>>(_CountryService.GetAll(clientId));

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
            IOrderedEnumerable<CountryModel> sortedData;
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

                // Apply pagination
                var totalRecords = sortedData.Count();
                var paginatedData = sortedData
                    .Skip(model.start)
                    .Take(model.length)
                    .ToList();

                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = paginatedData,
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

        [HttpGet("country/details")]
        // GET: Country/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        [HttpGet("country/create")]
        public ActionResult Create()
        {
            CountryModel _CountryModel = new CountryModel();
            return View(_CountryModel);
        }

        // POST: Country/Create
        [HttpPost("country/create")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CountryModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.ClientId = _clientHandler.GetClientId();
                    if (model.Id > 0)
                    {
                        var result = _CountryService.Update(_mapper.Map<CountryDTO>(model));

                        _toastNotification.AddSuccessToastMessage("Country updated successfully");

                    }
                    else
                    {
                        var result = _CountryService.Create(_mapper.Map<CountryDTO>(model));

                        _toastNotification.AddSuccessToastMessage("Country added successfully");
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

        [HttpGet("country/edit/{id}")]
        public ActionResult Edit(int id)
        {
            CountryModel _CountryModel = _mapper.Map<CountryModel>(_CountryService.GetDetails(id));
            return View("Create", _CountryModel);
        }



        // POST: Country/Delete/5
        [HttpDelete("country/delete/{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                var result = _CountryService.Delete(id);
                _toastNotification.AddSuccessToastMessage("Country deleted successfully");

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
