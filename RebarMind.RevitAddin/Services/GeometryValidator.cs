using Autodesk.Revit.DB;
using RebarMind.Core;
using RebarMind.Core.Codes;
using RebarMind.Core.Config;
using RebarMind.Core.Geometry;
using System;
using System.Linq;

namespace RebarMind.RevitAddin.Services;

/// <summary>
/// Validasi geometri sebelum create rebar (§7.2 PRD).
/// Cek spacing, cover, rebar ratio, dan clash detection.
/// </summary>
public class GeometryValidator
{
    private readonly ICodeProfile _codeProfile;

    public GeometryValidator(ICodeProfile codeProfile)
    {
        _codeProfile = codeProfile ?? throw new ArgumentNullException(nameof(codeProfile));
    }

    /// <summary>
    /// Validasi layout sebelum create rebar di Revit.
    /// Throw RebarMindValidationException jika ada violation.
    /// </summary>
    public void Validate(BarLayout layout, RebarMindConfig config)
    {
        ValidateCover(layout, config);
        ValidateSpacing(layout, config);
        ValidateRebarRatio(layout, config);
    }

    private void ValidateCover(BarLayout layout, RebarMindConfig config)
    {
        double minCover = Math.Min(config.Cover.Top, Math.Min(config.Cover.Bottom, config.Cover.Side));
        
        foreach (var bar in layout.MainBars)
        {
            double x = Math.Abs(bar.StartPoint[0]);
            double y = Math.Abs(bar.StartPoint[1]);
            double halfWidth = layout.HostWidth / 2.0;
            double halfHeight = layout.HostHeight / 2.0;

            double coverX = halfWidth - x - (bar.Diameter / 2.0);
            double coverY = halfHeight - y - (bar.Diameter / 2.0);

            if (coverX < minCover || coverY < minCover)
            {
                throw new RebarMindValidationException(
                    $"Bar {bar.Id} violates minimum cover requirement ({minCover}mm).",
                    ErrorSeverity.Blocker);
            }
        }
    }

    private void ValidateSpacing(BarLayout layout, RebarMindConfig config)
    {
        double minSpacing = _codeProfile.GetMinClearSpacing(
            config.MainBars.Diameter, 
            config.Material.AggregateSize);

        var mainBars = layout.MainBars
            .Where(b => b.Zone == "Full")
            .OrderBy(b => b.StartPoint[0])
            .ThenBy(b => b.StartPoint[1])
            .ToList();

        for (int i = 0; i < mainBars.Count - 1; i++)
        {
            var bar1 = mainBars[i];
            var bar2 = mainBars[i + 1];

            double distance = Math.Sqrt(
                Math.Pow(bar2.StartPoint[0] - bar1.StartPoint[0], 2) +
                Math.Pow(bar2.StartPoint[1] - bar1.StartPoint[1], 2));

            double clearSpacing = distance - (bar1.Diameter / 2.0) - (bar2.Diameter / 2.0);

            if (clearSpacing < minSpacing)
            {
                throw new RebarMindValidationException(
                    $"Clear spacing between {bar1.Id} and {bar2.Id} is {clearSpacing:F1}mm, " +
                    $"minimum required is {minSpacing:F1}mm.",
                    ErrorSeverity.Blocker);
            }
        }
    }

    private void ValidateRebarRatio(BarLayout layout, RebarMindConfig config)
    {
        double totalBarArea = layout.MainBars.Sum(b => 
            Math.PI * Math.Pow(b.Diameter / 2.0, 2));
        double grossArea = layout.HostWidth * layout.HostHeight;
        double ratio = totalBarArea / grossArea;

        double minRatio = _codeProfile.GetMinRebarRatio(StructuralElementType.Column);
        double maxRatio = _codeProfile.GetMaxRebarRatio(StructuralElementType.Column);

        if (ratio < minRatio)
        {
            throw new RebarMindValidationException(
                $"Rebar ratio {ratio:P2} is below minimum {minRatio:P2}.",
                ErrorSeverity.Warning);
        }

        if (ratio > maxRatio)
        {
            throw new RebarMindValidationException(
                $"Rebar ratio {ratio:P2} exceeds maximum {maxRatio:P2}.",
                ErrorSeverity.Blocker);
        }
    }
}
