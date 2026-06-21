using Autodesk.Revit.UI;
using RebarMind.RevitAddin.Services;
using RebarMind.RevitAddin.ViewModels;
using Microsoft.Win32;
using RebarMind.Core.Config;
using System.Windows;

namespace RebarMind.RevitAddin.Views;

/// <summary>
/// Code-behind untuk RebarMindWindow.
/// Minimal logic — semua state di ViewModel.
/// </summary>
public partial class RebarMindWindow : Window
{
    private readonly RebarMindViewModel _viewModel;
    private readonly ExternalEvent _externalEvent;
    private readonly GenerateRebarEvent _eventHandler;
    private readonly ConfigLoader _configLoader = new();

    public RebarMindWindow(RebarMindViewModel viewModel, UIApplication uiApp)
    {
        InitializeComponent();  // ← Ini akan di-generate otomatis dari XAML
        _viewModel = viewModel;
        DataContext = _viewModel;

        // Setup ExternalEvent bridge (§9)
        _eventHandler = new GenerateRebarEvent();
        _externalEvent = ExternalEvent.Create(_eventHandler);

        // Set window position relative to Revit
        WindowStartupLocation = WindowStartupLocation.Manual;
        Left = 100;
        Top = 100;
    }

    private void OnGenerateClicked(object sender, RoutedEventArgs e)
    {
        _viewModel.StatusMessage = "Generating...";
        _externalEvent.Raise();
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnLoadPresetClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Load RebarMind Preset",
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            InitialDirectory = System.IO.Path.Combine(
                System.AppDomain.CurrentDomain.BaseDirectory, "presets")
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var config = _configLoader.LoadFromFile(dialog.FileName);
                _viewModel.LoadFromConfig(config);
                _viewModel.ActivePresetName = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
                _viewModel.StatusMessage = $"Loaded preset: {_viewModel.ActivePresetName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load preset:\n\n{ex.Message}",
                    "RebarMind Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void OnSavePresetClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Save RebarMind Preset",
            Filter = "JSON Files (*.json)|*.json",
            DefaultExt = ".json",
            FileName = $"RebarMind-{_viewModel.ActivePresetName}.json"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                var config = _viewModel.ToConfig();
                _configLoader.SaveToFile(config, dialog.FileName);
                _viewModel.StatusMessage = $"Saved preset: {dialog.FileName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save preset:\n\n{ex.Message}",
                    "RebarMind Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}