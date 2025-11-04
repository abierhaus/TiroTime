# Feature Specification: Reporting & Export
**Specification ID**: 005
**Version**: 1.0
**Status**: Draft
**Parent**: [001 - TiroTime Overview](../001-tirotime-overview.md)

## Overview

Reporting and Export functionality enables users to analyze tracked time and generate professional reports for invoicing and management purposes. This module provides comprehensive reporting capabilities with export to Excel and PDF formats, specifically designed to support German invoicing requirements (Rechnung).

## Core Concepts

### Report
A filtered view of time entries with aggregations and summaries. Reports can be viewed on-screen, exported to Excel, or generated as PDF documents.

### Export
The process of generating formatted documents (Excel or PDF) containing time tracking data suitable for invoicing or record-keeping.

### Timesheet Report
A detailed report showing time entries for a specific period, grouped by various dimensions (employee, project, client, day).

## Functional Requirements

### FR-RE-001: Time Entry Reports
**Priority**: Critical
**Phase**: MVP

Users can generate reports of time entries with flexible filtering.

**Filter Options**:
- **Date Range**:
  - Today
  - Yesterday
  - This Week
  - Last Week
  - This Month
  - Last Month
  - This Quarter
  - This Year
  - Custom Date Range
- **User/Employee**: Single or multiple users (managers/admins)
- **Client**: Single or multiple clients
- **Project**: Single or multiple projects
- **Billable Status**: All, Billable Only, Non-Billable Only
- **Approval Status**: All, Approved, Not Approved, Pending
- **Tags**: Filter by tags (AND/OR logic)

**Grouping Options**:
- By Day
- By Week
- By Month
- By User
- By Client
- By Project
- By Task
- Custom grouping combinations

**Business Rules**:
- Employees can only report on own time entries
- Managers can report on team time entries
- Admins can report on all time entries
- Date range maximum: 2 years
- Reports generated asynchronously for large datasets (> 10,000 entries)

**Acceptance Criteria**:
- Given date range and filters, when user generates report, then matching entries displayed
- Given large dataset, when report requested, then progress indicator shown
- Given employee role, when attempting to view other's data, then access denied
- Given custom date range > 2 years, when requested, then validation error shown

### FR-RE-002: Report Display & Summary
**Priority**: High
**Phase**: MVP

Reports display detailed entries and summary statistics.

**Report Display**:
- Paginated table of time entries
- Columns (customizable):
  - Date
  - User (if multi-user report)
  - Project
  - Client
  - Task Description
  - Start Time
  - End Time
  - Duration
  - Billable
  - Hourly Rate
  - Amount
  - Tags
  - Approval Status
- Sort by any column
- Expand/collapse groups

**Summary Statistics**:
- **Total Hours**: Sum of all duration
- **Billable Hours**: Sum of billable duration
- **Non-Billable Hours**: Sum of non-billable duration
- **Total Amount**: Sum of billable hours × hourly rate
- **Entry Count**: Number of time entries
- **Average Hours per Day**: Total hours ÷ working days
- **Utilization Rate**: Billable hours ÷ total hours
- **Breakdown by**:
  - Client (hours and amount)
  - Project (hours and amount)
  - User (hours and amount)
  - Day/Week/Month

**Business Rules**:
- Amounts calculated only for billable entries
- Summaries update when filters change
- Grouped summaries show subtotals
- Grand total always displayed

### FR-RE-003: Excel Export
**Priority**: Critical
**Phase**: MVP

Users can export reports to Excel format (.xlsx).

**Excel Export Features**:
- **Detailed Sheet**: All time entries with columns
- **Summary Sheet**: Aggregated statistics
- **Formatting**:
  - Header row (bold, colored background)
  - Alternating row colors for readability
  - Number formatting (currency, time)
  - Auto-fit column widths
  - Freeze header row
  - Borders and gridlines
- **Formulas**:
  - SUM formulas for totals
  - Subtotals for grouped data
  - Grand total formulas
- **Multiple Sheets**:
  - Summary sheet
  - Details sheet
  - By Client sheet (optional)
  - By Project sheet (optional)

**Excel Structure Example**:

**Summary Sheet**:
```
TiroTime - Arbeitszeit Bericht
Zeitraum: 01.10.2024 - 31.10.2024
Mitarbeiter: Max Mustermann

Zusammenfassung:
Gesamtstunden:         160.5
Abrechenbare Stunden:  142.0
Nicht abrechenbar:      18.5
Gesamtbetrag:        €14,200.00

Nach Kunde:
Kunde               Stunden    Betrag
-------------------------------------------
Kunde A              80.0    €8,000.00
Kunde B              62.0    €6,200.00
-------------------------------------------
Summe:              142.0   €14,200.00
```

**Details Sheet**:
```
Datum      Projekt    Beschreibung    Start   Ende    Dauer   Satz      Betrag
2024-10-01 Projekt A  Feature dev     09:00   17:00   8:00    €100.00   €800.00
2024-10-02 Projekt A  Bug fixing      09:00   12:00   3:00    €100.00   €300.00
...
```

**Business Rules**:
- File name format: `TiroTime_Report_{User}_{DateRange}_{Timestamp}.xlsx`
- Maximum 100,000 rows per sheet (Excel limitation)
- For larger exports, split into multiple files
- Export respects current filters and grouping
- Generated asynchronously for > 1,000 entries
- File available for download for 24 hours

**Acceptance Criteria**:
- Given filtered report, when export to Excel, then file generated with correct data
- Given grouped report, when export to Excel, then subtotals included
- Given large dataset, when export requested, then async processing with notification
- Given German locale, when export, then German number/date formatting applied

### FR-RE-004: PDF Export
**Priority**: Critical
**Phase**: MVP

Users can export reports to PDF format suitable for invoicing.

**PDF Export Features**:
- **Professional Layout**:
  - Company logo (optional, from settings)
  - Report header with date range
  - User/employee information
  - Page numbers (Page X of Y)
  - Generated date/time
  - Footer with company info
- **Content Sections**:
  - Summary statistics (first page)
  - Detailed time entries table
  - Client/project breakdowns
  - Terms and conditions (optional)
- **Formatting**:
  - Professional fonts (Arial, Helvetica)
  - Proper margins (A4 paper size)
  - Landscape or Portrait orientation
  - Color coding for billable/non-billable
  - Page breaks at logical points
- **Localization**:
  - German language support
  - Date format: DD.MM.YYYY
  - Currency format: 1.234,56 €
  - Decimal separator: comma

**PDF Structure Example** (German Invoice Format):

```
┌─────────────────────────────────────────────────────────┐
│ [LOGO]                    STUNDENNACHWEIS                │
│                                                           │
│ Mitarbeiter: Max Mustermann                              │
│ Zeitraum: 01.10.2024 - 31.10.2024                       │
│ Erstellt am: 04.11.2024                                  │
└─────────────────────────────────────────────────────────┘

ZUSAMMENFASSUNG
────────────────────────────────────────────────────────────
Gesamtstunden:                      160,5 Std
Abrechenbare Stunden:              142,0 Std
Nicht abrechenbare Stunden:         18,5 Std
Gesamtbetrag:                    14.200,00 €

NACH KUNDE
────────────────────────────────────────────────────────────
Kunde               Stunden      Stundensatz    Betrag
────────────────────────────────────────────────────────────
Kunde A              80,0 Std      100,00 €   8.000,00 €
Kunde B              62,0 Std      100,00 €   6.200,00 €
────────────────────────────────────────────────────────────
Summe:              142,0 Std                14.200,00 €

                        [Seite 1 von 3]

┌─────────────────────────────────────────────────────────┐
│                    DETAILLIERTE ZEITERFASSUNG            │
└─────────────────────────────────────────────────────────┘

Datum      Projekt     Tätigkeit      Von    Bis    Std    Betrag
────────────────────────────────────────────────────────────────
01.10.2024 Projekt A   Feature dev    09:00  17:00  8,0   800,00 €
02.10.2024 Projekt A   Bug fixing     09:00  12:00  3,0   300,00 €
...

                        [Seite 2 von 3]
```

**Business Rules**:
- File name format: `TiroTime_Report_{User}_{DateRange}.pdf`
- PDF/A format for long-term archival (optional)
- Watermark for draft reports (not approved)
- Digital signature support (Phase 3)
- Maximum 500 pages (pagination for large reports)
- Generated asynchronously for > 100 entries

### FR-RE-005: Export Customization
**Priority**: Medium
**Phase**: Phase 2

Users can customize export templates and formats.

**Customization Options**:
- **Column Selection**: Choose which columns to include
- **Column Order**: Reorder columns
- **Summary Options**: Select which summaries to include
- **Grouping**: Choose grouping level and order
- **Branding**:
  - Company logo
  - Company name and address
  - Color scheme
  - Footer text
- **Language**: German or English
- **Number Format**: Locale-specific
- **Date Format**: Multiple formats (DD.MM.YYYY, YYYY-MM-DD, etc.)

**Template Management**:
- Save custom export templates
- Default template per user
- Organization-wide templates (admin)
- Template library (predefined templates)

**Business Rules**:
- Templates saved per user
- Admins can create org-wide templates
- Maximum 20 custom templates per user
- Templates include all formatting and filter settings

### FR-RE-006: Monthly Timesheet Report
**Priority**: High
**Phase**: MVP

Generate standard monthly timesheet for employee invoicing.

**Monthly Report Features**:
- Pre-configured report for current/previous month
- Grouped by day, then by project
- Shows all time entries for the month
- Summary by client and total
- Suitable for employee billing/invoicing
- One-click generation

**Report Sections**:
1. Header (employee, month, year)
2. Daily breakdown with project details
3. Weekly subtotals
4. Monthly summary by client
5. Grand total (hours and amount)
6. Approval section (for signatures)

**Business Rules**:
- Default to current month
- Includes only approved entries (configurable)
- Excludes non-billable by default (configurable)
- Generated in user's preferred format (Excel or PDF)

### FR-RE-007: Dashboard & Analytics
**Priority**: Medium
**Phase**: Phase 2

Visual dashboard with charts and analytics.

**Dashboard Widgets**:
- **Today's Summary**: Hours tracked today
- **This Week**: Bar chart by day
- **This Month**: Progress toward hours goal
- **Top Projects**: Pie chart of time distribution
- **Billable vs Non-Billable**: Donut chart
- **Revenue Trend**: Line chart over time
- **Utilization Rate**: Gauge chart
- **Recent Entries**: List of last 5-10 entries

**Chart Types**:
- Bar charts (time by day/week/month)
- Pie/Donut charts (distribution by project/client)
- Line charts (trends over time)
- Gauge charts (utilization, budget)
- Heatmaps (activity patterns)

**Business Rules**:
- Dashboard customizable (add/remove widgets)
- Data updates in real-time
- Click chart to drill down
- Export chart as image
- Responsive design for mobile

### FR-RE-008: Saved Reports
**Priority**: Low
**Phase**: Phase 3

Save frequently used report configurations.

**Saved Report Features**:
- Save current report with filters
- Name and description
- Schedule automatic generation (daily, weekly, monthly)
- Email delivery (optional)
- Share with team members

**Business Rules**:
- Maximum 20 saved reports per user
- Scheduled reports run during off-hours
- Email includes download link (not attachment)
- Reports auto-deleted after 30 days

### FR-RE-009: Invoice Generation
**Priority**: Medium
**Phase**: Phase 3

Generate formal invoices directly from time entries.

**Invoice Features**:
- Select time entries to invoice
- Invoice numbering (sequential, custom format)
- Invoice date and due date
- Client billing information
- Line items from time entries or grouped
- Tax calculation (VAT/MwSt)
- Payment terms
- Bank details
- Invoice status tracking (Draft, Sent, Paid, Overdue)

**German Invoice Requirements (Rechnung)**:
- Invoice number (Rechnungsnummer)
- Invoice date (Rechnungsdatum)
- Seller information (name, address, tax ID)
- Buyer information (name, address)
- Description of services (Leistungsbeschreibung)
- Dates of service (Leistungszeitraum)
- Net amount (Nettobetrag)
- VAT rate and amount (MwSt-Satz und Betrag)
- Gross amount (Bruttobetrag)
- Payment terms (Zahlungsbedingungen)
- Bank details (Bankverbindung)

**Business Rules**:
- Invoice numbers must be sequential and unique
- Cannot modify invoiced time entries
- Invoice locks associated time entries
- Support multiple tax rates
- German and English invoice templates

### FR-RE-010: Report Scheduling
**Priority**: Low
**Phase**: Phase 3

Automatically generate and send reports on schedule.

**Scheduling Options**:
- Frequency: Daily, Weekly (day of week), Monthly (day of month)
- Time of day to generate
- Recipients (email addresses)
- Format (Excel, PDF, or both)
- Filters and grouping

**Business Rules**:
- Maximum 10 scheduled reports per user
- Reports generated during off-hours (configurable)
- Delivery via email with download link
- Failed deliveries retry up to 3 times
- Admins can schedule organization-wide reports

### FR-RE-011: Export History
**Priority**: Low
**Phase**: Phase 3

Track history of generated reports and exports.

**History Features**:
- List of all generated exports
- Download previous exports (30-day retention)
- Re-generate previous report
- View parameters used for each export

**History Information**:
- Export date/time
- Generated by (user)
- Format (Excel, PDF)
- Filters and parameters
- File size
- Download count
- Expiry date

**Business Rules**:
- Export files retained for 30 days
- After 30 days, parameters retained but file deleted
- Users can re-generate expired reports
- Maximum 100 exports in history per user

## API Endpoints

### Report Endpoints

```
POST   /api/v1/reports/generate           # Generate report (preview)
POST   /api/v1/reports/export/excel       # Export to Excel
POST   /api/v1/reports/export/pdf         # Export to PDF
GET    /api/v1/reports/templates          # List export templates
POST   /api/v1/reports/templates          # Create export template
PUT    /api/v1/reports/templates/{id}     # Update template
DELETE /api/v1/reports/templates/{id}     # Delete template
GET    /api/v1/reports/saved              # List saved reports
POST   /api/v1/reports/saved              # Save report configuration
DELETE /api/v1/reports/saved/{id}         # Delete saved report
POST   /api/v1/reports/saved/{id}/run     # Run saved report
GET    /api/v1/reports/history            # List export history
GET    /api/v1/reports/history/{id}       # Get export details
GET    /api/v1/reports/history/{id}/download # Download export file
```

### Dashboard Endpoints

```
GET    /api/v1/dashboard/summary          # Get dashboard summary
GET    /api/v1/dashboard/today            # Today's statistics
GET    /api/v1/dashboard/week             # This week's statistics
GET    /api/v1/dashboard/month            # This month's statistics
GET    /api/v1/dashboard/charts/{type}    # Get specific chart data
```

### Invoice Endpoints (Phase 3)

```
GET    /api/v1/invoices                   # List invoices
GET    /api/v1/invoices/{id}              # Get invoice details
POST   /api/v1/invoices                   # Create invoice
PUT    /api/v1/invoices/{id}              # Update invoice
DELETE /api/v1/invoices/{id}              # Delete invoice (draft only)
POST   /api/v1/invoices/{id}/finalize     # Finalize invoice (locks entries)
POST   /api/v1/invoices/{id}/send         # Send invoice via email
GET    /api/v1/invoices/{id}/pdf          # Generate invoice PDF
PATCH  /api/v1/invoices/{id}/status       # Update invoice status
```

## Data Model

### Report Configuration

```csharp
public class ReportConfiguration
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public ReportType Type { get; set; }
    public ReportFilters Filters { get; set; }
    public ReportGrouping Grouping { get; set; }
    public List<string> Columns { get; set; }
    public bool IsShared { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReportFilters
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<Guid>? UserIds { get; set; }
    public List<Guid>? ClientIds { get; set; }
    public List<Guid>? ProjectIds { get; set; }
    public BillableFilter BillableFilter { get; set; }
    public ApprovalFilter ApprovalFilter { get; set; }
    public List<string>? Tags { get; set; }
}

public enum BillableFilter
{
    All = 0,
    BillableOnly = 1,
    NonBillableOnly = 2
}

public enum ApprovalFilter
{
    All = 0,
    ApprovedOnly = 1,
    NotApproved = 2,
    PendingOnly = 3
}

public enum ReportGrouping
{
    None = 0,
    ByDay = 1,
    ByWeek = 2,
    ByMonth = 3,
    ByUser = 4,
    ByClient = 5,
    ByProject = 6
}

public enum ReportType
{
    TimeEntries = 0,
    Summary = 1,
    Timesheet = 2,
    Invoice = 3
}
```

### Export Job

```csharp
public class ExportJob : Entity
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public ExportFormat Format { get; private set; }
    public ReportConfiguration Configuration { get; private set; }
    public ExportStatus Status { get; private set; }
    public string? FileUrl { get; private set; }
    public long? FileSizeBytes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int DownloadCount { get; private set; }

    public void MarkAsCompleted(string fileUrl, long fileSize);
    public void MarkAsFailed(string errorMessage);
    public void IncrementDownloadCount();
    public bool IsExpired();
}

public enum ExportFormat
{
    Excel = 0,
    PDF = 1
}

public enum ExportStatus
{
    Queued = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Expired = 4
}
```

### Export Template

```csharp
public class ExportTemplate : Entity
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; } // Null = organization-wide
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public ExportFormat Format { get; private set; }
    public List<string> Columns { get; private set; }
    public string? LogoUrl { get; private set; }
    public BrandingSettings? Branding { get; private set; }
    public string LanguageCode { get; private set; }
    public string DateFormat { get; private set; }
    public string NumberFormat { get; private set; }
    public bool IsDefault { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public bool IsOrganizationWide => UserId == null;
}

public class BrandingSettings
{
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string? CompanyTaxId { get; set; }
    public string? ColorScheme { get; set; }
    public string? FooterText { get; set; }
}
```

## Report Generation Logic

### Excel Generation Process

1. **Query Data**: Fetch time entries based on filters
2. **Calculate Summaries**: Aggregate data for summary sheet
3. **Create Workbook**: Initialize Excel workbook
4. **Create Summary Sheet**:
   - Add header with date range
   - Add summary statistics
   - Add breakdowns (by client, project, user)
5. **Create Details Sheet**:
   - Add column headers
   - Add time entry rows
   - Apply formatting (alternating rows, number formats)
   - Add formulas for totals
6. **Apply Styling**:
   - Freeze header rows
   - Auto-fit columns
   - Set print area
   - Add borders
7. **Save File**: Save to temporary storage
8. **Generate Download Link**: Create signed URL (expires in 24h)

### PDF Generation Process

1. **Query Data**: Fetch time entries based on filters
2. **Calculate Summaries**: Aggregate data
3. **Initialize PDF Document**: Create PDF with A4 page size
4. **Add Header**:
   - Logo (if configured)
   - Report title
   - Date range, employee info
5. **Add Summary Page**:
   - Summary statistics table
   - Breakdowns (by client, project)
6. **Add Detail Pages**:
   - Time entry table (with page breaks)
   - Grouped by day/week/project as configured
7. **Add Footer**: Page numbers, company info
8. **Apply Styling**: Fonts, colors, spacing
9. **Save File**: Save to temporary storage
10. **Generate Download Link**: Create signed URL

## Localization (German)

### German Terminology

| English | German |
|---------|--------|
| Time Entry | Zeiterfassung |
| Timesheet | Stundenzettel / Stundennachweis |
| Hours | Stunden (Std) |
| Duration | Dauer |
| Billable | Abrechenbar |
| Non-Billable | Nicht abrechenbar |
| Hourly Rate | Stundensatz |
| Total | Summe / Gesamt |
| Client | Kunde |
| Project | Projekt |
| Task | Aufgabe / Tätigkeit |
| Amount | Betrag |
| Invoice | Rechnung |
| Summary | Zusammenfassung |
| Report | Bericht |

### German Number Formatting

- **Decimal Separator**: Comma (,)
- **Thousands Separator**: Period (.)
- **Currency**: € symbol after number
- **Examples**:
  - 1,234.56 → 1.234,56 €
  - 8 hours → 8,0 Std
  - 142.5 hours → 142,5 Std

### German Date Formatting

- **Format**: DD.MM.YYYY
- **Examples**:
  - 2024-10-01 → 01.10.2024
  - October 1, 2024 → 01. Oktober 2024

## Performance Requirements

- Report generation (< 1,000 entries): < 2 seconds
- Report generation (< 10,000 entries): < 10 seconds
- Excel export (< 1,000 entries): < 5 seconds
- PDF export (< 100 entries): < 5 seconds
- Dashboard load: < 1 second
- Large exports processed asynchronously with notification

## Security Requirements

- Users can only export own data (unless manager/admin)
- Export files stored securely with signed URLs
- Export links expire after 24 hours
- Audit log for all exports
- Rate limiting: 20 exports per hour per user
- Large exports (> 10,000 entries) require admin approval

## Testing Requirements

### Unit Tests
- Report filter logic
- Aggregation calculations
- Summary statistics
- Number formatting (German locale)
- Date formatting

### Integration Tests
- Report generation with various filters
- Excel file generation and structure
- PDF file generation and content
- Export template application
- Large dataset handling

### E2E Tests
- Generate and download Excel report
- Generate and download PDF report
- Monthly timesheet generation
- Custom template creation and use
- Scheduled report execution (Phase 3)

## Dependencies

- Time Tracking module
- Client & Project Management module
- EPPlus or ClosedXML (Excel generation)
- iText or QuestPDF (PDF generation)
- Background job processor (Hangfire)
- File storage (Azure Blob Storage or S3)

## Future Enhancements (Phase 4+)

- Interactive reports with drill-down
- Custom chart builder
- Export to additional formats (CSV, JSON, XML)
- Report sharing with external stakeholders
- Embedded reports (iFrame)
- API for programmatic report generation
- Advanced invoice management (payment tracking, reminders)
- Multi-currency invoicing with exchange rates
- Recurring invoices
- Integration with accounting software (DATEV, Lexoffice)
- E-invoicing (ZUGFeRD format for Germany)
- Profit and loss statements
- Project profitability analysis
- Resource utilization reports
- Forecasting and predictions based on historical data

---

**Document Owner**: Development Team
**Reviewers**: Product Team, Finance Team, Legal Team
**Approval Status**: Pending
