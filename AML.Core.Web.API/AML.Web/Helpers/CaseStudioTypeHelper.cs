using System;
using System.Collections.Generic;
using System.Linq;
using AML.ViewModel.ViewModels.CustomerCase;

namespace AML.Web.Helpers
{
    public static class CaseStudioTypeHelper
    {
        public static string GetDynamicType(CaseModel child, IEnumerable<CaseModel> allShareholders, string currentCaseId, string currentCaseType, string parentCustomerId = null)
        {
            if (child == null) return "-";
            if (child.CustomerId == currentCaseId)
            {
                return currentCaseType == "C" || currentCaseType == "Corporate" ? "Corporate" : "Individual";
            }

            if (allShareholders == null)
            {
                allShareholders = Enumerable.Empty<CaseModel>();
            }

            var path = new List<string>();
            var current = child;
            var visited = new HashSet<string>();
            bool firstStep = true;

            while (current != null && current.CustomerId != currentCaseId)
            {
                if (visited.Contains(current.CustomerId)) break;
                visited.Add(current.CustomerId);

                path.Insert(0, current.CustomerType == "C" || current.CustomerType == "Corporate" ? "Corp" : "Ind");

                if (string.IsNullOrEmpty(current.ParentId) || current.ParentId == current.CustomerId)
                {
                    break;
                }

                if (ParentIdContains(current.ParentId, currentCaseId))
                {
                    if (firstStep && !string.IsNullOrEmpty(parentCustomerId) && parentCustomerId != currentCaseId)
                    {
                        // Walk through the rendering parent instead of breaking immediately
                    }
                    else
                    {
                        break;
                    }
                }

                if (firstStep && !string.IsNullOrEmpty(parentCustomerId))
                {
                    current = allShareholders.FirstOrDefault(s => s.CustomerId == parentCustomerId);
                }
                else
                {
                    current = allShareholders.FirstOrDefault(s => ParentIdContains(current.ParentId, s.CustomerId));
                }
                firstStep = false;
            }

            string rootPrefix = currentCaseType == "C" || currentCaseType == "Corporate" ? "Corp" : "Ind";
            path.Insert(0, rootPrefix);

            return string.Join("_", path);
        }

        public static string GetRelationshipForParent(string parentIdField, string relationshipField, string parentCustomerId)
        {
            if (string.IsNullOrEmpty(parentIdField) || string.IsNullOrEmpty(relationshipField)) 
                return relationshipField;

            var parents = parentIdField.Split(',').Select(p => p.Trim()).ToList();
            var rels = relationshipField.Split(',').Select(r => r.Trim()).ToList();

            int index = parents.IndexOf(parentCustomerId);
            if (index >= 0 && index < rels.Count)
            {
                return rels[index];
            }
            return rels.FirstOrDefault() ?? relationshipField;
        }

        public static string GetFlagTypeForParent(string parentIdField, string flagTypeField, string parentCustomerId)
        {
            if (string.IsNullOrEmpty(parentIdField) || string.IsNullOrEmpty(flagTypeField)) 
                return flagTypeField;

            var parents = parentIdField.Split(',').Select(p => p.Trim()).ToList();
            var flags = flagTypeField.Split(',').Select(f => f.Trim()).ToList();

            int index = parents.IndexOf(parentCustomerId);
            if (index >= 0 && index < flags.Count)
            {
                return flags[index];
            }
            return flags.FirstOrDefault() ?? flagTypeField;
        }

        public static bool ParentIdContains(string parentIdField, string targetId)
        {
            if (string.IsNullOrEmpty(parentIdField) || string.IsNullOrEmpty(targetId)) return false;
            return parentIdField.Split(',').Select(p => p.Trim()).Contains(targetId);
        }

        public static string GetFlagTypeShortcode(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return "ISH";
            
            var normalized = flag.Trim().ToLowerInvariant().Replace(" ", "_").Replace("-", "_");
            switch (normalized)
            {
                case "ish":
                case "individual_shareholder":
                case "individualshareholder":
                case "shareholder":
                    return "ISH";
                
                case "csh":
                case "corporate_shareholder":
                case "corporateshareholder":
                case "corporate":
                    return "CSH";
                
                case "irp":
                case "individual_related_party":
                case "individualrelatedparty":
                    return "IRP";
                
                case "crp":
                case "corporate_related_party":
                case "corporaterelatedparty":
                    return "CRP";
                
                case "as":
                case "authorised_signatory":
                case "authorisedsignatory":
                case "authorized_signatory":
                case "authorizedsignatory":
                    return "AS";
                
                case "sm":
                case "senior_management":
                case "seniormanagement":
                    return "SM";
                
                case "ubo":
                case "ultimate_beneficial_owner":
                case "ultimatebeneficialowner":
                    return "UBO";
                
                case "dir":
                case "director":
                    return "DIR";
                
                case "mng":
                case "manager":
                    return "MNG";
                
                case "poa":
                case "power_of_attorney":
                case "powerofattorney":
                    return "POA";
                
                case "rp":
                case "related_party":
                case "relatedparty":
                    return "RP";
                
                case "main_entity":
                case "mainentity":
                    return "Main Entity";
            }
            
            if (flag.Equals("Main Entity", StringComparison.OrdinalIgnoreCase))
            {
                return "Main Entity";
            }
            
            return flag;
        }

        public static string GetFlagTypeLabel(string flag)
        {
            if (string.IsNullOrEmpty(flag)) return "-";
            
            var normalized = flag.Trim().ToLowerInvariant().Replace(" ", "_");
            switch (normalized)
            {
                case "ish":
                case "individual_shareholder":
                case "individualshareholder":
                    return "Individual Shareholder";
                
                case "csh":
                case "corporate_shareholder":
                case "corporateshareholder":
                    return "Corporate Shareholder";
                
                case "irp":
                case "individual_related_party":
                case "individualrelatedparty":
                    return "Individual Related Party";
                
                case "crp":
                case "corporate_related_party":
                case "corporaterelatedparty":
                    return "Corporate Related Party";
                
                case "as":
                case "authorised_signatory":
                case "authorisedsignatory":
                case "authorized_signatory":
                case "authorizedsignatory":
                    return "Authorised Signatory";
                
                case "sm":
                case "senior_management":
                case "seniormanagement":
                    return "Senior Management";
                
                case "ubo":
                case "ultimate_beneficial_owner":
                case "ultimatebeneficialowner":
                    return "Ultimate Beneficial Owner";
                
                case "dir":
                case "director":
                    return "Director";
                
                case "mng":
                case "manager":
                    return "Manager";
                
                case "poa":
                case "power_of_attorney":
                case "powerofattorney":
                    return "Power of Attorney";
                
                case "rp":
                case "related_party":
                case "relatedparty":
                    return "Related Party";
            }
            
            return flag;
        }
    }
}
