namespace Cqrs.OrderService.Infrastructure.Jobs;

public sealed class HangfireOptions
{
    public bool Enabled { get; set; }
    public string DashboardPath { get; set; } = "/hangfire";
    public string BacklogMonitorCron { get; set; } = "*/5 * * * *";
    public string AutoCancelStaleOrdersCron { get; set; } = "*/5 * * * *";
    public int AutoCancelPendingOrderAfterMinutes { get; set; } = 30;
    public string DailyOperationsReportCron { get; set; } = "0 7 * * *";
    public string RetryFailedOutboxCron { get; set; } = "*/2 * * * *";
    public int RetryFailedOutboxMaxBatches { get; set; } = 3;
    public string CleanupAuditCron { get; set; } = "30 3 * * *";
    public int AuditRetentionDays { get; set; } = 30;
    public int WorkerCount { get; set; } = 5;
}
