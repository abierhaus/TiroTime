using TiroTime.Domain.Entities;
using TiroTime.Domain.Exceptions;

namespace TiroTime.Domain.Tests.Entities;

public class TimeEntryTests
{
    [Fact]
    public void StartTimer_WithValidData_ShouldCreateRunningTimeEntry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        // Act
        var timeEntry = TimeEntry.StartTimer(userId, projectId, "Working on feature");

        // Assert
        Assert.NotNull(timeEntry);
        Assert.Equal(userId, timeEntry.UserId);
        Assert.Equal(projectId, timeEntry.ProjectId);
        Assert.Equal("Working on feature", timeEntry.Description);
        Assert.True(timeEntry.IsRunning);
        Assert.Null(timeEntry.EndTime);
        Assert.Equal(TimeSpan.Zero, timeEntry.Duration);
    }

    [Fact]
    public void StartTimer_WithEmptyUserId_ShouldThrowDomainException()
    {
        // Arrange
        var projectId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            TimeEntry.StartTimer(Guid.Empty, projectId));

        Assert.Contains("Benutzer-ID ist erforderlich", exception.Message);
    }

    [Fact]
    public void StartTimer_WithEmptyProjectId_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            TimeEntry.StartTimer(userId, Guid.Empty));

        Assert.Contains("Projekt-ID ist erforderlich", exception.Message);
    }

    [Fact]
    public void CreateManual_WithValidData_ShouldCreateCompletedTimeEntry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(-2);
        var endTime = DateTime.UtcNow;

        // Act
        var timeEntry = TimeEntry.CreateManual(userId, projectId, startTime, endTime, "Completed work");

        // Assert
        Assert.NotNull(timeEntry);
        Assert.Equal(userId, timeEntry.UserId);
        Assert.Equal(projectId, timeEntry.ProjectId);
        Assert.False(timeEntry.IsRunning);
        Assert.Equal(startTime, timeEntry.StartTime);
        Assert.Equal(endTime, timeEntry.EndTime);
        Assert.True(timeEntry.Duration.TotalHours > 1.9 && timeEntry.Duration.TotalHours < 2.1);
    }

    [Fact]
    public void CreateManual_WithEndTimeBeforeStartTime_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var startTime = DateTime.UtcNow;
        var endTime = DateTime.UtcNow.AddHours(-1);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            TimeEntry.CreateManual(userId, projectId, startTime, endTime));

        Assert.Contains("Endzeit muss nach der Startzeit liegen", exception.Message);
    }

    [Fact]
    public void CreateManual_WithDurationOver24Hours_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var startTime = DateTime.UtcNow;
        var endTime = DateTime.UtcNow.AddHours(25);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            TimeEntry.CreateManual(userId, projectId, startTime, endTime));

        Assert.Contains("Zeiterfassung darf nicht länger als 24 Stunden sein", exception.Message);
    }

    [Fact]
    public void StopTimer_WithRunningTimer_ShouldStopAndCalculateDuration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var timeEntry = TimeEntry.StartTimer(userId, projectId);

        // Wait a moment to ensure duration is positive
        Thread.Sleep(10);

        // Act
        timeEntry.StopTimer();

        // Assert
        Assert.False(timeEntry.IsRunning);
        Assert.NotNull(timeEntry.EndTime);
        Assert.True(timeEntry.Duration > TimeSpan.Zero);
        Assert.NotNull(timeEntry.UpdatedAt);
    }

    [Fact]
    public void StopTimer_WithStoppedTimer_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(-1);
        var endTime = DateTime.UtcNow;
        var timeEntry = TimeEntry.CreateManual(userId, projectId, startTime, endTime);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            timeEntry.StopTimer());

        Assert.Contains("Timer läuft nicht", exception.Message);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateTimeEntry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(-2);
        var endTime = DateTime.UtcNow.AddHours(-1);
        var timeEntry = TimeEntry.CreateManual(userId, projectId, startTime, endTime);

        var newStartTime = DateTime.UtcNow.AddHours(-3);
        var newEndTime = DateTime.UtcNow;

        // Act
        timeEntry.Update(newStartTime, newEndTime, "Updated description");

        // Assert
        Assert.Equal(newStartTime, timeEntry.StartTime);
        Assert.Equal(newEndTime, timeEntry.EndTime);
        Assert.Equal("Updated description", timeEntry.Description);
        Assert.NotNull(timeEntry.UpdatedAt);
    }

    [Fact]
    public void Update_WithRunningTimer_ShouldThrowDomainException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var timeEntry = TimeEntry.StartTimer(userId, projectId);

        var newStartTime = DateTime.UtcNow.AddHours(-1);
        var newEndTime = DateTime.UtcNow;

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            timeEntry.Update(newStartTime, newEndTime));

        Assert.Contains("Laufende Zeiterfassung kann nicht bearbeitet werden", exception.Message);
    }

    [Fact]
    public void UpdateDescription_ShouldUpdateDescriptionOnly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var timeEntry = TimeEntry.StartTimer(userId, projectId, "Original description");

        // Act
        timeEntry.UpdateDescription("Updated description");

        // Assert
        Assert.Equal("Updated description", timeEntry.Description);
        Assert.NotNull(timeEntry.UpdatedAt);
    }

    [Fact]
    public void GetCurrentDuration_WithRunningTimer_ShouldReturnTimeSinceStart()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var timeEntry = TimeEntry.StartTimer(userId, projectId);

        // Wait a moment
        Thread.Sleep(100);

        // Act
        var duration = timeEntry.GetCurrentDuration();

        // Assert
        Assert.True(duration.TotalMilliseconds >= 100);
    }

    [Fact]
    public void GetCurrentDuration_WithStoppedTimer_ShouldReturnStoredDuration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(-2);
        var endTime = DateTime.UtcNow;
        var timeEntry = TimeEntry.CreateManual(userId, projectId, startTime, endTime);
        var expectedDuration = timeEntry.Duration;

        // Act
        var duration = timeEntry.GetCurrentDuration();

        // Assert
        Assert.Equal(expectedDuration, duration);
    }
}
