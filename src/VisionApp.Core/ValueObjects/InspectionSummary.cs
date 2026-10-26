using VisionApp.Core.Enums;

namespace VisionApp.Core.ValueObjects;

/* Kết quả tổng hợp của một lần kiểm tra: kết luận, lý do, mã lỗi chính và chi tiết từng bước.
   Tự viết Equals vì danh sách Steps phải so từng phần tử — record chỉ so được tham chiếu của danh sách. */
public sealed record InspectionSummary
{
    public InspectionSummary(
        InspectionDecision decision, string reason, string? defectCode, IReadOnlyList<StepResult> steps)
    {
        ArgumentNullException.ThrowIfNull(reason);
        ArgumentNullException.ThrowIfNull(steps);

        foreach (var step in steps)
        {
            ArgumentNullException.ThrowIfNull(step, nameof(steps));
        }

        Decision = decision;
        Reason = reason;
        DefectCode = defectCode;
        Steps = steps.ToArray();
    }

    public InspectionDecision Decision { get; }

    public string Reason { get; }

    public string? DefectCode { get; }

    public IReadOnlyList<StepResult> Steps { get; }

    public bool Equals(InspectionSummary? other)
        => other is not null
           && Decision == other.Decision
           && Reason == other.Reason
           && DefectCode == other.DefectCode
           && Steps.SequenceEqual(other.Steps);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Decision);
        hash.Add(Reason);
        hash.Add(DefectCode);

        foreach (var step in Steps)
        {
            hash.Add(step);
        }

        return hash.ToHashCode();
    }
}
