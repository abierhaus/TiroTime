using TiroTime.Domain.Entities;
using TiroTime.Domain.Exceptions;
using TiroTime.Domain.ValueObjects;

namespace TiroTime.Domain.Tests.Entities;

public class ProjectTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateProject()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var hourlyRate = Money.FromEuro(100m);

        // Act
        var project = Project.Create("Test Project", clientId, hourlyRate);

        // Assert
        Assert.NotNull(project);
        Assert.Equal("Test Project", project.Name);
        Assert.Equal(clientId, project.ClientId);
        Assert.Equal(hourlyRate, project.HourlyRate);
        Assert.True(project.IsActive);
        Assert.NotEqual(Guid.Empty, project.Id);
    }

    [Fact]
    public void Create_WithAllData_ShouldCreateProjectWithAllProperties()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var hourlyRate = Money.FromEuro(100m);
        var budget = Money.FromEuro(10000m);
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddMonths(6);

        // Act
        var project = Project.Create(
            "Test Project",
            clientId,
            hourlyRate,
            "Project Description",
            budget,
            "#FF5733",
            startDate,
            endDate);

        // Assert
        Assert.Equal("Test Project", project.Name);
        Assert.Equal("Project Description", project.Description);
        Assert.Equal(budget, project.Budget);
        Assert.Equal("#FF5733", project.ColorCode);
        Assert.Equal(startDate, project.StartDate);
        Assert.Equal(endDate, project.EndDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowDomainException(string invalidName)
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var hourlyRate = Money.FromEuro(100m);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            Project.Create(invalidName, clientId, hourlyRate));

        Assert.Contains("Projektname darf nicht leer sein", exception.Message);
    }

    [Fact]
    public void Create_WithEmptyClientId_ShouldThrowDomainException()
    {
        // Arrange
        var hourlyRate = Money.FromEuro(100m);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            Project.Create("Test Project", Guid.Empty, hourlyRate));

        Assert.Contains("Kunden-ID ist erforderlich", exception.Message);
    }

    [Fact]
    public void Create_WithNegativeHourlyRate_ShouldThrowDomainException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var invalidRate = Money.FromEuro(-50m);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            Project.Create("Test Project", clientId, invalidRate));

        Assert.Contains("Stundensatz darf nicht negativ sein", exception.Message);
    }

    [Fact]
    public void Create_WithNegativeBudget_ShouldThrowDomainException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var hourlyRate = Money.FromEuro(100m);
        var invalidBudget = Money.FromEuro(-1000m);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            Project.Create("Test Project", clientId, hourlyRate, budget: invalidBudget));

        Assert.Contains("Budget darf nicht negativ sein", exception.Message);
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_ShouldThrowDomainException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var hourlyRate = Money.FromEuro(100m);
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddMonths(-1);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            Project.Create("Test Project", clientId, hourlyRate, startDate: startDate, endDate: endDate));

        Assert.Contains("Enddatum muss nach dem Startdatum liegen", exception.Message);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateProject()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var hourlyRate = Money.FromEuro(100m);
        var project = Project.Create("Original Name", clientId, hourlyRate);
        var newHourlyRate = Money.FromEuro(150m);

        // Act
        project.Update("Updated Name", newHourlyRate, "New Description");

        // Assert
        Assert.Equal("Updated Name", project.Name);
        Assert.Equal(newHourlyRate, project.HourlyRate);
        Assert.Equal("New Description", project.Description);
        Assert.NotNull(project.UpdatedAt);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var hourlyRate = Money.FromEuro(100m);
        var project = Project.Create("Test Project", clientId, hourlyRate);

        // Act
        project.Deactivate();

        // Assert
        Assert.False(project.IsActive);
        Assert.NotNull(project.UpdatedAt);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var hourlyRate = Money.FromEuro(100m);
        var project = Project.Create("Test Project", clientId, hourlyRate);
        project.Deactivate();

        // Act
        project.Activate();

        // Assert
        Assert.True(project.IsActive);
        Assert.NotNull(project.UpdatedAt);
    }
}
