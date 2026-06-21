using Autodesk.Revit.UI;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace RebarMind.RevitAddin;

/// <summary>
/// Entry point RebarMind add-in. Dipanggil saat Revit start/shutdown.
/// Membuat Ribbon tab + panel + button.
/// </summary>
public class RebarMindApp : IExternalApplication
{
    public Result OnStartup(UIControlledApplication application)
    {
        string tabName = "RebarMind";
        string panelName = "Detailing";

        // Buat Ribbon Tab (catch kalau sudah ada)
        try
        {
            application.CreateRibbonTab(tabName);
        }
        catch { /* Tab sudah ada */ }

        // Buat Ribbon Panel
        RibbonPanel panel = application.CreateRibbonPanel(tabName, panelName);

        // Path ke assembly ini
        string assemblyPath = Assembly.GetExecutingAssembly().Location;

        // Button: Column Detailer
        PushButtonData btnColumnDetailer = new(
            "ColumnDetailer",
            "Column\nDetailer",
            assemblyPath,
            "RebarMind.RevitAddin.Commands.RebarMindCommand")
        {
            ToolTip = "Generate rebar detailing untuk kolom struktural",
            LongDescription = "Buka dialog RebarMind untuk konfigurasi dan generate tulangan kolom secara prosedural."
        };

        panel.AddItem(btnColumnDetailer);

        return Result.Succeeded;
    }

    public Result OnShutdown(UIControlledApplication application)
    {
        return Result.Succeeded;
    }
}
