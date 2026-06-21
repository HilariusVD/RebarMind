using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using RebarMind.Core;
using RebarMind.Core.Config;
using RebarMind.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RebarMind.RevitAddin.Services;

/// <summary>
/// Create actual Revit rebar elements dari BarLayout (§10 PRD).
/// TODO: Update signature CreateFromCurves sesuai Revit 2026 API documentation.
/// </summary>
public class RebarCreator
{
    private readonly Document _doc;

    public RebarCreator(Document doc)
    {
        _doc = doc ?? throw new ArgumentNullException(nameof(doc));
    }

    /// <summary>
    /// Create semua rebar dari layout. Return jumlah rebar yang berhasil dibuat.
    /// </summary>
    public int CreateRebar(BarLayout layout, RebarMindConfig config, Element host)
    {
        int totalCreated = 0;

        // Create main bars
        totalCreated += CreateMainBars(layout.MainBars, config, host);

        // Create stirrups
        totalCreated += CreateStirrups(layout.Stirrups, config, host);

        return totalCreated;
    }

    private int CreateMainBars(
        IReadOnlyList<BarSegment> mainBars, 
        RebarMindConfig config, 
        Element host)
    {
        int created = 0;
        var rebarType = GetRebarType(config.MainBars.Diameter);

        if (rebarType == null)
            throw new RebarMindValidationException(
                $"Rebar type for diameter {config.MainBars.Diameter}mm not found in project.",
                ErrorSeverity.Blocker);

        foreach (var bar in mainBars)
        {
            try
            {
                // TODO: Implementasi actual rebar creation setelah cek Revit 2026 API docs
                // Untuk sekarang, kita stub dulu supaya bisa compile dan test flow lainnya
                
                // Placeholder: log bahwa kita akan create bar ini
                System.Diagnostics.Debug.WriteLine($"Would create main bar {bar.Id} at [{bar.StartPoint[0]}, {bar.StartPoint[1]}, {bar.StartPoint[2]}]");
                
                created++;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create main bar {bar.Id}: {ex.Message}");
            }
        }

        return created;
    }

    private int CreateStirrups(
        IReadOnlyList<BarSegment> stirrups, 
        RebarMindConfig config, 
        Element host)
    {
        int created = 0;
        var rebarType = GetRebarType(config.Stirrups.Diameter);

        if (rebarType == null)
            throw new RebarMindValidationException(
                $"Rebar type for stirrup diameter {config.Stirrups.Diameter}mm not found.",
                ErrorSeverity.Blocker);

        foreach (var stirrup in stirrups)
        {
            try
            {
                // TODO: Implementasi actual stirrup creation
                System.Diagnostics.Debug.WriteLine($"Would create stirrup {stirrup.Id} at [{stirrup.StartPoint[0]}, {stirrup.StartPoint[1]}, {stirrup.StartPoint[2]}]");
                
                created++;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to create stirrup {stirrup.Id}: {ex.Message}");
            }
        }

        return created;
    }

    private RebarBarType? GetRebarType(double diameter)
    {
        var filter = new FilteredElementCollector(_doc)
            .OfClass(typeof(RebarBarType))
            .Cast<RebarBarType>()
            .FirstOrDefault(r => 
            {
                var barDiameter = r.LookupParameter("Bar Diameter")?.AsDouble();
                return barDiameter.HasValue && 
                       Math.Abs(barDiameter.Value * 304.8 - diameter) < 0.1;
            });

        return filter;
    }
}
