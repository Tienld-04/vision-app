namespace VisionApp.Core.Inspection;

/* Gộp kết quả các bước thành một kết luận. Ưu tiên: Failed > Error > NeedsReview > Passed;
   không có bước nào Passed thì ra Error, không bao giờ ra Ok. */
public static class ResultAggregator
{
    public static InspectionSummary Aggregate(IReadOnlyList<StepResult> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);

        var failed = steps.Where(s => s.Status == StepStatus.Failed).ToList();
        if (failed.Count > 0)
        {
            var primary = failed[0];
            var reason = WithOthersCount($"{primary.StepName}: {primary.Message}", failed.Count);
            return Summary(InspectionDecision.Ng, reason, primary.DefectCode, steps);
        }

        var error = steps.FirstOrDefault(s => s.Status == StepStatus.Error);
        if (error is not null)
        {
            return Summary(InspectionDecision.Error, $"{error.StepName}: {error.Message}", null, steps);
        }

        var review = steps.FirstOrDefault(s => s.Status == StepStatus.NeedsReview);
        if (review is not null)
        {
            return Summary(InspectionDecision.Review, $"{review.StepName}: {review.Message}", review.DefectCode, steps);
        }

        if (!steps.Any(s => s.Status == StepStatus.Passed))
        {
            return Summary(InspectionDecision.Error, "Không có bước kiểm tra nào được thực hiện", null, steps);
        }

        return Summary(InspectionDecision.Ok, "Đạt tất cả các bước kiểm tra", null, steps);
    }

    private static string WithOthersCount(string message, int failedCount)
        => failedCount > 1 ? $"{message} (+{failedCount - 1} lỗi khác)" : message;

    private static InspectionSummary Summary(
        InspectionDecision decision, string reason, string? defectCode, IReadOnlyList<StepResult> steps)
        => new()
        {
            Decision = decision,
            Reason = reason,
            DefectCode = defectCode,
            Steps = steps.ToArray()
        };
}
