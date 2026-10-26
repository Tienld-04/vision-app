namespace VisionApp.Core.ValueObjects;

/* Ngưỡng của bước kiểm linh kiện. Điểm khớp >= ngưỡng + biên: có mặt; < ngưỡng − biên: thiếu;
   nằm giữa hai mức: vùng xám, cần người duyệt. */
public sealed record ComponentInspectionSettings
{
    public ComponentInspectionSettings(
        double presenceThreshold, double reviewMargin, double maxShiftPx, double maxRotationDegrees)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(reviewMargin);
        ArgumentOutOfRangeException.ThrowIfLessThan(presenceThreshold, reviewMargin);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(presenceThreshold, 1 - reviewMargin);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxShiftPx);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRotationDegrees);

        PresenceThreshold = presenceThreshold;
        ReviewMargin = reviewMargin;
        MaxShiftPx = maxShiftPx;
        MaxRotationDegrees = maxRotationDegrees;
    }

    public double PresenceThreshold { get; }

    public double ReviewMargin { get; }

    public double MaxShiftPx { get; }

    public double MaxRotationDegrees { get; }
}
