using EcomShopping.Integration.Core;
using EcomShopping.Integration.Core.Providers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace EcomShopping.UnitTests.Integration;

public class IntegrationSchedulerTests
{
    private readonly IntegrationProviderRegistry _registry;
    private readonly IntegrationEngine _engine;
    private readonly IntegrationScheduler _scheduler;

    public IntegrationSchedulerTests()
    {
        _registry = new IntegrationProviderRegistry();
        _registry.Register("mock-erp", new MockErpIntegration());
        
        _engine = new IntegrationEngine(_registry, NullLogger<IntegrationEngine>.Instance);
        _scheduler = new IntegrationScheduler(_engine, NullLogger<IntegrationScheduler>.Instance);
    }

    [Fact]
    public void AddSchedule_ShouldAddScheduleSuccessfully()
    {
        // Act
        _scheduler.AddSchedule(1, "mock-erp", "getproduct", 60, "SKU123");

        // Assert
        var schedules = _scheduler.GetSchedules();
        schedules.Should().HaveCount(1);
        schedules.First().ScheduleId.Should().Be(1);
    }

    [Fact]
    public void RemoveSchedule_ShouldRemoveScheduleSuccessfully()
    {
        // Arrange
        _scheduler.AddSchedule(1, "mock-erp", "getproduct", 60, "SKU123");

        // Act
        _scheduler.RemoveSchedule(1);

        // Assert
        var schedules = _scheduler.GetSchedules();
        schedules.Should().BeEmpty();
    }

    [Fact]
    public void GetSchedules_ShouldReturnAllSchedules()
    {
        // Arrange
        _scheduler.AddSchedule(1, "mock-erp", "getproduct", 60, "SKU123");
        _scheduler.AddSchedule(2, "mock-erp", "syncorder", 30, "ORD123");

        // Act
        var schedules = _scheduler.GetSchedules();

        // Assert
        schedules.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecuteDueSchedulesAsync_WithDueSchedule_ShouldExecute()
    {
        // Arrange
        _scheduler.AddSchedule(1, "mock-erp", "getproduct", -1, "SKU123"); // Negative interval makes it immediately due

        // Act
        var results = await _scheduler.ExecuteDueSchedulesAsync();

        // Assert
        results.Should().HaveCount(1);
        results.First().Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteDueSchedulesAsync_WithoutDueSchedule_ShouldNotExecute()
    {
        // Arrange
        _scheduler.AddSchedule(1, "mock-erp", "getproduct", 1000, "SKU123"); // Long interval, not due yet

        // Act
        var results = await _scheduler.ExecuteDueSchedulesAsync();

        // Assert
        results.Should().BeEmpty();
    }
}
