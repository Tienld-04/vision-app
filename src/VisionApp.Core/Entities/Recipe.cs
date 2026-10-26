using VisionApp.Core.ValueObjects;

namespace VisionApp.Core.Entities;

/* Bộ cấu hình kiểm tra cho MỘT loại PCB. Là Entity: hai recipe bằng nhau khi cùng định danh
   ModelCode + Version, bất kể nội dung cấu hình bên trong. */
public sealed class Recipe : IEquatable<Recipe>
{
    public Recipe(
        string modelCode,
        int version,
        AlignmentSettings alignment,
        IReadOnlyList<ComponentSpec> components,
        ComponentInspectionSettings componentInspection)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelCode);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(version);
        ArgumentNullException.ThrowIfNull(alignment);
        ArgumentNullException.ThrowIfNull(components);
        ArgumentNullException.ThrowIfNull(componentInspection);

        foreach (var component in components)
        {
            ArgumentNullException.ThrowIfNull(component, nameof(components));
        }

        ModelCode = modelCode;
        Version = version;
        Alignment = alignment;
        Components = components.ToArray();
        ComponentInspection = componentInspection;
    }

    public string ModelCode { get; }

    public int Version { get; }

    public AlignmentSettings Alignment { get; }

    public IReadOnlyList<ComponentSpec> Components { get; }

    public ComponentInspectionSettings ComponentInspection { get; }

    public bool Equals(Recipe? other)
        => other is not null
           && string.Equals(ModelCode, other.ModelCode, StringComparison.Ordinal)
           && Version == other.Version;

    public override bool Equals(object? obj) => Equals(obj as Recipe);

    public override int GetHashCode() => HashCode.Combine(ModelCode, Version);

    public static bool operator ==(Recipe? left, Recipe? right)
        => left is null ? right is null : left.Equals(right);

    public static bool operator !=(Recipe? left, Recipe? right)
        => !(left == right);
}
