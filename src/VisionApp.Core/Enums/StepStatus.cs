namespace VisionApp.Core.Enums;

/* Kết quả của MỘT bước kiểm tra đơn lẻ (định vị, kiểm linh kiện, kiểm bề mặt...). */
public enum StepStatus
{
    Passed = 1,
    Failed = 2,
    NeedsReview = 3,
    Error = 4,
    Skipped = 5
}
