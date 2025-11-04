# Feature Specification: Time Tracking
**Specification ID**: 003
**Version**: 1.0
**Status**: Draft
**Parent**: [001 - TiroTime Overview](../001-tirotime-overview.md)

## Overview

Time Tracking is the core functionality of TiroTime, enabling users to record time spent on client projects. This module provides multiple time entry methods, flexible tracking options, and comprehensive time management capabilities inspired by leading time tracking solutions (Toggl, Harvest, Clockify).

## Core Concepts

### Time Entry
A time entry represents a discrete period of work on a specific project. Each entry contains:
- **Who**: User who performed the work
- **What**: Project and task description
- **When**: Start time and duration
- **How Much**: Calculated based on project hourly rate
- **Type**: Billable or non-billable

### Timer
A running stopwatch that tracks time in real-time. Only one timer can run per user at a time.

### Timesheet
A structured view of time entries organized by day or week, allowing quick review and bulk entry.

## Functional Requirements

### FR-TT-001: Start/Stop Timer
**Priority**: Critical
**Phase**: MVP

Users can start a timer to track time in real-time.

**Timer Features**:
- Start timer with project selection
- Optional task description
- Display running time (HH:MM:SS format)
- Stop timer to create time entry
- Discard timer without saving

**Business Rules**:
- Only one timer can run per user
- Timer persists across page refreshes and browser sessions
- Timer continues running if browser is closed
- Maximum continuous timer duration: 24 hours
- Timer data synced to server every 30 seconds
- Starting new timer stops existing timer (with confirmation)

**Acceptance Criteria**:
- Given no running timer, when user starts timer, then timer begins counting
- Given running timer, when user stops timer, then time entry is created
- Given running timer, when user closes browser and returns, then timer continues from correct time
- Given timer running for 24 hours, when duration exceeds limit, then user is notified
- Given running timer, when user starts new timer, then confirmation dialog appears

**UI Components**:
- Timer widget (always visible in header/sidebar)
- Project/task selection dropdown
- Start/Stop button
- Current duration display
- Discard button

### FR-TT-002: Manual Time Entry
**Priority**: Critical
**Phase**: MVP

Users can manually enter time entries with specific start and end times.

**Manual Entry Fields**:
- Project (required)
- Task description (optional, max 500 chars)
- Date (required)
- Start time (required)
- End time (required)
- Duration (calculated automatically)
- Billable flag (default from project)
- Tags (optional)
- Notes (optional, max 2000 chars)

**Business Rules**:
- End time must be after start time
- Entry cannot span multiple days
- Cannot create entries in the future
- Cannot overlap with existing entries (warning, not error)
- Cannot modify locked entries (approved or invoiced)
- Maximum entry duration: 24 hours

**Acceptance Criteria**:
- Given valid start/end times, when user creates entry, then duration is calculated correctly
- Given end time before start time, when user saves, then validation error is shown
- Given overlapping times, when user saves, then warning is displayed
- Given future date, when user saves, then validation error is shown

### FR-TT-003: Duration-Based Entry
**Priority**: High
**Phase**: MVP

Users can enter time by specifying duration directly without start/end times.

**Duration Entry Fields**:
- Project (required)
- Task description (optional)
- Date (required)
- Duration (required, formats: "2h", "1.5h", "90m", "1h 30m")
- Billable flag
- Tags, notes

**Business Rules**:
- Duration assigned to a specific time slot (user's working hours)
- If no working hours defined, use 09:00 as default start
- Support multiple duration formats
- Minimum duration: 1 minute
- Maximum duration: 24 hours

**Duration Format Examples**:
- `2` or `2h` = 2 hours
- `30m` = 30 minutes
- `1.5h` = 1 hour 30 minutes
- `1h 30m` or `1:30` = 1 hour 30 minutes
- `90m` = 1 hour 30 minutes

### FR-TT-004: Quick Entry Presets
**Priority**: Medium
**Phase**: Phase 2

Users can add time entries using preset durations for common time blocks.

**Preset Options**:
- 15 minutes
- 30 minutes
- 45 minutes
- 1 hour
- 2 hours
- 4 hours
- Custom presets (user-defined)

**Business Rules**:
- Quick entry creates manual time entry with current date
- Time slot assigned based on user's working hours
- User can customize preset durations in settings

### FR-TT-005: Calendar View Entry
**Priority**: Medium
**Phase**: Phase 2

Users can add time entries by clicking on a calendar.

**Calendar Features**:
- Month view with day cells
- Week view with time slots (30-min increments)
- Day view with hourly slots (15-min increments)
- Click to create time entry for selected slot
- Drag to select time range
- Visual indicators for existing entries
- Color coding by project

**Business Rules**:
- Calendar shows user's own entries only
- Managers can view team calendars
- Click behavior: opens time entry form with pre-filled date/time
- Drag behavior: creates entry with selected duration

### FR-TT-006: Timesheet View Entry
**Priority**: High
**Phase**: Phase 2

Users can view and edit time entries in a grid/spreadsheet format.

**Timesheet Features**:
- Weekly grid: columns = days, rows = projects
- Quick cell editing (double-click to edit duration)
- Total hours per day (column footer)
- Total hours per project (row footer)
- Grand total (bottom-right corner)
- Copy time entries from previous week
- Duplicate previous day's entries

**Business Rules**:
- One row per project
- Empty cells = no time tracked
- Cell shows total duration for project/day
- Clicking cell shows breakdown if multiple entries
- Inline editing updates time entries

### FR-TT-007: Running Timer Display
**Priority**: High
**Phase**: MVP

System displays running timer persistently across all pages.

**Display Features**:
- Always visible timer widget (header or sidebar)
- Shows current duration (updates every second)
- Shows project name and task description
- Quick stop button
- Click to expand for more options

**Business Rules**:
- Timer visible on all application pages
- Timer syncs with server to prevent data loss
- Visual indicator (color, pulse) for running timer
- Browser tab title shows running time

### FR-TT-008: Idle Detection
**Priority**: Medium
**Phase**: Phase 3

System detects user inactivity and prompts for action.

**Idle Detection Features**:
- Monitor user activity (mouse, keyboard)
- After X minutes of inactivity, show prompt
- Options: Keep time, Discard idle time, Discard all
- Customizable idle threshold (default: 5 minutes)
- Idle time highlighted in entry

**Business Rules**:
- Idle detection only when timer is running
- User can disable idle detection
- Idle threshold: 5-30 minutes (user configurable)
- Prompt appears on activity resume

**Prompt Options**:
1. **Keep all time**: No changes, timer continues
2. **Discard idle time**: Removes inactive period
3. **Stop timer**: Stops timer at idle start time

### FR-TT-009: Edit Time Entry
**Priority**: High
**Phase**: MVP

Users can edit existing time entries.

**Editable Fields**:
- Project
- Task description
- Date
- Start time
- End time
- Duration
- Billable flag
- Tags
- Notes

**Business Rules**:
- Cannot edit locked entries (approved/invoiced)
- Cannot edit other users' entries (unless manager/admin)
- Validation same as creating entry
- Audit log records all changes
- Cannot change entry older than X days (configurable, default: 90)

**Acceptance Criteria**:
- Given unlocked entry, when user edits and saves, then changes are persisted
- Given locked entry, when user attempts to edit, then error message is shown
- Given manager role, when editing team entry, then changes are allowed
- Given editing that creates overlap, when user saves, then warning is displayed

### FR-TT-010: Delete Time Entry
**Priority**: High
**Phase**: MVP

Users can delete time entries.

**Business Rules**:
- Cannot delete locked entries
- Cannot delete other users' entries (unless manager/admin)
- Soft delete (entries marked as deleted, not removed)
- Deleted entries can be restored within 30 days
- Confirmation required before deletion

**Acceptance Criteria**:
- Given unlocked entry, when user deletes with confirmation, then entry is soft-deleted
- Given locked entry, when user attempts to delete, then error message is shown
- Given deleted entry within 30 days, when user restores, then entry becomes active

### FR-TT-011: Duplicate Time Entry
**Priority**: Low
**Phase**: Phase 2

Users can duplicate existing entries to quickly create similar entries.

**Duplicate Behavior**:
- Copies all fields except date and times
- Opens edit form with copied data
- User sets new date and times
- Saves as new entry

### FR-TT-012: Billable/Non-Billable Toggle
**Priority**: High
**Phase**: MVP

Users can mark time entries as billable or non-billable.

**Business Rules**:
- Default billable status from project settings
- User can override per entry
- Visual distinction (icon, color) in UI
- Cannot change billable status on locked entries
- Reports can filter by billable status

### FR-TT-013: Tags/Categories
**Priority**: Medium
**Phase**: Phase 2

Users can categorize time entries with tags.

**Tag Features**:
- Create custom tags
- Assign multiple tags per entry
- Color-code tags
- Search and filter by tags
- Suggested tags based on project/task

**Business Rules**:
- Tags are user-specific (unless shared)
- Maximum 10 tags per entry
- Tag names: 1-30 characters
- Case-insensitive tag matching

### FR-TT-014: Break Management
**Priority**: Medium
**Phase**: Phase 2

System can automatically deduct breaks from time entries.

**Break Rules**:
- Configurable break policies (e.g., 30 min for > 6 hours)
- Manual break entry option
- Automatic break deduction based on work duration
- Break time shown separately in entries

**Example Break Policies**:
- Work > 6 hours: deduct 30 minutes
- Work > 8 hours: deduct 45 minutes
- Work > 10 hours: deduct 60 minutes

### FR-TT-015: Overtime Tracking
**Priority**: Medium
**Phase**: Phase 3

System tracks overtime hours based on user's working hours.

**Overtime Features**:
- Calculate overtime based on daily/weekly limits
- Configurable overtime rules
- Visual indicator for overtime entries
- Overtime summary in reports
- Different rates for overtime (optional)

**Business Rules**:
- Standard work day: from user's working hours setting
- Overtime = hours exceeding standard hours
- Weekend work can be marked as overtime automatically
- Holiday work marked as special overtime

### FR-TT-016: Time Entry Approval
**Priority**: Medium
**Phase**: Phase 2

Managers can review and approve time entries.

**Approval Workflow**:
1. Employee submits entries for approval (weekly/monthly)
2. Manager reviews entries
3. Manager approves or rejects with comments
4. Approved entries are locked (cannot be edited)
5. Rejected entries return to employee for correction

**Business Rules**:
- Only managers and admins can approve
- Approval status: Draft, Submitted, Approved, Rejected
- Cannot submit entries with validation errors
- Cannot approve own entries
- Approved entries can be unapproved (with reason)

### FR-TT-017: Favorites/Recent Projects
**Priority**: Medium
**Phase**: Phase 2

System provides quick access to frequently used projects.

**Features**:
- Favorite projects (starred)
- Recently used projects (last 10)
- Quick select from favorites
- Pin projects to top of list

**Business Rules**:
- Favorites are user-specific
- Recent projects updated on each time entry
- Maximum 20 favorite projects

### FR-TT-018: Reminders
**Priority**: Low
**Phase**: Phase 3

System reminds users to track time.

**Reminder Types**:
- No time tracked today (configurable time)
- Timer running too long (> X hours)
- End of day summary (no timer running)
- Weekly summary (incomplete days)

**Business Rules**:
- Reminders configurable per user
- Can be disabled
- Delivered via in-app notification or email
- Not sent on weekends/holidays (unless configured)

### FR-TT-019: Bulk Operations
**Priority**: Low
**Phase**: Phase 3

Users can perform actions on multiple entries at once.

**Bulk Actions**:
- Delete multiple entries
- Change project for multiple entries
- Toggle billable status
- Add/remove tags
- Submit for approval

**Business Rules**:
- Maximum 100 entries per bulk operation
- Cannot bulk edit locked entries
- Confirmation required for destructive actions

### FR-TT-020: Time Entry Search & Filter
**Priority**: High
**Phase**: MVP

Users can search and filter their time entries.

**Filter Options**:
- Date range (today, yesterday, this week, this month, last month, custom)
- Project
- Client
- Billable/non-billable
- Approval status
- Tags
- User (managers/admins only)

**Search**:
- Free-text search in task descriptions and notes
- Search across all user's entries
- Saved searches/filters (Phase 2)

**Business Rules**:
- Default view: current week entries
- Filters persist in user session
- Export respects current filters

## API Endpoints

### Timer Endpoints

```
GET    /api/v1/timer              # Get current running timer
POST   /api/v1/timer/start        # Start timer
POST   /api/v1/timer/stop         # Stop timer and create entry
DELETE /api/v1/timer              # Discard running timer
PATCH  /api/v1/timer              # Update timer description/project
```

### Time Entry Endpoints

```
GET    /api/v1/time-entries                    # List entries (filtered)
GET    /api/v1/time-entries/{id}               # Get entry details
POST   /api/v1/time-entries                    # Create manual entry
PUT    /api/v1/time-entries/{id}               # Update entry
DELETE /api/v1/time-entries/{id}               # Soft delete entry
POST   /api/v1/time-entries/{id}/restore       # Restore deleted entry
POST   /api/v1/time-entries/{id}/duplicate     # Duplicate entry
PATCH  /api/v1/time-entries/{id}/billable      # Toggle billable status
POST   /api/v1/time-entries/bulk               # Bulk operations
GET    /api/v1/time-entries/summary            # Get summary (total hours)
```

### Approval Endpoints

```
POST   /api/v1/time-entries/submit             # Submit entries for approval
GET    /api/v1/time-entries/pending-approval   # Get pending approvals (manager)
POST   /api/v1/time-entries/{id}/approve       # Approve entry
POST   /api/v1/time-entries/{id}/reject        # Reject entry
POST   /api/v1/time-entries/{id}/unapprove     # Unapprove entry
```

### Tag Endpoints

```
GET    /api/v1/tags               # List user's tags
POST   /api/v1/tags               # Create tag
PUT    /api/v1/tags/{id}          # Update tag
DELETE /api/v1/tags/{id}          # Delete tag
```

## Data Model

### TimeEntry Entity

```csharp
public class TimeEntry : AggregateRoot
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Description { get; private set; }
    public DateTime Date { get; private set; }
    public TimeOnly? StartTime { get; private set; }
    public TimeOnly? EndTime { get; private set; }
    public TimeSpan Duration { get; private set; }
    public bool IsBillable { get; private set; }
    public TimeEntryStatus Status { get; private set; }
    public ApprovalStatus ApprovalStatus { get; private set; }
    public string? Notes { get; private set; }
    public List<Tag> Tags { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Navigation properties
    public User User { get; private set; }
    public Project Project { get; private set; }

    // Domain methods
    public void UpdateDescription(string description);
    public void UpdateTimeRange(TimeOnly start, TimeOnly end);
    public void UpdateDuration(TimeSpan duration);
    public void ToggleBillable();
    public void AddTag(Tag tag);
    public void RemoveTag(Tag tag);
    public void SubmitForApproval();
    public void Approve(Guid approverId, string? comment);
    public void Reject(Guid approverId, string comment);
    public void Delete();
    public void Restore();
    public bool CanBeEdited();
    public bool CanBeDeleted();
    public Money CalculateAmount(Money hourlyRate);
}

public enum TimeEntryStatus
{
    Draft = 0,
    Active = 1,
    Deleted = 2
}

public enum ApprovalStatus
{
    NotSubmitted = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3
}
```

### Timer Entity

```csharp
public class Timer
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ProjectId { get; private set; }
    public string? Description { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime LastSyncedAt { get; private set; }
    public bool IsRunning { get; private set; }

    public TimeSpan CurrentDuration => DateTime.UtcNow - StartedAt;

    public void UpdateDescription(string description);
    public void UpdateProject(Guid projectId);
    public void Sync();
    public TimeEntry Stop(DateTime stoppedAt);
}
```

### Tag Value Object

```csharp
public class Tag : ValueObject
{
    public string Name { get; private set; }
    public string ColorHex { get; private set; }

    public Tag(string name, string colorHex)
    {
        // Validation
    }
}
```

## Validation Rules

### Time Entry Validation

| Field | Rule |
|-------|------|
| Project | Required, must exist and be active |
| Description | Optional, max 500 characters |
| Date | Required, cannot be future date, cannot be > 90 days old |
| Start Time | Required for manual entry |
| End Time | Required for manual entry, must be after start time |
| Duration | Required, minimum 1 minute, maximum 24 hours |
| Notes | Optional, max 2000 characters |

### Timer Validation

| Field | Rule |
|-------|------|
| Project | Required, must exist and be active |
| Duration | Maximum 24 hours continuous running |
| Description | Optional, max 500 characters |

## Business Rules Summary

1. **One Timer Per User**: Only one timer can run per user at a time
2. **No Overlapping Entries**: Warn users about overlapping time entries
3. **No Future Entries**: Cannot create entries in the future
4. **Locked Entry Protection**: Cannot edit/delete approved or invoiced entries
5. **Duration Limits**: Entries limited to 24 hours maximum
6. **Billable Default**: Billable status defaults from project settings
7. **Soft Delete**: Deleted entries can be restored within 30 days
8. **Approval Locking**: Approved entries cannot be modified
9. **Permission Checks**: Users can only edit own entries (unless manager/admin)

## Performance Requirements

- Start timer: < 200ms
- Stop timer and create entry: < 500ms
- Create manual entry: < 500ms
- Load weekly entries: < 300ms
- Load monthly entries: < 500ms
- Timer sync interval: 30 seconds
- UI timer update: Every second (client-side only)

## Security Requirements

- Users can only view own time entries (unless manager/admin)
- Managers can view team entries only
- Admins can view all entries
- Cannot modify entries of other users (unless manager/admin)
- Approval permissions validated server-side
- Rate limiting: 100 time entry operations per minute per user

## Testing Requirements

### Unit Tests
- Time duration calculations
- Validation rules
- Business rule enforcement (overlapping, locking, etc.)
- Domain event generation
- Timer duration calculations

### Integration Tests
- Timer start/stop flow
- Time entry CRUD operations
- Approval workflow
- Bulk operations
- Search and filtering

### E2E Tests
- Complete timer workflow
- Manual entry creation workflow
- Editing and deleting entries
- Approval process (employee + manager)
- Timesheet view interactions

## Dependencies

- User Management module
- Project Management module
- Client Management module
- MediatR for commands/queries
- SignalR for real-time timer sync
- FluentValidation
- Entity Framework Core

## Future Enhancements (Phase 4+)

- Pomodoro timer integration
- GPS location tracking (mobile)
- Screenshot capture for time verification
- Integration with calendar apps (Google Calendar, Outlook)
- AI-powered time suggestions based on patterns
- Voice commands for time entry
- Smartwatch integration
- Automatic time tracking based on application usage
- Team time visualization (heatmaps, graphs)
- Time blocking and scheduling

---

**Document Owner**: Development Team
**Reviewers**: Product Team, UX Team
**Approval Status**: Pending
