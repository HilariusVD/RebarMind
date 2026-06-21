using System.Text.Json.Serialization;

namespace RebarMind.Core.Config;

/// <summary>
/// Root schema for rebarMindConfig.json
/// Maps 1:1 to JSON structure — property names use camelCase via JsonPropertyName.
/// </summary>
public sealed record RebarMindConfig
{
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "2.0.0";

    [JsonPropertyName("activeCodeProfile")]
    public string ActiveCodeProfile { get; init; } = "SNI2847";

    [JsonPropertyName("codeProfiles")]
    public Dictionary<string, string> CodeProfiles { get; init; } = new()
    {
        ["SNI2847"] = "RebarMind.Codes.SNI2847Profile",
        ["ACI318"]  = "RebarMind.Codes.ACI318Profile"
    };

    [JsonPropertyName("material")]
    public MaterialConfig Material { get; init; } = new();

    [JsonPropertyName("cover")]
    public CoverConfig Cover { get; init; } = new();

    [JsonPropertyName("mainBars")]
    public MainBarsConfig MainBars { get; init; } = new();

    [JsonPropertyName("stirrups")]
    public StirrupsConfig Stirrups { get; init; } = new();

    [JsonPropertyName("splicing")]
    public SplicingConfig Splicing { get; init; } = new();
}

public sealed record MaterialConfig
{
    [JsonPropertyName("fc_prime_MPa")]
    public double FcPrime { get; init; } = 30.0;

    [JsonPropertyName("fy_MPa")]
    public double Fy { get; init; } = 420.0;

    [JsonPropertyName("aggregateSize_mm")]
    public double AggregateSize { get; init; } = 20.0;
}

public sealed record CoverConfig
{
    [JsonPropertyName("top_mm")]
    public double Top { get; init; } = 40.0;

    [JsonPropertyName("bottom_mm")]
    public double Bottom { get; init; } = 40.0;

    [JsonPropertyName("side_mm")]
    public double Side { get; init; } = 40.0;

    [JsonPropertyName("linkAll")]
    public bool LinkAll { get; init; } = true;
}

public sealed record MainBarsConfig
{
    [JsonPropertyName("diameter_mm")]
    public double Diameter { get; init; } = 25.0;

    [JsonPropertyName("shapeCode")]
    public string ShapeCode { get; init; } = "M_00";

    [JsonPropertyName("layoutRule")]
    public string LayoutRule { get; init; } = "FixedNumber";

    [JsonPropertyName("nx")]
    public int Nx { get; init; } = 4;

    [JsonPropertyName("ny")]
    public int Ny { get; init; } = 4;
}

public sealed record StirrupsConfig
{
    [JsonPropertyName("diameter_mm")]
    public double Diameter { get; init; } = 10.0;

    [JsonPropertyName("shapeCode")]
    public string ShapeCode { get; init; } = "M_T1";

    [JsonPropertyName("supportZoneExpression")]
    public string SupportZoneExpression { get; init; } = "L/4";

    [JsonPropertyName("spacingAtSupport_mm")]
    public double SpacingAtSupport { get; init; } = 100.0;

    [JsonPropertyName("spacingAtMid_mm")]
    public double SpacingAtMid { get; init; } = 150.0;

    [JsonPropertyName("enableCrossTies")]
    public bool EnableCrossTies { get; init; } = false;
}

public sealed record SplicingConfig
{
    [JsonPropertyName("enableAutoSplit")]
    public bool EnableAutoSplit { get; init; } = true;

    [JsonPropertyName("maxStockLength_mm")]
    public double MaxStockLength { get; init; } = 12000.0;

    [JsonPropertyName("spliceType")]
    public string SpliceType { get; init; } = "LapSplice";

    [JsonPropertyName("lapMultiplier")]
    public string LapMultiplier { get; init; } = "40*d";

    [JsonPropertyName("enableStaggering")]
    public bool EnableStaggering { get; init; } = true;
}
