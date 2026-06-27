# Comprehensive Briefing: Risk Configuration Framework

This document provides a technical and functional overview of the Risk Assessment system, including its hierarchical structure, calculation logic, and identified vulnerabilities.

## 1. Risk Hierarchy structure & Entity Properties

The system implements a strict four-layer hierarchy designed to compartmentalize risk factors. Each layer has specific metadata properties that drive the final assessment.

### L1: Risk Category (Entity Scope)
- **Primary Property**: `RiskCategoryID` (`I`, `C`, `B`, `V`)
- **Function**: Filters the entire risk configuration universe. A "Corporate" case will only ever see risks mapped to the `C` category.
- **System Impact**: Determines the target database tables for persistence (e.g., `tran_risk` for Individual vs `tran_risk_corporate` for Corp).

### L2: Risk Type Category (Logical Buckets)
- **Properties**: `Id`, `LovTypeCategory` (Name)
- **Function**: Organizes risks into intuitive groups for the user interface (e.g., *Customer Profile*, *Geographic Risk*, *Transaction Patterns*).
- **System Impact**: Used primarily for UI rendering and grouped reporting in Excel/PDF exports.

### L3: Risk Type (The Factor Layer)
This is the most critical layer for numeric calculation.
- **Key Properties**:
    - **`RiskTypePercentage` (Weightage)**: An integer (0-100) representing the factor's influence on the total score.
    - **`lov_country_duplicate`**: If `1`, the system dynamically pulls items from the global `CountryMaster` instead of static `lov_master` items.
    - **`tooltip`**: Contextual guidance provided to the officer during assessment.
- **Calculation Rule**: The selected item's score is multiplied by $( \text{Weightage} / 100 )$.

### L4: Risk Item (The Data Point Layer)
The granular level where data is selected.
- **Key Properties**:
    - **`RiskScore`**: Numeric base (typically 1-4).
        - `1`: Low
        - `2`: Medium
        - `3`: Medium High
        - `4`: High
    - **`OverrideScore`**: If set to `3`, this item is a "Killer Variable" that forces the final rating to High Risk.
    - **`isActive`**: Controls visibility in the dropdowns.
- **UI Logic**: Items with `OverrideScore = 3` trigger a "Pulse" animation to warn the user of an automatic escalation.

---

## 2. Risk Scale & System Effects Summary

This table summarizes how the numeric "Risk Score Sum" translates into operational compliance actions and UI indicators.

| Score Range | Base Rating | UI Color | Effect / Compliance Action |
| :--- | :--- | :--- | :--- |
| **0.00 - 1.00** | Low | Green | **Standard Monitoring**: Minimum due diligence required. |
| **1.01 - 2.00** | Medium | Yellow | **Routine Review**: Yearly re-assessment scheduled. |
| **2.01 - 3.00** | Medium High | Orange | **Escalated Monitoring**: Higher transaction scrutiny applied. |
| **3.01 - 4.00** | High | Red | **Mandatory EDD**: Senior Management/Compliance sign-off. |
| **Any Override** | High | **Red (Pulse)** | **Immediate Escalation**: Forces "High Risk" regardless of score. |

---

## 3. Deep Dive: Risk Hierarchy Components

### L1: Risk Category
The top-most level of the system, used to select the broad entity assessment model.
- **Distinction**:
    - `"I"`: Individual Customer
    - `"C"`: Corporate Customer
    - `"V"`: Vendor
    - `"B"`: Bank
- **Property Role**: This selection acts as the global filter for all subordinate risk types and items.

### L2: Risk Type Category
Logical groupings used to bucket related operational risks.
- **Common Examples**:
    - **Customer Risk**: Occupation, Income Source, Business Activity.
    - **Geography/Country Risk**: Residence, Nationality, Jurisdiction of operations.
    - **Product/Service Risk**: Delivery Channels, Transaction Volumes, Type of products held.

### L3: Risk Type
The specific risk factors within each category that an officer must evaluate.
- **Dynamic Property**: **Percentage Weightage**. Each Risk Type can be assigned a relative weight (0-100%) to indicate its importance to the overall profile.
- **Example Factors**: *Occupation*, *Nationality*, *PEP Status*, *Screening Hits*.

### L4: Risk Items
The granular, selectable values provided for a specific Risk Type.
- **Properties**:
    - **Risk Item (Value)**: e.g., for Occupation, values include *Salaried*, *Self-Employed*, *Unemployed*.
    - **Risk Score**: The base numeric risk inherent to the value (1-4).
    - **Override Flag**: A binary trigger (3 = Override) that marks high-risk outliers.

## 2. Risk Calculation Logic

### A. Weighted Scoring
The numeric assessment is determined by summing the results of each selected item multiplied by its parent type's weight.

**Formula:**
$$ \text{Total Risk Score} = \sum \left( \frac{\text{Risk Type Percentage}}{100} \times \text{Risk Item Score} \right) $$

### B. Rating Thresholds
The Final Rating is determined by comparing the `Total Risk Score` against these hardcoded boundaries:
- **0.00 – 1.00**: Low Risk
- **1.01 – 2.00**: Medium Risk
- **2.01 – 3.00**: Medium High Risk
- **3.01 – 4.00**: High Risk

### C. Automatic "High Risk" Overrides
If any Risk Item has an `OverrideScore` property set to `3`, the system triggers an immediate escalation. The numeric total is bypassed, and the **Final Risk Score** is set to **High Risk**.

## 3. Configuration Properties

### Risk Type Properties
- `Id`: Primary key in `lov_type`.
- `RiskTypePercentage`: Weightage applied to items under this type.
- `lov_country_duplicate`: Flag (0 or 1) indicating if items should be pulled from a static list or the global Country Master.
- `tooltip`: Descriptive hint for UI users.

### Risk Item Properties
- `Id`: Primary key in `lov_master`.
- `RiskScore`: Numeric base (1-4).
- `OverrideScore`: Trigger flag (3 = Force High Risk).
- `isActive`: Boolean flag for item availability.

---

## 6. Data Inventory & Assessment Profile (Snapshot)

This section documents a specific configuration profile used for individual risk assessments, detailing the baseline scores and override triggers for core parameters.

### I. Customer & Geographic Profile
| Category / Type | selected Item | Base Score | Weighted Influence |
| :--- | :--- | :--- | :--- |
| **Customer Risk** | -- | -- | -- |
| Profession | Salaried In Private Sector | **2** | Moderate |
| Residence Country | --EMPTY-- | **0** | **Vulnerability (Deflation)** |
| **Geographic Risk** | -- | -- | -- |
| Nationality | INDONESIA | **2** | Moderate |
| Second Nationality | --EMPTY-- | **0** | No Influence |

### II. Product & Delivery Risk
| Category / Type | selected Item | Base Score | System impact |
| :--- | :--- | :--- | :--- |
| **Product Risk** | -- | -- | -- |
| Activity | Retail Jewelry | **1** | Low |
| Secondary Product | --EMPTY-- | **0** | No Influence |
| **Delivery Channel** | -- | -- | -- |
| Delivery Channel | **Non-Face to face** | **3** | **Critical Override Trigger** |

### III. Screening, Compliance & Findings
| Compliance Parameter | Assessment | Base Score | System Effect |
| :--- | :--- | :--- | :--- |
| **Domestic PEP** | **Yes** | **3** | **Killer Variable (Force High)** |
| Foreign PEP | No | 1 | Standard |
| Red Flags Noticed | No | 1 | Standard |
| Very High Networth | No | 1 | Standard |
| Sanction Match (Local) | No | 1 | Standard |
| Sanction Match (UNSC) | No | 1 | Standard |

### IV. Transactional & Special Goods
| Parameter | assessment | Base Score | system effect |
| :--- | :--- | :--- | :--- |
| **Mode of Payment** | Others | **2** | Moderate (Investigation needed) |
| **Dual Use Goods** | **Yes** | **3** | **Mandatory EDD Trigger** |

---

## 7. Critical Combination & Override Summary

Based on the profile above, the following combinations trigger specific system-level behaviors that bypass the standard numeric sum:

1.  **The "Killer Variable" Combo**:
    - `Delivery Channel (Non-Face to face)` + `Domestic PEP (Yes)`.
    - **Effect**: Even if all other factors were "Low," these two **Level 3 Overrides** ensure the case is locked into **High Risk** and pulse-animated in the UI for immediate compliance escalation.
2.  **The High-Activity High-Risk goods Combo**:
    - `Retail Jewelry` + `Dual Use Goods (Yes)`.
    - **Effect**: This creates a specific high-risk nexus between the business nature and the goods provided, automatically elevating the case for manual senior approval.
3.  **The Indonesia-Residency Inconsistency**:
    - `Nationality (Indonesia)` + `Residence (Empty)`.
    - **Vulnerability Note**: Since Residence is `0`, the system averages out the geographic risk, potentially masking a non-resident assessment as a "Medium" rather than a "High" geographic inconsistency.

Analysis of the current implementation reveals several critical logic gaps and architectural vulnerabilities that impact the robustness of the risk assessment framework.

### I. The "Single Point of Failure" (One-Hit Override)
The most significant logic gap is the **Unary Override** mechanism.
- **The Gap**: If any single selected item has an `OverrideScore` of 3, the entire case is forced to "High Risk."
- **Limitation**: The system does not support "Negative Overrides" or "Mitigating Combinations."
- **Combination Example**: 
    - **Factor A**: PEP (Politically Exposed Person) status (Override = 3).
    - **Factor B**: 30-year banking relationship with zero late payments (Low Risk).
    - **Result**: The system ignores the 30-year history entirely. The PEP flag forces a "High Risk" rating, preventing the logic from recognizing a "Low Risk PEP" who is a well-known local entity.

### II. Flat Aggregate vs. Contextual Weighting
The system uses a linear weighted sum, which treats all risk factors as independent variables.
- **The Gap**: Risk factors in AML are often **multiplicative**, not additive.
- **Combination Example**:
    - **Profile A**: Student (Medium) + High Cash Turns (Medium). Sum = ~2.5 (Medium High).
    - **Profile B**: Arms Dealer (High) + $100 Salary (Low). Sum = ~2.5 (Medium High).
    - **Vulnerability**: Profile A's combination is exponentially riskier (a student with high cash is a "mule" indicator), but because the logic only *sums* them, it ranks the arms dealer and the potential mule as having the same risk intensity.

### III. Missing Inter-Category Correlation (Siloed Risks)
The risk configuration is siloed into "Risk Type Categories" (Customer, Geography, Product).
- **The Gap**: There is no logic to detect conflicts or patterns *across* these categories.
- **Combination Example**:
    - **Geography**: Main Nationality = "UK" (Low Risk Score: 1).
    - **Customer Detail**: Residential Address = "Grand Cayman" (Low Risk Score: 1).
    - **Vulnerability**: Individually, both are low risk. However, the *combination* of a UK national residing in a tax haven is a "Complex Structure" indicator. The system treats them as two `1`s, resulting in a Low risk assessment rather than flagging the geographic inconsistency.

### IV. Zero-Score Sensitivity & Documentation Gaps
- **The Gap**: The system allows items to have a score of 0 or be left "Empty."
- **Combination Example**:
    - **Case A**: Businessman (3) + UK based (1) + High Volume (3). Average = 2.33.
    - **Case B**: Businessman (3) + Unknown Country (0) + High Volume (3). Average = 2.0.
    - **Consequence**: Case B appears "safer" because the country data is missing. The system rewards incomplete documentation by lowering the numeric average, creating an incentive for officers to leave difficult fields set to "--EMPTY--."

### V. Static Thresholds Across Clients
The risk boundaries (1.00, 2.00, 3.00) are hardcoded directly in the front-end layer (`RiskCreate.cshtml`).
- **The Gap**: Different clients (Banks, Fintechs, Casinos) have vastly different risk appetites.
- **Combination Example**:
    - **Risk Combination**: A cluster of medium factors sums to **2.95**.
    - **Client A (Traditional Bank)**: Might consider 2.95 as "High Risk."
    - **Client B (Aggressive Fintech)**: Might consider 2.95 as "Medium Risk."
    - **Problem**: Both clients are forced to see "Medium High" because the code does not allow for a per-client adjustment of the 3.01 threshold.

### VI. Temporal & Version Blindness
- **The Gap**: The current logic only evaluates the "Current State" of a case.
- **Combination Example**:
    - **Version 1**: Occupation = "Unemployed" (Score 2).
    - **Version 2**: Occupation = "Art Dealer" (Score 3).
    - **Risk**: The change itself is the risk (sudden high-value business acquisition). Because the system only looks at the combination in Version 2, it misses the **Velocity Spike** which is a hallmark of account takeovers for money laundering.

### VII. Client-Side Calculation Dependency
- **Vulnerability**: A significant portion of the final risk determination logic resides in the client-side JavaScript (`calculateTotal()`).
- **Risk Example**:
    - **Combination**: The user selects "High Risk Nationality" (Override = 3).
    - **Manipulation**: A user with basic browser inspection skills could manipulate the `OverRideVariables` array in the JS console to `false` before clicking Submit, potentially bypassing the server's override logic if the server solely trusts the posted `FinalRiskScore`.

---

## 5. Mitigation & Remediation Strategy

To address the identified gaps and transition to a "Next-Gen" Risk Framework, the following technical and architectural remediations are recommended.

### I. Solving "One-Hit Override" Fragility
- **Remediation**: Implement **Mitigating Factor Logic**.
- **Solution**: Introduce a new `MitigationValue` property in `lov_master`. If an item is selected that carries a mitigation flag (e.g., "Publicly Listed Company"), it can counteract a specific number of override points or lower the Final Rating by one tier on the server side.

### II. Moving from Additive to Multiplicative Risk
- **Remediation**: Implement **Risk Correlators**.
- **Solution**: Create a `RiskInconsistencyRules` table. If the server detects high-risk combinations (e.g., *Student + High Cash Turns*), it applies a "Correlation Penalty" (e.g., +1.0) to the `RiskScoreSum` before final categorization. This ensures clusters are weighted more heavily than independent factors.

### III. Bridging Inter-Category Silos
- **Remediation**: **Cross-Category Validation Engine**.
- **Solution**: Add a backend validation step in `RiskV2Service` that compares Geography DTOs with Customer DTOs. 
    - *Example*: `If (Nationality != AddressPrimaryCountry) { TriggerInconsistencyFlag(Score: +0.5); }`

### IV. Addressing Zero-Score Sensitivity
- **Remediation**: **Default Uncertainty Score**.
- **Solution**: Shift from a "0 for Empty" model to an "Inherent Risk" model. 
    - *Strategy*: Set the default value for an unselected item to `1.5` (Unmarked/Potential Risk) instead of `0`. This ensures that a case with missing documentation naturally trends toward "Medium" risk, forcing the officer to provide data to lower it.

### V. Decoupling Configuration from Code
- **Remediation**: **Persistent Client-Specific Thresholds**.
- **Solution**: Move the 1.0, 2.0, and 3.0 boundaries from `RiskCreate.cshtml` to a new `ClientRiskThresholds` table in the database. Fetch these values during the `GetRiskConfig` call so the UI and Backend calculate limits dynamically based on the specific `ClientId`.

### VI. Solving Temporal Blindness
- **Remediation**: **Risk Migration Detection**.
- **Solution**: Modify the `Create` method in `RiskV2Repository` to fetch the previous version's `FinalRiskScore`. 
    - *Logic*: Compare `Version(N).Score` with `Version(N-1).Score`. If the tier jump is > 1 (e.g., Low to High), automatically flag the case for "Immediate Investigation," regardless of the numeric total.

### VII. Hardening the Calculation Layer
- **Remediation**: **Server-Side Truth Principle**.
- **Solution**: Treat the client-side `calculateTotal()` only as a UI/Visual aid. 
    - *Action*: The Backend service MUST re-calculate the entire risk sum and override logic using the raw `SelectedItemId` list before committing to the database. NEVER trust the `FinalRiskScore` or `RiskScoreSum` value posted directly from the browser.

---
*Status: Remediation Roadmap Drafted | AML Engineering & Compliance Review Required*
