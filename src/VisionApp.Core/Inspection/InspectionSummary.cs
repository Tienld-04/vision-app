namespace VisionApp.Core.Inspection;

/* Kết quả tổng hợp của một lần kiểm tra: kết luận, lý do, mã lỗi chính và chi tiết từng bước. */
public sealed record InspectionSummary
{
    public required InspectionDecision Decision { get; init; }

    public required string Reason { get; init; }

    public string? DefectCode { get; init; }

    public required IReadOnlyList<StepResult> Steps { get; init; }
}
