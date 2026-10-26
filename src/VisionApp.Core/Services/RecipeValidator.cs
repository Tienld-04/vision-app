using VisionApp.Core.Constants;
using VisionApp.Core.Entities;
using VisionApp.Core.ValueObjects;

namespace VisionApp.Core.Services;

/* Kiểm tra chéo giữa các thành phần của recipe. Trả về TẤT CẢ lỗi tìm được thay vì văng exception
   ở lỗi đầu tiên, vì recipe do người soạn tay — cần thấy hết lỗi trong một lần. Danh sách rỗng = hợp lệ. */
public static class RecipeValidator
{
    public static IReadOnlyList<string> Validate(Recipe recipe, int imageWidth, int imageHeight)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(imageWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(imageHeight);

        var errors = new List<string>();

        if (recipe.Components.Count == 0)
        {
            errors.Add(RecipeMessages.NoComponents);
        }

        var duplicates = recipe.Components
            .GroupBy(c => c.Designator, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var designator in duplicates)
        {
            errors.Add(string.Format(RecipeMessages.DuplicateDesignatorFormat, designator));
        }

        var fiducial1 = recipe.Alignment.Fiducial1.Center;
        var fiducial2 = recipe.Alignment.Fiducial2.Center;
        var fiducialDistance = double.Hypot(fiducial2.X - fiducial1.X, fiducial2.Y - fiducial1.Y);

        if (fiducialDistance < RecipeLimits.MinFiducialDistancePx)
        {
            errors.Add(string.Format(
                RecipeMessages.FiducialsTooCloseFormat, fiducialDistance, RecipeLimits.MinFiducialDistancePx));
        }

        CheckInsideImage(recipe.Alignment.Fiducial1, RecipeMessages.Fiducial1Label, imageWidth, imageHeight, errors);
        CheckInsideImage(recipe.Alignment.Fiducial2, RecipeMessages.Fiducial2Label, imageWidth, imageHeight, errors);

        foreach (var component in recipe.Components)
        {
            CheckInsideImage(component.Roi, component.Designator, imageWidth, imageHeight, errors);
        }

        return errors;
    }

    private static void CheckInsideImage(Roi roi, string label, int imageWidth, int imageHeight, List<string> errors)
    {
        var inside = roi.X >= 0
                     && roi.Y >= 0
                     && roi.X + roi.Width <= imageWidth
                     && roi.Y + roi.Height <= imageHeight;

        if (!inside)
        {
            errors.Add(string.Format(RecipeMessages.RoiOutOfImageFormat, label, imageWidth, imageHeight));
        }
    }
}
