namespace RebarMind.Core.Codes;

/// <summary>
/// Implementasi SNI 2847:2019 (§8.2).
/// </summary>
public class SNI2847Profile : ICodeProfile
{
    public string CodeName => "SNI 2847:2019";
    public string Edition => "2019";

    public double CalculateDevelopmentLength(
        double fy, double fc, double db, bool isTension)
    {
        // SNI 2847:2019 Pasal 25.5.2
        // ld = (fy / (4 * sqrt(f'c))) * db
        if (!isTension)
            return (0.24 * fy / Math.Sqrt(fc)) * db; // Tekan

        double ld = (fy / (4.0 * Math.Sqrt(fc))) * db;
        return Math.Max(ld, 300.0); // Minimum 300mm per SNI
    }

    public double GetMinimumCover(ExposureClass exposure, RebarRole role)
        => exposure switch
        {
            ExposureClass.Interior => 40.0,
            ExposureClass.Exterior => 50.0,
            ExposureClass.Aggressive => 75.0,
            _ => 40.0
        };

    public double GetMinRebarRatio(StructuralElementType t) =>
        t == StructuralElementType.Column ? 0.01 : 0.0018;

    public double GetMaxRebarRatio(StructuralElementType t) =>
        t == StructuralElementType.Column ? 0.08 : 0.04;

    public double GetMinClearSpacing(double db, double agg) =>
        Math.Max(Math.Max(25.0, db), 1.33 * agg);

    public double GetMaxStirrupSpacing(double db, double dim) =>
        Math.Min(Math.Min(16 * db, 48 * 10), dim);
}
