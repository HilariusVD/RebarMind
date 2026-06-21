using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RebarMind.Core;
using RebarMind.RevitAddin.ViewModels;
using RebarMind.RevitAddin.Views;
using System.Linq;

namespace RebarMind.RevitAddin.Commands;

/// <summary>
/// Command yang dipanggil saat user klik button di Ribbon.
/// Melakukan pre-condition checks (§7.1) lalu buka modeless dialog.
/// </summary>
[Transaction(TransactionMode.Manual)]
public class RebarMindCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIApplication uiApp = commandData.Application;
        UIDocument uiDoc = uiApp.ActiveUIDocument;
        Document doc = uiDoc.Document;

        // ===== §7.1 PRE-CONDITION CHECKS =====

        // CHECK 1: Active view harus bukan null
        if (doc is null)
        {
            TaskDialog.Show("RebarMind", "No active document. Please open a Revit project first.");
            return Result.Cancelled;
        }

        // CHECK 2: User harus select elemen dulu
        var selectedIds = uiDoc.Selection.GetElementIds();
        if (!selectedIds.Any())
        {
            TaskDialog.Show("RebarMind", "Please select a structural host (column) before running RebarMind.");
            return Result.Cancelled;
        }

        // CHECK 3: Validasi category elemen yang dipilih
        Element firstElement = doc.GetElement(selectedIds.First());
        var category = firstElement.Category;
        if (category is null || !IsValidHostCategory(category))
        {
            TaskDialog.Show("RebarMind",
                "Selected element is not a supported structural host.\n\n" +
                "Supported: Structural Columns, Structural Framing, Structural Foundation.");
            return Result.Cancelled;
        }

        // ===== BUKA MODELESS DIALOG =====
        var viewModel = RebarMindViewModel.Instance;
        viewModel.HostInfo = $"{firstElement.Name} | {category.Name}";

        var window = new RebarMindWindow(viewModel, uiApp);
        window.Show(); // Modeless — tidak block Revit

        return Result.Succeeded;
    }

    private static bool IsValidHostCategory(Category category)
    {
        var builtIn = (BuiltInCategory)category.Id.Value;
        return builtIn == BuiltInCategory.OST_StructuralColumns
            || builtIn == BuiltInCategory.OST_StructuralFraming
            || builtIn == BuiltInCategory.OST_StructuralFoundation;
    }
}
