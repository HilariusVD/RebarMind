using RebarMind.Core.Config;

namespace RebarMind.Core.Geometry;

/// <summary>
/// Pure-logic mesh generator for rectangular columns.
/// Implements §1.3: cover offset + Fixed Number layout.
/// All coordinates in mm, origin at bottom-center of column.
/// </summary>
public sealed class MeshGenerator : IMeshGenerator
{
    public BarLayout GenerateColumnLayout(
        double hostWidth,
        double hostHeight,
        double hostLength,
        RebarMindConfig config)
    {
        // §7.2 Pre-validation
        ValidateHost(hostWidth, hostHeight, hostLength, config);

        var mainBars = GenerateMainBars(hostWidth, hostHeight, hostLength, config);
        var (stirrups, zones) = GenerateStirrups(hostWidth, hostHeight, hostLength, config);

        return new BarLayout
        {
            MainBars = mainBars,
            Stirrups = stirrups,
            Zones = zones,
            HostWidth = hostWidth,
            HostHeight = hostHeight,
            HostLength = hostLength
        };
    }

    private static void ValidateHost(double w, double h, double L, RebarMindConfig cfg)
    {
        double maxCover = Math.Max(cfg.Cover.Top, Math.Max(cfg.Cover.Bottom, cfg.Cover.Side));
        if (maxCover > w / 2.0 || maxCover > h / 2.0)
            throw new RebarMindValidationException(
                $"Cover {maxCover}mm exceeds section half-width. Reduce cover or check host size.",
                ErrorSeverity.Blocker);

        if (L <= 0)
            throw new RebarMindValidationException(
                "Host length must be positive.",
                ErrorSeverity.Blocker);
    }

    /// <summary>
    /// Generate longitudinal bars at corners + evenly distributed along faces.
    /// Layout: Nx bars along X-axis (top & bottom face), Ny along Y-axis (left & right face).
    /// Total = 2*Nx + 2*(Ny-2) to avoid corner duplication.
    /// </summary>
    private List<BarSegment> GenerateMainBars(
        double w, double h, double L, RebarMindConfig cfg)
    {
        var bars = new List<BarSegment>();
        double cover = cfg.Cover.Side;
        double dia = cfg.MainBars.Diameter;

        // Inner rectangle (centerline of bars)
        double innerW = w - 2 * cover - dia;
        double innerH = h - 2 * cover - dia;

        int nx = cfg.MainBars.Nx;
        int ny = cfg.MainBars.Ny;

        // Generate bar positions along perimeter (counter-clockwise from bottom-left)
        var positions = new List<(double x, double y)>();

        // Bottom face: nx bars
        for (int i = 0; i < nx; i++)
        {
            double x = -innerW / 2.0 + (nx > 1 ? i * innerW / (nx - 1) : 0);
            positions.Add((x, -innerH / 2.0));
        }
        // Right face: ny-2 bars (skip corners)
        for (int i = 1; i < ny - 1; i++)
        {
            double y = -innerH / 2.0 + i * innerH / (ny - 1);
            positions.Add((innerW / 2.0, y));
        }
        // Top face: nx bars (reverse)
        for (int i = nx - 1; i >= 0; i--)
        {
            double x = -innerW / 2.0 + (nx > 1 ? i * innerW / (nx - 1) : 0);
            positions.Add((x, innerH / 2.0));
        }
        // Left face: ny-2 bars (skip corners, reverse)
        for (int i = ny - 2; i >= 1; i--)
        {
            double y = -innerH / 2.0 + i * innerH / (ny - 1);
            positions.Add((-innerW / 2.0, y));
        }

        // Create vertical bar segments
        for (int i = 0; i < positions.Count; i++)
        {
            var (x, y) = positions[i];
            bars.Add(new BarSegment
            {
                Id = $"MB-{i + 1:D2}",
                StartPoint = [x, y, 0],
                EndPoint = [x, y, L],
                Diameter = dia,
                Zone = "Full"
            });
        }

        return bars;
    }

    /// <summary>
    /// Generate stirrups with zone-based spacing.
    /// Support zones at top & bottom (expression-based), mid zone in between.
    /// </summary>
    private (List<BarSegment> stirrups, List<StirrupZone> zones) GenerateStirrups(
        double w, double h, double L, RebarMindConfig cfg)
    {
        var stirrups = new List<BarSegment>();
        var zones = new List<StirrupZone>();

        // Parse support zone expression (simplified: "L/4" → L/4 length)
        double supportLength = ParseZoneExpression(cfg.Stirrups.SupportZoneExpression, L);
        supportLength = Math.Min(supportLength, L / 2.0); // Cap at half-length

        double midStart = supportLength;
        double midEnd = L - supportLength;

        // Bottom support zone
        var bottomZone = CreateStirrupZone(
            "Bottom Support", 0, supportLength, cfg.Stirrups.SpacingAtSupport, cfg.Stirrups.Diameter, w, h);
        zones.Add(bottomZone.Zone);
        stirrups.AddRange(bottomZone.Stirrups);

        // Mid zone
        if (midEnd > midStart)
        {
            var midZone = CreateStirrupZone(
                "Mid", midStart, midEnd, cfg.Stirrups.SpacingAtMid, cfg.Stirrups.Diameter, w, h);
            zones.Add(midZone.Zone);
            stirrups.AddRange(midZone.Stirrups);
        }

        // Top support zone
        var topZone = CreateStirrupZone(
            "Top Support", L - supportLength, L, cfg.Stirrups.SpacingAtSupport, cfg.Stirrups.Diameter, w, h);
        zones.Add(topZone.Zone);
        stirrups.AddRange(topZone.Stirrups);

        return (stirrups, zones);
    }

    private (List<BarSegment> Stirrups, StirrupZone Zone) CreateStirrupZone(
        string name, double startZ, double endZ, double spacing, double dia, double w, double h)
    {
        var stirrups = new List<BarSegment>();
        double zoneLength = endZ - startZ;
        int count = (int)Math.Ceiling(zoneLength / spacing) + 1;

        // Clamp count to prevent over-generation at boundaries
        count = Math.Max(1, count);

        for (int i = 0; i < count; i++)
        {
            double z = startZ + i * spacing;
            if (z > endZ + 0.1) break; // tolerance 0.1mm

            // Simplified: represent stirrup as 4 segments (rectangular perimeter)
            double cover = 20; // stirrup cover (outer)
            double hw = w / 2.0 - cover;
            double hh = h / 2.0 - cover;

            // Bottom edge
            stirrups.Add(new BarSegment
            {
                Id = $"ST-{name[0]}-{i + 1:D2}-B",
                StartPoint = [-hw, -hh, z],
                EndPoint = [hw, -hh, z],
                Diameter = dia,
                Zone = name
            });
            // Right edge
            stirrups.Add(new BarSegment
            {
                Id = $"ST-{name[0]}-{i + 1:D2}-R",
                StartPoint = [hw, -hh, z],
                EndPoint = [hw, hh, z],
                Diameter = dia,
                Zone = name
            });
            // Top edge
            stirrups.Add(new BarSegment
            {
                Id = $"ST-{name[0]}-{i + 1:D2}-T",
                StartPoint = [hw, hh, z],
                EndPoint = [-hw, hh, z],
                Diameter = dia,
                Zone = name
            });
            // Left edge
            stirrups.Add(new BarSegment
            {
                Id = $"ST-{name[0]}-{i + 1:D2}-L",
                StartPoint = [-hw, hh, z],
                EndPoint = [-hw, -hh, z],
                Diameter = dia,
                Zone = name
            });
        }

        return (stirrups, new StirrupZone
        {
            Name = name,
            StartElevation = startZ,
            EndElevation = endZ,
            Spacing = spacing,
            StirrupCount = count
        });
    }

    /// <summary>
    /// Parse simple zone expressions: "L/4", "L/3", "0.25*L", "500"
    /// Returns length in mm. Full mXparser integration in §1.4.
    /// </summary>
    private static double ParseZoneExpression(string expr, double L)
    {
        expr = expr.Trim().Replace(" ", "");

        // Handle "L/n" pattern
        if (expr.StartsWith("L/") && double.TryParse(expr[2..], out double divisor))
            return L / divisor;

        // Handle "n*L" or "L*n"
        if (expr.Contains("*L"))
        {
            string coef = expr.Replace("*L", "");
            if (double.TryParse(coef, out double c)) return c * L;
        }
        if (expr.StartsWith("L*"))
        {
            string coef = expr[2..];
            if (double.TryParse(coef, out double c)) return c * L;
        }

        // Handle plain number (absolute mm)
        if (double.TryParse(expr, out double abs)) return abs;

        throw new RebarMindValidationException(
            $"Invalid zone expression '{expr}'. Use: L/4, 0.25*L, or absolute mm.",
            ErrorSeverity.Blocker);
    }
}
