using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RebarMind.Core.Config;

/// <summary>
/// Loads, deserializes, and validates rebarMindConfig.json.
/// Thread-safe. Stateless.
/// </summary>
public sealed class ConfigLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    private readonly ConfigValidator _validator = new();

    /// <summary>
    /// Load config from file path. Throws RebarMindValidationException on failure.
    /// </summary>
    public RebarMindConfig LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Config file not found: {filePath}");

        string json = File.ReadAllText(filePath);
        return LoadFromJson(json);
    }

    /// <summary>
    /// Load config from raw JSON string.
    /// </summary>
    public RebarMindConfig LoadFromJson(string json)
    {
        RebarMindConfig? config;
        try
        {
            config = JsonSerializer.Deserialize<RebarMindConfig>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new RebarMindValidationException(
                $"Invalid JSON structure: {ex.Message}",
                ErrorSeverity.Blocker,
                ex);
        }

        if (config is null)
            throw new RebarMindValidationException(
                "Config deserialized to null.",
                ErrorSeverity.Blocker);

        // Run validation rules (§7.2)
        var validationResult = _validator.Validate(config);
        if (!validationResult.IsValid)
        {
            string errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new RebarMindValidationException(
                $"Config validation failed: {errors}",
                ErrorSeverity.Blocker);
        }

        return config;
    }

    /// <summary>
    /// Serialize config back to JSON (for Save Preset feature).
    /// </summary>
    public string Serialize(RebarMindConfig config)
        => JsonSerializer.Serialize(config, JsonOptions);

    /// <summary>
    /// Save config to file atomically (write to temp, then move).
    /// </summary>
    public void SaveToFile(RebarMindConfig config, string filePath)
    {
        string json = Serialize(config);
        string tempPath = filePath + ".tmp";
        File.WriteAllText(tempPath, json);
        File.Move(tempPath, filePath, overwrite: true);
    }
}