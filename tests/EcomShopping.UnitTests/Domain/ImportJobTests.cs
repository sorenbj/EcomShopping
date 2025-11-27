using Xunit;
using FluentAssertions;
using EcomShopping.Domain.Entities;

namespace EcomShopping.UnitTests.Domain;

public class ImportJobTests
{
    [Fact]
    public void ImportJob_Create_ShouldHaveDefaultValues()
    {
        // Act
        var job = new ImportJob();

        // Assert
        job.Status.Should().Be(ImportJobStatus.Pending);
        job.TotalRecords.Should().Be(0);
        job.ProcessedRecords.Should().Be(0);
        job.SuccessfulRecords.Should().Be(0);
        job.FailedRecords.Should().Be(0);
    }

    [Fact]
    public void ImportJob_SetProperties_ShouldUpdateCorrectly()
    {
        // Arrange
        var job = new ImportJob();
        var now = DateTime.UtcNow;

        // Act
        job.FileName = "products.xlsx";
        job.FileType = "Excel";
        job.Status = ImportJobStatus.Processing;
        job.TotalRecords = 100;
        job.ProcessedRecords = 50;
        job.SuccessfulRecords = 45;
        job.FailedRecords = 5;
        job.CreatedAt = now;
        job.StartedAt = now.AddMinutes(-10);
        job.CreatedBy = "admin@example.com";

        // Assert
        job.FileName.Should().Be("products.xlsx");
        job.FileType.Should().Be("Excel");
        job.Status.Should().Be(ImportJobStatus.Processing);
        job.TotalRecords.Should().Be(100);
        job.ProcessedRecords.Should().Be(50);
        job.SuccessfulRecords.Should().Be(45);
        job.FailedRecords.Should().Be(5);
        job.CreatedAt.Should().Be(now);
        job.StartedAt.Should().Be(now.AddMinutes(-10));
        job.CreatedBy.Should().Be("admin@example.com");
    }

    [Fact]
    public void ImportJob_UpdateStatus_ShouldChangeStatus()
    {
        // Arrange
        var job = new ImportJob { Status = ImportJobStatus.Pending };

        // Act
        job.Status = ImportJobStatus.Processing;

        // Assert
        job.Status.Should().Be(ImportJobStatus.Processing);
    }

    [Fact]
    public void ImportJob_CompleteSuccessfully_ShouldSetCompletedStatus()
    {
        // Arrange
        var job = new ImportJob 
        { 
            Status = ImportJobStatus.Processing,
            TotalRecords = 100,
            ProcessedRecords = 100,
            SuccessfulRecords = 100,
            FailedRecords = 0
        };

        // Act
        job.Status = ImportJobStatus.Completed;
        job.CompletedAt = DateTime.UtcNow;

        // Assert
        job.Status.Should().Be(ImportJobStatus.Completed);
        job.CompletedAt.Should().NotBeNull();
        job.SuccessfulRecords.Should().Be(100);
        job.FailedRecords.Should().Be(0);
    }

    [Fact]
    public void ImportJob_PartialCompletion_ShouldSetPartiallyCompletedStatus()
    {
        // Arrange
        var job = new ImportJob 
        { 
            Status = ImportJobStatus.Processing,
            TotalRecords = 100,
            ProcessedRecords = 100,
            SuccessfulRecords = 85,
            FailedRecords = 15
        };

        // Act
        job.Status = ImportJobStatus.PartiallyCompleted;
        job.CompletedAt = DateTime.UtcNow;

        // Assert
        job.Status.Should().Be(ImportJobStatus.PartiallyCompleted);
        job.CompletedAt.Should().NotBeNull();
        job.SuccessfulRecords.Should().Be(85);
        job.FailedRecords.Should().Be(15);
    }

    [Fact]
    public void ImportJob_Failed_ShouldSetFailedStatus()
    {
        // Arrange
        var job = new ImportJob 
        { 
            Status = ImportJobStatus.Processing,
            ErrorLog = "Database connection error"
        };

        // Act
        job.Status = ImportJobStatus.Failed;
        job.CompletedAt = DateTime.UtcNow;

        // Assert
        job.Status.Should().Be(ImportJobStatus.Failed);
        job.CompletedAt.Should().NotBeNull();
        job.ErrorLog.Should().Be("Database connection error");
    }
}
