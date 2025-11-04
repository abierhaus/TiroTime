# Feature Specification: Client & Project Management
**Specification ID**: 004
**Version**: 1.0
**Status**: Draft
**Parent**: [001 - TiroTime Overview](../001-tirotime-overview.md)

## Overview

Client and Project Management enables users to organize their work by clients and projects. This module provides the organizational structure for time tracking, allowing users to associate time entries with specific projects and maintain detailed information about clients and their associated work.

## Core Concepts

### Client
A client represents a customer or organization for whom work is performed. Clients are the top-level organizational entity and can have multiple projects.

### Project
A project represents a specific engagement, assignment, or work stream for a client. Projects contain the hourly rate information and serve as the primary categorization for time entries.

## Functional Requirements

### FR-CP-001: Create Client
**Priority**: High
**Phase**: MVP

Users can create new clients with detailed information.

**Client Fields**:
- Name (required, max 200 chars)
- Company name (optional, max 200 chars)
- Contact person (optional, max 100 chars)
- Email (optional, validated format)
- Phone (optional, max 20 chars)
- Address (optional)
  - Street
  - City
  - Postal code
  - State/Province
  - Country
- Billing address (optional, can differ from main address)
- Tax ID / VAT number (optional)
- Currency (required, default from user settings)
- Notes (optional, max 2000 chars)
- Status (Active/Inactive)
- Color (for visual identification)

**Business Rules**:
- Client name must be unique per user/organization
- At least one of: company name or contact person required
- Email validated if provided
- Currency cannot be changed if invoices exist
- Default status: Active
- Admins and Managers can create clients
- Employees can only view assigned clients

**Acceptance Criteria**:
- Given valid client data, when user creates client, then client is saved
- Given duplicate client name, when user saves, then validation error is shown
- Given invalid email, when user saves, then validation error is shown
- Given employee role, when attempting to create client, then access denied

### FR-CP-002: View Clients
**Priority**: High
**Phase**: MVP

Users can view list of clients and client details.

**Client List Features**:
- Paginated list (default 20 per page)
- Search by name, company, contact person
- Filter by status (active/inactive)
- Sort by name, creation date, last activity
- Grid or card view
- Quick actions (edit, archive, view projects)
- Show project count per client
- Show total tracked hours per client

**Client Detail View**:
- All client information
- List of associated projects
- Recent time entries
- Total hours tracked
- Total revenue (if applicable)
- Activity timeline
- Edit button (if authorized)

**Business Rules**:
- Employees see only assigned clients
- Managers see clients for their projects
- Admins see all clients
- Inactive clients shown separately or filtered

### FR-CP-003: Edit Client
**Priority**: High
**Phase**: MVP

Authorized users can update client information.

**Editable Fields**:
- All fields from creation except currency (if invoices exist)

**Business Rules**:
- Cannot edit deleted clients
- Currency locked if invoices exist
- Changes logged in audit trail
- Only admins and managers can edit
- Cannot change client if invoices are locked

**Acceptance Criteria**:
- Given authorized user, when editing client, then changes are saved
- Given employee role, when attempting to edit, then access denied
- Given client with invoices, when changing currency, then error is shown
- Given archived client, when attempting to edit, then warning is shown

### FR-CP-004: Archive/Delete Client
**Priority**: Medium
**Phase**: MVP

Users can archive or delete clients.

**Archive**:
- Soft delete (mark as inactive)
- Hidden from active lists
- Cannot track time to archived clients
- Can be restored
- Preserves all historical data

**Delete**:
- Hard delete only if no time entries or invoices
- Confirmation required with impact warning
- Shows count of projects, time entries, invoices

**Business Rules**:
- Archive preferred over delete
- Cannot delete client with time entries
- Cannot delete client with invoices
- Can archive client anytime
- Only admins can delete clients
- Archived clients can be restored

### FR-CP-005: Create Project
**Priority**: High
**Phase**: MVP

Users can create projects associated with clients.

**Project Fields**:
- Name (required, max 200 chars)
- Client (required, dropdown selection)
- Description (optional, max 2000 chars)
- Hourly rate (required, decimal with 2 decimals)
- Currency (inherited from client)
- Project code (optional, max 20 chars)
- Start date (optional)
- End date (optional)
- Estimated hours (optional)
- Budget amount (optional)
- Budget type (hours or amount)
- Billable by default (boolean, default true)
- Status (Active, Paused, Completed, Archived)
- Color (for visual identification, default from client)
- Task template (optional, Phase 2)

**Business Rules**:
- Project name must be unique within client
- End date must be after start date
- Hourly rate must be positive
- Currency inherited from client (cannot differ)
- Default status: Active
- Color defaults to client color
- Only active projects available for time tracking
- Admins and Managers can create projects
- Employees can view assigned projects only

**Acceptance Criteria**:
- Given valid project data, when user creates project, then project is saved
- Given duplicate project name for client, when user saves, then error shown
- Given end date before start date, when user saves, then validation error
- Given negative hourly rate, when user saves, then validation error

### FR-CP-006: View Projects
**Priority**: High
**Phase**: MVP

Users can view list of projects and project details.

**Project List Features**:
- Paginated list (default 20 per page)
- Search by name, code, description
- Filter by:
  - Client
  - Status
  - Assigned user
  - Date range
- Sort by name, client, start date, hours tracked
- Grid or card view
- Group by client
- Show tracked hours per project
- Show budget utilization
- Color coding
- Favorite/star projects

**Project Detail View**:
- All project information
- Client information (linked)
- Assigned users
- Time entries summary
- Recent time entries
- Budget tracking (progress bar)
- Revenue calculation
- Activity timeline
- Quick actions (edit, archive, start timer)

**Business Rules**:
- Employees see only assigned projects
- Managers see projects they manage
- Admins see all projects
- Archived projects shown separately
- Default view: active projects only

### FR-CP-007: Edit Project
**Priority**: High
**Phase**: MVP

Authorized users can update project information.

**Editable Fields**:
- All fields from creation
- Cannot change client if time entries exist (warning shown)

**Business Rules**:
- Cannot edit deleted projects
- Client change requires confirmation if time entries exist
- Hourly rate change affects future entries only
- Changes logged in audit trail
- Only admins and managers can edit
- Cannot change project if invoices are locked

### FR-CP-008: Archive/Delete Project
**Priority**: Medium
**Phase**: MVP

Users can archive or delete projects.

**Archive**:
- Set status to Archived
- Hidden from active lists
- Cannot track time to archived projects
- Can be restored (set status back to Active)
- Preserves all historical data

**Delete**:
- Hard delete only if no time entries
- Confirmation required
- Shows impact (time entry count)

**Business Rules**:
- Archive preferred over delete
- Cannot delete project with time entries
- Can archive project anytime
- Only admins can delete projects
- Archived projects can be reactivated

### FR-CP-009: Project Budget Tracking
**Priority**: Medium
**Phase**: Phase 2

Track project budgets and utilization.

**Budget Types**:
1. **Hours Budget**: Maximum hours allocated
2. **Amount Budget**: Maximum monetary value

**Budget Features**:
- Set budget limit (hours or amount)
- Track current utilization
- Visual progress indicator
- Warning at 80% utilization
- Alert at 100% utilization
- Notification to project manager
- Budget history (changes over time)

**Business Rules**:
- Budget tracking optional per project
- Can exceed budget (warning only)
- Utilization updates in real-time
- Budget can be adjusted
- History preserved for adjustments

**Acceptance Criteria**:
- Given project with hours budget, when time tracked, then utilization updates
- Given budget at 80%, when threshold reached, then warning shown
- Given budget exceeded, when tracking time, then alert shown
- Given budget adjustment, when saved, then history record created

### FR-CP-010: Project Team Assignment
**Priority**: Medium
**Phase**: MVP

Assign users to projects for access control.

**Assignment Features**:
- Add users to project
- Remove users from project
- Assign roles (Owner, Member, Viewer)
- Set default hourly rate per user (can override project rate)
- Set access dates (from/to)

**Project Roles**:
- **Owner**: Can edit project, manage team, view all time entries
- **Member**: Can track time, view own entries
- **Viewer**: Can view project info, cannot track time

**Business Rules**:
- Only assigned users can track time to project
- At least one owner required
- Admins always have access to all projects
- Managers have access to projects in their team
- User-specific rates override project rate

### FR-CP-011: Project Status Management
**Priority**: Medium
**Phase**: MVP

Manage project lifecycle through status changes.

**Project Statuses**:
- **Active**: Normal operation, time tracking enabled
- **Paused**: Temporarily inactive, time tracking disabled
- **Completed**: Work finished, time tracking disabled, appears in reports
- **Archived**: Historical, hidden from default views

**Status Transitions**:
- Active → Paused, Completed, Archived
- Paused → Active, Archived
- Completed → Archived
- Archived → Active (restore)

**Business Rules**:
- Cannot track time to Paused, Completed, or Archived projects
- Completed projects included in completed work reports
- Status changes logged with reason (optional)
- Automatic status change: Completed when end date reached (optional)

### FR-CP-012: Favorite Projects
**Priority**: Low
**Phase**: Phase 2

Users can mark frequently used projects as favorites.

**Favorite Features**:
- Star/favorite icon on projects
- Favorites shown at top of lists
- Quick access in time entry form
- Favorites persist per user

**Business Rules**:
- Favorites are user-specific
- No limit on favorite count
- Archived projects can remain favorited

### FR-CP-013: Client Contact Management
**Priority**: Low
**Phase**: Phase 3

Manage multiple contacts per client.

**Contact Fields**:
- Name
- Title/Position
- Email
- Phone
- Primary contact flag
- Notes

**Business Rules**:
- Multiple contacts per client
- One primary contact required
- Primary contact used for invoicing

### FR-CP-014: Project Templates
**Priority**: Low
**Phase**: Phase 3

Create reusable project templates.

**Template Features**:
- Save project as template
- Create project from template
- Template includes:
  - Default name pattern
  - Description
  - Hourly rate
  - Estimated hours
  - Task list
  - Default team assignments

**Business Rules**:
- Templates are organization-wide
- Only admins can manage templates
- Creating from template allows customization

### FR-CP-015: Client/Project Custom Fields
**Priority**: Low
**Phase**: Phase 4

Add custom fields to clients and projects.

**Custom Field Types**:
- Text (short and long)
- Number
- Date
- Dropdown (single select)
- Checkbox (boolean)
- URL

**Business Rules**:
- Admins define custom fields
- Fields can be required or optional
- Fields searchable and filterable
- Up to 20 custom fields per entity

## API Endpoints

### Client Endpoints

```
GET    /api/v1/clients                # List clients
GET    /api/v1/clients/{id}           # Get client details
POST   /api/v1/clients                # Create client
PUT    /api/v1/clients/{id}           # Update client
DELETE /api/v1/clients/{id}           # Delete client (if no dependencies)
PATCH  /api/v1/clients/{id}/archive   # Archive client
PATCH  /api/v1/clients/{id}/restore   # Restore archived client
GET    /api/v1/clients/{id}/projects  # Get client's projects
GET    /api/v1/clients/{id}/stats     # Get client statistics
```

### Project Endpoints

```
GET    /api/v1/projects                   # List projects
GET    /api/v1/projects/{id}              # Get project details
POST   /api/v1/projects                   # Create project
PUT    /api/v1/projects/{id}              # Update project
DELETE /api/v1/projects/{id}              # Delete project (if no entries)
PATCH  /api/v1/projects/{id}/status       # Update project status
PATCH  /api/v1/projects/{id}/favorite     # Toggle favorite
GET    /api/v1/projects/{id}/team         # Get project team
POST   /api/v1/projects/{id}/team         # Add user to project
DELETE /api/v1/projects/{id}/team/{userId} # Remove user from project
GET    /api/v1/projects/{id}/budget       # Get budget info
PUT    /api/v1/projects/{id}/budget       # Update budget
GET    /api/v1/projects/{id}/stats        # Get project statistics
GET    /api/v1/projects/favorites         # Get user's favorite projects
GET    /api/v1/projects/recent            # Get recently used projects
```

## Data Model

### Client Entity

```csharp
public class Client : AggregateRoot
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string? CompanyName { get; private set; }
    public ContactInfo? PrimaryContact { get; private set; }
    public Address? Address { get; private set; }
    public Address? BillingAddress { get; private set; }
    public string? TaxId { get; private set; }
    public string CurrencyCode { get; private set; }
    public string? Notes { get; private set; }
    public ClientStatus Status { get; private set; }
    public Color Color { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    // Collections
    public IReadOnlyList<Project> Projects { get; private set; }

    // Domain methods
    public void UpdateDetails(string name, string? companyName, ContactInfo? contact);
    public void UpdateAddress(Address address);
    public void UpdateBillingAddress(Address billingAddress);
    public void Archive();
    public void Restore();
    public void ChangeColor(Color color);
    public bool CanDelete(); // No projects with time entries
}

public enum ClientStatus
{
    Active = 0,
    Inactive = 1
}
```

### Project Entity

```csharp
public class Project : AggregateRoot
{
    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Money HourlyRate { get; private set; }
    public string? ProjectCode { get; private set; }
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public ProjectBudget? Budget { get; private set; }
    public bool IsBillableByDefault { get; private set; }
    public ProjectStatus Status { get; private set; }
    public Color Color { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    // Navigation properties
    public Client Client { get; private set; }
    public IReadOnlyList<ProjectMember> TeamMembers { get; private set; }
    public IReadOnlyList<TimeEntry> TimeEntries { get; private set; }

    // Domain methods
    public void UpdateDetails(string name, string? description, Money hourlyRate);
    public void UpdateDates(DateTime? startDate, DateTime? endDate);
    public void UpdateBudget(ProjectBudget budget);
    public void ChangeStatus(ProjectStatus newStatus);
    public void AddTeamMember(Guid userId, ProjectRole role, Money? customRate = null);
    public void RemoveTeamMember(Guid userId);
    public void ChangeColor(Color color);
    public bool CanTrackTime(); // Status is Active
    public bool CanDelete(); // No time entries
    public ProjectBudgetStatus GetBudgetStatus();
    public Money CalculateTotalRevenue();
    public TimeSpan CalculateTotalHours();
}

public enum ProjectStatus
{
    Active = 0,
    Paused = 1,
    Completed = 2,
    Archived = 3
}

public enum ProjectRole
{
    Viewer = 0,
    Member = 1,
    Owner = 2
}
```

### Value Objects

```csharp
public record ContactInfo
{
    public string? Name { get; init; }
    public Email? Email { get; init; }
    public string? Phone { get; init; }
}

public record Address
{
    public string Street { get; init; }
    public string City { get; init; }
    public string PostalCode { get; init; }
    public string? StateProvince { get; init; }
    public string Country { get; init; }
}

public record Money
{
    public decimal Amount { get; init; }
    public string CurrencyCode { get; init; }

    public Money Add(Money other);
    public Money Multiply(decimal factor);
}

public record Color
{
    public string HexValue { get; init; }
    // Validation: valid hex color
}

public record ProjectBudget
{
    public BudgetType Type { get; init; }
    public decimal Limit { get; init; }
    public DateTime? ResetDate { get; init; }
}

public enum BudgetType
{
    Hours = 0,
    Amount = 1
}

public class ProjectMember : Entity
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid UserId { get; private set; }
    public ProjectRole Role { get; private set; }
    public Money? CustomHourlyRate { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }
}
```

## Validation Rules

### Client Validation

| Field | Rule |
|-------|------|
| Name | Required, 1-200 chars, unique per organization |
| Company Name | Optional, max 200 chars |
| Email | Optional, valid email format |
| Phone | Optional, max 20 chars |
| Currency | Required, valid ISO 4217 code |
| Tax ID | Optional, max 50 chars |

### Project Validation

| Field | Rule |
|-------|------|
| Name | Required, 1-200 chars, unique per client |
| Client | Required, must exist and be active |
| Description | Optional, max 2000 chars |
| Hourly Rate | Required, must be positive |
| Project Code | Optional, max 20 chars, unique per organization |
| Start Date | Optional, valid date |
| End Date | Optional, must be after start date |
| Estimated Hours | Optional, must be positive |
| Budget Amount | Optional, must be positive |

## Business Rules Summary

1. **Client Uniqueness**: Client name unique per organization
2. **Project Uniqueness**: Project name unique per client
3. **Currency Inheritance**: Project currency inherited from client
4. **Active Status for Tracking**: Only active projects allow time tracking
5. **Deletion Protection**: Cannot delete clients/projects with time entries
6. **Archive Over Delete**: Prefer archiving to maintain historical data
7. **Budget Warnings**: Notify at 80% and 100% budget utilization
8. **Access Control**: Users only see assigned projects (unless admin/manager)
9. **At Least One Owner**: Projects must have at least one owner
10. **Hourly Rate Positive**: Hourly rate must be greater than zero

## Performance Requirements

- List clients/projects: < 300ms
- Get client/project details: < 200ms
- Create client/project: < 500ms
- Update client/project: < 500ms
- Search clients/projects: < 400ms
- Calculate project statistics: < 1 second

## Security Requirements

- Admins can manage all clients and projects
- Managers can manage clients and projects in their scope
- Employees can only view assigned projects
- Cannot view deleted/archived entities without permission
- Audit log for all modifications
- Rate limiting: 100 requests per minute per user

## Testing Requirements

### Unit Tests
- Client and Project entity domain logic
- Validation rules
- Business rule enforcement
- Budget calculations
- Status transitions

### Integration Tests
- Client CRUD operations
- Project CRUD operations
- Team assignment flows
- Budget tracking updates
- Status change workflows

### E2E Tests
- Create client and associated projects
- Project lifecycle (create → active → complete → archive)
- Budget utilization and warnings
- Team assignment and permission checks

## Dependencies

- User Management module
- MediatR for commands/queries
- FluentValidation
- Entity Framework Core

## Future Enhancements (Phase 4+)

- Multi-currency support with exchange rates
- Client hierarchy (parent/child clients)
- Project phases and milestones
- Resource allocation and capacity planning
- Gantt chart for project timelines
- Project profitability analysis
- Contract and SOW management
- Client portal for viewing progress
- Project risk tracking
- Dependency management between projects

---

**Document Owner**: Development Team
**Reviewers**: Product Team, Finance Team
**Approval Status**: Pending
