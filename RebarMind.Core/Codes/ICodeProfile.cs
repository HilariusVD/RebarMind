namespace RebarMind.Core.Codes;

/// <summary>
/// Abstraksi standar kode struktural (§8.1).
/// Implementasi baru dapat ditambahkan tanpa mengubah engine.
/// </summary>
public interface ICodeProfile
{
    string CodeName { get; }   // "SNI 2847:2019"
    string Edition { get; }    // "2019"

    // LAP SPLICE
    double CalculateDevelopmentLength(
        double fy, double fc, double db, bool isTension);

    // COVER
    double GetMinimumCover(ExposureClass exposure, RebarRole role);

    // REBAR RATIO
    double GetMinRebarRatio(StructuralElementType elementType);
    double GetMaxRebarRatio(StructuralElementType elementType);

    // SPACING
    double GetMinClearSpacing(double db, double aggregateSize);

    // STIRRUP
    double GetMaxStirrupSpacing(double db, double sectionDimension);
}

/// <summary>
/// Klasifikasi lingkungan eksposur untuk penentuan cover minimum.
/// </summary>
public enum ExposureClass
{
    Interior,
    Exterior,
    Aggressive
}

/// <summary>
/// Peran tulangan dalam elemen struktural.
/// </summary>
public enum RebarRole
{
    MainBar,
    Stirrup
}

/// <summary>
/// Tipe elemen struktural untuk validasi rebar ratio.
/// </summary>
public enum StructuralElementType
{
    Column,
    Beam,
    Slab,
    Foundation
}
