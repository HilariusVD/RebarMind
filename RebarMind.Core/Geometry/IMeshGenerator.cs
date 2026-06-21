using RebarMind.Core.Config;

namespace RebarMind.Core.Geometry;

/// <summary>
/// Contract for mesh generation. Revit-agnostic.
/// Implementations: PureMeshGenerator (unit-testable), RevitMeshGenerator (uses API).
/// </summary>
public interface IMeshGenerator
{
    /// <summary>
    /// Calculate bar layout for a rectangular column host.
    /// All dimensions in mm. Pure math — no Revit dependency.
    /// </summary>
    BarLayout GenerateColumnLayout(
        double hostWidth,
        double hostHeight,
        double hostLength,
        RebarMindConfig config);
}
