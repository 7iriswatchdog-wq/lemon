# Sectoral Transaction Monitoring System (STM)

**Version:** 1.0
**Module:** Insurance & Real Estate AML Transaction Monitoring
**Base URL (Web):** `/stm`
**Base URL (API):** `/api/stm`

---

## 1. Overview

The Sectoral Transaction Monitoring System (STM) is an industry-specific AML/CFT transaction monitoring module. It is purpose-built for the **Insurance** and **Real Estate** sectors and operates alongside the existing generic Transaction Monitoring System (TMS).

The module is organised into four functional areas that share the same UI template:

| Area              | Web Route                  | Purpose                                                     |
|-------------------|----------------------------|-------------------------------------------------------------|
| **Rules**         | `/stm/rules`               | Create, edit, activate / deactivate monitoring rules        |
| **Transactions**  | `/stm/transactions`        | Enter & list transactions; auto-evaluate all active rules   |
| **Open Cases**    | `/stm/cases/open`          | Cases auto-created when a rule hits a transaction           |
| **Reviewed Cases**| `/stm/cases/completed`     | Cases that have been approved, rejected, escalated or closed|
| **Reports**       | `/stm/reports`             | Per-customer transaction & case reports                     |

---

## 2. Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│  Web (Razor + Tailwind + AdminLTE template)                          │
│  /stm    /stm/rules    /stm/transactions    /stm/cases/*  /stm/reports│
└─────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│  Controllers                                                          │
│    StmController        - MVC pages                                  │
│    StmApiController     - REST API   (/api/stm/*)                    │
└─────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│  Service Layer                                                        │
│    IStmService → StmService          (orchestration)                  │
│    StmRulesEngine                    (dynamic rule evaluation)        │
└─────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│  Repository Layer (Dapper)                                            │
│    IStmRepository → StmRepository                                     │
└─────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│  MySQL (database: lemon_uat)                                          │
│    stm_sector  stm_rule  stm_rule_condition                           │
│    stm_transaction  stm_transaction_party                             │
│    stm_case  stm_case_comment  stm_rule_exec_log                      │
└─────────────────────────────────────────────────────────────────────┘
```

### Project Layout

```
AML.DTO/DTO/SectoralTMS/                   - StmSectorDTO, StmRuleDTO, StmTransactionDTO, StmCaseDTO
AML.ViewModel/ViewModels/SectoralTMS/      - StmRuleModel, StmTransactionModel, StmCaseModel, ...
AML.Core.RepositoryContract/SectoralTMS/   - IStmRepository
AML.Core.Repository/SectoralTMS/           - StmRepository (Dapper, MySQL)
AML.Core.ServiceContract/SectoralTMS/      - IStmService
AML.Core.Service/SectoralTMS/              - StmService, StmRulesEngine
AML.Web/Controllers/SectoralTMS/           - StmController, StmApiController
AML.Web/Views/SectoralTMS/                 - Index, Rules, RuleEditor, CreateTransaction,
                                              Transactions, ViewTransaction, CaseList,
                                              ViewCase, Reports, _StmLayout
AML.Web/Mappings/StmMappingProfile.cs      - AutoMapper DTO <-> ViewModel
Db/Migrations/STM/                         - SQL schema + seed scripts
```

---

## 3. Installation & Setup

### 3.1 Run database migrations

Run, in this order, against the configured MySQL instance (default `lemon_uat`):

```bash
mysql -u root -p lemon_uat < Db/Migrations/STM/01_stm_schema.sql
mysql -u root -p lemon_uat < Db/Migrations/STM/02_stm_seed_sectors.sql
mysql -u root -p lemon_uat < Db/Migrations/STM/03_stm_seed_rules.sql
```

The first script creates the eight STM tables. The seed scripts populate the
two sectors and sixteen industry-standard rules (eight Insurance + eight Real
Estate).

### 3.2 DI registration

`StmService` and `StmRepository` are registered in `AML.Web/Startup.cs` under
the existing dependency-injection block:

```csharp
services.AddTransient<IStmService, StmService>();
services.AddScoped<IStmRepository, StmRepository>();
```

### 3.3 Sidebar navigation

A "Sectoral TMS" entry is wired into the AdminLTE sidebar at
`AML.Web/Views/Shared/AdminLTE/_Layout.cshtml`. The active state is computed
from the URL prefix `/stm`.

---

## 4. Database Schema

### stm_sector
Sector master. Seeded with `INS` (Insurance) and `RE` (Real Estate).

### stm_rule
Master rule definition. Each rule belongs to a sector and has a risk rating,
a score, and a logical operator (AND / OR) that joins its conditions.

| Column          | Notes                                          |
|-----------------|------------------------------------------------|
| rule_code       | Unique business code (per client)              |
| rule_name       | Human-readable name                            |
| sector_id       | FK to stm_sector                               |
| risk_rating     | `High` / `Medium` / `Low`                      |
| rule_score      | 1-100, summed for total case score             |
| logical_operator| `AND` / `OR` - fallback when condition row uses an inconsistent conjunction |
| action_on_hit   | `CREATE_CASE` / `FLAG` / `BLOCK`               |
| is_active       | 0 = inactive, 1 = active                       |
| is_system_rule  | 1 = seeded (cannot be deleted via UI)          |

### stm_rule_condition
One rule has many conditions. Each row is a single predicate:

```
<field_name> [<aggregation>] <operator> <compare_value> [within <timeframe>]
```

Joined with the next condition by the row's `conjunction` (AND / OR).

| Column            | Notes                                          |
|-------------------|------------------------------------------------|
| field_name        | DTO field, e.g. `Amount`, `TranType`, `BeneficiaryCountry` |
| field_aggregation | `Sum` / `Count` / `Avg` / null                 |
| operator          | `equals`, `gt`, `gte`, `lt`, `lte`, `contains`, `in`, `not_in`, `in_high_risk_list`, `between`, `not_equals_field`, ... |
| compare_value     | Single value or comma-separated list           |
| compare_field     | Used by `not_equals_field` operator            |
| timeframe_value   | Optional numeric window                        |
| timeframe_unit    | `Day` / `Week` / `Month` / `Year`              |
| conjunction       | `AND` / `OR` - applied to the NEXT condition   |

### stm_transaction
One row per submitted transaction. Common AML fields plus sector-specific
columns: `policy_no` (Insurance) and `property_ref` / `property_value` (Real
Estate).

### stm_transaction_party
Multi-party support. When a transaction involves multiple parties (e.g. joint
policyholders, co-buyers), each party gets a row with their role
(PolicyHolder / Buyer / Nominee / Agent / ...) and link to the customer master.

### stm_case
Auto-created when one or more rules hit a transaction.

| Column            | Notes                                          |
|-------------------|------------------------------------------------|
| case_ref_no       | Format `STM-YYYYMMDD-NNNNNN`                   |
| transaction_id    | FK to stm_transaction                          |
| rules_violated    | JSON array of rule IDs                         |
| rule_names        | Denormalised comma-separated for UI            |
| total_risk_score  | Sum of `rule_score` for all hit rules          |
| risk_rating       | Highest risk among hit rules                   |
| status            | `OPEN` / `IN_REVIEW` / `APPROVED` / `REJECTED` / `ESCALATED` / `CLOSED` |
| review_decision   | `TRUE_POSITIVE` / `FALSE_POSITIVE` / `ESCALATE`|

### stm_case_comment
Activity log: case creation, status changes, reviewer comments.

### stm_rule_exec_log
Per-rule execution log per transaction (debugging / regulator audit).

---

## 5. Rules Engine

### 5.1 Operators

| Operator              | Behaviour                                                       |
|-----------------------|-----------------------------------------------------------------|
| `equals` / `eq`       | Case-insensitive string equality                                |
| `not_equals` / `neq`  | Negated `equals`                                                |
| `not_equals_field`    | Compares `field_name` against another transaction field (`compare_field`) - e.g. "Remitter ID ≠ Customer ID" |
| `gt`, `gte`, `lt`, `lte` | Numeric comparison (decimal-safe)                            |
| `contains`            | Substring match (case-insensitive)                              |
| `not_contains`        | Negated `contains`                                              |
| `in`                  | Value in comma/semicolon/pipe-separated list                    |
| `not_in`              | Value not in list                                               |
| `in_high_risk_list`   | ISO-2 country code is in the built-in high-risk country set     |
| `between`             | Value in numeric range (`"100,1000"` or `"100-1000"`)           |

### 5.2 Condition Joining

Each condition row has a `conjunction` (`AND` or `OR`) that applies to the
**next** row. Evaluation is strictly left-to-right; standard precedence is not
applied. Example - rule with three conditions:

```
C1 AND C2 OR C3      ⇒   (C1 AND C2) OR C3
```

To force priority, split the rule into two rules.

### 5.3 Adding a custom field

1. Add a column to `stm_transaction` and the matching property on `StmTransactionDTO`.
2. (Optional) Add an alias in `StmRulesEngine.AliasMap` if you want the rule
   editor to use a friendly name.
3. The field becomes immediately available to all rules without code changes -
   that's what makes the engine "dynamic".

### 5.4 Adding a custom operator

Edit `StmRulesEngine.EvaluateCondition` and add a new `case` branch. Return
`bool` and set the `out reason` for traceability.

---

## 6. Creating Rules (Step-by-Step)

1. Navigate to **Sectoral TMS → Rules → New Rule**.
2. Fill in the header fields:
   - **Rule Code**: short business code, e.g. `INS-R009`
   - **Sector**: Insurance or Real Estate
   - **Risk Rating**: High / Medium / Low (drives the case's risk badge)
   - **Score**: 1-100 (summed across hit rules → total case score)
   - **Rule Name** and **Description**
   - **Combine conditions using**: AND or OR (controls the overall logical operator)
   - **Action on hit**: usually `CREATE_CASE`
3. Add one or more conditions. For each:
   - Pick a **Field** (transaction, customer, counter-party, or sector-specific field)
   - Pick an **Operator** (see [§5.1](#51-operators))
   - Enter a **Value** (or a comma-separated list for `in`/`not_in`)
   - (Optional) Add a **Timeframe** for aggregation rules
   - Pick a **Join** (AND / OR) - applied to the NEXT condition
4. Click **Save Rule**. Rule is active immediately for new transactions.

### Example: "Cash Premium Above AED 55,000"

| Field        | Operator | Value | Join |
|--------------|----------|-------|------|
| TranType     | equals   | Premium | AND |
| TranMode     | equals   | Cash    | AND |
| Amount       | gte      | 55000   | -   |

Combine: AND | Action: CREATE_CASE | Risk: High | Score: 75

---

## 7. Transaction Entry (Step-by-Step)

1. Navigate to **Sectoral TMS → Transactions → New Transaction**.
2. **Section 1 - Transaction Details**:
   - Pick the sector
   - Enter a unique reference number
   - Pick transaction date, type, mode, channel
   - Enter amount and currency
3. **Section 2 - Customer Selection**:
   - Type at least 2 characters in the search box. The system queries the
     existing `customermaster` table and shows a dropdown.
   - Click a result. The system calls `/stm/customers/details/{customerId}`
     and populates all the customer info (nationality, ID, mobile, profession,
     source of funds, policy number, risk score, ...) in a card.
   - If the case involves multiple parties, switch the radio to "Multiple
     parties". A list appears - click "Add Party" for each party. Each row
     has its own search box to pull a different customer from the master.
4. **Section 3 - Counter-party / Sector Details**: remitter, beneficiary,
   countries, policy number, property reference, etc.
5. Click **Submit & Run Rules**.
   - The transaction is stored.
   - All active rules for the selected sector are run.
   - If any rule hits, a case is auto-created and shown in the result panel
     along with a link to the case.
   - The trace of every condition is shown for transparency.

---

## 8. Case Workflow

### 8.1 Statuses

```
OPEN ────► IN_REVIEW ────► APPROVED   (cleared, false positive or low risk)
                       │
                       ├──► REJECTED   (blocked / refused)
                       │
                       ├──► ESCALATED  (sent to senior compliance)
                       │
                       └──► CLOSED     (resolved without action)
```

### 8.2 Reviewing a Case

1. Go to **Open Cases**.
2. Click any case.
3. Read the violated rules, transaction detail, parties.
4. In the **Decision** panel:
   - Decision: TRUE_POSITIVE / FALSE_POSITIVE / ESCALATE
   - Final Status: APPROVED / REJECTED / ESCALATED / CLOSED
   - Remarks: investigation notes (required)
5. Submit. The case moves to **Reviewed Cases** and an audit row is added.
6. Additional comments can be added at any time and appear in the activity log.

---

## 9. REST API

All endpoints require a valid JWT (`[JwtAuthorize]`). Base path: `/api/stm`.

### 9.1 Sectors

| Method | Path                          | Body | Returns |
|--------|-------------------------------|------|---------|
| GET    | `/api/stm/sectors?clientId=1` | -    | `List<StmSectorDTO>` |

### 9.2 Rules

| Method | Path                                       | Body                          | Returns |
|--------|--------------------------------------------|-------------------------------|---------|
| GET    | `/api/stm/rules?clientId=1&sectorCode=INS` | -                             | `List<StmRuleDTO>` |
| GET    | `/api/stm/rules/{id}`                      | -                             | `StmRuleDTO` (with conditions) |
| POST   | `/api/stm/rules`                           | `StmRuleDTO` (with conditions)| `{ Result: id }` |
| PUT    | `/api/stm/rules/{id}`                      | `StmRuleDTO`                  | `{ Result: rowsAffected }` |
| PATCH  | `/api/stm/rules/{id}/status`               | `{ isActive: 0|1, userId }`   | `{ Result: rowsAffected }` |
| POST   | `/api/stm/rules/test`                      | `{ Rule, Transaction }`       | Evaluation trace |

#### Example - Create Rule

```http
POST /api/stm/rules
Authorization: Bearer <JWT>
Content-Type: application/json

{
  "RuleCode": "INS-R099",
  "RuleName": "VIP Cash Transactions",
  "RuleDescription": "Cash transactions by VIP customers > 25,000",
  "SectorId": 1,
  "RiskRating": "High",
  "RuleScore": 70,
  "LogicalOperator": "AND",
  "ActionOnHit": "CREATE_CASE",
  "IsActive": 1,
  "ClientId": 1,
  "CreatedBy": 12,
  "Conditions": [
    { "SequenceNo": 1, "FieldName": "TranMode", "Operator": "equals", "CompareValue": "Cash", "Conjunction": "AND" },
    { "SequenceNo": 2, "FieldName": "Amount",   "Operator": "gte",    "CompareValue": "25000" }
  ]
}
```

### 9.3 Transactions

| Method | Path                                                | Body                | Returns                |
|--------|-----------------------------------------------------|---------------------|------------------------|
| POST   | `/api/stm/transactions?userId=12`                   | `StmTransactionDTO` | `StmTransactionResultDTO` (transactionId, caseId, ruleResults) |
| POST   | `/api/stm/transactions/search`                      | `StmTransactionSearchDTO` | `List<StmTransactionDTO>` |
| GET    | `/api/stm/transactions/{id}`                        | -                   | `StmTransactionDTO` |
| GET    | `/api/stm/transactions/customer/{custId}?clientId=1`| -                   | `List<StmTransactionDTO>` |

#### Example - Submit Transaction

```http
POST /api/stm/transactions?userId=12
Authorization: Bearer <JWT>
Content-Type: application/json

{
  "TranRefNo": "TXN-2026-00099",
  "SectorId": 2,
  "TranDate": "2026-05-21T14:30:00",
  "TranType": "Purchase",
  "TranMode": "Cash",
  "DeliveryChannel": "Branch",
  "Amount": 850000,
  "Currency": "AED",
  "CustomerId": "C100045",
  "CustomerName": "Ahmed Al-Mansouri",
  "CustomerType": "I",
  "RemitterName": "Ahmed Al-Mansouri",
  "RemitterCountry": "AE",
  "BeneficiaryName": "Sunrise Developers LLC",
  "BeneficiaryCountry": "AE",
  "PropertyRef": "DXB-MARINA-1204",
  "PropertyValue": 850000,
  "Purpose": "Property purchase",
  "IsMultiParty": 0,
  "ClientId": 1,
  "Parties": []
}
```

Response:

```json
{
  "Result": {
    "transactionId": 1042,
    "tranRefNo": "TXN-2026-00099",
    "anyRuleHit": true,
    "caseId": 87,
    "caseRefNo": "STM-20260521-001042",
    "ruleResults": [
      {
        "ruleId": 9,
        "ruleCode": "RE-R001",
        "ruleName": "High-Value Cash Property Purchase",
        "riskRating": "High",
        "score": 90,
        "isHit": true,
        "conditionResults": [
          "[Seq 1] TranType equals Purchase -> HIT (actual='Purchase' expected='Purchase')",
          "[Seq 2] TranMode equals Cash -> HIT (actual='Cash' expected='Cash')",
          "[Seq 3] Amount gte 55000 -> HIT (actual=850000 compare=55000)"
        ]
      }
    ]
  },
  "Status": 200,
  "Message": "Transaction stored. 1 rule(s) hit. Case STM-20260521-001042 created."
}
```

### 9.4 Customers

| Method | Path                                          | Returns |
|--------|-----------------------------------------------|---------|
| GET    | `/api/stm/customers/search?q=ahmed&clientId=1`| `[ { CustomerId, CustomerName, ... } ]` |
| GET    | `/api/stm/customers/{customerId}?clientId=1`  | Detailed customer record from `customermaster` |

### 9.5 Cases

| Method | Path                                  | Body                  | Returns |
|--------|---------------------------------------|-----------------------|---------|
| POST   | `/api/stm/cases/open`                 | `StmCaseSearchDTO`    | Cases with status OPEN |
| POST   | `/api/stm/cases/completed`            | `StmCaseSearchDTO`    | Cases with status in [APPROVED, REJECTED, CLOSED, ESCALATED] |
| GET    | `/api/stm/cases/{id}`                 | -                     | `StmCaseDTO` (with transaction and comments) |
| POST   | `/api/stm/cases/{id}/review`          | `StmReviewPayload`    | `{ Result: rowsAffected }` |
| POST   | `/api/stm/cases/{id}/comment`         | `StmCommentPayload`   | `{ Result: commentId }` |

`StmReviewPayload`:
```json
{
  "decision": "TRUE_POSITIVE | FALSE_POSITIVE | ESCALATE",
  "status":   "APPROVED | REJECTED | ESCALATED | CLOSED",
  "remarks":  "investigation notes",
  "userId":   12,
  "userName": "compliance.officer"
}
```

---

## 10. Seeded Rules

### Insurance (Sector code `INS`)

| Code      | Name                          | Risk   | Score | Description                                  |
|-----------|-------------------------------|--------|-------|----------------------------------------------|
| INS-R001  | High-Value Single Premium     | High   | 80    | Premium ≥ 100,000                            |
| INS-R002  | Early Policy Surrender        | High   | 90    | Surrender within 12 months                   |
| INS-R003  | Cash Premium Above Threshold  | High   | 75    | Cash premium ≥ 55,000                        |
| INS-R004  | Third-Party Premium Payment   | High   | 70    | Payer ≠ policy holder                        |
| INS-R005  | Frequent Top-ups in 90 Days   | Medium | 60    | ≥ 3 top-ups in 90 days                       |
| INS-R006  | High-Risk Country Beneficiary | High   | 85    | Claim paid to high-risk country              |
| INS-R007  | Premium then Withdrawal       | High   | 85    | Premium + withdrawal within 30 days          |
| INS-R008  | Nominee Different Nationality | Medium | 55    | Nominee nationality ≠ policy holder          |

### Real Estate (Sector code `RE`)

| Code     | Name                           | Risk   | Score | Description                                    |
|----------|--------------------------------|--------|-------|------------------------------------------------|
| RE-R001  | High-Value Cash Property       | High   | 90    | Cash purchase ≥ 55,000 (AED) regulatory limit  |
| RE-R002  | Quick Resale within 90 Days    | High   | 85    | Resold ≤ 90 days from purchase                 |
| RE-R003  | Price Deviation from Market    | High   | 80    | Sale price ≥ 30% above/below market            |
| RE-R004  | Multiple Property Purchases    | High   | 75    | ≥ 3 purchases in 60 days by same buyer         |
| RE-R005  | Foreign Buyer High-Risk Country| High   | 85    | Buyer nationality in high-risk list            |
| RE-R006  | Third-Party Property Payment   | High   | 80    | Remitter ≠ buyer                               |
| RE-R007  | Corporate Buyer with PEP       | High   | 90    | Corporate buyer with PEP director/shareholder  |
| RE-R008  | Cash Rental Income High        | Medium | 60    | Cash rental ≥ 50,000 monthly                   |

These rules are seeded as `is_system_rule = 1` and can be deactivated but not
deleted via the UI; users can clone them or create their own rules with custom
thresholds at any time.

---

## 11. Frequently Asked Questions

**Q. How do I change the threshold of a seeded rule?**
A. Open the rule from `/stm/rules`, click Edit, modify the `compare_value`
   of the relevant condition, and save. The change applies to new
   transactions only.

**Q. Can a single transaction hit multiple rules?**
A. Yes. The case will list all violated rule names, and `total_risk_score`
   is the sum of their scores. The case's risk rating is the highest among
   the hit rules.

**Q. What happens if no rules are active for a sector?**
A. Transactions still persist with `rule_hit_status = NO_HIT`. No case is
   created.

**Q. Where is the high-risk country list?**
A. Hard-coded in `StmRulesEngine.HighRiskCountries`. To externalise it, create
   a `stm_high_risk_country` table and load it in the static constructor.

**Q. How do I add a sector (e.g. Banking)?**
A. Insert a new row into `stm_sector` (e.g. `('BANK','Banking','...')`),
   then create rules referencing the new sector ID. The UI auto-discovers
   sectors from the table.

---

## 12. Roadmap / Future Enhancements

- Aggregation operators (`Sum`, `Count`, `Avg`) with persisted state across transactions of the same customer
- Subsequent-transaction operator (`then within X days`)
- High-risk country list externalised into a configurable table
- Bulk transaction upload (CSV / Excel)
- Email alert on case creation
- Per-rule efficacy metrics (hit rate, false-positive rate)
- Role-based assignment of cases to compliance officers

---

*Last updated: 2026-05-21*
