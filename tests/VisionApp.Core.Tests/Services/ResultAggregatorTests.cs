using VisionApp.Core.Constants;
using VisionApp.Core.Enums;
using VisionApp.Core.ValueObjects;
using VisionApp.Core.Services;

namespace VisionApp.Core.Tests.Services;

/* Kiểm tra luật gộp kết quả: thứ tự ưu tiên và chống "đạt cho có". */
public class ResultAggregatorTests
{
    [Fact]
    public void Moi_buoc_deu_dat_thi_OK()
    {
        var summary = ResultAggregator.Aggregate(
        [
            StepResult.Passed("Alignment"),
            StepResult.Passed("ComponentPresence")
        ]);

        Assert.Equal(InspectionDecision.Ok, summary.Decision);
        Assert.Equal(InspectionMessages.AllStepsPassed, summary.Reason);
        Assert.Null(summary.DefectCode);
    }

    [Fact]
    public void Mot_buoc_Failed_thi_NG_va_mang_ma_loi_cua_buoc_do()
    {
        var summary = ResultAggregator.Aggregate(
        [
            StepResult.Passed("Alignment"),
            StepResult.Failed("ComponentPresence", DefectCodes.MissingComponent, "Thiếu R12")
        ]);

        Assert.Equal(InspectionDecision.Ng, summary.Decision);
        Assert.Equal(DefectCodes.MissingComponent, summary.DefectCode);
        Assert.Contains("ComponentPresence", summary.Reason);
    }

    [Fact]
    public void Failed_thang_Error()
    {
        var summary = ResultAggregator.Aggregate(
        [
            StepResult.Error("SurfaceDefect", "Hết thời gian xử lý"),
            StepResult.Failed("ComponentPresence", DefectCodes.MissingComponent, "Thiếu R12")
        ]);

        Assert.Equal(InspectionDecision.Ng, summary.Decision);
    }

    [Fact]
    public void Error_thang_NeedsReview()
    {
        var summary = ResultAggregator.Aggregate(
        [
            StepResult.NeedsReview("ComponentPresence", "C5 mờ, độ tin cậy thấp"),
            StepResult.Error("SurfaceDefect", "Hết thời gian xử lý")
        ]);

        Assert.Equal(InspectionDecision.Error, summary.Decision);
    }

    [Fact]
    public void NeedsReview_thang_Passed()
    {
        var summary = ResultAggregator.Aggregate(
        [
            StepResult.Passed("Alignment"),
            StepResult.NeedsReview("ComponentPresence", "C5 mờ, độ tin cậy thấp", DefectCodes.MissingComponent)
        ]);

        Assert.Equal(InspectionDecision.Review, summary.Decision);
        Assert.Equal(DefectCodes.MissingComponent, summary.DefectCode);
    }

    [Fact]
    public void Danh_sach_rong_thi_ERROR_khong_phai_OK()
    {
        var summary = ResultAggregator.Aggregate([]);

        Assert.Equal(InspectionDecision.Error, summary.Decision);
        Assert.Equal(InspectionMessages.NoStepExecuted, summary.Reason);
    }

    [Fact]
    public void Tat_ca_bi_Skipped_thi_ERROR_khong_phai_OK()
    {
        var summary = ResultAggregator.Aggregate(
        [
            StepResult.Skipped("ComponentPresence", "Tắt trong recipe"),
            StepResult.Skipped("SurfaceDefect", "Tắt trong recipe")
        ]);

        Assert.Equal(InspectionDecision.Error, summary.Decision);
    }

    [Fact]
    public void Co_buoc_Passed_va_buoc_Skipped_thi_van_OK()
    {
        var summary = ResultAggregator.Aggregate(
        [
            StepResult.Passed("Alignment"),
            StepResult.Skipped("SurfaceDefect", "Tắt trong recipe")
        ]);

        Assert.Equal(InspectionDecision.Ok, summary.Decision);
    }

    [Fact]
    public void Nhieu_buoc_Failed_thi_lay_buoc_dau_tien_lam_loi_chinh()
    {
        var summary = ResultAggregator.Aggregate(
        [
            StepResult.Failed("ComponentPresence", DefectCodes.MissingComponent, "Thiếu R12"),
            StepResult.Failed("SurfaceDefect", DefectCodes.Scratch, "Xước 0.4 mm")
        ]);

        var primaryReason = string.Format(InspectionMessages.StepReasonFormat, "ComponentPresence", "Thiếu R12");
        var expectedReason = string.Format(InspectionMessages.AdditionalFailuresFormat, primaryReason, 1);

        Assert.Equal(DefectCodes.MissingComponent, summary.DefectCode);
        Assert.Equal(expectedReason, summary.Reason);
    }

    [Fact]
    public void Ket_qua_giu_du_cac_buoc_dung_thu_tu()
    {
        var summary = ResultAggregator.Aggregate(
        [
            StepResult.Passed("Alignment"),
            StepResult.Passed("ComponentPresence")
        ]);

        Assert.Equal(new[] { "Alignment", "ComponentPresence" }, summary.Steps.Select(s => s.StepName));
    }

    [Fact]
    public void Sua_list_dau_vao_sau_khi_tong_hop_khong_lam_doi_ket_qua()
    {
        var steps = new List<StepResult> { StepResult.Passed("Alignment") };

        var summary = ResultAggregator.Aggregate(steps);
        steps.Add(StepResult.Failed("ComponentPresence", DefectCodes.MissingComponent, "Thiếu R12"));

        Assert.Single(summary.Steps);
    }
}
