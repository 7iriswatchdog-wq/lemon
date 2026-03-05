using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AML.Core.ServiceContract.UserAccess;
using AML.Core.ServiceContract.UserGroup;
using AML.DTO.DTO.UserAccess;
using AML.DTO.DTO.UserGroup;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.UserAccess;
using AML.ViewModel.ViewModels.UserGroup;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NToastNotify;

namespace AML.Web.Controllers.UserGroup
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class UserGroupController : Controller
    {

        private IUserGroupService _UserGroupService;
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private IModuleService _moduleService;
        private IFunctionalityService _functionalityService;
        private IUserGroupRightService _userGroupRightService;
        private IHttpClientHandler _clientHandler;
        public UserGroupController(IUserGroupService UserGroupService, IUserGroupRightService userGroupRightService, IToastNotification toastNotification, IMapper mapper, IHttpClientHandler clientHandler, IConfiguration _configuration, IModuleService moduleService, IFunctionalityService functionalityService)
        {
            _UserGroupService = UserGroupService;
            _mapper = mapper;
            _toastNotification = toastNotification;
            _moduleService = moduleService;
            _functionalityService = functionalityService;
            _userGroupRightService = userGroupRightService;
            _clientHandler = clientHandler;
        }
        // GET: UserGroup
        public ActionResult Index(int? clientId)
        {
            ViewBag.ClientId = clientId ?? _clientHandler.GetClientId();
            return View();
        }
        [HttpPost("usergroup/custompagination")]
        //ToDo
        public JsonResult CustomPagination(DataTableModel model, int Id)
        {
            var clientId = 0;
            if (Id == 0)
            {
                clientId = _clientHandler.GetClientId();
            }
            else
            {
                clientId = Id;
            }
            List<UserGroupModel> userGroup = _mapper.Map<List<UserGroupModel>>(_UserGroupService.GetAll(clientId).Where(x => x.IsActive == 1));
            if (!string.IsNullOrEmpty(model.search.value))
            {
                userGroup = userGroup.Where(m => m.Name.ToLower().Contains(model.search.value.ToLower())
                //|| m.Description.ToLower().Contains(model.search.value.ToLower()) 
                || m.Code.ToLower().Contains(model.search.value.ToLower())).ToList();
            }

            var data = Sort(userGroup, model.columns[model.order[0].column].data ?? "code", model.order[0].dir ?? "dec")

.Skip(model.start)

.Take(model.length)

.ToList();

            //var data = userGroup.Skip(model.start).Take(model.length).ToList();
            return Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = userGroup.Count,//totalResultsCount,
                recordsFiltered = userGroup.Count,//filteredResultsCount,
                data = data,
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
        public ActionResult Create(int? clientId)
        {
            UserGroupModel _UserGroupModel = new UserGroupModel();
            _UserGroupModel.ClientId = clientId ?? _clientHandler.GetClientId();
            return View(_UserGroupModel);
        }

        [HttpPost]
        // POST: UserGroup/UserGroup-add
        [HttpPost]
        public ActionResult Create(UserGroupModel model)
        {
            try
            {
                if (model.ClientId == 0)
                    model.ClientId = _clientHandler.GetClientId();
                if (ModelState.IsValid)
                {
                    if (model.Id > 0)
                    {
                        var result = _UserGroupService.Update(_mapper.Map<UserGroupDTO>(model));
                        _toastNotification.AddSuccessToastMessage("User group updated successfully");

                    }
                    else
                    {
                        var result = _UserGroupService.Create(_mapper.Map<UserGroupDTO>(model));
                        _toastNotification.AddSuccessToastMessage("User group added successfully");
                    }
                    return RedirectToAction("Details", "AdminManagement", new { id = model.ClientId });
                }
                return View(model);

            }
            catch
            {
                return View(model);
            }
        }

        [HttpGet("usergroup/edit/{id}")]
        public ActionResult Edit(int id)
        {
            UserGroupModel _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(id));
            return View("Create", _UserGroupModel);
        }



        [HttpDelete("usergroup/delete/{id}")]
        //[ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                var result = _UserGroupService.Delete(id);
                _toastNotification.AddSuccessToastMessage("User group deleted successfully");

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        [HttpGet("usergroup/GroupRight/{id}")]
        public ActionResult GroupRight(int id)
        {
            UserGroupRightDetailsModel _UserModel = new UserGroupRightDetailsModel();
            List<UserGroupRightDetailsModel> _UserModelGroupList = new List<UserGroupRightDetailsModel>();
            _UserModel.UserGroupName = _UserGroupService.GetDetails(id).Name;
            _UserModel.UserGroupId = id;
            var clientId = _clientHandler.GetClientId();
            var result = _mapper.Map<List<UserGroupRightDetailsModel>>(_userGroupRightService.GetDetailsByUserGroupId(id).Result).GroupBy(x => x.ModuleId);
            var funResult = _mapper.Map<List<FunctionalityModel>>(_functionalityService.GetAll(clientId).Result).GroupBy(x => x.ModuleId);
            foreach (var item in funResult)
            {
                UserGroupRightDetailsModel _UserModelGrouped = new UserGroupRightDetailsModel();
                List<UserGroupRightDetailsModel> _UserModelGroup = new List<UserGroupRightDetailsModel>();
                _UserModelGrouped.ModuleName = item.FirstOrDefault().ModuleName;
                _UserModelGrouped.ModuleId = item.FirstOrDefault().ModuleId;
                _UserModelGrouped.IsModule = false;
                foreach (var module in result)
                {
                    if (module.FirstOrDefault().ModuleId == item.FirstOrDefault().ModuleId)
                    {
                        _UserModelGrouped.IsModule = true;
                    }
                }

                foreach (var k in item)
                {
                    UserGroupRightDetailsModel m = new UserGroupRightDetailsModel();
                    m.FunctionalityName = k.Name;
                    m.FunctionalityId = k.Id;
                    m.IsFunctionality = false;
                    m.UserRightData = new UserRightsSaveModel();
                    m.UserRightData.GroupId = id;
                    //m.UserRightData.Modules = 
                    foreach (var module in result)
                    {
                        foreach (var function in module)
                        {
                            if (function.FunctionalityId == m.FunctionalityId)
                            {
                                m.IsFunctionality = true;
                            }
                        }
                    }
                    _UserModelGroup.Add(m);
                }
                _UserModelGrouped.UserGroupRightDetails = _UserModelGroup;
                _UserModelGroupList.Add(_UserModelGrouped);
            }
            _UserModel.UserGroupRightDetails = _UserModelGroupList;
            ViewBag.ClientId = clientId;
            return View(_UserModel);
        }

        [HttpPost]
        public JsonResult GroupRight(UserGroupRights usergrouprights)
        {
            var sa = new JsonSerializerSettings();
            List<UserGroupRightModel> UserGroup = new List<UserGroupRightModel>();
            var clientId = _clientHandler.GetClientId();
            var funResult = _mapper.Map<List<FunctionalityModel>>(_functionalityService.GetAll(clientId).Result).GroupBy(x => x.ModuleId);
            foreach (var item in usergrouprights.FunctionalityIds)
            {
                UserGroupRightModel m = new UserGroupRightModel();
                foreach (var fun in funResult)
                {
                    foreach (var function in fun)
                    {
                        if (function.Id == item)
                        {
                            m.ModuleId = function.ModuleId;
                            m.FunctionalityId = item;
                            m.UserGroupId = usergrouprights.UserGroupId;
                            m.CreatedBy = 1;
                            UserGroup.Add(m);
                        }
                    }
                }
            }
            var deleteResult = _userGroupRightService.DeleteByUserGroupId(usergrouprights.UserGroupId);
            foreach (var item in UserGroup)
            {
                _userGroupRightService.Create(_mapper.Map<UserGroupRightDTO>(item));
            }
            _toastNotification.AddSuccessToastMessage("Group Right added successfully");
            return Json(new { Url = "usergroup/GroupRight?id=userModel.UserGroupId" });
        }
    }
}
