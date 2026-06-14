using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.Sanction;
using AML.DTO.DTO.FreeSource;
using AML.DTO.DTO.Sanction;
using AML.ViewModel.ViewModels.Country;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.Sanction;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NToastNotify;
using static AML.DTO.DTO.FreeSource.BlackListMongoDTO;

namespace AML.Web.Controllers.Sanction
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class SanctionController : Controller
    {
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly IHttpClientHandler _clientHandler;
        private readonly ICountryService _countryService;
        private readonly ISanctionService _sanctionService;
        private int clientId = 0;
        public SanctionController(
            IMapper mapper, 
            IToastNotification toastNotification, 
            IHttpClientHandler clientHandler,
            ICountryService countryService,
            ISanctionService sanctionService
        )
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
            _countryService = countryService;
            _sanctionService = sanctionService;
            clientId = clientHandler.GetClientId();
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult WatchList()
        {
            return View();
        }
        [HttpPost("Sanction/custompagination")]
        public JsonResult CustomPagination(DataTableModel model)
        {
            List<SanctionWatchModel> watchList = _mapper.Map<List<SanctionWatchModel>>(_sanctionService.GetAll());
            if (!string.IsNullOrEmpty(model.search.value))
            {
                watchList = watchList.Where(m => m.FirstName.ToLower().Contains(model.search.value.ToLower())
                || m.LastName.ToLower().Contains(model.search.value.ToLower())
                || m.MiddleName.ToLower().Contains(model.search.value.ToLower())).ToList();
            }

            var data = watchList.Skip(model.start).Take(model.length).ToList();
            var response = Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = watchList.Count,//totalResultsCount,
                recordsFiltered = watchList.Count,//filteredResultsCount,
                data = data
            });
            return response;
        }
        [HttpGet("Sanction/create")]
        public ActionResult Create()
        {
            SanctionWatchModel model = new SanctionWatchModel();
            model.CreatedBy = _clientHandler.GetUserId();
            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Id", "Name");
            return View(model);
        }
        // POST: Branch/Create
        [HttpPost("Sanction/create")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SanctionWatchModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.Nationality = _mapper.Map<CountryModel>(_countryService.GetDetails(model.CountryId)).Name;
                    var result= _sanctionService.Create(_mapper.Map<WatchListDTO>(model));
                    if(result.Result > 0)
                    {
                    TempData["Message"] = result.Message;
                    TempData["Status"] = result.Status;
                    _toastNotification.AddSuccessToastMessage("Watch List added successfully");
                    return RedirectToAction("WatchList");
                    }
                    else
                    {
                        TempData["Message"] = result.Message;
                        TempData["Status"] = result.Status;
                        _toastNotification.AddSuccessToastMessage("Something went wrong.please try again.");
                        model.CreatedBy = _clientHandler.GetUserId();
                        model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Id", "Name");
                        return View(model);
                    }
                }
                else
                {
                    model.CreatedBy = _clientHandler.GetUserId();
                    model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Id", "Name");
                    return View(model);
                }
                
            }
            catch (Exception ex)
            {
                TempData["Message"] = ex.Message;
                TempData["Status"] = 500;
                return View(model);
            }
        }
    }
}
