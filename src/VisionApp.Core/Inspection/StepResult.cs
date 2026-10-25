namespace VisionApp.Core.Inspection;

/* Kết quả bất biến của một bước kiểm tra, chỉ tạo được qua các factory method.
   Chỉ Duration sửa được bằng `with`, để pipeline gán thời gian chạy. */
public sealed record StepResult
{
    private StepResult(string stepName, StepStatus status, string? defectCode, string message)
    {
        StepName = stepName;
        Status = status;
        DefectCode = defectCode;
        Message = message;
    }

    public string StepName { get; }

    public StepStatus Status { get; }

    public string? DefectCode { get; }

    public string Message { get; }

    public TimeSpan Duration { get; init; }

    public static StepResult Passed(string stepName, string message = "Đạt")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepName);
        return new StepResult(stepName, StepStatus.Passed, null, message);
    }

    public static StepResult Failed(string stepName, string defectCode, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepName);
        ArgumentException.ThrowIfNullOrWhiteSpace(defectCode);
        return new StepResult(stepName, StepStatus.Failed, defectCode, message);
    }

    public static StepResult NeedsReview(string stepName, string message, string? suspectedDefectCode = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepName);
        return new StepResult(stepName, StepStatus.NeedsReview, suspectedDefectCode, message);
    }

    public static StepResult Error(string stepName, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepName);
        return new StepResult(stepName, StepStatus.Error, null, message);
    }

    public static StepResult Skipped(string stepName, string reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepName);
        return new StepResult(stepName, StepStatus.Skipped, null, reason);
    }
}
