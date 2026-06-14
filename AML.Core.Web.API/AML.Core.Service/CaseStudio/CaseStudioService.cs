using AML.Core.Common.StaticResource;
using AML.Core.ServiceContract.CaseComment;
using AML.Core.ServiceContract.CaseDocument;
using AML.Core.ServiceContract.CaseStudio;
using AML.Core.ServiceContract.Common;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.Risk;
using AML.DTO.DTO.CaseComment;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.CustomerCase;
using AML.Core.ServiceContract.Kyc;
using AML.Core.RepositoryContract.CustomerCase;
using AML.Core.ServiceContract.LovMaster;
using AML.DTO.DTO.Kyc;
using AML.DTO.DTO.Risk;
using AML.ViewModel.ViewModels.Common;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using AML.ViewModel.ViewModels.CustomerCase;

namespace AML.Core.Service.CaseStudio
{
    public class CaseStudioService : BaseService, ICaseStudioService
    {
        private readonly ICustomerCaseService _customerCaseService;
        private readonly ICommonService _commonService;
        private readonly ICaseCommentService _caseCommentService;
        private readonly IRiskService _riskService;
        private readonly ICaseDocumentService _caseDocumentService;
        private readonly IKycService _kycService;
        private readonly ILovMasterService _lovMasterService;
        private readonly ICustomerMasterRepository _customerMasterRepository;
        private readonly IMapper _mapper;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CaseStudioService(
            ICustomerCaseService customerCaseService,
            ICommonService commonService,
            ICaseCommentService caseCommentService,
            IRiskService riskService,
            ICaseDocumentService caseDocumentService,
            IKycService kycService,
            ILovMasterService lovMasterService,
            ICustomerMasterRepository customerMasterRepository,
            IMapper mapper,
            IConfiguration configuration) : base(configuration)
        {
            _customerCaseService = customerCaseService;
            _commonService = commonService;
            _caseCommentService = caseCommentService;
            _riskService = riskService;
            _caseDocumentService = caseDocumentService;
            _kycService = kycService;
            _lovMasterService = lovMasterService;
            _customerMasterRepository = customerMasterRepository;
            _mapper = mapper;
        }

        public async Task<CaseStudioResult> ProcessHierarchyAsync(CaseStudioPayload payload, int clientId, int userId, string baseURL, string baseC6URL)
        {
            var result = new CaseStudioResult { Success = true };
            var studioToBackendIdMap = new Dictionary<string, string>();
            var studioToTypeMap = new Dictionary<string, string>();
            
            try
            {
                var clientInfo = _customerCaseService.GetCustomerCodeprefixByclient(clientId);
                string clientPrefix = clientInfo?.Prefix ?? "NAT";
                // Phase 1 & 2: Database Operations
                // NOTE: TransactionScope removed temporarily to troubleshoot assembly load error 'ta'
                {
                    // Phase 1: Create Cases
                    string rootBackendId = null;
                    var roots = payload.Nodes.Where(n => n.IsRoot).ToList();
                    
                    // Prioritize Main Entity, then new roots, then any root for the return ID
                    var mainEntity = payload.Nodes.FirstOrDefault(n => n.IsMainEntity) 
                                  ?? payload.Nodes.FirstOrDefault(n => n.IsRoot && !n.IsFetched) 
                                  ?? roots.FirstOrDefault();
                                  
                    if (mainEntity != null && !string.IsNullOrEmpty(mainEntity.Id) && mainEntity.Id != "0" && mainEntity.StudioId != null && !mainEntity.StudioId.StartsWith("ext-"))
                    {
                        rootBackendId = mainEntity.Id;
                    }

                    // Order nodes such that parents are processed before children (Topological Sort / Kahn's Algorithm)
                    var orderedNodes = new List<CaseStudioNode>();
                    var inDegree = new Dictionary<string, int>();
                    foreach (var node in payload.Nodes)
                    {
                        inDegree[node.StudioId ?? node.Id] = 0;
                    }
                    if (payload.Edges != null)
                    {
                        foreach (var edge in payload.Edges)
                        {
                            var targetId = edge.To;
                            if (inDegree.ContainsKey(targetId))
                            {
                                inDegree[targetId]++;
                            }
                        }
                    }

                    var queue = new Queue<CaseStudioNode>();
                    // Prefer true roots first, then others with inDegree 0
                    foreach (var root in roots)
                    {
                        string id = root.StudioId ?? root.Id;
                        if (inDegree.ContainsKey(id) && inDegree[id] == 0)
                        {
                            queue.Enqueue(root);
                        }
                    }
                    // Add remaining inDegree 0 nodes that weren't marked as root
                    foreach (var node in payload.Nodes)
                    {
                        string id = node.StudioId ?? node.Id;
                        if (inDegree[id] == 0 && !roots.Any(r => (r.StudioId ?? r.Id) == id))
                        {
                            queue.Enqueue(node);
                        }
                    }

                    while (queue.Any())
                    {
                        var current = queue.Dequeue();
                        if (orderedNodes.Any(on => on.StudioId == current.StudioId)) continue;
                        
                        orderedNodes.Add(current);
                        string currentId = current.StudioId ?? current.Id;
                        var childIds = payload.Edges?.Where(e => e.From == currentId).Select(e => e.To).ToList() ?? new List<string>();
                        foreach (var childId in childIds)
                        {
                            if (inDegree.ContainsKey(childId))
                            {
                                inDegree[childId]--;
                                if (inDegree[childId] == 0)
                                {
                                    var childNode = payload.Nodes.FirstOrDefault(n => (n.StudioId ?? n.Id) == childId);
                                    if (childNode != null) queue.Enqueue(childNode);
                                }
                            }
                        }
                    }

                    // Phase 0.5: Append any orphans or disconnected components missed by BFS
                    foreach (var node in payload.Nodes)
                    {
                        if (!orderedNodes.Any(on => on.StudioId == node.StudioId))
                        {
                            orderedNodes.Add(node);
                        }
                    }

                    // Phase 1.0: Pre-populate Identity Map for existing nodes
                    // This ensures children can find their parent's prefixed ID regardless of processing order.
                    foreach (var node in payload.Nodes)
                    {
                        if (node.IsFetched && !string.IsNullOrEmpty(node.Id) && node.Id != "0" && !node.Id.StartsWith("ext-"))
                        {
                            string sId = node.StudioId ?? node.Id;
                            string backendId = CaseStudioHelper.NormalizeCustomerId(node.Id, clientPrefix);
                            
                            studioToBackendIdMap[sId] = backendId;
                            if (!sId.Contains("_parent_"))
                            {
                                var numericMatch = Regex.Match(sId, @"\d+").Value;
                                if (!string.IsNullOrEmpty(numericMatch)) studioToBackendIdMap[numericMatch] = backendId;
                            }

                            // Update Group Info for existing case to ensure parity
                            try
                            {
                                int numericId = backendId.ParseInt();
                                if (numericId > 0 && !string.IsNullOrEmpty(node.GroupId))
                                {
                                    _customerMasterRepository.UpdateGroupInfo(numericId, node.GroupId, node.GroupRisk, node.IsDuplicate ? 1 : 0, node.GroupEntityof);
                                }
                            }
                            catch { }
                        }
                    }

                    foreach (var node in orderedNodes)
                    {
                        string sId = node.StudioId ?? node.Id;
                        var nodeResult = new NodeResult { StudioId = sId };
                        
                        // Handle External Nodes
                        if (sId != null && sId.StartsWith("ext-"))
                        {
                            var backendId = CaseStudioHelper.NormalizeCustomerId(node.StudioId.Replace("ext-", ""), clientPrefix);
                            studioToBackendIdMap[sId] = backendId;
                             nodeResult.BackendId = backendId;
                             if (node.IsRoot) rootBackendId = backendId;

                             nodeResult.Success = true;
                             nodeResult.BackendId = backendId;
                             nodeResult.NumericId = 0;
                             nodeResult.Message = "Linked to external customer";
                             result.NodeResults.Add(nodeResult);
                             continue;
                        }

                        // Handle Fetched/Pooled Nodes — link only, no Create/Update so existing data is fully preserved
                        if (node.IsFetched)
                        {
                            var fetchedBackendId = CaseStudioHelper.NormalizeCustomerId(node.Id ?? node.StudioId, clientPrefix);
                            studioToBackendIdMap[node.StudioId] = fetchedBackendId;
                            if (node.StudioId != null && !node.StudioId.Contains("_parent_"))
                            {
                                var numericMatch = Regex.Match(node.StudioId, @"\d+").Value;
                                if (!string.IsNullOrEmpty(numericMatch)) studioToBackendIdMap[numericMatch] = fetchedBackendId;
                            }
                            nodeResult.BackendId = fetchedBackendId;
                            if (node.IsRoot && string.IsNullOrEmpty(rootBackendId)) rootBackendId = fetchedBackendId;
                            nodeResult.Success = true;
                            nodeResult.NumericId = 0;
                            nodeResult.Message = "Existing customer linked";

                            // Ensure hierarchical metadata (CompanyCode, ParentID) is synced even for fetched nodes
                            // This is critical for visibility in legacy Related Parties Detail tables
                            try
                            {
                                var fetchedNumericId = _customerCaseService.GetCaseId(fetchedBackendId);
                                if (fetchedNumericId > 0)
                                {
                                    _customerMasterRepository.TouchUpdatedOn(fetchedNumericId, userId);

                                    // Resolve hierarchy for fetched nodes
                                    string hCompanyCode = null;
                                    string hParentId = null;
                                    string rel = null;
                                    string ft = null;

                                    if (!node.IsRoot)
                                    {
                                        var currentRoots = FindAllRootsForNode(node, payload);
                                        var rootBackendIds = currentRoots
                                            .Select(cr => studioToBackendIdMap.GetValueOrDefault(cr.StudioId))
                                            .Where(id => !string.IsNullOrEmpty(id))
                                            .ToList();
                                        if (rootBackendIds.Count == 0 && !string.IsNullOrEmpty(rootBackendId))
                                            rootBackendIds.Add(rootBackendId);

                                        if (rootBackendIds.Any())
                                        {
                                            var cCodes = rootBackendIds.Select(id => char.IsDigit(id[0]) ? clientPrefix + id : id).Distinct();
                                            hCompanyCode = string.Join(",", cCodes);

                                            var parentEdges = payload.Edges?.Where(e => e.To == sId).ToList();
                                            if (parentEdges != null && parentEdges.Any())
                                            {
                                                var pIds = parentEdges
                                                    .Where(pe => studioToBackendIdMap.ContainsKey(pe.From))
                                                    .Select(pe => new {
                                                        ParentId = CaseStudioHelper.NormalizeCustomerId(studioToBackendIdMap[pe.From], clientPrefix),
                                                        Relationship = pe.Relationship
                                                    })
                                                    .GroupBy(x => x.ParentId)
                                                     .Select(g => g.First())
                                                     .ToList();

                                                if (pIds.Any())
                                                {
                                                    hParentId = string.Join(",", pIds.Select(p => p.ParentId));
                                                    var relList = pIds.Select(p => string.IsNullOrEmpty(p.Relationship) ? "Related Party" : p.Relationship).ToList();
                                                    var ftList = relList.Select(r => GetFlagTypeShortcode(r)).ToList();
                                                    rel = string.Join(",", relList);
                                                    ft = string.Join(",", ftList);
                                                }
                                                else
                                                {
                                                    hParentId = hCompanyCode;
                                                    rel = "Related Party";
                                                    ft = GetFlagTypeShortcode(node.FlagType);
                                                }
                                            }
                                            else
                                            {
                                                hParentId = hCompanyCode;
                                                rel = "Related Party";
                                                ft = GetFlagTypeShortcode(node.FlagType);
                                            }
                                        }
                                    }

                                    if (string.IsNullOrEmpty(ft))
                                    {
                                        ft = GetFlagTypeShortcode(node.FlagType);
                                    }

                                    var fetchedDto = _customerCaseService.GetCaseFullDetailsByCustId(fetchedBackendId);
                                    if (fetchedDto != null)
                                    {
                                        // Preserve legacy hierarchy for fetched nodes
                                        hCompanyCode = fetchedDto.CompanyCode;
                                        
                                        // Merge existing parents with new edge relationships from the studio payload
                                        var existingPIds = (fetchedDto.ParentID ?? "").Split(',').Select(p => p.Trim()).ToList();
                                        var existingRels = (fetchedDto.Relationship ?? "").Split(',').Select(p => p.Trim()).ToList();
                                        var existingFts = (fetchedDto.FlagType ?? "").Split(',').Select(p => p.Trim()).ToList();

                                        var unifiedList = new List<(string ParentId, string Relationship, string FlagType)>();
                                        for (int i = 0; i < existingPIds.Count; i++)
                                        {
                                            if (string.IsNullOrEmpty(existingPIds[i])) continue;
                                            string r = i < existingRels.Count ? existingRels[i] : "Related Party";
                                            string fType = i < existingFts.Count ? existingFts[i] : GetFlagTypeShortcode(r);
                                            unifiedList.Add((existingPIds[i], r, fType));
                                        }
                                        
                                        var parentEdges = payload.Edges?.Where(e => e.To == node.StudioId).ToList();
                                        if (parentEdges != null && parentEdges.Any())
                                        {
                                            var edgeData = parentEdges
                                                .Where(pe => studioToBackendIdMap.ContainsKey(pe.From))
                                                .Select(pe => new {
                                                    ParentId = CaseStudioHelper.NormalizeCustomerId(studioToBackendIdMap[pe.From], clientPrefix),
                                                    Relationship = string.IsNullOrEmpty(pe.Relationship) ? "Related Party" : pe.Relationship
                                                })
                                                .GroupBy(x => x.ParentId)
                                                .Select(g => g.First())
                                                .ToList();
                                            
                                            foreach (var ed in edgeData)
                                            {
                                                string f = GetFlagTypeShortcode(ed.Relationship);
                                                var existingIdx = unifiedList.FindIndex(u => u.ParentId == ed.ParentId);
                                                if (existingIdx >= 0)
                                                {
                                                    unifiedList[existingIdx] = (ed.ParentId, ed.Relationship, f);
                                                }
                                                else
                                                {
                                                    unifiedList.Add((ed.ParentId, ed.Relationship, f));
                                                }
                                            }
                                        }

                                        if (unifiedList.Any())
                                        {
                                            hParentId = string.Join(",", unifiedList.Select(u => u.ParentId));
                                            rel = string.Join(",", unifiedList.Select(u => u.Relationship));
                                            ft = string.Join(",", unifiedList.Select(u => u.FlagType));
                                        }
                                        else
                                        {
                                            hParentId = fetchedDto.ParentID;
                                        }
                                        
                                        // Update group metadata
                                        CaseStudioHelper.HydrateGroupMetadata(fetchedDto, node.GroupId, node.GroupRisk, node.GroupEntityof);
                                        _customerCaseService.Update(fetchedDto);
                                    }

                                    _customerMasterRepository.UpdateHierarchyInfo(fetchedNumericId, hCompanyCode, hParentId, node.GroupId, node.GroupRisk, node.IsDuplicate ? 1 : 0, node.GroupEntityof, rel, ft);
                                }
                            }
                            catch (Exception ex) { Logger.Warn(ex, string.Format("Failed to sync hierarchy for fetched node {0}", fetchedBackendId)); }

                            result.NodeResults.Add(nodeResult);
                            continue;
                        }

                        try
                        {
                            // Fix 3: Don't pass 'ALL' or anything during case creation for ScreeningOptions/CaseChangeStatus
                            var dto = _mapper.Map<CustomerCaseDTO>(node);
                            dto.ScreeningOptions = "";
                            dto.CaseChangeStatus = "";
                            if (node.Type == "C" && string.IsNullOrEmpty(dto.LastName))
                            {
                                dto.LastName = node.Name;
                            }
                             
                             // Fix 2: Type/CustomerType mapping (Support case-insensitive and upper/lower)
                             bool isNodeCorporate = IsNodeCorporate(node);
                             CaseStudioHelper.HydrateNode(dto);

                             // Ensure MatchCategory is always populated (legacy: "INDIVIDUAL"/"CORPORATE")
                             dto.MatchCategory = isNodeCorporate ? "CORPORATE" : "INDIVIDUAL";

                            // Resolve actual root for this specific node to ensure correct type context
                            var currentRoots = FindAllRootsForNode(node, payload);
                            var firstRoot = currentRoots.FirstOrDefault();
                            var rootType = (firstRoot != null) ? (IsNodeCorporate(firstRoot) ? "C" : "I") : "C";
                            var rootBackendIds = currentRoots
                                            .Select(cr => studioToBackendIdMap.GetValueOrDefault(cr.StudioId))
                                            .Where(id => !string.IsNullOrEmpty(id))
                                            .ToList();
                            if (rootBackendIds.Count == 0 && !string.IsNullOrEmpty(rootBackendId))
                                rootBackendIds.Add(rootBackendId);
                            var hCompanyCodeList = rootBackendIds.Select(id => char.IsDigit(id[0]) ? clientPrefix + id : id).Distinct().ToList();
                            var rootBackendIdForNode = hCompanyCodeList.FirstOrDefault() ?? rootBackendId;

                            // A node that has a parent edge can still be a main entity if explicitly set (IsMainEntity)
                            var nodeParentEdges = payload.Edges?.Where(e => e.To == node.StudioId).ToList();
                            bool nodeHasParent = nodeParentEdges != null && nodeParentEdges.Any();

                            if (node.IsRoot || node.IsMainEntity)
                            {
                                dto.Type = isNodeCorporate ? "Corporate" : "Individual";
                                dto.CustomerType = isNodeCorporate ? "C" : "I";
                                CaseStudioHelper.HydrateGroupMetadata(dto, node.GroupId, node.GroupRisk, node.GroupEntityof);
                                if (!string.IsNullOrEmpty(node.FlagType))
                                {
                                    dto.FlagType = GetFlagTypeShortcode(node.FlagType);
                                }
                                else
                                {
                                    dto.FlagType = node.IsRoot ? "Main Entity" : (isNodeCorporate ? "CSH" : "ISH");
                                }
                                if (node.StudioId != null)
                                {
                                    studioToTypeMap[node.StudioId] = dto.Type;
                                }
                            }
                            else
                            {
                                // Fix: Account for the specific root node this entity belongs to
                                // Fix: Multi-level classification (up to 7 levels) parity with studio.js
                                if (!string.IsNullOrEmpty(node.ShareholderType))
                                {
                                    dto.ShareholderType = node.ShareholderType;
                                    dto.Type = node.ShareholderType;
                                }
                                else
                                {
                                    // Fallback: Build recursive type path from parent
                                    var parentEdges = payload.Edges?.Where(e => e.To == node.StudioId).ToList();
                                    string parentType = null;
                                    if (parentEdges != null && parentEdges.Any())
                                    {
                                        var parentNodes = parentEdges.Select(pe => payload.Nodes.FirstOrDefault(n => n.StudioId == pe.From)).Where(n => n != null).ToList();
                                        var mainParent = parentNodes.FirstOrDefault(p => p.IsRoot || p.IsMainEntity);
                                        var selectedParent = mainParent ?? parentNodes.FirstOrDefault();
                                        if (selectedParent != null && studioToTypeMap.TryGetValue(selectedParent.StudioId, out var pType))
                                        {
                                            parentType = pType;
                                        }
                                    }
                                    
                                    if (string.IsNullOrEmpty(parentType) && firstRoot != null)
                                    {
                                        parentType = IsNodeCorporate(firstRoot) ? "Corporate" : "Individual";
                                    }

                                    if (!string.IsNullOrEmpty(parentType))
                                    {
                                        string baseType = isNodeCorporate ? "Corp" : "Ind";
                                        dto.ShareholderType = string.Format("{0}_{1}", parentType, baseType);
                                        if (dto.ShareholderType.Length > 150)
                                        {
                                            dto.ShareholderType = dto.ShareholderType.Substring(0, 150);
                                        }
                                        dto.Type = dto.ShareholderType;
                                    }
                                    else
                                    {
                                        // Root-level or orphan fallback
                                        dto.Type = isNodeCorporate ? "Corporate" : "Individual";
                                        dto.ShareholderType = dto.Type;
                                    }
                                }

                                if (!string.IsNullOrEmpty(node.FlagType))
                                {
                                    dto.FlagType = GetFlagTypeShortcode(node.FlagType);
                                }
                                else
                                {
                                    // Fallback FlagType
                                    dto.FlagType = node.IsRoot ? "Main Entity" : (isNodeCorporate ? "CSH" : "ISH");
                                }

                                studioToTypeMap[node.StudioId] = dto.Type;
                                
                                dto.CustomerType = isNodeCorporate ? "C" : "I";
                                CaseStudioHelper.HydrateGroupMetadata(dto, node.GroupId, node.GroupRisk, node.GroupEntityof);
                            }

                             // Resolve Numeric ID for Existing Cases
                             int numericId = 0;
                             string existingCustId = null;
                             if (node.Id != null && node.Id != "0" && !node.Id.StartsWith("ext-"))
                             {
                                 numericId = _customerCaseService.GetCaseId(node.Id);
                                 if (numericId > 0) 
                                 {
                                     existingCustId = CaseStudioHelper.NormalizeCustomerId(node.Id, clientPrefix);
                                 }
                                 else numericId = node.Id.ParseInt(); 
                             }
                             
                             dto.Id = numericId;
                             dto.ClientId = clientId;
                             dto.UserId = userId.ToString();
                             dto.CreatedBy = userId;
                             dto.Status = 0; 
                             dto.IsWhiteListed = "NO";
                             dto.customerCodeprefix = clientPrefix;
                             
                             if (!node.IsRoot && hCompanyCodeList.Any())
                             {
                                 dto.CompanyCode = string.Join(",", hCompanyCodeList);
                                 // Preserve the human-readable group label from the studio (node.GroupEntityof).
                                 // Only fall back to rootBackendIdForNode if no label was provided.
                                 if (string.IsNullOrEmpty(node.GroupEntityof) || node.GroupEntityof == "0")
                                     dto.GroupEntityof = string.Join(",", hCompanyCodeList);
                                 // else: keep dto.GroupEntityof already set by HydrateGroupMetadata above
                                 
                                 // Resolve actual parent from edges for legacy ParentID
                                 var parentEdges = payload.Edges?.Where(e => e.To == sId).ToList();
                                 if (parentEdges != null && parentEdges.Any())
                                 {
                                     var pIds = parentEdges
                                        .Where(pe => studioToBackendIdMap.ContainsKey(pe.From))
                                        .Select(pe => new {
                                            ParentId = CaseStudioHelper.NormalizeCustomerId(studioToBackendIdMap[pe.From], clientPrefix),
                                            Relationship = pe.Relationship
                                        })
                                        .GroupBy(x => x.ParentId)
                                        .Select(g => g.First())
                                        .ToList();

                                     if (pIds.Any())
                                     {
                                         dto.ParentID = string.Join(",", pIds.Select(p => p.ParentId));
                                         var relList = pIds.Select(p => string.IsNullOrEmpty(p.Relationship) ? "Related Party" : p.Relationship).ToList();
                                         var ftList = relList.Select(r => GetFlagTypeShortcode(r)).ToList();
                                         dto.Relationship = string.Join(",", relList);
                                         dto.FlagType = string.Join(",", ftList);
                                     }
                                     else
                                     {
                                         dto.ParentID = dto.CompanyCode;
                                         dto.Relationship = "Related Party";
                                         dto.FlagType = GetFlagTypeShortcode(node.FlagType);
                                     }
                                 }
                                 else
                                 {
                                     dto.ParentID = dto.CompanyCode; // Default to root if no specific edge
                                     dto.Relationship = "Related Party";
                                     dto.FlagType = GetFlagTypeShortcode(node.FlagType);
                                 }
                             }

                            // Phase 0.1: Server-side validation
                            if (node.Name?.Contains("<") == true || node.FirstName?.Contains("<") == true) {
                                nodeResult.Success = false;
                                nodeResult.Message = "Invalid characters detected in name.";
                                result.NodeResults.Add(nodeResult);
                                continue;
                            }

                            ServiceResponse<string> createResult;
                            if (dto.Id > 0)
                            {
                                var updateResp = _customerCaseService.Update(dto);
                                createResult = new ServiceResponse<string> { 
                                    Status = updateResp.Status, 
                                    Message = updateResp.Message,
                                    Result = string.Format("{0}|{1}", dto.Id, existingCustId ?? node.Id)
                                };
                            }
                            else
                             {
                                 createResult = _customerCaseService.Create(dto);
                             }

                            if (createResult.Status == StaticResource.SuccessStatusCode || createResult.Status == 200 || createResult.Status == 1)
                            {
                                // After Create(), dto.CustomerId is populated with the prefixed ID (e.g. NAT135)
                                // and dto.Id is populated with the numeric primary key (e.g. 135)
                                string backendId = !string.IsNullOrEmpty(dto.CustomerId) ? dto.CustomerId : (createResult.Result ?? "");
                                string numericIdStr = dto.Id > 0 ? dto.Id.ToString() : "";

                                // Robust fallback for legacy or complex return strings (e.g. "135ØNAT135")
                                if (backendId.Contains("|") || backendId.Contains((char)216))
                                {
                                    var ids = backendId.Split(new char[] { '|', (char)216 });
                                    numericIdStr = ids[0];
                                    backendId = ids.Length > 1 ? ids[1] : ids[0];
                                }
                                else if (backendId.Contains("A~"))
                                {
                                    var ids = backendId.Split(new string[] { "A~" }, StringSplitOptions.None);
                                    numericIdStr = ids[0];
                                    backendId = ids.Length > 1 ? ids[1] : ids[0];
                                }

                                // Final fallback: if numericIdStr is still empty, try to extract digits from backendId
                                if (string.IsNullOrEmpty(numericIdStr) && !string.IsNullOrEmpty(backendId))
                                {
                                    var match = Regex.Match(backendId, @"\d+");
                                    if (match.Success) numericIdStr = match.Value;
                                }

                                if (sId != null) {
                                    studioToBackendIdMap[sId] = backendId;
                                }
                                if (node.StudioId != null && !node.StudioId.Contains("_parent_"))
                                {
                                    var numericMatch = Regex.Match(node.StudioId, @"\d+").Value;
                                    if (!string.IsNullOrEmpty(numericMatch)) {
                                        studioToBackendIdMap[numericMatch] = backendId;
                                    }
                                }

                                nodeResult.BackendId = backendId;
                                nodeResult.NumericId = numericIdStr.ParseInt();
                                if (node.IsRoot && string.IsNullOrEmpty(rootBackendId)) rootBackendId = backendId;

                                nodeResult.Success = true;
                                nodeResult.Message = createResult.Message;

                                // Stamp updated_on so this case surfaces at the top of Due Diligence
                                try
                                {
                                    int newNumericId = numericIdStr.ParseInt();
                                    if (newNumericId > 0)
                                    {
                                        _customerMasterRepository.TouchUpdatedOn(newNumericId, userId); 
                                        _customerMasterRepository.UpdateHierarchyInfo(newNumericId, dto.CompanyCode, dto.ParentID, dto.GroupId, dto.GroupRisk, node.IsDuplicate ? 1 : 0, dto.GroupEntityof, dto.Relationship, dto.FlagType);
                                    }
                                }
                                catch (Exception ex) { Logger.Warn(ex, string.Format("Failed to touch updated_on for new node {0}", backendId)); }

                                // Phase 1.1: Audit Trail & Case Remark (Fix: Numeric ID in comments)
                                 try
                                 {
                                     _caseCommentService.Create(new CaseCommentDTO
                                     {
                                         CaseId = numericIdStr,
                                         CustomerId = backendId,
                                         Comment = dto.Id > 0 ? "Case Updated via Studio" : "Case Created via Studio",
                                         CommentType = node.IsRoot ? (isNodeCorporate ? "Corporate Screening" : "Individual Screening") : "Related Party Studio",
                                         CreatedBy = userId,
                                         CreatedOnDB = DateTime.Now
                                     });

                                     // Mandatory legacy dashboard integration: UpdateCaseRemark with numeric ID
                                     if (!string.IsNullOrEmpty(numericIdStr) && int.TryParse(numericIdStr, out int finalNumericId))
                                     {
                                          _commonService.UpdateCaseRemark(finalNumericId, new List<DataListModel> {
                                              new DataListModel { remarks = dto.Id > 0 ? "Case Updated via Studio" : "Case created from Studio", matchidno = numericIdStr, matchuid = backendId }
                                          });
                                     }
                                 }
                                 catch (Exception ex) { Logger.Warn(ex, "Failed to create audit trail/remarks"); }

                                 // Phase 1.2: Initial Risk Record (Fix: Deferred to Detailed Assessment to avoid overwrites)
                                 // Initial risk initialization is removed here as it is handled in the asynchronous processing phase.

                                // Phase 1.3: Attachments
                                if (node.Attachments != null && node.Attachments.Count > 0)
                                {
                                    foreach (var att in node.Attachments)
                                    {
                                        try
                                        {
                                            _caseDocumentService.Create(new AML.DTO.DTO.CaseDocument.CaseDocumentDTO
                                            {
                                                CaseId = numericIdStr,
                                                DocumentFileName = att.FileName,
                                                DocumentFullPath = att.FullPath,
                                                DocumentName = att.Name,
                                                IssuedDateOnDB = att.IssuedDate.ParseDB(),
                                                ExpiryDateOnDB = att.ExpiryDate.ParseDB(),
                                                CreatedBy = userId,
                                                CreatedOnDB = DateTime.Now
                                            });
                                        }
                                        catch (Exception ex) { Logger.Warn(ex, string.Format("Failed to link attachment {0} to case {1}", att.FileName, backendId)); }
                                    }
                                }
                            }
                            else
                            {
                                nodeResult.Success = false;
                                nodeResult.Error = createResult.Message;
                                result.Success = false;
                                Logger.Warn(string.Format("Failed to create case for node {0}: {1}", node.StudioId, createResult.Message));
                            }
                        }
                        catch (Exception ex)
                        {
                            nodeResult.Success = false;
                            nodeResult.Error = "An unexpected error occurred.";
                            result.Success = false;
                            Logger.Error(ex, string.Format("Exception creating case for node {0}", sId ?? "UNKNOWN"));
                        }
                        result.NodeResults.Add(nodeResult);
                    }

                    // Phase 2: Create Relationships (Shareholders)
                    var processedParents = new HashSet<string>();
                    foreach (var edge in payload.Edges)
                    {
                        if (studioToBackendIdMap.TryGetValue(edge.From, out var parentId) && 
                            studioToBackendIdMap.TryGetValue(edge.To, out var childId))
                        {
                            if (!processedParents.Contains(parentId))
                            {
                                try { _customerCaseService.DeletePendingShareholders(parentId); }
                                catch (Exception ex) { Logger.Warn(ex, string.Format("Failed to cleanup shareholders for {0}", parentId)); }
                                processedParents.Add(parentId);
                            }

                            var childNode = payload.Nodes.FirstOrDefault(n => n.StudioId == edge.To);
                            if (childNode != null)
                            {
                                bool isNodeCorporate = IsNodeCorporate(childNode);
                                // Resolve actual root for this specific node to ensure correct type context
                                var currentRoot = FindRootForNode(childNode, payload);
                                var rootType = (currentRoot != null) ? (IsNodeCorporate(currentRoot) ? "C" : "I") : "C";

                                try
                                {
                                    var shDto = new ShareholderDTO
                                    {
                                        ClientId = clientId,
                                        UserId = userId,
                                        CompanyCode = parentId,
                                        Name = childNode.Name ?? string.Format("{0} {1}", childNode.FirstName, childNode.LastName).Trim(),
                                        Share = (int)(edge.Share ?? childNode.Share ?? 0),
                                        Designation = edge.Designation ?? childNode.Designation ?? edge.Relationship ?? childNode.Relationship ?? "Related Party",
                                        Nationality = childNode.Nationality,
                                        Type = !string.IsNullOrEmpty(childNode.ShareholderType) ? childNode.ShareholderType : (rootType == "C" ? (isNodeCorporate ? "Corporate_Corp" : "Corporate_Ind") : (isNodeCorporate ? "Individual_Corp" : "Individual_Ind")),
                                        CompanyName = (isNodeCorporate) ? childNode.Name : null,
                                        RegistrationDate = (isNodeCorporate) ? childNode.RegistrationDate.ParseDB() : childNode.Dob.ParseDB(),
                                        Relationship = edge.Relationship ?? childNode.Relationship ?? "Related Party",
                                        MainPartyCode = parentId,
                                        PassportId = childNode.PassportId,
                                        EmiratesIdNumber = childNode.EmiratesIdNumber,
                                        Cif = !string.IsNullOrEmpty(childNode.Cif) ? childNode.Cif : childId,
                                        Employer = childNode.Employer,
                                        EmployerIndustry = childNode.EmployerIndustry,
                                        EmployerSector = childNode.EmployerSector,
                                        SOWSOFCountry = childNode.Sowsofcountry,
                                        Residence = childNode.Residence,
                                        PlaceOfBirth = childNode.PlaceOfBirth,

                                        FlagType = GetFlagTypeShortcode(!string.IsNullOrEmpty(childNode.FlagType) ? childNode.FlagType : (isNodeCorporate ? "CSH" : "ISH")),
                                        Gender = childNode.Gender,
                                        TradeLicenseAuthority = childNode.TradeLicenseAuthority,
                                        IsDuplicate = 0
                                    };
                                    _customerCaseService.CreateShareholdersData(shDto);
                                }
                                catch (Exception ex) { Logger.Error(ex, string.Format("Failed to create relationship for node {0}", childNode.StudioId)); }
                            }
                        }
                    }
                }

                // Phase 3: Parallel Screening (skip existing/fetched nodes — they are already screened)
                var screeningTasks = new List<Task>();
                foreach (var nr in result.NodeResults.Where(x => x.Success))
                {
                    var node = payload.Nodes.FirstOrDefault(n => n.StudioId == nr.StudioId);
                    if (node != null && !node.IsFetched)
                    {
                        var capturedNr = nr;
                        var capturedNode = node;
                        screeningTasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                string normalizedId = capturedNr.BackendId;
                                // Defensive: Ensure BackendId has prefix for service lookup
                                if (!string.IsNullOrEmpty(normalizedId) && char.IsDigit(normalizedId[0])) {
                                    var clientInfo = _customerCaseService.GetCustomerCodeprefixByclient(clientId);
                                    normalizedId = (clientInfo?.Prefix ?? "NAT") + normalizedId;
                                }

                                var dto = _customerCaseService.GetCaseFullDetailsByCustId(normalizedId);
                                if (dto != null)
                                {
                                    // Dirty Check: Skip screening if data hasn't changed
                                    if (!string.IsNullOrEmpty(dto.CaseChangeStatus) && !CaseStudioHelper.IsDirty(dto, capturedNode))
                                    {
                                        Logger.Info(string.Format("Screening skipped for {0}: Data is pristine (Dirty Check).", normalizedId));
                                        return;
                                    }

                                    bool isAll = (capturedNode.ScreeningSources == null || capturedNode.ScreeningSources.Count == 0) || capturedNode.ScreeningSources.Contains("ALL");
                                    dto.IsPep = isAll || (capturedNode.ScreeningSources?.Contains("PEP") ?? false);
                                    dto.IsSan = isAll || (capturedNode.ScreeningSources?.Contains("SAN") ?? false);
                                    dto.IsRre = isAll || (capturedNode.ScreeningSources?.Contains("RRE") ?? false);
                                    dto.IsIns = isAll || (capturedNode.ScreeningSources?.Contains("INS") ?? false);
                                    dto.IsDd = isAll || (capturedNode.ScreeningSources?.Contains("DD") ?? false);
                                    dto.IsPoi = isAll || (capturedNode.ScreeningSources?.Contains("POI") ?? false);
                                    dto.IsRel = isAll || (capturedNode.ScreeningSources?.Contains("REL") ?? false);

                                    Logger.Info(string.Format("Triggering screening for {0} (IsPep: {1}, IsSan: {2})", normalizedId, dto.IsPep, dto.IsSan));
                                    await _commonService.CustomerScreeningCall(dto, baseURL, baseC6URL, dto.MatchCategory, "", dto.Threshold, normalizedId);
                                    capturedNr.Screened = true;
                                    Logger.Info(string.Format("Screening completed for BackendID {0}", capturedNr.BackendId));
                                }
                                else
                                {
                                    Logger.Warn(string.Format("Screening skipped for BackendID {0}: Case details not found.", normalizedId));
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, string.Format("Screening failed for BackendID {0}", capturedNr.BackendId));
                                capturedNr.Error += string.Format(" [Screening Failed: {0}]", ex.Message);
                            }
                        }));
                    }
                }
                if (screeningTasks.Any()) await Task.WhenAll(screeningTasks);

                // Phase 4: Risk Assessment & Notifications (skip existing/fetched nodes)
                foreach (var nr in result.NodeResults.Where(x => x.Success))
                {
                    var node = payload.Nodes.FirstOrDefault(n => n.StudioId == nr.StudioId);
                    if (node != null && !node.IsFetched)
                    {
                        string normalizedId = nr.BackendId;
                        Logger.Info(string.Format("Node {0} (BackendId: {1}): IsRoot={2}", nr.StudioId, nr.BackendId, node?.IsRoot));
                        
                        try
                        {
                            // Dirty Check for Risk Assessment
                            var existingDto = _customerCaseService.GetCaseFullDetailsByCustId(normalizedId);
                            if (existingDto != null && (!string.IsNullOrEmpty(existingDto.FinalRiskScore) || !string.IsNullOrEmpty(existingDto.Individual_final_risk_score) || !string.IsNullOrEmpty(existingDto.corporate_final_risk_score)) && !CaseStudioHelper.IsDirty(existingDto, node))
                            {
                                Logger.Info(string.Format("Risk Assessment skipped for {0}: Data is pristine (Dirty Check).", normalizedId));
                            }
                            else
                            {
                                // Perform risk assessment for all valid nodes (Main Entities always, shareholders if requested)
                                if (node.IsRoot || node.IsMainEntity || !string.IsNullOrEmpty(node.RiskCategory))
                                {
                                    await PerformDetailedRiskAssessmentAsync(node, clientId, userId, normalizedId);
                                }
                            }

                            // Whitelist the case so it appears in the dashboard after screening/risk
                            //if (nr.NumericId > 0)
                            //{
                            //    _customerMasterRepository.UpdateWhiteList(nr.NumericId, "YES");
                            //}
                        }
                        catch (Exception ex) { Logger.Error(ex, string.Format("Risk Assessment failed for {0}", nr.BackendId)); }

                        _ = Task.Run(async () => {
                            try { await SendScreenedMailAsync(node, clientId, normalizedId, baseURL); }
                            catch (Exception ex) { Logger.Error(ex, "Email notification failed"); }
                        });
                    }
                }

                var rootNodeResult = result.NodeResults.FirstOrDefault(nr => payload.Nodes.Any(n => n.StudioId == nr.StudioId && n.IsMainEntity))
                                  ?? result.NodeResults.FirstOrDefault(nr => payload.Nodes.Any(n => n.StudioId == nr.StudioId && n.IsRoot && !n.IsFetched))
                                  ?? result.NodeResults.FirstOrDefault(nr => payload.Nodes.Any(n => n.StudioId == nr.StudioId && n.IsRoot));
                                  
                string rootId = rootNodeResult?.BackendId ?? string.Join(", ", result.NodeResults.Select(nr => nr.BackendId));
                result.Message = result.Success ? string.Format("Hierarchy processed successfully ID: {0}", rootId) : "Some nodes failed to process";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = string.Format("Critical failure in ProcessHierarchy: {0}", ex.Message);
                Logger.Error(ex, "Critical failure in ProcessHierarchyAsync");
            }
            return result;
        }

        public async Task PerformDetailedRiskAssessmentAsync(CaseStudioNode node, int clientId, int userId, string backendId)
        {
            if (node == null) return;
            Logger.Info(string.Format("Starting Risk Assessment for Node: {0}, BackendId: {1}, Type: {2}", node.Id, backendId, node.Type));

            // Risk calculation for entities

            try
            {
                var culture = "en-US";
                bool isCorporate = IsNodeCorporate(node);
                
                var typeCode = isCorporate ? "C" : "I";
                var individualKyc = !isCorporate ? _mapper.Map<KycIndividualDTO>(node) : null;
                var corporateKyc = isCorporate ? _mapper.Map<CorporateKycDTO>(node) : null;

                // Full name resolution for risk records
                var fullName = node.Name ?? string.Format("{0} {1} {2}", node.FirstName, node.MiddleName, node.LastName).Replace("  ", " ").Trim();

                // Legacy risk calculation requires both LOV IDs and Item IDs
                var riskLovResponse = _kycService.GetRiskLovId(individualKyc, corporateKyc, typeCode, culture, clientId);
                var riskItemResponse = _kycService.GetRiskTypeId(individualKyc, corporateKyc, typeCode, culture, clientId);

                Logger.Info(string.Format("Risk Service Response for {0}: LOV={1}, Items={2}", backendId, riskLovResponse?.Result ?? "NULL", riskItemResponse?.Result ?? "NULL"));

                if (riskLovResponse?.Result != null && riskItemResponse?.Result != null)
                {
                    char legacySep = (char)216; // Ø separator
                    var lovIds = riskLovResponse.Result.Split(new[] { legacySep }, StringSplitOptions.None);
                    var itemIds = riskItemResponse.Result.Split(new[] { legacySep }, StringSplitOptions.None);

                    var selectedRiskTypes = new List<Tuple<string, string>>();
                    
                    Logger.Info(string.Format("Processing risk indices for {0}. Type={1}", backendId, typeCode));
                    
                    if (isCorporate)
                    {
                        // Corporate Indices mapping
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 2), GetSafely(itemIds, 2));   // Entity Type
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 4), GetSafely(itemIds, 4));   // Nature of Business
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 3), GetSafely(itemIds, 3));   // Country of Inc
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 11), GetSafely(itemIds, 11)); // Product
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 12), GetSafely(itemIds, 12)); // Delivery Channel
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 16), GetSafely(itemIds, 16)); // Mode of Payment
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 18), GetSafely(itemIds, 18)); // Domestic PEP
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 20), GetSafely(itemIds, 20)); // Foreign PEP
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 22), GetSafely(itemIds, 22)); // Red Flags
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 24), GetSafely(itemIds, 24)); // Sanction Match
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 26), GetSafely(itemIds, 26)); // UAE/UNSC
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 27), GetSafely(itemIds, 27)); // FATF
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 29), GetSafely(itemIds, 29)); // Highest Risk Product
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 31), GetSafely(itemIds, 31)); // Very High Networth
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 32), GetSafely(itemIds, 32)); // Dual Use Goods
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 34), GetSafely(itemIds, 34)); // More Dual Use Goods
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 36), GetSafely(itemIds, 36)); // Military Goods
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 38), GetSafely(itemIds, 38)); // More Military Goods
                        // Partner Nationalities
                        for (int i = 6; i <= 10; i++)
                            AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, i), GetSafely(itemIds, i));
                    }
                    else
                    {
                        // Individual Indices mapping
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 0), GetSafely(itemIds, 0));   // Profession
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 1), GetSafely(itemIds, 1));   // Nationality
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 5), GetSafely(itemIds, 5));   // Residence
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 13), GetSafely(itemIds, 13)); // Product
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 14), GetSafely(itemIds, 14)); // Delivery Channel
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 15), GetSafely(itemIds, 15)); // Mode of Payment
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 17), GetSafely(itemIds, 17)); // Domestic PEP
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 19), GetSafely(itemIds, 19)); // Foreign PEP
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 21), GetSafely(itemIds, 21)); // Red Flags
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 23), GetSafely(itemIds, 23)); // Sanction Match
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 25), GetSafely(itemIds, 25)); // UAE/UNSC
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 28), GetSafely(itemIds, 28)); // Highest Risk Product
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 30), GetSafely(itemIds, 30)); // Very High Networth
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 33), GetSafely(itemIds, 33)); // Dual Use Goods
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 35), GetSafely(itemIds, 35)); // More Dual Use Goods
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 37), GetSafely(itemIds, 37)); // Military Goods
                        AddIfNotNull(selectedRiskTypes, GetSafely(lovIds, 39), GetSafely(itemIds, 39)); // More Military Goods
                    }

                    var riskConfig = _lovMasterService.GetAllRiskConfig(typeCode, 1, 0, 0, clientId);
                    int totalScore = 0;
                    int parameterCount = 0;
                    bool hasOverrides = false;

                    Logger.Info(string.Format("Selected Risk Types for {0}: {1} parameters found.", backendId, selectedRiskTypes.Count));

                    foreach (var selection in selectedRiskTypes)
                    {
                        if (int.TryParse(selection.Item1, out int lId) && int.TryParse(selection.Item2, out int iId) && iId > 0)
                        {
                            foreach (var cat in riskConfig)
                            {
                                var riskType = cat.RiskTypes.FirstOrDefault(t => t.Id == lId);
                                if (riskType != null)
                                {
                                    riskType.SelectedItemId = iId;
                                    var selectedItem = riskType.RiskItems.FirstOrDefault(ri => ri.Id == iId);
                                    if (selectedItem != null)
                                    {
                                        totalScore += selectedItem.RiskScore.ParseInt();
                                        if (selectedItem.OverrideScore == "3") hasOverrides = true;
                                        parameterCount++;
                                    }
                                    break;
                                }
                            }
                        }
                    }

                    // Score calculation logic from RiskAPIController
                    string riskStatus = "Low Risk";
                    string riskOverride = "";
                    
                    if (parameterCount > 0)
                    {
                        var highScoreVal = (parameterCount * 3.0);
                        var mediumScoreVal = (parameterCount * 2.24);
                        var lowScoreVal = (parameterCount * 1.49);

                        if (totalScore <= highScoreVal) riskStatus = "High Risk";
                        if (totalScore <= mediumScoreVal) riskStatus = "Medium Risk";
                        if (totalScore <= lowScoreVal) riskStatus = "Low Risk";
                    }

                    if (hasOverrides)
                    {
                        riskStatus = "High Risk";
                        riskOverride = "Override";
                    }

                    Logger.Info(string.Format("Risk Assessment Summary for {0}: ScoreSum={1}, Count={2}, Status={3}, HasOverrides={4}", 
                        backendId, totalScore, parameterCount, riskStatus, hasOverrides));

                    // Persist the risk assessment
                    if (isCorporate)
                    {
                        var riskCorp = new RiskCorpCustomerDTO
                        {
                            UniqueID = backendId,
                            LegalNameOfEntity = fullName,
                            DateofAssessment = DateTime.Now,
                            RiskAssessmentRating = riskStatus,
                            RiskAssessmentRatingWithoutOverride = hasOverrides ? "Low Risk" : riskStatus, // Simplified fallback
                            RiskOverRide = riskOverride,
                            RiskScoreSum = totalScore,
                            RiskScoreCount = parameterCount,
                            ClientId = clientId,
                            CreatedBy = userId,
                            CountryOfIncorporationTxt = node.Nationality,
                            version = 1, // Use version 2 for Studio to ensure parity
                            CaseVersion = 1,
                            RiskTypeCategoryDTO = riskConfig
                        };
                        _riskService.CreateCorpCustomerRisk(riskCorp);
                    }
                    else
                    {
                        var riskInd = new RiskDTO
                        {
                            CustomerCode = backendId,
                            CustomerName = fullName,
                            DateofAssessment = DateTime.Now,
                            FinalRiskScore = riskStatus,
                            RiskScoreBeforeOverride = hasOverrides ? "Low Risk" : riskStatus, // Simplified fallback
                            RiskOverRide = riskOverride,
                            RiskScoreSum = totalScore,
                            RiskScoreCount = parameterCount,
                            ClientId = clientId,
                            CreatedBy = userId,
                            MainNationalityTxt = node.Nationality,
                            CustomerType = typeCode,
                            Remarks = "Case Studio Risk Assessment",
                            Type = isCorporate ? "Corporate" : "Individual",
                            version = 1,
                            CaseVersion = 1,
                            RiskTypeCategoryDTO = riskConfig
                        };
                        _riskService.Create(riskInd);
                    }
                }
            }
            catch (Exception ex) { Logger.Error(ex, string.Format("Detailed Risk Assessment failed for {0}", backendId)); }
        }

        private string GetSafely(string[] array, int index)
        {
            if (array == null || index < 0 || index >= array.Length) return "0";
            return array[index];
        }

        private void AddIfNotNull(List<Tuple<string, string>> list, string lovId, string itemId)
        {
            if (lovId != "0" && itemId != "0")
            {
                list.Add(new Tuple<string, string>(lovId, itemId));
            }
        }


        private bool IsNodeCorporate(CaseStudioNode node)
        {
            if (node == null) return false;

            // Priority 1: Direct Type Indicator ('C' or 'I')
            string t = node.Type?.ToUpper() ?? "";
            if (t == "C" || t == "CORPORATE") return true;
            if (t == "I" || t == "INDIVIDUAL") return false;

            // Priority 2: Shareholder Type context labels (e.g. Corporate_Ind means it is an Individual)
            if (t.Contains("_"))
            {
                if (t.EndsWith("_CORP")) return true;
                if (t.EndsWith("_IND")) return false;
            }

            // Priority 3: Fallback to ShareholderType for legacy payloads
            if (!string.IsNullOrEmpty(node.ShareholderType))
            {
                string st = node.ShareholderType.ToUpper();
                if (st == "CORPORATE" || st.EndsWith("_CORP")) return true;
                if (st == "INDIVIDUAL" || st.EndsWith("_IND")) return false;
            }

            return false;
        }


        private async Task SendScreenedMailAsync(CaseStudioNode node, int clientId, string backendId, string baseURL)
        {
            try
            {
                var caseDetails = _customerCaseService.GetCaseFullDetailsByCustId(backendId);
                if (caseDetails == null) return;

                var clientDetails = _customerCaseService.GetClientDetailsByID(clientId);
                var clientName = clientDetails?.ClientName ?? "Client";
                
                string body = "";
                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views/Risk/RiskEmailBody.html");
                
                if (File.Exists(templatePath)) body = File.ReadAllText(templatePath);
                else body = "<h3>Screening Notification</h3><p>Case {NAME} ({ID}) has been screened.</p>";

                body = body.Replace("{NAME}", node.Name)
                           .Replace("{ID}", backendId)
                           .Replace("{TYPE}", node.Type)
                           .Replace("{DATE}", DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                           .Replace("{URL}", baseURL);

                string recipientEmail = !string.IsNullOrEmpty(caseDetails.UserEmail) ? caseDetails.UserEmail : "compliance@aml-internal.com";
                _commonService.SendEmail(new EmailModel(recipientEmail, "Case Screening Notification", body, true));
                Logger.Info(string.Format("Screening email sent for {0} to {1}", backendId, recipientEmail));
            }
            catch (Exception ex) { Logger.Error(ex, string.Format("Failed to send screening email for {0}", backendId)); }
        }

        private CaseStudioNode FindRootForNode(CaseStudioNode node, CaseStudioPayload payload)
        {
            var roots = FindAllRootsForNode(node, payload);
            return roots.FirstOrDefault();
        }

        private List<CaseStudioNode> FindAllRootsForNode(CaseStudioNode node, CaseStudioPayload payload)
        {
            var roots = new List<CaseStudioNode>();
            if (node.IsRoot) 
            {
                roots.Add(node);
                return roots;
            }

            var queue = new Queue<CaseStudioNode>();
            var visited = new HashSet<string>();
            queue.Enqueue(node);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (visited.Contains(current.StudioId)) continue; // Prevent cycles
                visited.Add(current.StudioId);

                if (current.IsRoot)
                {
                    roots.Add(current);
                    continue;
                }

                var parentEdges = payload.Edges?.Where(e => e.To == current.StudioId).ToList();
                if (parentEdges != null && parentEdges.Any())
                {
                    foreach (var edge in parentEdges)
                    {
                        var parentNode = payload.Nodes.FirstOrDefault(n => n.StudioId == edge.From);
                        if (parentNode != null && !visited.Contains(parentNode.StudioId))
                        {
                            queue.Enqueue(parentNode);
                        }
                    }
                }
            }
            return roots.Distinct().ToList();
        }

        public async Task<string> SaveCaseDraftAsync(int clientId, int userId, CaseStudioPayload payload)
        {
            return await Task.Run(() =>
            {
                try
                {
                    if (payload == null) return null;
                    if (string.IsNullOrEmpty(payload.DraftId))
                    {
                        payload.DraftId = Guid.NewGuid().ToString();
                    }
                    if (string.IsNullOrEmpty(payload.DraftName))
                    {
                        payload.DraftName = "Draft - " + DateTime.Now.ToString("MMM dd, yyyy HH:mm");
                    }
                    string draftJson = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
                    int nodeCount = payload.Nodes?.Count ?? 0;
                    int edgeCount = payload.Edges?.Count ?? 0;
                    var response = _customerMasterRepository.SaveCaseDraft(clientId, userId, payload.DraftId, payload.DraftName, nodeCount, edgeCount, draftJson);
                    return response.Result ? payload.DraftId : null;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to save case draft in CaseStudioService");
                    return null;
                }
            });
        }

        public async Task<IEnumerable<CaseStudioDraftSummaryDTO>> GetCaseDraftSummariesAsync(int clientId, int userId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var response = _customerMasterRepository.GetCaseDraftSummaries(clientId, userId);
                    if (response.Status == StaticResource.SuccessStatusCode)
                    {
                        return response.Result;
                    }
                    return new List<CaseStudioDraftSummaryDTO>();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to get case draft summaries in CaseStudioService");
                    return new List<CaseStudioDraftSummaryDTO>();
                }
            });
        }

        public async Task<CaseStudioPayload> GetCaseDraftAsync(int clientId, int userId, string draftId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var response = _customerMasterRepository.GetCaseDraft(clientId, userId, draftId);
                    if (response.Status == StaticResource.SuccessStatusCode && !string.IsNullOrEmpty(response.Result))
                    {
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<CaseStudioPayload>(response.Result);
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to get case draft in CaseStudioService");
                    return null;
                }
            });
        }

        public async Task<bool> DeleteCaseDraftAsync(int clientId, int userId, string draftId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var response = _customerMasterRepository.DeleteCaseDraft(clientId, userId, draftId);
                    return response.Result;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to delete case draft in CaseStudioService");
                    return false;
                }
            });
        }

        public async Task<bool> RenameCaseDraftAsync(int clientId, int userId, string draftId, string newDraftName)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var response = _customerMasterRepository.RenameCaseDraft(clientId, userId, draftId, newDraftName);
                    return response.Result;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to rename case draft in CaseStudioService");
                    return false;
                }
            });
        }

        private static string GetFlagTypeShortcode(string flag)
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
    }

    public static class StringExtensions
    {
        public static int ParseInt(this string data)
        {
            if (string.IsNullOrEmpty(data)) return 0;
            var numericPart = Regex.Match(data, @"\d+").Value;
            int.TryParse(numericPart, out int val);
            return val;
        }
    }
}
