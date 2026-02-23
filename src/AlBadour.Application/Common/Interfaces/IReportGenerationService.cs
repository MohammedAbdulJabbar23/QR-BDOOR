namespace AlBadour.Application.Common.Interfaces;

public interface IReportGenerationService
{
    byte[] GenerateDailyReport(DateTime date, object data);
    byte[] GenerateMedicalSummaryReport(DateTime from, DateTime to, object data);
    byte[] GenerateStatusBreakdownReport(DateTime from, DateTime to, object data);
    byte[] GenerateCancelledReport(DateTime from, DateTime to, object data);
}
