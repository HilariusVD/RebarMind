namespace RebarMind.Core.Geometry;

/// <summary>
/// Pure-data DTO representing calculated bar positions.
/// Revit-agnostic — uses double coordinates in mm.
/// </summary>
public sealed record BarLayout
{
    public required IReadOnlyList<BarSegment> MainBars { get; init; }
    public required IReadOnlyList<BarSegment> Stirrups { get; init; }
    public required IReadOnlyList<StirrupZone> Zones { get; init; }
    public required double HostLength { get; init; }
    public required double HostWidth { get; init; }
    public required double HostHeight { get; init; }
}

public sealed record BarSegment
{
    public required string Id { get; init; }              // "MB-01", "ST-S-03"
    public required double[] StartPoint { get; init; }    // [x, y, z] in mm
    public required double[] EndPoint { get; init; }
    public required double Diameter { get; init; }
    public required string Zone { get; init; }            // "Support" / "Mid"
}

public sealed record StirrupZone
{
    public required string Name { get; init; }            // "Bottom Support", "Mid"
    public required double StartElevation { get; init; }  // mm from bottom
    public required double EndElevation { get; init; }
    public required double Spacing { get; init; }
    public required int StirrupCount { get; init; }
}