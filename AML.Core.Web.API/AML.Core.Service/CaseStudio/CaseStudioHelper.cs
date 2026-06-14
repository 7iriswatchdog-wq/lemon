using AML.DTO.DTO.CustomerCase;
using AML.Core.ServiceContract.CaseStudio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AML.Core.Service.CaseStudio
{
    public static class CaseStudioHelper
    {
        /// <summary>
        /// Ensures a customer ID is properly prefixed (e.g. "NAT123") and normalized.
        /// </summary>
        public static string NormalizeCustomerId(string id, string prefix = "NAT")
        {
            if (string.IsNullOrEmpty(id) || id == "0") return id;
            if (id.StartsWith("ext-")) return id;

            // Remove existing prefixes if they match the desired prefix (avoid NATNAT123)
            string normalized = id;
            if (normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(prefix.Length);
            }

            // If it's numeric, re-apply prefix
            if (Regex.IsMatch(normalized, @"^\d+$"))
            {
                return prefix + normalized;
            }

            return id; // Return original if it has some other alphabetic prefix
        }

        /// <summary>
        /// Hydrates a node with common parsing logic for names and dates.
        /// Consolidates logic from CaseController and CaseStudioService.
        /// </summary>
        public static void HydrateNode(CustomerCaseDTO node)
        {
            if (node == null) return;

            // 1. Name Splitting
            var sourceName = (node.FirstName ?? node.LastName ?? node.onb_name ?? "").Trim();
            if (string.IsNullOrEmpty(node.FirstName) && !string.IsNullOrEmpty(sourceName))
            {
                var nameParts = sourceName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                node.FirstName = nameParts.Length > 0 ? nameParts[0] : "";
                node.LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : "";
            }

            // 2. Date Correction (Legacy Parity)
            if (node.DOB.Year <= 1900 && !string.IsNullOrEmpty(node.EstablishmentDate))
            {
                if (DateTime.TryParse(node.EstablishmentDate, out var dob))
                {
                    node.DOB = dob;
                }
            }

            // 3. Customer Type Normalization
            if (!string.IsNullOrEmpty(node.CustomerType))
            {
                string type = node.CustomerType.ToLower();
                if (type == "corporate" || type == "corp") node.CustomerType = "C";
                else if (type == "individual" || type == "ind") node.CustomerType = "I";
            }
        }

        /// <summary>
        /// Resolves the relationship label/type consistently.
        /// </summary>
        public static string ResolveRelationshipType(string currentType, bool isCorporate, bool parentIsCorporate)
        {
            if (!string.IsNullOrEmpty(currentType)) return currentType;

            string parentPrefix = parentIsCorporate ? "Corporate" : "Individual";
            string childSuffix = isCorporate ? "Corp" : "Ind";

            return $"{parentPrefix}_{childSuffix}";
        }

        /// <summary>
        /// Determines if a node's core data has changed relative to an existing DTO.
        /// Used to skip redundant screening/risk if data is pristine.
        /// </summary>
        public static bool IsDirty(CustomerCaseDTO existing, CaseStudioNode node)
        {
            if (existing == null || node == null) return true;

            // Normalize names for comparison
            string existingFull = (existing.FirstName + " " + existing.LastName).Trim();
            string nodeFull = (node.FirstName + " " + node.LastName).Trim();
            if (string.IsNullOrEmpty(nodeFull)) nodeFull = node.Label ?? node.Name ?? "";

            if (!string.Equals(existingFull, nodeFull, StringComparison.OrdinalIgnoreCase)) return true;
            
            // Check IDs
            if (existing.PassportId != node.PassportId) return true;
            if (existing.EmiratesIdNumber != node.EmiratesIdNumber) return true;
            if (existing.CustomerIdNumber != node.Cif) return true;

            // Check Dates
            if (node.Dob != null)
            {
                if (DateTime.TryParse(node.Dob, out var nodeDob))
                {
                    if (existing.DOB.Date != nodeDob.Date) return true;
                }
            }

            // Check Nationality
            if (existing.Nationality != node.Nationality) return true;

            return false; // Pristine
        }

        /// <summary>
        /// Populates hierarchy-specific metadata (Level, Roots, etc.) to simplify frontend/Razor rendering.
        /// </summary>
        public static void CalculateHierarchyMetadata(List<CustomerCaseDTO> hierarchy, string rootId)
        {
            if (hierarchy == null || !hierarchy.Any()) return;

            var idMap = hierarchy.ToDictionary(h => h.CustomerId, h => h);
            var root = !string.IsNullOrEmpty(rootId) && idMap.TryGetValue(rootId, out var r) ? r : hierarchy.FirstOrDefault();
            
            if (root == null) return;

            foreach (var item in hierarchy)
            {
                // Sync Group Info from root if missing
                if (string.IsNullOrEmpty(item.GroupId))
                {
                    item.GroupId = root.GroupId;
                    item.GroupRisk = root.GroupRisk;
                    item.GroupEntityof = root.GroupEntityof;
                }

                // Standardize CaseStatus labels
                if (string.IsNullOrEmpty(item.CaseStatus))
                {
                    item.CaseStatus = ((AML.Core.DataContract.Enum.CaseStatus)item.Status).ToString();
                }
            }
        }

        /// <summary>
        /// Hydrates group-level metadata and standardizes relationship labeling.
        /// </summary>
        public static void HydrateGroupMetadata(CustomerCaseDTO dto, string groupId, string groupRisk, string groupLabel)
        {
            if (dto == null) return;

            dto.GroupId = groupId;
            dto.GroupRisk = groupRisk;
            dto.GroupEntityof = groupLabel;

            // Standardize CaseStatus labels if missing
            if (string.IsNullOrEmpty(dto.CaseStatus))
            {
                dto.CaseStatus = ((AML.Core.DataContract.Enum.CaseStatus)dto.Status).ToString();
            }
        }
    }
}
