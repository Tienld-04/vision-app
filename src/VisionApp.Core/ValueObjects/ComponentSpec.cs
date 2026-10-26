namespace VisionApp.Core.ValueObjects;

/* Một linh kiện cần kiểm: mã linh kiện in trên bo (R12, C5...) và vùng kiểm tra của nó trên ảnh chuẩn. */
public sealed record ComponentSpec
{
    public ComponentSpec(string designator, Roi roi)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(designator);
        ArgumentNullException.ThrowIfNull(roi);

        Designator = designator;
        Roi = roi;
    }

    public string Designator { get; }

    public Roi Roi { get; }
}
