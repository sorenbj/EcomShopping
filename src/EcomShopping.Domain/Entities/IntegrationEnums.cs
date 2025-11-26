namespace EcomShopping.Domain.Entities;

public enum IntegrationType
{
    ERP = 0,
    CRM = 1,
    Shipping = 2,
    Payment = 3
}

public enum IntegrationExecutionStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4
}

public enum ScheduleType
{
    Manual = 0,
    Interval = 1,
    Cron = 2,
    EventBased = 3
}

public enum TriggerType
{
    Manual = 0,
    Scheduled = 1,
    OrderCreated = 2,
    OrderStatusChanged = 3,
    InventoryChanged = 4,
    CustomerCreated = 5,
    CustomerUpdated = 6
}
