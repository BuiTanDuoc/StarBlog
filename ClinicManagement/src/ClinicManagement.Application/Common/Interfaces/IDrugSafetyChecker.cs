namespace ClinicManagement.Application.Common.Interfaces;

public record DrugSafetyWarning(string Severity, string Message);

/// <summary>
/// Checks a prescription's medicine list against the patient's recorded
/// allergies and against a small known drug-interaction reference list.
/// This is a starter safety net, not a clinical decision-support system —
/// see DrugInteractionReference in Infrastructure for the important
/// caveats about coverage. It returns warnings for the prescribing doctor
/// to see, it never blocks prescription creation: a licensed doctor's
/// clinical judgement about their own patient takes precedence over a
/// substring match against a short reference list.
/// </summary>
public interface IDrugSafetyChecker
{
    IReadOnlyList<DrugSafetyWarning> Check(string? patientAllergies, IEnumerable<string> medicineNames);
}
