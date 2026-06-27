# KG CHANGELOG — Lemon (Watchdog AML)

## 2026-04-16 | Commit: fafc8ee | KG_CREATE

**Action:** Initial KG build from scratch.

**Entities created:** 40+
- All major domain modules mapped
- Controller → Service → Repository → DB relationships fully traced
- Infrastructure (MySQL, MongoDB, C6 API, Ollama, Screening API) documented
- Key Views (Process.cshtml, BlockListUpload.cshtml, Edit.cshtml, Layout) mapped
- 7 Architecture Decision Records (ADRs) captured

**Why:** KG directory existed but was empty. Full scan performed from Startup.cs, appsettings.json, directory structure, and git log.

---

## 2026-04-17 | Commit: 59255ea | KG_SYNC

**New commits since last check (fafc8ee):**
| SHA | Message |
|-----|---------|
| `59255ea` | Enhanced expansion/collapse of dropdowns in sidebar |
| `79212ec` | Added UN-Former when status is D |
| `9c3c489` | Resolved conflicts |
| `9c8e254` | Removed console name in jquery |

**Changed files:**
| File | Change |
|------|--------|
| `AML.Core.Repository/FreeSource/FreeSourceRepository.cs` | FreeSource repo updated |
| `AML.Core.Service/Common/CommonService.cs` | CommonService updated (MongoDB TransactionCaseLog, new DTOs) |
| `AML.DTO/DTO/FreeSource/BlackListMongoDTO.cs` | New MongoDB DTO for blocklist |
| `AML.DTO/DTO/FreeSource/CaseLogsMongoDTO.cs` | New MongoDB DTO for case logs |
| `AML.ViewModel/ViewModels/CustomerCase/DataListModel.cs` | Added RiskAssessmentDto nested class, searchTypes, Status |
| `AML.Web/Controllers/RiskAPIController.cs` | Risk API controller updated |
| `AML.Web/Views/Case/Process.cshtml` | Passport/EID fields added to detail modal |
| `AML.Web/Views/SanctionScreening/Index.cshtml` | Sanction screening view updated |
| `AML.Web/Views/Shared/AdminLTE/_Layout.cshtml` | Sidebar accordion expand/collapse enhanced |
| `AML.Web/appsettings.json` | Config updated |

**NEW entities detected:**
- `Shareholder_PDF.cshtml` — PDF export for shareholder case (Tailwind + html2pdf.js, no layout)
- `ProcessNew.cshtml` — Legacy AdminLTE process view (was present but not tracked)
- `BlackListMongoDTO` / `CaseLogsMongoDTO` — new Mongo DTOs in AML.DTO

**Build fix applied:**
- **CS1503 (16 errors)** in `Process.cshtml` (L1372, 1376, 1388, 1392) and `ProcessNew.cshtml` (L131, 132, 138, 139)
- **Root cause:** Razor compiler misinterpreting `? "-"` ternary as nullable operator in C# nullable context
- **Fix:** Added explicit `(string)` cast → `? (string)"-"` to disambiguate
- **Result:** Build succeeded. 0 errors.

---

## 2026-04-17 | Commit: ba7b6db | KG_SYNC

**New commits since last check (59255ea → ba7b6db):**

| SHA | Message |
|-----|---------|
| `7712b83` | Refined to allow both Passport and EID can be selected and shown in UI |
| `584fed4` | Refined to allow multiple selection of files |
| `8326377` | Fixed the version of Alpine.js |
| `1ee69b8` | Added Remarks model for SM, Column to Related Parties Table; Fixed Datepicker in Case/Create |
| `7f3336a` | Added `*_Poa` for Authorised Signitories |
| `ba7b6db` | Added `*_Poa` for Authorised Signitories (duplicate / merge) |

**Changed files (15 total):**

| File | Layer | Change |
|------|-------|--------|
| `AML.Core.Repository/CustomerCase/CustomerMasterRepository.cs` | Repository | Updated — likely Poa-related query changes |
| `AML.DTO/DTO/CustomerCase/CustomerCaselDTO.cs` | DTO | `ShareholderDTO` — confirmed already has `PassportIssueDate`, `EmiratesIdIssueDate`. No new fields noted. |
| `AML.ViewModel/ViewModels/Corporate/CorporateScreeningModel.cs` | ViewModel | `CorporateScreeningModel` — stable, no new top-level fields |
| `AML.ViewModel/ViewModels/CustomerCase/CaseModel.cs` | ViewModel | `CaseModel` stable — 263 lines, no new Poa fields yet (Poa likely in Create/Corporate views only) |
| `AML.Web/Controllers/Corporate/CorporateController.cs` | Controller | Poa action handling added |
| `AML.Web/Views/Case/Create.cshtml` | View | Datepicker fix + `*_Poa` fields for Authorised Signatories (both commits) |
| `AML.Web/Views/Case/Process.cshtml` | View | Remarks model for Senior Management; Passport+EID dual-select UI |
| `AML.Web/Views/Case/ProcessNew.cshtml` | View | Minor UI update |
| `AML.Web/Views/Case/Process_PDF.cshtml` | View | SM Remarks column added |
| `AML.Web/Views/Case/Shareholder.cshtml` | View | Related Parties table — new column for remarks/SM |
| `AML.Web/Views/Case/Shareholder_PDF.cshtml` | View | PDF export view (previously new — now updated) |
| `AML.Web/Views/Corporate/CorporateScreening.cshtml` | View | `*_Poa` fields for Authorised Signatories added |
| `AML.Web/Views/Report/ViewIndividualCaseDetail.cshtml` | View | Remarks column display updated |
| `AML.Web/Views/Report/ViewIndividualCaseDetail_PDF.cshtml` | View | PDF — Remarks column |
| `AML.Web/Views/Shared/AdminLTE/_Layout.cshtml` | View | Touched (likely no-op) |

**Key schema/model changes:**
- `CaseModel` → No new server-side fields; Poa fields appear to be client-side view-only bindings
- `ShareholderDTO` → Already had full Passport/EID date fields (`PassportIssueDate`, `PassportExpiryDate`, `EmiratesIdIssueDate`, `EmiratesIdExpiryDate`) as `DateTime?`
- `CustomerMasterDTO` → Same full ID field set confirmed
- `CaseModel.PassportIssueDate` / `EmiratesIdIssueDate` = `string` (view-layer format, not `DateTime?`)

**State:** Build running (`dotnet run` active on port). No compile errors expected.

---

## 2026-05-15 | Commit: e27e617 | KG_SYNC

**Changed files:**
| File | Layer | Change |
|------|-------|--------|
| `AML.DTO/DTO/CustomerCase/CustomerCaselDTO.cs` | DTO | Added `ShareholderType` property to `CustomerCaseDTO` |

**Build fix applied:**
- **CS1061 (3 errors)** in `CaseStudioService.cs` (L257, 280, 287)
- **Root cause:** `CustomerCaseDTO` was missing `ShareholderType` definition required by Case Studio hierarchy resolution logic
- **Fix:** Added `public string ShareholderType { get; set; }` to `CustomerCaseDTO` class
- **Result:** Build succeeded. 0 errors.

---

## 2026-05-18 | Commit: a453f22 | KG_SYNC

**Changed files:**
| File | Layer | Change |
|------|-------|--------|
| `AML.Web/Controllers/Corporate/CorporateController.cs` | Controller | Refactored `MapLegacyToStudioPayload` to replace hardcoded `IsScreened = true` with dynamic checks |

**Bug fix applied:**
- **Issue:** Unscreened nodes were appearing with the screened flag on draft restoration.
- **Root cause:** `MapLegacyToStudioPayload` was hardcoding `IsScreened = true` for all corporate and shareholder nodes.
- **Fix:** Implemented dynamic checks (`!string.IsNullOrEmpty(corp.ApiResultJsonCorp)` for corporate nodes and `!string.IsNullOrEmpty(sh.CustomerId)` for shareholder nodes).
- **Result:** Unscreened nodes correctly maintain their unscreened status on draft restoration.

---

## 2026-05-18 | Commit: f96000a | KG_SYNC

**Changed files:**
| File | Layer | Change |
|------|-------|--------|
| `AML.Web/wwwroot/js/studio.js` | Frontend | Fixed hardcoded `true` `isScreened` parameter in `getSvg` call during draft restoration |

**Bug fix applied:**
- **Issue:** Unscreened nodes were still appearing with the screened lock icon on the canvas after draft restoration, despite backend payload having `isScreened: false`.
- **Root cause:** `restoreSpecificDraft` in `studio.js` was passing `true` hardcoded as the third argument (`isScreened`) to `this.getSvg(...)` when generating the node's SVG image.
- **Fix:** Replaced hardcoded `true` with `!!(nodeDto.isScreened || nodeDto.IsScreened)` in the `this.getSvg(...)` call.
- **Result:** Unscreened nodes now correctly render without the green lock icon upon draft restoration.

---

## 2026-05-18 | Commit: pending | KG_SYNC

**Changed files:**
| File | Layer | Change |
|------|-------|--------|
| `AML.Core.ServiceContract/CaseStudio/ICaseStudioService.cs` | ServiceContract | Added `CaseId` property to `CaseStudioNode` DTO |
| `AML.Web/wwwroot/js/studio.js` | Frontend | Added `CaseId` mapping to `mapNodeToDto` |

**Bug fix applied:**
- **Issue:** The `caseId` property of canvas nodes (`S_*`) was becoming `0` upon draft restoration.
- **Root cause:** `CaseStudioNode` in C# lacked a `CaseId` property, causing `mapNodeToDto` to omit `node.caseId`. Upon restoration of existing drafts, `nodeDto.caseId` evaluated to empty, and `openNode` incorrectly fell back to `node.databaseId` (`0`) before checking canvas `data.id`.
- **Fix:** Added `CaseId` to `CaseStudioNode` in C#, mapped `CaseId` in `mapNodeToDto`, added `StudioId` fallback in `restoreSpecificDraft`, and updated `openNode` to ignore `databaseId` when it is `0`.
- **Result:** Canvas nodes correctly preserve their `caseId` across draft save and restore cycles without resetting to `0` for both new and legacy drafts.

---

## 2026-05-18 | Commit: pending | KG_SYNC

**Changed files:**
| File | Layer | Change |
|------|-------|--------|
| `AML.Web/wwwroot/js/studio.js` | Frontend | Refactored `saveCase` payload generation to expand multi-parented nodes and their descendants |

**Feature / Enhancement applied:**
- **Issue:** Nodes in Case Studio with multiple parents (e.g. node C attached to both A and B) were only creating/updating a single database record under whichever parent edge was processed last.
- **Root cause:** Canvas nodes exist as single entities. `saveCase` mapped canvas nodes 1:1 to DTOs, resulting in a single DTO in the batch creation payload.
- **Fix:** Refactored `saveCase` to perform a root-to-leaf graph traversal, cloning any multi-parented nodes (and their descendants) into separate DTOs with unique `StudioId` suffixes (`_parent_{id}`) and setting `Id = '0'`, `IsFetched = false`, and `IsRoot = false` for duplicate instances to ensure separate database records are created for each parent branch with correct hierarchical specifications (`CompanyCode`, `ShareholderType`).
- **Result:** Multi-parented nodes correctly generate distinct cases under each respective parent in the backend database (even when sharing an existing/fetched canvas node).

---

## 2026-05-26 | Commit: abc6743 | KG_SYNC

**Changed files:**
| File | Layer | Change |
|------|-------|--------|
| `AML.Web/wwwroot/js/studio.js` | Frontend | Refactored `updateHierarchyTypes`, `createEdge`, and edge deletion methods to support proper badge rendering on multi-parent and root nodes |

**Bug fix applied:**
- **Issue 1:** CSH badge appearing on Main Corp (root node).
- **Issue 2:** AS and ISH badges only appearing in multi-parent nodes after page reload.
- **Fix:** 
  1. Updated `updateHierarchyTypes` to calculate the full array of relationships `relsToPass` (using incoming edges and falling back to `node.relationship` only if non-root and parentless), preventing it from overwriting multi-relationship badges with a single badge.
  2. Updated `createEdge` to correctly run `updateHierarchyTypes` on the child (or the parent if promoted to root).
  3. Added `updateHierarchyTypes` calls during edge deletion to refresh the canvas visuals immediately.
- **Result:** Badges render correctly on root nodes and update immediately without requiring page reloads.

---

## 2026-05-27 | Commit: pending | KG_SYNC

**Changed files:**
| File | Layer | Change |
|------|-------|--------|
| `AML.Core.Repository/CustomerCase/CustomerMasterRepository.cs` | Repository | Updated `UpdateGroupInfo` and `UpdateHierarchyInfo` to save `groupEntityOf` to the `customermaster` database table |
| `AML.Core.RepositoryContract/CustomerCase/ICustomerMasterRepository.cs` | RepositoryContract | Updated method signatures for `UpdateGroupInfo` and `UpdateHierarchyInfo` to accept `groupEntityOf` parameter |
| `AML.Core.Service/CaseStudio/CaseStudioService.cs` | Service | Passed `groupEntityOf` and `dto.GroupEntityof` to repository update methods |
| `AML.Core.Repository/CustomerCase/CustomerCaseRepository.cs` | Repository | Selected `cu.group_entity_of` in SQL queries in `GetCaseFullDetailsByCustomerId` (string/int) and `GetCasesByGroupId` |
| `get_customercase_full_details_by_id` | DB Stored Proc | Updated MySQL stored procedure to retrieve `group_entity_of` |

**Bug fix applied:**
- **Issue:** Grouping label (e.g. "Lemon Deal 1") not saved or shown between A En and B En.
- **Root cause:** `group_entity_of` column in `customermaster` table was neither written/updated during case updates nor fetched from the database in repository queries.
- **Fix:**
  1. Modified `UpdateGroupInfo` and `UpdateHierarchyInfo` methods to save `groupEntityOf` parameter to `customermaster.group_entity_of`.
  2. Modified SQL queries in `CustomerCaseRepository` (`GetCaseFullDetailsByCustomerId`, `GetCasesByGroupId`) and stored procedure `get_customercase_full_details_by_id` to select `cu.group_entity_of`.
  3. Manually updated `customermaster` records for A En (`NAT71`) and B En (`NAT78`) to set `group_entity_of = 'Lemon Deal 1'`.
- **Result:** Grouping is correctly saved in the DB and displayed between A En and B En with label "Lemon Deal 1".


