namespace VisionApp.Core.Constants;

/* Thông báo lỗi khi kiểm tra recipe, hiển thị cho kỹ sư soạn recipe.
   Hằng số có hậu tố Format là chuỗi mẫu, dùng với string.Format. */
public static class RecipeMessages
{
    public const string Fiducial1Label = "dấu định vị 1";

    public const string Fiducial2Label = "dấu định vị 2";

    public const string NoComponents = "Recipe chưa có linh kiện nào để kiểm";

    public const string DuplicateDesignatorFormat = "Trùng mã linh kiện: {0}";

    public const string FiducialsTooCloseFormat =
        "Hai dấu định vị cách nhau {0:F1} px, cần tối thiểu {1} px để tính được góc xoay";

    public const string RoiOutOfImageFormat = "Vùng kiểm tra của {0} nằm ngoài khung ảnh {1}×{2}";
}
