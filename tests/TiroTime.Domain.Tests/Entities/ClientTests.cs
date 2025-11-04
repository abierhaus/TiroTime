using TiroTime.Domain.Entities;
using TiroTime.Domain.Exceptions;
using TiroTime.Domain.ValueObjects;

namespace TiroTime.Domain.Tests.Entities;

public class ClientTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateClient()
    {
        // Arrange & Act
        var client = Client.Create("Test Client");

        // Assert
        Assert.NotNull(client);
        Assert.Equal("Test Client", client.Name);
        Assert.True(client.IsActive);
        Assert.NotEqual(Guid.Empty, client.Id);
        Assert.True(client.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Create_WithAllData_ShouldCreateClientWithAllProperties()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var phone = PhoneNumber.Create("+49 123 456789");
        var address = Address.Create("Test Street 123", "12345", "Test City", "Germany");

        // Act
        var client = Client.Create(
            "Test Client",
            "John Doe",
            email,
            phone,
            address,
            "DE123456789",
            "Test notes");

        // Assert
        Assert.Equal("Test Client", client.Name);
        Assert.Equal("John Doe", client.ContactPerson);
        Assert.Equal(email, client.Email);
        Assert.Equal(phone, client.PhoneNumber);
        Assert.Equal(address, client.Address);
        Assert.Equal("DE123456789", client.TaxId);
        Assert.Equal("Test notes", client.Notes);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldThrowDomainException(string invalidName)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            Client.Create(invalidName));

        Assert.Contains("Kundenname darf nicht leer sein", exception.Message);
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldTrimName()
    {
        // Act
        var client = Client.Create("  Test Client  ");

        // Assert
        Assert.Equal("Test Client", client.Name);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateClient()
    {
        // Arrange
        var client = Client.Create("Original Name");
        var originalCreatedAt = client.CreatedAt;

        // Act
        client.Update("Updated Name", "New Contact");

        // Assert
        Assert.Equal("Updated Name", client.Name);
        Assert.Equal("New Contact", client.ContactPerson);
        Assert.NotNull(client.UpdatedAt);
        Assert.True(client.UpdatedAt >= originalCreatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Update_WithInvalidName_ShouldThrowDomainException(string invalidName)
    {
        // Arrange
        var client = Client.Create("Test Client");

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            client.Update(invalidName));

        Assert.Contains("Kundenname darf nicht leer sein", exception.Message);
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var client = Client.Create("Test Client");

        // Act
        client.Deactivate();

        // Assert
        Assert.False(client.IsActive);
        Assert.NotNull(client.UpdatedAt);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var client = Client.Create("Test Client");
        client.Deactivate();

        // Act
        client.Activate();

        // Assert
        Assert.True(client.IsActive);
        Assert.NotNull(client.UpdatedAt);
    }
}
