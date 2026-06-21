using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RebarMind.Core;
using RebarMind.Core.Config;
using RebarMind.Core.Geometry;
using RebarMind.RevitAddin.ViewModels;
using System;

namespace RebarMind.RevitAddin.Services;

/// <summary>
/// ExternalEvent handler — jembatan aman dari UI thread ke Revit API thread.
/// Semua operasi Revit API WAJIB lewat sini (§9 PRD).
/// </summary>
public class GenerateRebarEvent : IExternalEventHandler
{
    private readonly MeshGenerator _meshGenerator = new();
    private readonly ConfigLoader _configLoader = new();

    public void Execute(UIApplication app)
    {
        UIDocument uidoc = app.ActiveUIDocument;
        Document doc = uidoc.Document;

        try
        {
            // 1. Ambil config dari ViewModel
            var viewModel = RebarMindViewModel.Instance;
            RebarMindConfig config = viewModel.ToConfig();

            // 2. Ambil elemen yang dipilih
            var selectedIds = uidoc.Selection.GetElementIds();
            if (selectedIds.Count == 0)
            {
                TaskDialog.Show("RebarMind", "No element selected.");
                return;
            }

            Element host = doc.GetElement(selectedIds.First());

            // 3. Extract dimensi host (simplified — full extraction di Tahap 3)
            double hostWidth = 600;    // TODO: Extract dari parameter Revit
            double hostHeight = 600;   // TODO: Extract dari parameter Revit
            double hostLength = 3000;  // TODO: Extract dari parameter Revit

            // 4. Generate layout (pure logic, NO Revit API)
            BarLayout layout = _meshGenerator.GenerateColumnLayout(
                hostWidth, hostHeight, hostLength, config);

            // 5. §9 TransactionGroup — Atomic Generate
            using var tg = new TransactionGroup(doc, "RebarMind: Generate Column Rebar");
            tg.Start();

            try
            {
                // TODO Tahap 3: Sub-transactions
                // Transaction 1: Purge existing rebar (if Overwrite)
                // Transaction 2: Generate main bars via Rebar.Create()
                // Transaction 3: Generate stirrups via Rebar.Create()
                // Transaction 4: Write BBS parameters

                // Placeholder: show result
                tg.Assimilate(); // Commit semua sebagai 1 undo step

                TaskDialog.Show("RebarMind - Success",
                    $"Generated for: {viewModel.HostInfo}\n\n" +
                    $"Main Bars: {layout.MainBars.Count}\n" +
                    $"Stirrup Segments: {layout.Stirrups.Count}\n" +
                    $"Zones: {layout.Zones.Count}\n" +
                    $"Code Profile: {layout.CodeProfileUsed}");
            }
            catch
            {
                tg.RollBack(); // §9.2: Rollback di setiap path exception
                throw;
            }
        }
        catch (RebarMindValidationException ex)
        {
            // §7.3: User-friendly error, bukan stack trace
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

    public string GetName() => "RebarMind: Generate Rebar";
}
