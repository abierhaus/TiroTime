using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;
using TiroTime.Application.Services;

namespace TiroTime.Application.Tests.Services;

public class TimeEntryValidationServiceTests
{
    private readonly ITimeEntryValidationService _validationService;

    public TimeEntryValidationServiceTests()
    {
        _validationService = new TimeEntryValidationService();
    }

    #region Weekend Validation Tests

    [Fact]
    public void ValidateTimeEntries_SaturdayEntry_ReturnsWeekendWarning()
    {
        // Arrange
        var saturday = new DateTime(2025, 1, 4); // A Saturday
        var entries = new[]
        {
            CreateTimeEntry(Guid.NewGuid(), saturday, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0))
        };

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Single(warnings);
        Assert.Equal("weekend", warnings[0].Type);
        Assert.Contains("Wochenendarbeit", warnings[0].Message);
        Assert.Contains("Samstag", warnings[0].Message);
    }

    [Fact]
    public void ValidateTimeEntries_SundayEntry_ReturnsWeekendWarning()
    {
        // Arrange
        var sunday = new DateTime(2025, 1, 5); // A Sunday
        var entries = new[]
        {
            CreateTimeEntry(Guid.NewGuid(), sunday, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0))
        };

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Single(warnings);
        Assert.Equal("weekend", warnings[0].Type);
        Assert.Contains("Sonntag", warnings[0].Message);
    }

    [Fact]
    public void ValidateTimeEntries_WeekdayEntry_NoWeekendWarning()
    {
        // Arrange
        var monday = new DateTime(2025, 1, 6); // A Monday
        var entries = new[]
        {
            CreateTimeEntry(Guid.NewGuid(), monday, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0))
        };

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Empty(warnings);
    }

    #endregion

    #region Overlap Validation Tests

    [Fact]
    public void ValidateTimeEntries_OverlappingEntries_ReturnsOverlapWarning()
    {
        // Arrange
        var date = new DateTime(2025, 1, 6); // A Monday
        var entries = new[]
        {
            CreateTimeEntry(Guid.NewGuid(), date, new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0)),
            CreateTimeEntry(Guid.NewGuid(), date, new TimeSpan(11, 0, 0), new TimeSpan(14, 0, 0)) // Overlaps
        };

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Single(warnings);
        Assert.Equal("overlap", warnings[0].Type);
        Assert.Contains("Zeitüberschneidung", warnings[0].Message);
        Assert.Contains("09:00-12:00", warnings[0].Message);
        Assert.Contains("11:00-14:00", warnings[0].Message);
        Assert.Equal(2, warnings[0].AffectedEntryIds.Count);
    }

    [Fact]
    public void ValidateTimeEntries_NonOverlappingEntries_NoOverlapWarning()
    {
        // Arrange
        var date = new DateTime(2025, 1, 6); // A Monday
        var entries = new[]
        {
            CreateTimeEntry(Guid.NewGuid(), date, new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0)),
            CreateTimeEntry(Guid.NewGuid(), date, new TimeSpan(12, 0, 0), new TimeSpan(15, 0, 0)) // Adjacent, not overlapping
        };

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Empty(warnings);
    }

    [Fact]
    public void ValidateTimeEntries_ThreeOverlappingEntries_ReturnsMultipleOverlapWarnings()
    {
        // Arrange
        var date = new DateTime(2025, 1, 6); // A Monday
        var entries = new[]
        {
            CreateTimeEntry(Guid.NewGuid(), date, new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0)),
            CreateTimeEntry(Guid.NewGuid(), date, new TimeSpan(10, 0, 0), new TimeSpan(13, 0, 0)), // Overlaps with first
            CreateTimeEntry(Guid.NewGuid(), date, new TimeSpan(11, 0, 0), new TimeSpan(14, 0, 0))  // Overlaps with both
        };

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Equal(3, warnings.Count); // 3 pairwise overlaps
        Assert.All(warnings, w => Assert.Equal("overlap", w.Type));
    }

    [Fact]
    public void ValidateTimeEntries_PartialOverlap_ReturnsOverlapWarning()
    {
        // Arrange
        var date = new DateTime(2025, 1, 6);
        var entries = new[]
        {
            CreateTimeEntry(Guid.NewGuid(), date, new TimeSpan(9, 0, 0), new TimeSpan(11, 30, 0)),
            CreateTimeEntry(Guid.NewGuid(), date, new TimeSpan(11, 0, 0), new TimeSpan(13, 0, 0)) // 30 min overlap
        };

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Single(warnings);
        Assert.Equal("overlap", warnings[0].Type);
    }

    #endregion

    #region Whitelist/Exclusion Tests

    [Fact]
    public void ValidateTimeEntries_SaturdayWithPauschalInDescription_NoWarning()
    {
        // Arrange
        var saturday = new DateTime(2025, 1, 4); // A Saturday
        var entries = new[]
        {
            CreateTimeEntry(Guid.NewGuid(), saturday, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), "Pauschal")
        };

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Empty(warnings);
    }

    [Fact]
    public void ValidateTimeEntries_OverlapWithPauschalInDescription_NoWarning()
    {
        // Arrange
        var date = new DateTime(2025, 1, 6);
        var entries = new[]
        {
            CreateTimeEntry(Guid.NewGuid(), date, new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0), "Pauschal"),
            CreateTimeEntry(Guid.NewGuid(), date, new TimeSpan(11, 0, 0), new TimeSpan(14, 0, 0), "Pauschal")
        };

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Empty(warnings);
    }

    [Fact]
    public void ValidateTimeEntries_PauschalCaseInsensitive_NoWarning()
    {
        // Arrange
        var saturday = new DateTime(2025, 1, 4);
        var entries = new[]
        {
            CreateTimeEntry(Guid.NewGuid(), saturday, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), "pauschal"),
            CreateTimeEntry(Guid.NewGuid(), saturday, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), "PAUSCHAL"),
            CreateTimeEntry(Guid.NewGuid(), saturday, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), "Pauschale Abrechnung")
        };

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Empty(warnings);
    }

    [Fact]
    public void ValidateTimeEntries_OneEntryWithPauschalOneWithout_ReturnsWarningForOneWithout()
    {
        // Arrange
        var saturday = new DateTime(2025, 1, 4);
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var entries = new[]
        {
            CreateTimeEntry(id1, saturday, new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0), "Pauschal"),
            CreateTimeEntry(id2, saturday, new TimeSpan(13, 0, 0), new TimeSpan(17, 0, 0), "Normal work")
        };

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Single(warnings);
        Assert.Equal("weekend", warnings[0].Type);
        Assert.Single(warnings[0].AffectedEntryIds);
        Assert.Contains(id2, warnings[0].AffectedEntryIds);
        Assert.DoesNotContain(id1, warnings[0].AffectedEntryIds);
    }

    [Fact]
    public void ValidateTimeEntries_EmptyDescription_IncludesInValidation()
    {
        // Arrange
        var saturday = new DateTime(2025, 1, 4);
        var entries = new[]
        {
            CreateTimeEntry(Guid.NewGuid(), saturday, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0), null)
        };

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Single(warnings);
        Assert.Equal("weekend", warnings[0].Type);
    }

    #endregion

    #region Edge Cases and Combined Scenarios

    [Fact]
    public void ValidateTimeEntries_EmptyList_ReturnsNoWarnings()
    {
        // Arrange
        var entries = Array.Empty<TimeEntryDto>();

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Empty(warnings);
    }

    [Fact]
    public void ValidateTimeEntries_WeekendAndOverlap_ReturnsBothWarnings()
    {
        // Arrange
        var saturday = new DateTime(2025, 1, 4);
        var entries = new[]
        {
            CreateTimeEntry(Guid.NewGuid(), saturday, new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0)),
            CreateTimeEntry(Guid.NewGuid(), saturday, new TimeSpan(11, 0, 0), new TimeSpan(14, 0, 0))
        };

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Equal(2, warnings.Count);
        Assert.Contains(warnings, w => w.Type == "weekend");
        Assert.Contains(warnings, w => w.Type == "overlap");
    }

    [Fact]
    public void ValidateTimeEntries_MultipleDays_ValidatesEachDaySeparately()
    {
        // Arrange
        var monday = new DateTime(2025, 1, 6);
        var saturday = new DateTime(2025, 1, 11);
        var entries = new[]
        {
            CreateTimeEntry(Guid.NewGuid(), monday, new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0)),
            CreateTimeEntry(Guid.NewGuid(), monday, new TimeSpan(11, 0, 0), new TimeSpan(14, 0, 0)), // Overlap on Monday
            CreateTimeEntry(Guid.NewGuid(), saturday, new TimeSpan(9, 0, 0), new TimeSpan(12, 0, 0)) // Weekend
        };

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Equal(2, warnings.Count);
        Assert.Contains(warnings, w => w.Type == "overlap" && w.Date == monday);
        Assert.Contains(warnings, w => w.Type == "weekend" && w.Date == saturday);
    }

    [Fact]
    public void ValidateTimeEntries_RunningEntry_IgnoresEntry()
    {
        // Arrange
        var monday = new DateTime(2025, 1, 6);
        var entries = new[]
        {
            CreateTimeEntry(Guid.NewGuid(), monday, new TimeSpan(9, 0, 0), null) // Running entry (no end time)
        };

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Empty(warnings);
    }

    [Fact]
    public void ValidateTimeEntries_IdenticalTimeRanges_ReturnsOverlapWarning()
    {
        // Arrange
        var date = new DateTime(2025, 1, 6);
        var entries = new[]
        {
            CreateTimeEntry(Guid.NewGuid(), date, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0)),
            CreateTimeEntry(Guid.NewGuid(), date, new TimeSpan(9, 0, 0), new TimeSpan(17, 0, 0)) // Identical
        };

        // Act
        var warnings = _validationService.ValidateTimeEntries(entries).ToList();

        // Assert
        Assert.Single(warnings);
        Assert.Equal("overlap", warnings[0].Type);
    }

    #endregion

    #region Helper Methods

    private static TimeEntryDto CreateTimeEntry(
        Guid id,
        DateTime date,
        TimeSpan startTime,
        TimeSpan? endTime,
        string? description = null)
    {
        var startDateTime = DateTime.SpecifyKind(date.Date + startTime, DateTimeKind.Local).ToUniversalTime();
        DateTime? endDateTime = endTime.HasValue
            ? DateTime.SpecifyKind(date.Date + endTime.Value, DateTimeKind.Local).ToUniversalTime()
            : null;

        var duration = endDateTime.HasValue
            ? endDateTime.Value - startDateTime
            : TimeSpan.Zero;

        return new TimeEntryDto(
            Id: id,
            UserId: Guid.NewGuid(),
            ProjectId: Guid.NewGuid(),
            ProjectName: "Test Project",
            ClientName: "Test Client",
            ProjectColorCode: "#000000",
            Description: description,
            StartTime: startDateTime,
            EndTime: endDateTime,
            Duration: duration,
            IsRunning: !endTime.HasValue,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null
        );
    }

    #endregion
}
