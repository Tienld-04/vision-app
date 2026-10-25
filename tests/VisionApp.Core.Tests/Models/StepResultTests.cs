using VisionApp.Core.Constants;
using VisionApp.Core.Models;

namespace VisionApp.Core.Tests.Models;

/* Kiểm tra các ràng buộc khi tạo StepResult qua factory method. */
public class StepResultTests
{
    [Fact]
    public void Failed_bat_buoc_phai_co_ma_loi()
    {
        Assert.Throws<ArgumentException>(() => StepResult.Failed("ComponentPresence", "", "Thiếu R12"));
    }

    [Fact]
    public void Ten_buoc_khong_duoc_rong()
    {
        Assert.Throws<ArgumentException>(() => StepResult.Passed(" "));
    }

    [Fact]
    public void Passed_khong_truyen_message_thi_dung_thong_diep_mac_dinh()
    {
        var result = StepResult.Passed("Alignment");

        Assert.Equal(InspectionMessages.StepPassed, result.Message);
    }

    [Fact]
    public void Pipeline_gan_thoi_gian_bang_with_ma_khong_doi_ket_qua_goc()
    {
        var original = StepResult.Passed("Alignment");

        var timed = original with { Duration = TimeSpan.FromMilliseconds(42) };

        Assert.Equal(TimeSpan.Zero, original.Duration);
        Assert.Equal(TimeSpan.FromMilliseconds(42), timed.Duration);
        Assert.Equal(original.Status, timed.Status);
    }
}
