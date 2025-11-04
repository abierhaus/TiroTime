# User Stories & Use Cases
**Specification ID**: 006
**Version**: 1.0
**Status**: Draft
**Parent**: [001 - TiroTime Overview](../001-tirotime-overview.md)

## Overview

This document contains user stories and use cases for the TiroTime application, organized by persona and feature area. User stories follow the format: "As a [role], I want [goal] so that [benefit]."

## Personas

### 1. Freelance Developer (Felix)
- **Background**: Solo freelance software developer
- **Clients**: 3-5 active clients
- **Projects**: 5-10 concurrent projects
- **Pain Points**:
  - Forgets to track time
  - Manual invoice creation is tedious
  - Switches between projects frequently
  - Needs accurate hourly tracking for billing

### 2. Agency Project Manager (Maria)
- **Background**: Manages team of 5 developers at digital agency
- **Responsibilities**: Project oversight, client communication, billing
- **Pain Points**:
  - Needs visibility into team time allocation
  - Must approve timesheets before invoicing
  - Tracks project budgets and profitability
  - Generates client reports monthly

### 3. Consultant (Thomas)
- **Background**: Management consultant working on-site
- **Projects**: 2-3 large, long-term engagements
- **Pain Points**:
  - Needs detailed activity descriptions for client transparency
  - Tracks billable vs. non-billable time strictly
  - Requires professional reports for compliance
  - Works across different time zones

### 4. Agency Employee (Sarah)
- **Background**: Junior developer at agency
- **Work Style**: Assigned to multiple client projects
- **Pain Points**:
  - Needs simple, fast time tracking
  - Sometimes forgets to start/stop timer
  - Requires manager approval for timesheet
  - Wants to see own time statistics

### 5. Administrator (Alex)
- **Background**: Operations manager at agency
- **Responsibilities**: System setup, user management, compliance
- **Pain Points**:
  - Needs to manage users and permissions
  - Configures clients and projects
  - Ensures data accuracy and compliance
  - Generates company-wide reports

## User Stories by Feature Area

## Authentication & User Management

### US-001: User Registration
**As a** new user
**I want** to register for an account with my email and password
**So that** I can start tracking my time

**Acceptance Criteria**:
- User can register with email and password
- Email must be unique
- Password must meet security requirements (12+ chars, complexity)
- User receives email verification
- User cannot log in until email is verified

**Priority**: High | **Phase**: MVP

---

### US-002: User Login
**As a** registered user
**I want** to log in with my credentials
**So that** I can access my time tracking data

**Acceptance Criteria**:
- User can log in with email and password
- Invalid credentials show error message
- Account locked after 5 failed attempts
- "Remember me" option available
- User redirected to dashboard after login

**Priority**: High | **Phase**: MVP

---

### US-003: Password Reset
**As a** user who forgot password
**I want** to reset my password via email
**So that** I can regain access to my account

**Acceptance Criteria**:
- User can request password reset with email
- Reset link sent to email (valid 1 hour)
- User can set new password via link
- Old password cannot be reused (last 5)
- All sessions invalidated after reset

**Priority**: High | **Phase**: MVP

---

### US-004: Profile Management
**As a** logged-in user
**I want** to update my profile information
**So that** my account details are current

**Acceptance Criteria**:
- User can edit name, phone, timezone
- User can upload profile picture
- Email change requires re-verification
- Changes saved and confirmed
- Profile picture max 5MB

**Priority**: Medium | **Phase**: MVP

---

## Time Tracking

### US-005: Start Timer
**As a** freelancer (Felix)
**I want** to start a timer when I begin working on a project
**So that** I can track time automatically without manual entry

**Acceptance Criteria**:
- User selects project from dropdown
- Timer starts immediately with current time
- Optional task description can be added
- Running timer visible across all pages
- Only one timer can run at a time

**Priority**: Critical | **Phase**: MVP

**Use Case Flow**:
1. Felix starts working on "Client A - Website Redesign"
2. Opens TiroTime and clicks "Start Timer"
3. Selects project from dropdown
4. Types description: "Implementing header component"
5. Clicks "Start"
6. Timer begins counting (00:00:01, 00:00:02...)
7. Felix sees timer in header while working
8. Switches to other tabs/applications, timer continues

---

### US-006: Stop Timer
**As a** freelancer (Felix)
**I want** to stop the running timer when I finish a task
**So that** the time is recorded as a time entry

**Acceptance Criteria**:
- User clicks stop button on running timer
- Time entry created with duration
- User can edit entry details before saving
- Timer resets after stopping
- Entry appears in time entry list

**Priority**: Critical | **Phase**: MVP

**Use Case Flow**:
1. Felix finishes work after 2 hours 15 minutes
2. Clicks "Stop" on running timer
3. Modal shows: Project, Duration (2:15), Description
4. Felix can edit description or accept as-is
5. Clicks "Save"
6. Time entry created and timer resets
7. Felix sees entry in "Today's Entries" list

---

### US-007: Manual Time Entry
**As a** consultant (Thomas)
**I want** to manually enter time with specific start and end times
**So that** I can record time for work done earlier

**Acceptance Criteria**:
- User can open "Add Time Entry" form
- User selects date, project, start time, end time
- Duration calculated automatically
- Optional task description and notes
- Entry saved to time log
- Validation: end time after start time

**Priority**: Critical | **Phase**: MVP

**Use Case Flow**:
1. Thomas realizes he forgot to track time for morning meeting
2. Clicks "Add Time Entry" button
3. Selects today's date
4. Selects project: "Client B - Strategy Consulting"
5. Enters start time: 09:00
6. Enters end time: 11:00
7. Duration auto-calculated: 2:00
8. Enters description: "Stakeholder workshop"
9. Marks as billable
10. Clicks "Save"
11. Entry appears in time log

---

### US-008: Edit Time Entry
**As an** agency employee (Sarah)
**I want** to edit a time entry I created
**So that** I can correct mistakes or add details

**Acceptance Criteria**:
- User can click edit on any unlocked entry
- User can modify all fields except user
- Duration recalculated if times changed
- Cannot edit approved/invoiced entries
- Changes saved with confirmation

**Priority**: High | **Phase**: MVP

---

### US-009: Delete Time Entry
**As a** freelancer (Felix)
**I want** to delete a time entry I accidentally created
**So that** my time log is accurate

**Acceptance Criteria**:
- User can delete unlocked entries
- Confirmation dialog shown before deletion
- Entry soft-deleted (can be restored)
- Cannot delete approved/invoiced entries
- Deletion logged in audit trail

**Priority**: High | **Phase**: MVP

---

### US-010: Idle Detection
**As a** freelancer (Felix)
**I want** the system to detect when I'm inactive
**So that** I don't track time when not actually working

**Acceptance Criteria**:
- System monitors mouse/keyboard activity
- After 5 minutes of inactivity, prompt shown
- User can keep time, discard idle time, or stop timer
- Idle threshold configurable in settings
- Idle time highlighted in entry

**Priority**: Medium | **Phase**: Phase 3

**Use Case Flow**:
1. Felix has timer running for "Website Development"
2. Felix steps away for 15 minutes (lunch break)
3. When Felix returns and moves mouse, dialog appears:
   "You've been idle for 15 minutes. What would you like to do?"
   - Keep all time (8:15 total)
   - Discard idle time (8:00 active)
   - Stop timer at idle start
4. Felix selects "Discard idle time"
5. Timer continues with 15 minutes removed

---

### US-011: Weekly Timesheet View
**As an** agency employee (Sarah)
**I want** to view my time entries in a weekly grid
**So that** I can quickly review and complete my timesheet

**Acceptance Criteria**:
- Grid shows days as columns, projects as rows
- Cells show total hours for project/day
- Can click cell to see entry details
- Can edit duration inline
- Shows daily and weekly totals
- Can navigate between weeks

**Priority**: High | **Phase**: Phase 2

**Use Case Flow**:
1. Sarah opens "Timesheet" view
2. Sees week of Oct 28 - Nov 3, 2024
3. Grid shows:
   ```
   Project       Mon   Tue   Wed   Thu   Fri   Total
   Website       8.0   6.0   7.5   8.0   4.0   33.5
   Mobile App    -     2.0   0.5   -     4.0    6.5
   Internal      -     -     -     -     -      0.0
   Total         8.0   8.0   8.0   8.0   8.0   40.0
   ```
4. Sarah double-clicks Wednesday's "Website" cell (7.5)
5. Modal shows 2 time entries for that day/project
6. Sarah edits one entry
7. Cell updates to reflect change

---

## Client & Project Management

### US-012: Create Client
**As an** administrator (Alex)
**I want** to create a new client record
**So that** I can organize projects by client

**Acceptance Criteria**:
- Admin can access client management
- Form includes name, contact info, address
- Client name must be unique
- Client saved with success message
- Client appears in client list

**Priority**: High | **Phase**: MVP

---

### US-013: Create Project
**As a** project manager (Maria)
**I want** to create a project for a client
**So that** my team can track time to that project

**Acceptance Criteria**:
- Manager can access project creation
- Form includes name, client, hourly rate, description
- Project name unique within client
- Hourly rate required and positive
- Project saved and available for time tracking

**Priority**: High | **Phase**: MVP

**Use Case Flow**:
1. Maria needs to set up new project for existing client
2. Navigates to Projects → "Create Project"
3. Enters project name: "Mobile App Development"
4. Selects client: "TechCorp Inc."
5. Sets hourly rate: €120.00
6. Sets estimated hours: 200
7. Adds description: "iOS and Android app for inventory management"
8. Sets project as Active
9. Clicks "Create Project"
10. Project saved and appears in project list
11. Team members can now track time to this project

---

### US-014: Assign Team to Project
**As a** project manager (Maria)
**I want** to assign team members to a project
**So that** only authorized users can track time to it

**Acceptance Criteria**:
- Manager can access project team management
- Can add/remove users from project
- Can set user role (owner, member, viewer)
- Only assigned users see project in time tracking
- Changes saved and effective immediately

**Priority**: Medium | **Phase**: MVP

---

### US-015: Track Project Budget
**As a** project manager (Maria)
**I want** to set and track a project budget
**So that** I can ensure projects stay within budget

**Acceptance Criteria**:
- Can set hours budget or amount budget
- Dashboard shows budget utilization
- Warning at 80% utilization
- Alert at 100% utilization
- Cannot exceed budget (configurable)
- Budget progress shown visually

**Priority**: Medium | **Phase**: Phase 2

**Use Case Flow**:
1. Maria sets budget for "Website Redesign": 160 hours
2. Team tracks time over several weeks
3. After 3 weeks, 128 hours tracked (80% utilized)
4. Maria sees orange warning indicator: "Budget 80% utilized"
5. Week 4, team continues work
6. At 160 hours (100%), Maria receives notification
7. Budget bar shows 100% (red)
8. Maria can choose to:
   - Extend budget to 200 hours
   - Mark project as over-budget and continue
   - Pause project and discuss with client

---

### US-016: Archive Completed Project
**As a** project manager (Maria)
**I want** to archive completed projects
**So that** active project lists remain focused on current work

**Acceptance Criteria**:
- Manager can change project status to Archived
- Archived projects hidden from default views
- Cannot track time to archived projects
- Historical data preserved
- Can restore archived projects

**Priority**: Medium | **Phase**: MVP

---

## Reporting & Export

### US-017: View Monthly Time Summary
**As a** freelancer (Felix)
**I want** to see a summary of my tracked time for the month
**So that** I know how much I've worked and earned

**Acceptance Criteria**:
- Dashboard shows current month summary
- Displays total hours, billable hours, revenue
- Breakdown by client and project
- Can select different months
- Visual charts for distribution

**Priority**: High | **Phase**: MVP

---

### US-018: Export Monthly Timesheet to Excel
**As a** freelancer (Felix)
**I want** to export my monthly timesheet to Excel
**So that** I can create an invoice for my client

**Acceptance Criteria**:
- User selects month and clicks "Export to Excel"
- Excel file generated with summary and details
- German formatting (dates, numbers, currency)
- File downloadable within 10 seconds
- File includes professional formatting

**Priority**: Critical | **Phase**: MVP

**Use Case Flow**:
1. Felix needs to invoice "Client A" for October 2024
2. Opens Reports → "Monthly Timesheet"
3. Selects month: October 2024
4. Filters by client: "Client A"
5. Clicks "Export to Excel"
6. Progress indicator shows "Generating export..."
7. After 3 seconds, download link appears
8. Felix clicks "Download"
9. Excel file opens with:
   - Summary sheet: 82 hours, €8,200.00
   - Details sheet: All time entries with dates, descriptions, hours
   - Professional formatting with company logo
10. Felix uses this to create invoice

---

### US-019: Export Monthly Timesheet to PDF
**As a** consultant (Thomas)
**I want** to export my monthly timesheet to PDF
**So that** I can send a professional report to my client

**Acceptance Criteria**:
- User selects month and clicks "Export to PDF"
- PDF generated with German invoice format
- Includes company logo and branding
- Professional layout suitable for client
- Download available within 10 seconds

**Priority**: Critical | **Phase**: MVP

**Use Case Flow**:
1. Thomas completes work for October 2024
2. Opens Reports → "Monthly Report"
3. Selects October 2024
4. Clicks "Export to PDF"
5. System generates PDF with:
   - Header with logo and Thomas's name
   - "Stundennachweis - Oktober 2024"
   - Summary: 168,5 Stunden, 16.850,00 €
   - Daily breakdown with projects
   - Weekly subtotals
   - Terms and signature line
6. PDF downloads automatically
7. Thomas reviews and sends to client

---

### US-020: Team Time Report
**As a** project manager (Maria)
**I want** to view time tracked by all team members
**So that** I can monitor team productivity and project progress

**Acceptance Criteria**:
- Manager can access team reports
- Shows time entries for all team members
- Can filter by date range, project, user
- Summary by user and project
- Export to Excel or PDF

**Priority**: High | **Phase**: Phase 2

---

### US-021: Project Profitability Report
**As a** project manager (Maria)
**I want** to see project profitability
**So that** I can identify profitable and unprofitable projects

**Acceptance Criteria**:
- Report shows revenue vs. cost per project
- Cost calculated from employee rates or standard rate
- Profit margin displayed as percentage
- Can compare multiple projects
- Visual indicators for profitable/unprofitable

**Priority**: Medium | **Phase**: Phase 3

---

## Approval Workflow

### US-022: Submit Timesheet for Approval
**As an** agency employee (Sarah)
**I want** to submit my weekly timesheet for manager approval
**So that** my hours are officially recorded and can be invoiced

**Acceptance Criteria**:
- Employee can select week and click "Submit"
- Cannot submit incomplete timesheets (validation)
- Status changes to "Pending Approval"
- Manager receives notification
- Submitted entries cannot be edited

**Priority**: Medium | **Phase**: Phase 2

---

### US-023: Approve Timesheet
**As a** project manager (Maria)
**I want** to review and approve team timesheets
**So that** I can ensure accuracy before invoicing

**Acceptance Criteria**:
- Manager sees list of pending timesheets
- Can view details of each timesheet
- Can approve or reject with comments
- Approved entries are locked
- Rejected entries return to employee for correction

**Priority**: Medium | **Phase**: Phase 2

**Use Case Flow**:
1. Sarah submits timesheet for week of Oct 28
2. Maria receives notification: "Sarah submitted timesheet"
3. Maria opens "Pending Approvals"
4. Sees Sarah's timesheet: 40 hours total
5. Reviews each entry for accuracy
6. Notices one entry has vague description
7. Clicks "Reject" and adds comment: "Please clarify task description for Oct 30 entry"
8. Sarah receives notification
9. Sarah edits entry with better description
10. Resubmits timesheet
11. Maria reviews again and clicks "Approve"
12. Timesheet locked and ready for invoicing

---

## Dashboard & Analytics

### US-024: Daily Time Summary
**As a** freelancer (Felix)
**I want** to see today's tracked time on my dashboard
**So that** I know if I've met my daily hours goal

**Acceptance Criteria**:
- Dashboard shows today's total hours
- Shows breakdown by project
- Compares to daily goal (if set)
- Lists recent time entries
- Shows running timer (if active)

**Priority**: Medium | **Phase**: Phase 2

---

### US-025: Weekly Activity Heatmap
**As a** consultant (Thomas)
**I want** to see a heatmap of my weekly activity
**So that** I can identify my most productive times

**Acceptance Criteria**:
- Heatmap shows hours by day and time slot
- Color intensity indicates activity level
- Can select different weeks
- Interactive (click to see details)
- Helps identify work patterns

**Priority**: Low | **Phase**: Phase 3

---

## Advanced Features

### US-026: Recurring Time Entries
**As a** consultant (Thomas)
**I want** to create recurring time entries
**So that** I don't have to manually enter daily standup meetings

**Acceptance Criteria**:
- Can create recurring entry template
- Supports daily, weekly, monthly patterns
- Automatically creates entries on schedule
- Can edit or skip individual occurrences
- Can end recurrence

**Priority**: Low | **Phase**: Phase 4

---

### US-027: Calendar Integration
**As a** consultant (Thomas)
**I want** to sync time entries with my Google Calendar
**So that** my calendar and time tracking are consistent

**Acceptance Criteria**:
- Can connect Google Calendar account
- Calendar events automatically create time entry suggestions
- Can accept or reject suggestions
- Two-way sync supported
- Can disconnect at any time

**Priority**: Low | **Phase**: Phase 4

---

### US-028: Mobile Time Tracking
**As a** freelancer (Felix)
**I want** to track time on my mobile device
**So that** I can track time while working on-site or remotely

**Acceptance Criteria**:
- Responsive web design works on mobile
- Timer accessible from mobile
- Quick entry for common projects
- Mobile-optimized forms
- Offline support (sync when online)

**Priority**: Medium | **Phase**: Phase 3

---

## Use Case: Complete Monthly Invoicing Workflow

**Actors**: Felix (Freelancer), Client

**Preconditions**:
- Felix has tracked time throughout October
- Client "ABC Corp" has active projects
- Hourly rate is €100

**Main Flow**:

1. **Track Time Throughout Month**
   - Felix uses timer and manual entries all month
   - October: 42 time entries, 87.5 hours total

2. **End of Month Review (Nov 1)**
   - Felix opens Reports → "This Month"
   - Reviews all October entries
   - Edits 2 entries with missing descriptions
   - Marks 3 internal calls as non-billable

3. **Filter Client Time**
   - Applies filter: Client = "ABC Corp"
   - Result: 72 hours billable, 3.5 hours non-billable

4. **Generate Excel Report**
   - Clicks "Export to Excel"
   - System generates file with:
     - Summary: 72 hours, €7,200.00
     - Daily breakdown
     - Project breakdown
   - Downloads file

5. **Generate PDF Invoice**
   - Clicks "Export to PDF"
   - System generates professional PDF:
     - Header with Felix's company info
     - "Stundennachweis - Oktober 2024"
     - Client: ABC Corp
     - Period: 01.10.2024 - 31.10.2024
     - Total: 72,0 Stunden à €100,00 = €7.200,00
     - Daily details
     - Terms and signature
   - Downloads PDF

6. **Create Invoice in Accounting Software**
   - Felix opens accounting software
   - Creates invoice referencing PDF
   - Line item: "Software Development Services - October 2024"
   - Amount: €7,200.00
   - Attaches TiroTime PDF as supporting documentation

7. **Send to Client**
   - Felix emails invoice and PDF to client
   - Client reviews attached timesheet
   - Client approves and pays within 30 days

**Alternative Flow: Generate Invoice Directly (Phase 3)**
- Felix uses TiroTime's invoice generation
- Creates invoice from time entries
- System generates formal invoice with number
- Felix sends directly from TiroTime
- Invoice status tracked in system

---

## Summary Statistics

- **Total User Stories**: 28
- **MVP Stories**: 15
- **Phase 2 Stories**: 7
- **Phase 3 Stories**: 4
- **Phase 4 Stories**: 2

**Priority Breakdown**:
- Critical: 4
- High: 10
- Medium: 11
- Low: 3

---

**Document Owner**: Product Team
**Reviewers**: Development Team, UX Team, Stakeholders
**Approval Status**: Pending
