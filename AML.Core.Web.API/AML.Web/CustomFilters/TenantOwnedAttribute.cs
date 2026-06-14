using System;

namespace AML.Web.CustomFilters
{
    /// <summary>
    /// Tenant-isolation enum: which kind of resource the action's id parameter refers to.
    /// Each value maps to a service the TenantOwnedFilter knows how to load by id.
    /// </summary>
    public enum TenantResource
    {
        Case,
        CustomerMaster,
        UserGroup,
        Branch,
        Department,
        Designation,
        //VisaType
    }

    /// <summary>
    /// Marks an action as operating on a tenant-owned resource. The TenantOwnedFilter
    /// will load the resource by id and 403 if its ClientId differs from the session's
    /// ClientId — closes the IDOR class identified in the 2026-05-02 security audit.
    ///
    /// Example:
    ///   [TenantOwned(TenantResource.Case, "CaseId")]
    ///   public async Task&lt;ActionResult&gt; Process(int CaseId) { ... }
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class TenantOwnedAttribute : Attribute
    {
        public TenantResource Resource { get; }
        public string ParamName { get; }

        public TenantOwnedAttribute(TenantResource resource, string paramName)
        {
            Resource = resource;
            ParamName = paramName ?? throw new ArgumentNullException(nameof(paramName));
        }
    }
}
