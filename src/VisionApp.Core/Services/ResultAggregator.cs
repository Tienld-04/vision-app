using VisionApp.Core.Constants;
using VisionApp.Core.Enums;
using VisionApp.Core.ValueObjects;

namespace VisionApp.Core.Services;

/* Gộp kết quả các bước thành một kết luận. Ưu tiên: Failed > Error > NeedsReview > Passed;
   không có bước nào Passed thì ra Error, không bao giờ ra Ok. */
public static class ResultAggregator
{
    public static InspectionSummary Aggregate(IReadOnlyList<StepResult> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);

        foreach (var step in steps)
        {
            ArgumentNullException.ThrowIfNull(step, nameof(steps));
        }

        var failed = steps.Where(s => s.Status == StepStatus.Failed).ToList();
        if (failed.Count > 0)
        {
            var primary = failed[0];
            var reason = WithAdditionalFailures(StepReason(primary), failed.Count - 1);
            return Summary(InspectionDecision.Ng, reason, primary.DefectCode, steps);
        }

        var error = steps.FirstOrDefault(s => s.Status == StepStatus.Error);
        if (error is not null)
        {
            return Summary(InspectionDecision.Error, StepReason(error), null, steps);
        }

        var review = steps.FirstOrDefault(s => s.Status == StepStatus.NeedsReview);
        if (review is not null)
        {
            return Summary(InspectionDecision.Review, StepReason(review), review.DefectCode, steps);
        }

        if (!steps.Any(s => s.Status == StepStatus.Passed))
        {
            return Summary(InspectionDecision.Error, InspectionMessages.NoStepExecuted, null, steps);
        }

        return Summary(InspectionDecision.Ok, InspectionMessages.AllStepsPassed, null, steps);
    }

    private static string StepReason(StepResult step)
        => string.Format(InspectionMessages.StepReasonFormat, step.StepName, step.Message);

    private static string WithAdditionalFailures(string reason, int additionalCount)
        => additionalCount > 0
            ? string.Format(InspectionMessages.AdditionalFailuresFormat, reason, additionalCount)
            : reason;

    private static InspectionSummary Summary(
        InspectionDecision decision, string reason, string? defectCode, IReadOnlyList<StepResult> steps)
        => new(decision, reason, defectCode, steps);
}
