# Case Creation Studio Documentation

This document serves as the definitive reference for form fields, mandatory requirements, and data provenance for the Case Creation Studio. It aligns the Studio implementation with the validated legacy interfaces (`Case/Create` and `Corporate/CorporateScreening`) while utilizing the **Watchdog Blue** design system.

---

## 1. Design & Ergonimics

The Studio follows a high-density, symmetric design system:
- **Branding**: Watchdog Blue (`#2563EB`) primary palette.
- **Normalization**: Standardized `44px` height for all inputs, selects, and buttons.
- **Responsiveness**: State-aware accordion chevrons (Up = Expanded, Down = Collapsed).
- **Density**: Optimized `2-column grid` with reduced internal paddings (`p-6`) for maximum information visibility.

---

## 2. Individual Entity Intake

### Tab: Identity
| Field Name | Backend Property | Type | Mandatory | Source / Notes |
| :--- | :--- | :--- | :---: | :--- |
| **Full Name** | `FirstName` | Text | ✅ | Manual entry (Auto-splits for DTO) |
| **Nationality** | `Nationality` | Dropdown | ❌ | `window.studioData.Nationalities` |
| **Date of Birth** | `Dob` | Date | ❌ | Format: DD/MM/YYYY |
| **Gender** | `Gender` | Dropdown | ❌ | Male, Female |
| **Mobile Number** | `MobileNo` | Text | ❌ | Manual entry |
| **Profession** | `Profession` | Dropdown | ✅ | `window.studioData.BusinessTypeList` |
| **Residence Country** | `Residence` | Dropdown | ✅ | `window.studioData.Nationalities` |
| **Employer Name** | `EmployerName` | Text | ❌ | Manual entry |
| **SOW/SOF Country** | `Sowsofcountry` | Dropdown | ❌ | `window.studioData.Nationalities` |
| **ID Type** | `IDType` | Multi-select | ❌ | Passport, Emirates ID |
| **Product Type** | `ProductType` | Dropdown | ✅ | `window.studioData.Products` |
| **Delivery Channel** | `DeliveryChannel` | Dropdown | ✅ | `window.studioData.DeliveryChannels` |
| **Mode of Payment** | `PaymentMode` | Dropdown | ✅ | `window.studioData.PaymentModes` |
| **Match Parameter** | `Threshold` | Slider | ❌ | Default: 70 |

---

## 3. Corporate Entity Intake

### Tab: Identity
| Field Name | Backend Property | Type | Mandatory | Source / Notes |
| :--- | :--- | :--- | :---: | :--- |
| **Company Full Name** | `FirstName` | Text | ✅ | Manual entry |
| **Country of Incorporation** | `Nationality` | Dropdown | ❌ | `window.studioData.Nationalities` |
| **Residence Country** | `Residence` | Dropdown | ❌ | `window.studioData.Nationalities` |
| **Trade License No** | `TradeLicence` | Text | ❌ | Manual entry |
| **Mobile Number** | `MobileNo` | Text | ❌ | Manual entry |
| **Corporate Email** | `Email` | Text | ❌ | Manual entry |
| **Legal Status** | `EntityTypeTxt` | Dropdown | ✅ | `window.studioData.EntityType` |
| **Business Activities** | `BusinessType` | Dropdown | ✅ | `window.studioData.BusinessTypeList` |
| **Product Type** | `ProductType` | Dropdown | ✅ | `window.studioData.Products` |
| **Delivery Channel** | `DeliveryChannel` | Dropdown | ✅ | `window.studioData.DeliveryChannels` |
| **Mode of Payment** | `PaymentMode` | Dropdown | ✅ | `window.studioData.PaymentModes` |
| **Match Parameter** | `Threshold` | Slider | ❌ | Default: 70 |

---

## 4. Hierarchical Linkage (Related Parties)

Depending on the selection in the "Relationship" dropdown:

- **Individual Shareholder**: Requires `Share (%)`.
- **Corporate Shareholder**: Requires `Share (%)`.
- **Signatory / Management**: Requires `Designation` (Mandatory).
- **Related Party**: General individual/corporate fields apply based on type.

---

## 5. System Operations

### Iconography
- Icons are powered by `Lucide`. 
- Global initialization via `lucide.createIcons()`.
- Reactive refresh occurs on tab switches, sheet toggles, and data updates.

### State Management
- **Alpine.js**: Drives the property panel, tabs, and modal states.
- **Vis.js**: Manages the network canvas and entity positioning.
- **Validation**: Real-time parity check against `errors` array before node updates or case creation.

### Bulk & OCR
- **Bulk Import**: Legacy XLSX support with hierarchical mapping.
- **OCR Engine**: Field extraction from ID documents and trade licenses.
