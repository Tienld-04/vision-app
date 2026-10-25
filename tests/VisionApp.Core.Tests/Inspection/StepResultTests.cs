using VisionApp.Core.Inspection;

namespace VisionApp.Core.Tests.Inspection;

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
    public void Pipeline_gan_thoi_gian_bang_with_ma_khong_doi_ket_qua_goc()
    {
        var original = StepResult.Passed("Alignment");

        var timed = original with { Duration = TimeSpan.FromMilliseconds(42) };

        Assert.Equal(TimeSpan.Zero, original.Duration);
        Assert.Equal(TimeSpan.FromMilliseconds(42), timed.Duration);
        Assert.Equal(original.Status, timed.Status);
    }
}
