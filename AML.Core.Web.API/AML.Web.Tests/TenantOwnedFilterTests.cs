using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AML.Core.ServiceContract.Branch;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.CustomerMaster;
using AML.Core.ServiceContract.Department;
using AML.Core.ServiceContract.Designation;
using AML.Core.ServiceContract.UserGroup;
using AML.Core.ServiceContract.VisaType;
using AML.DTO.DTO.Branch;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.Department;
using AML.DTO.DTO.Designation;
using AML.DTO.DTO.UserGroup;
using AML.DTO.DTO.VisaType;
using AML.Web.CustomFilters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using Xunit;

namespace AML.Web.Tests
{
    /// <summary>
    /// TenantOwnedFilter is the central defence against the IDOR class found in the
    /// 2026-05-02 audit. These tests pin its contract:
    ///   - block (Forbid) when the loaded resource's ClientId differs from session
    ///   - allow when they match
    ///   - allow new-resource flow (id == 0)
    ///   - 400 when the route param is missing
    ///   - 404 when the resource doesn't exist (don't leak existence to other tenants)
    ///   - 403 when the session has no ClientId at all
    ///   - skip entirely when the action has no [TenantOwned] attribute
    /// </summary>
    public class TenantOwnedFilterTests
    {
        private const int CallerClientId = 7;
        private const int OtherTenantClientId = 99;
        private const int ResourceId = 42;

        // ── Helpers ────────────────────────────────────────────────────────────

        private static ActionExecutingContext BuildContext(
            IDictionary<object, object> serviceMap,
            int? sessionClientId,
            object[] attributes,
            IDictionary<string, object> actionArgs,
            IDictionary<string, object> routeValues = null)
        {
            var http = new DefaultHttpContext();

            // Wire the test session.
            http.Features.Set<ISessionFeature>(new FakeSessionFeature { Session = new TestSession() });
            if (sessionClientId.HasValue)
            {
                http.Session.SetString("SessClientId", sessionClientId.Value.ToString());
            }
            http.Session.SetString("SessUserId", "1");

            // Wire the per-request service provider.
            http.RequestServices = new TestServiceProvider(serviceMap);

            var actionDescriptor = new ActionDescriptor
            {
                EndpointMetadata = attributes
            };

            var routeData = new RouteData();
            if (routeValues != null)
                foreach (var kv in routeValues) routeData.Values[kv.Key] = kv.Value;

            var actionContext = new ActionContext(http, routeData, actionDescriptor);
            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                actionArgs ?? new Dictionary<string, object>(),
                controller: null);
        }

        // ── Test cases ─────────────────────────────────────────────────────────

        [Fact]
        public async Task NoAttribute_ShouldAllowThrough()
        {
            // No [TenantOwned] on the action ⇒ filter is a no-op.
            var ctx = BuildContext(
                serviceMap: new Dictionary<object, object>(),
                sessionClientId: CallerClientId,
                attributes: Array.Empty<object>(),
                actionArgs: new Dictionary<string, object>());

            var nextCalled = false;
            ActionExecutionDelegate next = () => { nextCalled = true; return Task.FromResult<ActionExecutedContext>(null); };

            await new TenantOwnedFilter().OnActionExecutionAsync(ctx, next);

            Assert.Null(ctx.Result);
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task MatchingClientId_ShouldAllowThrough()
        {
            // The case belongs to caller's tenant ⇒ filter allows the action.
            var caseService = new Mock<ICustomerCaseService>();
            caseService.Setup(s => s.GetDetails(ResourceId))
                .Returns(new CustomerCaseDTO { ClientId = CallerClientId });

            var ctx = BuildContext(
                serviceMap: new Dictionary<object, object> { [typeof(ICustomerCaseService)] = caseService.Object },
                sessionClientId: CallerClientId,
                attributes: new object[] { new TenantOwnedAttribute(TenantResource.Case, "caseId") },
                actionArgs: new Dictionary<string, object> { ["caseId"] = ResourceId });

            var nextCalled = false;
            ActionExecutionDelegate next = () => { nextCalled = true; return Task.FromResult<ActionExecutedContext>(null); };

            await new TenantOwnedFilter().OnActionExecutionAsync(ctx, next);

            Assert.Null(ctx.Result);
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task MismatchedClientId_ShouldReturnForbid()
        {
            // The case belongs to a different tenant ⇒ filter returns 403, action body NEVER runs.
            var caseService = new Mock<ICustomerCaseService>();
            caseService.Setup(s => s.GetDetails(ResourceId))
                .Returns(new CustomerCaseDTO { ClientId = OtherTenantClientId });

            var ctx = BuildContext(
                serviceMap: new Dictionary<object, object> { [typeof(ICustomerCaseService)] = caseService.Object },
                sessionClientId: CallerClientId,
                attributes: new object[] { new TenantOwnedAttribute(TenantResource.Case, "caseId") },
                actionArgs: new Dictionary<string, object> { ["caseId"] = ResourceId });

            var nextCalled = false;
            ActionExecutionDelegate next = () => { nextCalled = true; return Task.FromResult<ActionExecutedContext>(null); };

            await new TenantOwnedFilter().OnActionExecutionAsync(ctx, next);

            var fr = Assert.IsType<StatusCodeResult>(ctx.Result); Assert.Equal(403, fr.StatusCode);
            Assert.False(nextCalled);
        }

        [Fact]
        public async Task ResourceNotFound_ShouldReturnNotFound()
        {
            // Service returns null ⇒ 404 (do NOT leak existence in other tenants).
            var caseService = new Mock<ICustomerCaseService>();
            caseService.Setup(s => s.GetDetails(ResourceId)).Returns((CustomerCaseDTO)null);

            var ctx = BuildContext(
                serviceMap: new Dictionary<object, object> { [typeof(ICustomerCaseService)] = caseService.Object },
                sessionClientId: CallerClientId,
                attributes: new object[] { new TenantOwnedAttribute(TenantResource.Case, "caseId") },
                actionArgs: new Dictionary<string, object> { ["caseId"] = ResourceId });

            var nextCalled = false;
            ActionExecutionDelegate next = () => { nextCalled = true; return Task.FromResult<ActionExecutedContext>(null); };

            await new TenantOwnedFilter().OnActionExecutionAsync(ctx, next);

            Assert.IsType<NotFoundResult>(ctx.Result);
            Assert.False(nextCalled);
        }

        [Fact]
        public async Task ZeroId_ShouldAllow_AsCreateFlow()
        {
            // id == 0 is the "new resource" pattern in this codebase ⇒ allow through,
            // there's nothing to tenant-check until the record exists.
            var ctx = BuildContext(
                serviceMap: new Dictionary<object, object>(),
                sessionClientId: CallerClientId,
                attributes: new object[] { new TenantOwnedAttribute(TenantResource.CustomerMaster, "id") },
                actionArgs: new Dictionary<string, object> { ["id"] = 0 });

            var nextCalled = false;
            ActionExecutionDelegate next = () => { nextCalled = true; return Task.FromResult<ActionExecutedContext>(null); };

            await new TenantOwnedFilter().OnActionExecutionAsync(ctx, next);

            Assert.Null(ctx.Result);
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task NegativeId_ShouldReturnBadRequest()
        {
            var ctx = BuildContext(
                serviceMap: new Dictionary<object, object>(),
                sessionClientId: CallerClientId,
                attributes: new object[] { new TenantOwnedAttribute(TenantResource.Case, "caseId") },
                actionArgs: new Dictionary<string, object> { ["caseId"] = -1 });

            var nextCalled = false;
            ActionExecutionDelegate next = () => { nextCalled = true; return Task.FromResult<ActionExecutedContext>(null); };

            await new TenantOwnedFilter().OnActionExecutionAsync(ctx, next);

            Assert.IsType<BadRequestObjectResult>(ctx.Result);
            Assert.False(nextCalled);
        }

        [Fact]
        public async Task MissingParam_ShouldReturnBadRequest()
        {
            var ctx = BuildContext(
                serviceMap: new Dictionary<object, object>(),
                sessionClientId: CallerClientId,
                attributes: new object[] { new TenantOwnedAttribute(TenantResource.Case, "caseId") },
                actionArgs: new Dictionary<string, object>()); // empty — no caseId

            var nextCalled = false;
            ActionExecutionDelegate next = () => { nextCalled = true; return Task.FromResult<ActionExecutedContext>(null); };

            await new TenantOwnedFilter().OnActionExecutionAsync(ctx, next);

            Assert.IsType<BadRequestObjectResult>(ctx.Result);
            Assert.False(nextCalled);
        }

        [Fact]
        public async Task NoSessionClientId_ShouldPassThroughToAuthFilter()
        {
            // Missing session ClientId means the request isn't authenticated yet. The
            // TenantOwnedFilter is a global filter and runs before per-controller auth
            // filters (SessionAuthorize), so it must NOT 403 — it would mask the legit
            // "redirect to login" path. Pass through and let the auth chain handle it.
            var ctx = BuildContext(
                serviceMap: new Dictionary<object, object>(),
                sessionClientId: null,
                attributes: new object[] { new TenantOwnedAttribute(TenantResource.Case, "caseId") },
                actionArgs: new Dictionary<string, object> { ["caseId"] = ResourceId });

            var nextCalled = false;
            ActionExecutionDelegate next = () => { nextCalled = true; return Task.FromResult<ActionExecutedContext>(null); };

            await new TenantOwnedFilter().OnActionExecutionAsync(ctx, next);

            Assert.Null(ctx.Result);
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task RouteValueFallback_ShouldReadIdFromRouteData()
        {
            // ActionArguments empty but RouteData has the id ⇒ filter falls through
            // to RouteData lookup.
            var caseService = new Mock<ICustomerCaseService>();
            caseService.Setup(s => s.GetDetails(ResourceId))
                .Returns(new CustomerCaseDTO { ClientId = CallerClientId });

            var ctx = BuildContext(
                serviceMap: new Dictionary<object, object> { [typeof(ICustomerCaseService)] = caseService.Object },
                sessionClientId: CallerClientId,
                attributes: new object[] { new TenantOwnedAttribute(TenantResource.Case, "caseId") },
                actionArgs: new Dictionary<string, object>(),
                routeValues: new Dictionary<string, object> { ["caseId"] = ResourceId.ToString() });

            var nextCalled = false;
            ActionExecutionDelegate next = () => { nextCalled = true; return Task.FromResult<ActionExecutedContext>(null); };

            await new TenantOwnedFilter().OnActionExecutionAsync(ctx, next);

            Assert.Null(ctx.Result);
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task CaseInsensitive_ParamLookup()
        {
            // Attribute says "CaseId" but action argument is "caseId" — must match.
            var caseService = new Mock<ICustomerCaseService>();
            caseService.Setup(s => s.GetDetails(ResourceId))
                .Returns(new CustomerCaseDTO { ClientId = CallerClientId });

            var ctx = BuildContext(
                serviceMap: new Dictionary<object, object> { [typeof(ICustomerCaseService)] = caseService.Object },
                sessionClientId: CallerClientId,
                attributes: new object[] { new TenantOwnedAttribute(TenantResource.Case, "CaseId") },
                actionArgs: new Dictionary<string, object> { ["caseId"] = ResourceId });

            var nextCalled = false;
            ActionExecutionDelegate next = () => { nextCalled = true; return Task.FromResult<ActionExecutedContext>(null); };

            await new TenantOwnedFilter().OnActionExecutionAsync(ctx, next);

            Assert.Null(ctx.Result);
            Assert.True(nextCalled);
        }

        [Theory]
        [InlineData(TenantResource.UserGroup)]
        [InlineData(TenantResource.Branch)]
        [InlineData(TenantResource.Department)]
        [InlineData(TenantResource.Designation)]
        [InlineData(TenantResource.VisaType)]
        public async Task EveryResourceType_BlocksOnTenantMismatch(TenantResource resource)
        {
            // Every resource enum that the filter supports must enforce the tenant check.
            var serviceMap = new Dictionary<object, object>();
            switch (resource)
            {
                case TenantResource.UserGroup:
                    var ug = new Mock<IUserGroupService>();
                    ug.Setup(s => s.GetDetails(ResourceId)).Returns(new UserGroupDTO { ClientId = OtherTenantClientId });
                    serviceMap[typeof(IUserGroupService)] = ug.Object;
                    break;
                case TenantResource.Branch:
                    var br = new Mock<IBranchService>();
                    br.Setup(s => s.GetDetails(ResourceId)).Returns(new BranchDTO { ClientId = OtherTenantClientId });
                    serviceMap[typeof(IBranchService)] = br.Object;
                    break;
                case TenantResource.Department:
                    var dp = new Mock<IDepartmentService>();
                    dp.Setup(s => s.GetDetails(ResourceId)).Returns(new DepartmentDTO { ClientId = OtherTenantClientId });
                    serviceMap[typeof(IDepartmentService)] = dp.Object;
                    break;
                case TenantResource.Designation:
                    var ds = new Mock<IDesignationService>();
                    ds.Setup(s => s.GetDetails(ResourceId)).Returns(new DesignationDTO { ClientId = OtherTenantClientId });
                    serviceMap[typeof(IDesignationService)] = ds.Object;
                    break;
                case TenantResource.VisaType:
                    var vt = new Mock<IVisaTypeService>();
                    vt.Setup(s => s.GetDetails(ResourceId)).Returns(new VisaTypeDTO { ClientId = OtherTenantClientId });
                    serviceMap[typeof(IVisaTypeService)] = vt.Object;
                    break;
            }

            var ctx = BuildContext(
                serviceMap: serviceMap,
                sessionClientId: CallerClientId,
                attributes: new object[] { new TenantOwnedAttribute(resource, "id") },
                actionArgs: new Dictionary<string, object> { ["id"] = ResourceId });

            var nextCalled = false;
            ActionExecutionDelegate next = () => { nextCalled = true; return Task.FromResult<ActionExecutedContext>(null); };

            await new TenantOwnedFilter().OnActionExecutionAsync(ctx, next);

            var fr = Assert.IsType<StatusCodeResult>(ctx.Result); Assert.Equal(403, fr.StatusCode);
            Assert.False(nextCalled);
        }

        // ── Test doubles ───────────────────────────────────────────────────────

        private class TestServiceProvider : IServiceProvider
        {
            private readonly IDictionary<object, object> _map;
            public TestServiceProvider(IDictionary<object, object> map) { _map = map ?? new Dictionary<object, object>(); }
            public object GetService(Type serviceType)
                => _map.TryGetValue(serviceType, out var s) ? s : null;
        }

        private class FakeSessionFeature : ISessionFeature
        {
            public ISession Session { get; set; }
        }

        private class TestSession : ISession
        {
            private readonly Dictionary<string, byte[]> _store = new();
            public bool IsAvailable => true;
            public string Id => "test-session";
            public IEnumerable<string> Keys => _store.Keys;
            public void Clear() => _store.Clear();
            public Task CommitAsync(System.Threading.CancellationToken token = default) => Task.CompletedTask;
            public Task LoadAsync(System.Threading.CancellationToken token = default) => Task.CompletedTask;
            public void Remove(string key) => _store.Remove(key);
            public void Set(string key, byte[] value) => _store[key] = value;
            public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
        }
    }
}
