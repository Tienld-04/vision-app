namespace VisionApp.Core.Constants;

/* Toàn bộ thông điệp hiển thị cho người vận hành mà tầng Core sinh ra, gom về một chỗ.
   Hằng số có hậu tố Format là chuỗi mẫu, dùng với string.Format. */
public static class InspectionMessages
{
    public const string StepPassed = "Đạt";

    public const string AllStepsPassed = "Đạt tất cả các bước kiểm tra";

    public const string NoStepExecuted = "Không có bước kiểm tra nào được thực hiện";

    public const string StepReasonFormat = "{0}: {1}";

    public const string AdditionalFailuresFormat = "{0} (+{1} lỗi khác)";
}
