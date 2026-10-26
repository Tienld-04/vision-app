namespace VisionApp.Core.ValueObjects;

/* Cấu hình bước định vị: hai dấu định vị (fiducial) trên ảnh chuẩn và độ khớp tối thiểu.
   Pivot luôn là tâm dấu định vị thứ nhất — AlignmentResult phải dùng đúng điểm này. */
public sealed record AlignmentSettings
{
    public AlignmentSettings(Roi fiducial1, Roi fiducial2, double minScore)
    {
        ArgumentNullException.ThrowIfNull(fiducial1);
        ArgumentNullException.ThrowIfNull(fiducial2);
        ArgumentOutOfRangeException.ThrowIfLessThan(minScore, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minScore, 1);

        Fiducial1 = fiducial1;
        Fiducial2 = fiducial2;
        MinScore = minScore;
    }

    public Roi Fiducial1 { get; }

    public Roi Fiducial2 { get; }

    public double MinScore { get; }

    public Point2D Pivot => Fiducial1.Center;
}
