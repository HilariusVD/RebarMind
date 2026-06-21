using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RebarMind.Core.Config;
using System.Text.Json;

namespace RebarMind.RevitAddin.ViewModels;

/// <summary>
/// ViewModel untuk RebarMind Dialog.
/// MVVM pattern — semua property observable untuk WPF binding.
/// Singleton untuk akses dari ExternalEvent handler.
/// </summary>
public partial class RebarMindViewModel : ObservableObject
{
    private static RebarMindViewModel? _instance;
    public static RebarMindViewModel Instance => _instance ??= new RebarMindViewModel();

    private readonly ConfigLoader _configLoader = new();

    // ===== HOST INFO (read-only) =====
    [ObservableProperty]
    private string _hostInfo = "No host selected";

    // ===== COVER SETTINGS =====
    [ObservableProperty]
    private double _topCover = 40.0;

    [ObservableProperty]
    private double _bottomCover = 40.0;

    [ObservableProperty]
    private double _sideCover = 40.0;

    [ObservableProperty]
    private bool _linkAllCovers = true;

    // ===== MAIN BARS =====
    [ObservableProperty]
    private double _mainBarDiameter = 25.0;

    [ObservableProperty]
    private string _mainBarShapeCode = "M_00";

    [ObservableProperty]
    private string _layoutRule = "FixedNumber";

    [ObservableProperty]
    private int _nx = 4;

    [ObservableProperty]
    private int _ny = 4;

    [ObservableProperty]
    private int _totalBars = 12;

    // ===== STIRRUPS =====
    [ObservableProperty]
    private double _stirrupDiameter = 10.0;

    [ObservableProperty]
    private string _stirrupShapeCode = "M_T1";

    [ObservableProperty]
    private string _supportZoneExpression = "L/4";

    [ObservableProperty]
    private double _spacingAtSupport = 100.0;

    [ObservableProperty]
    private double _spacingAtMid = 150.0;

    [ObservableProperty]
    private bool _enableCrossTies = false;

    // ===== SPLICING =====
    [ObservableProperty]
    private bool _enableAutoSplit = true;

    [ObservableProperty]
    private double _maxStockLength = 12000.0;

    [ObservableProperty]
    private string _spliceType = "LapSplice";

    [ObservableProperty]
    private string _lapMultiplier = "40*d";

    [ObservableProperty]
    private bool _enableStaggering = true;

    // ===== LIVE PREVIEW =====
    [ObservableProperty]
    private bool _isLivePreviewEnabled = false;

    // ===== STATUS BAR =====
    [ObservableProperty]
    private string _statusMessage = "Ready";

    // ===== PRESET LABEL =====
    [ObservableProperty]
    private string _activePresetName = "Custom";

    // ===== COMMANDS =====

    partial void OnTopCoverChanged(double value)
    {
        if (LinkAllCovers)
        {
            BottomCover = value;
            SideCover = value;
        }
        RecalculateTotalBars();
    }

    partial void OnNxChanged(int value) => RecalculateTotalBars();
    partial void OnNyChanged(int value) => RecalculateTotalBars();

    private void RecalculateTotalBars()
    {
        if (Nx >= 2 && Ny >= 2)
            TotalBars = 2 * Nx + 2 * (Ny - 2);
    }

    [RelayCommand]
    private void ToggleLinkCovers()
    {
        LinkAllCovers = !LinkAllCovers;
        if (LinkAllCovers)
        {
            BottomCover = TopCover;
            SideCover = TopCover;
        }
    }

    [RelayCommand]
    private void ResetDefaults()
    {
        TopCover = 40.0;
        BottomCover = 40.0;
        SideCover = 40.0;
        LinkAllCovers = true;
        MainBarDiameter = 25.0;
        MainBarShapeCode = "M_00";
        LayoutRule = "FixedNumber";
        Nx = 4;
        Ny = 4;
        StirrupDiameter = 10.0;
        StirrupShapeCode = "M_T1";
        SupportZoneExpression = "L/4";
        SpacingAtSupport = 100.0;
        SpacingAtMid = 150.0;
        EnableCrossTies = false;
        EnableAutoSplit = true;
        MaxStockLength = 12000.0;
        SpliceType = "LapSplice";
        LapMultiplier = "40*d";
        EnableStaggering = true;
        ActivePresetName = "Custom";
        StatusMessage = "Reset to defaults";
    }

    // ===== PUBLIC METHODS =====

    public RebarMindConfig ToConfig()
    {
        return new RebarMindConfig
        {
            Cover = new CoverConfig
            {
                Top = TopCover,
                Bottom = BottomCover,
                Side = SideCover,
                LinkAll = LinkAllCovers
            },
            MainBars = new MainBarsConfig
            {
                Diameter = MainBarDiameter,
                ShapeCode = MainBarShapeCode,
                LayoutRule = LayoutRule,
                Nx = Nx,
                Ny = Ny
            },
            Stirrups = new StirrupsConfig
            {
                Diameter = StirrupDiameter,
                ShapeCode = StirrupShapeCode,
                SupportZoneExpression = SupportZoneExpression,
                SpacingAtSupport = SpacingAtSupport,
                SpacingAtMid = SpacingAtMid,
                EnableCrossTies = EnableCrossTies
            },
            Splicing = new SplicingConfig
            {
                EnableAutoSplit = EnableAutoSplit,
                MaxStockLength = MaxStockLength,
                SpliceType = SpliceType,
                LapMultiplier = LapMultiplier,
                EnableStaggering = EnableStaggering
            }
        };
    }

    public void LoadFromConfig(RebarMindConfig config)
    {
        TopCover = config.Cover.Top;
        BottomCover = config.Cover.Bottom;
        SideCover = config.Cover.Side;
        LinkAllCovers = config.Cover.LinkAll;
        MainBarDiameter = config.MainBars.Diameter;
        MainBarShapeCode = config.MainBars.ShapeCode;
        LayoutRule = config.MainBars.LayoutRule;
        Nx = config.MainBars.Nx;
        Ny = config.MainBars.Ny;
        StirrupDiameter = config.Stirrups.Diameter;
        StirrupShapeCode = config.Stirrups.ShapeCode;
        SupportZoneExpression = config.Stirrups.SupportZoneExpression;
        SpacingAtSupport = config.Stirrups.SpacingAtSupport;
        SpacingAtMid = config.Stirrups.SpacingAtMid;
        EnableCrossTies = config.Stirrups.EnableCrossTies;
        EnableAutoSplit = config.Splicing.EnableAutoSplit;
        MaxStockLength = config.Splicing.MaxStockLength;
        SpliceType = config.Splicing.SpliceType;
        LapMultiplier = config.Splicing.LapMultiplier;
        EnableStaggering = config.Splicing.EnableStaggering;
    }
}
