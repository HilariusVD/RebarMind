using FluentValidation;

namespace RebarMind.Core.Config;

/// <summary>
/// FluentValidation rules for RebarMindConfig.
/// Implements §7.2 Geometry Validation at config level.
/// </summary>
public sealed class ConfigValidator : AbstractValidator<RebarMindConfig>
{
    public ConfigValidator()
    {
        RuleFor(x => x.SchemaVersion)
            .NotEmpty().WithMessage("schemaVersion is required.");

        RuleFor(x => x.ActiveCodeProfile)
            .NotEmpty().WithMessage("activeCodeProfile is required.")
            .Must((cfg, profile) => cfg.CodeProfiles.ContainsKey(profile))
            .WithMessage($"Active code profile '{{PropertyValue}}' not found in codeProfiles map.");

        RuleFor(x => x.Material.FcPrime)
            .InclusiveBetween(15, 100)
            .WithMessage("fc_prime_MPa must be between 15 and 100 MPa.");

        RuleFor(x => x.Material.Fy)
            .InclusiveBetween(240, 600)
            .WithMessage("fy_MPa must be between 240 and 600 MPa.");

        RuleFor(x => x.Cover.Top)
            .GreaterThan(0).WithMessage("Cover top must be > 0.")
            .LessThanOrEqualTo(200).WithMessage("Cover top unreasonably large (>200mm).");

        RuleFor(x => x.Cover.Bottom)
            .GreaterThan(0).WithMessage("Cover bottom must be > 0.");

        RuleFor(x => x.Cover.Side)
            .GreaterThan(0).WithMessage("Cover side must be > 0.");

        RuleFor(x => x.MainBars.Diameter)
            .InclusiveBetween(6, 50)
            .WithMessage("Main bar diameter must be between 6 and 50 mm.");

        RuleFor(x => x.MainBars.Nx)
            .InclusiveBetween(2, 50)
            .WithMessage("Nx must be between 2 and 50.");

        RuleFor(x => x.MainBars.Ny)
            .InclusiveBetween(2, 50)
            .WithMessage("Ny must be between 2 and 50.");

        RuleFor(x => x.Stirrups.Diameter)
            .InclusiveBetween(6, 25)
            .WithMessage("Stirrup diameter must be between 6 and 25 mm.");

        RuleFor(x => x.Stirrups.SpacingAtSupport)
            .GreaterThan(0).WithMessage("Spacing at support must be > 0.")
            .LessThanOrEqualTo(500).WithMessage("Spacing at support unreasonably large.");

        RuleFor(x => x.Stirrups.SpacingAtMid)
            .GreaterThan(0).WithMessage("Spacing at mid must be > 0.");

        RuleFor(x => x.Splicing.MaxStockLength)
            .GreaterThan(0).WithMessage("Max stock length must be > 0.");
    }
}
