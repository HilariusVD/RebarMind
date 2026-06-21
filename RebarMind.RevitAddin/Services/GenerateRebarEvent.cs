using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RebarMind.Core;
using RebarMind.Core.Codes;
using RebarMind.Core.Config;
using RebarMind.Core.Geometry;
using RebarMind.RevitAddin.ViewModels;
using System;
using System.Linq;

namespace RebarMind.RevitAddin.Services;

/// <summary>
/// ExternalEvent handler — jembatan aman dari UI thread ke Revit API thread.
/// Implementasi lengkap dengan TransactionManager dan RebarCreator (§9, §10 PRD).
/// </summary>
public class GenerateRebarEvent : IExternalEventHandler
{
    private readonly MeshGenerator _meshGenerator = new();
    private readonly ConfigLoader _configLoader = new();
    private readonly SNI2847Profile _codeProfile = new();

    public void Execute(UIApplication app)
    {
        UIDocument uidoc = app.ActiveUIDocument;
        Document doc = uidoc.Document;

        try
        {
            var viewModel = RebarMindViewModel.Instance;
            RebarMindConfig config = viewModel.ToConfig();

            var selectedIds = uidoc.Selection.GetElementIds();
            if (selectedIds.Count == 0)
            {
                TaskDialog.Show("RebarMind", "No element selected.");
                return;
            }

            Element host = doc.GetElement(selectedIds.First());

            var hostDimensions = ExtractHostDimensions(host);
            double hostWidth = hostDimensions.Width;
            double hostHeight = hostDimensions.Height;
            double hostLength = hostDimensions.Length;

            BarLayout layout = _meshGenerator.GenerateColumnLayout(
                hostWidth, hostHeight, hostLength, config);

            var validator = new GeometryValidator(_codeProfile);
            validator.Validate(layout, config);

            int totalCreated = 0;

            using (var txManager = new TransactionManager(doc, "RebarMind: Generate Column Rebar"))
            {
                try
                {
                    var rebarCreator = new RebarCreator(doc);
                    totalCreated = rebarCreator.CreateRebar(layout, config, host);

                    txManager.Commit();

                    TaskDialog.Show("RebarMind - Success",
                        $"✅ Successfully generated rebar for: {viewModel.HostInfo}\n\n" +
                        $"Main Bars: {layout.MainBars.Count}\n" +
                        $"Stirrup Segments: {layout.Stirrups.Count}\n" +
                        $"Zones: {layout.Zones.Count}\n" +
                        $"Code Profile: {layout.CodeProfileUsed}\n" +
                        $"Total Rebar Created: {totalCreated}");
                }
                catch (Exception ex)
                {
                    txManager.RollBack();
                    throw new RebarMindValidationException(
                        $"Failed to create rebar: {ex.Message}",
                        ErrorSeverity.Blocker,
                        ex);
                }
            }
        }
        catch (RebarMindValidationException ex)
        {
            string icon = ex.Severity == ErrorSeverity.Blocker ? "❌" : "⚠️";
            TaskDialog.Show("RebarMind - Validation Error",
                $"{icon} {ex.Message}\n\nSeverity: {ex.Severity}");
        }
        catch (Exception ex)
        {
            TaskDialog.Show("RebarMind - Error",
                $"An unexpected error occurred:\n\n{ex.Message}\n\n" +
                $"See log at: %APPDATA%/RebarMind/logs/");
        }
    }

    private (double Width, double Height, double Length) ExtractHostDimensions(Element host)
    {
        double width = GetParameterValue(host, "Width") ?? 600;
        double height = GetParameterValue(host, "Height") ?? 600;
        double length = GetParameterValue(host, "Length") ?? 3000;

        double feetToMm = 304.8;

        return (width * feetToMm, height * feetToMm, length * feetToMm);
    }

    private double? GetParameterValue(Element element, string parameterName)
    {
        var param = element.LookupParameter(parameterName);
        if (param == null) return null;

        return param.AsDouble();
    }

    public string GetName() => "RebarMind: Generate Rebar";
}
