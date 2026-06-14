using AML.Core.Common.StaticResource;
using AML.Core.ServiceContract.Branch;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.CustomerMaster;
using AML.Core.ServiceContract.Department;
using AML.Core.ServiceContract.Designation;
using AML.Core.ServiceContract.UserGroup;
using AML.Core.ServiceContract.VisaType;
using AML.Web.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AML.Web.CustomFilters
{
    /// <summary>
    /// Global action filter that, for any action carrying [TenantOwned], looks up the
    /// resource by id and short-circuits to 403 when the resource's ClientId differs
    /// from the caller's session ClientId. Closes the IDOR class identified in the
    /// 2026-05-02 security audit.
    ///
    /// Registered globally in Startup.ConfigureServices via:
    ///   services.AddControllersWithViews(o =&gt; o.Filters.Add&lt;TenantOwnedFilter&gt;());
    /// so it runs after model binding but before the action body.
    ///
    /// On a missing or zero id we return 400 BadRequest (caller bug, not an IDOR).
    /// On a not-found resource we return 404 NotFound (don't leak existence).
    /// On a tenant mismatch we return 403 Forbid + log to detect probing.
    /// </summary>
    public class TenantOwnedFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var attr = context.ActionDescriptor.EndpointMetadata
                .OfType<TenantOwnedAttribute>()
                .FirstOrDefault();

            if (attr == null)
            {
                await next();
                return;
            }

            // Resolve session ClientId — must be present for an authenticated request.
            // If absent, the request is unauthenticated; let SessionAuthorize (or whichever
            // auth filter) handle the redirect to login. Returning 403 here would mask the
            // legitimate "please log in" path and surface as a confusing error to users.
            var session = context.HttpContext.Session;
            var sessClientIdStr = session?.GetString(StaticResource.sessClientId);
            if (string.IsNullOrEmpty(sessClientIdStr) || !int.TryParse(sessClientIdStr, out var sessClientId) || sessClientId <= 0)
            {
                await next();
                return;
            }

            // Pull the id from action arguments by name (case-insensitive).
            // id == 0 is conventional in this codebase for "new resource" (create flows
            // that share an Edit endpoint), so we allow it through — there's nothing to
            // tenant-check until the record is created.
            if (!TryReadIntArg(context, attr.ParamName, out var resourceId))
            {
                context.Result = new BadRequestObjectResult($"Missing '{attr.ParamName}'.");
                return;
            }
            if (resourceId == 0)
            {
                await next();
                return;
            }
            if (resourceId < 0)
            {
                context.Result = new BadRequestObjectResult($"Invalid '{attr.ParamName}'.");
                return;
            }

            // Load the resource via the appropriate service and capture its ClientId.
            int? recordClientId = LoadClientIdForResource(context.HttpContext, attr.Resource, resourceId);

            if (recordClientId == null)
            {
                context.Result = new NotFoundResult();
                return;
            }

            if (recordClientId.Value != sessClientId)
            {
                // Log probing attempts so security can spot them. Never reveal that the
                // record exists in a different tenant — return generic 403.
                Console.WriteLine($"TenantOwnedFilter: cross-tenant access blocked. " +
                                  $"Resource={attr.Resource} Id={resourceId} " +
                                  $"OwningClientId={recordClientId.Value} CallerClientId={sessClientId} " +
                                  $"User={session?.GetString(StaticResource.sessUserId)} " +
                                  $"Path={context.HttpContext.Request.Path}");
                // Use plain 403 instead of ForbidResult — this app has no auth-scheme registered,
                // so ForbidResult would throw InvalidOperationException at result-execution time.
                context.Result = new StatusCodeResult(403);
                return;
            }

            await next();
        }

        // -------- helpers --------

        private static bool TryReadIntArg(ActionExecutingContext ctx, string name, out int value)
        {
            value = 0;

            // Try action arguments — exact match first, then case-insensitive lookup.
            object raw = null;
            if (ctx.ActionArguments.TryGetValue(name, out var v))
            {
                raw = v;
            }
            else
            {
                var match = ctx.ActionArguments.FirstOrDefault(kv => string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase));
                if (match.Key != null) raw = match.Value;
            }

            if (raw != null)
            {
                if (raw is int i) { value = i; return true; }
                if (int.TryParse(raw.ToString(), out var parsed)) { value = parsed; return true; }
            }

            // Fall back to route values (some actions read id from route directly).
            var route = ctx.RouteData?.Values;
            if (route != null && route.TryGetValue(name, out var rv) && rv != null && int.TryParse(rv.ToString(), out var rparsed))
            {
                value = rparsed;
                return true;
            }

            return false;
        }

        private static int? LoadClientIdForResource(HttpContext httpContext, TenantResource resource, int id)
        {
            var sp = httpContext.RequestServices;
            try
            {
                switch (resource)
                {
                    case TenantResource.Case:
                        var caseSvc = sp.GetService<ICustomerCaseService>();
                        return caseSvc?.GetDetails(id)?.ClientId;

                    case TenantResource.CustomerMaster:
                        var cmSvc = sp.GetService<ICustomerMasterService>();
                        return cmSvc?.GetDetailsById(id)?.ClientId;

                    case TenantResource.UserGroup:
                        var ugSvc = sp.GetService<IUserGroupService>();
                        return ugSvc?.GetDetails(id)?.ClientId;

                    case TenantResource.Branch:
                        var brSvc = sp.GetService<IBranchService>();
                        return brSvc?.GetDetails(id)?.ClientId;

                    case TenantResource.Department:
                        var deptSvc = sp.GetService<IDepartmentService>();
                        return deptSvc?.GetDetails(id)?.ClientId;

                    case TenantResource.Designation:
                        var desSvc = sp.GetService<IDesignationService>();
                        return desSvc?.GetDetails(id)?.ClientId;

                    //case TenantResource.VisaType:
                    //    var vtSvc = sp.GetService<IVisaTypeService>();
                    //    return vtSvc?.GetDetails(id)?.ClientId;

                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                // Don't leak internal errors as 5xx — fail closed (treat as not-found).
                Console.WriteLine($"TenantOwnedFilter: load failed for {resource} id={id}: {ex.Message}");
                return null;
            }
        }
    }
}
