using ClinicManagement.Application.Common.Interfaces;

namespace ClinicManagement.Application.Common.Services;

/// <summary>
/// IMPORTANT — READ BEFORE RELYING ON THIS IN PRODUCTION:
/// The interaction pairs and allergy-family groupings below are a small,
/// illustrative starter list (a handful of well-known examples), not a
/// clinically validated or exhaustive drug-interaction database. Before
/// this is used to inform real prescribing decisions, a licensed
/// pharmacist/clinician should review and expand this list, and a clinic
/// should strongly consider integrating a real drug-interaction API
/// (e.g. a licensed source like First Databank, Multum, or a national
/// formulary service) instead of — or in addition to — string matching.
/// This class exists to demonstrate the pattern (check on create, warn,
/// never silently block), not to replace pharmacological review.
/// </summary>
public class DrugSafetyChecker : IDrugSafetyChecker
{
    // "Family name" -> substrings that count as a match against that family, used both for
    // allergy matching (patient.Allergies mentions "penicillin") and cross-item interaction
    // pairs. Matching is deliberately loose (case-insensitive substring) because free-text
    // allergy fields and medicine names are never perfectly standardized in a real chart.
    private static readonly Dictionary<string, string[]> DrugFamilies = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Penicillins"] = new[] { "penicillin", "amoxicillin", "ampicillin", "augmentin" },
        ["Cephalosporins"] = new[] { "cephalexin", "ceftriaxone", "cefuroxime" },
        ["NSAIDs"] = new[] { "ibuprofen", "diclofenac", "naproxen", "aspirin" },
        ["Sulfonamides"] = new[] { "sulfamethoxazole", "bactrim", "sulfasalazine" },
        ["Warfarin"] = new[] { "warfarin", "coumadin" },
        ["ACE Inhibitors"] = new[] { "lisinopril", "enalapril", "ramipril" },
        ["Statins"] = new[] { "atorvastatin", "simvastatin", "rosuvastatin" },
        ["Macrolides"] = new[] { "erythromycin", "clarithromycin", "azithromycin" },
        ["MAOIs"] = new[] { "phenelzine", "tranylcypromine" },
        ["SSRIs"] = new[] { "fluoxetine", "sertraline", "paroxetine", "citalopram" },
    };

    // Pairs of families known to warrant a caution — again: illustrative, not exhaustive.
    private static readonly (string A, string B, string Message)[] InteractionPairs =
    {
        ("Warfarin", "NSAIDs", "Combining an anticoagulant with an NSAID increases bleeding risk."),
        ("Warfarin", "Macrolides", "Some macrolides can raise warfarin levels — INR monitoring advised."),
        ("ACE Inhibitors", "NSAIDs", "NSAIDs can reduce the effectiveness of ACE inhibitors and affect renal function."),
        ("MAOIs", "SSRIs", "Combining an MAOI with an SSRI risks serotonin syndrome."),
        ("Statins", "Macrolides", "Some macrolides increase statin levels, raising myopathy risk."),
    };

    public IReadOnlyList<DrugSafetyWarning> Check(string? patientAllergies, IEnumerable<string> medicineNames)
    {
        var warnings = new List<DrugSafetyWarning>();
        var names = medicineNames.Where(n => !string.IsNullOrWhiteSpace(n)).ToList();

        var familiesInPrescription = names
            .Select(n => (Name: n, Family: FindFamily(n)))
            .Where(x => x.Family is not null)
            .ToList();

        // 1) Allergy check: does any prescribed medicine belong to a family mentioned in
        // the patient's free-text allergy notes?
        if (!string.IsNullOrWhiteSpace(patientAllergies))
        {
            foreach (var (name, family) in familiesInPrescription)
            {
                if (DrugFamilies[family!].Any(term => patientAllergies.Contains(term, StringComparison.OrdinalIgnoreCase))
                    || patientAllergies.Contains(family!, StringComparison.OrdinalIgnoreCase))
                {
                    warnings.Add(new DrugSafetyWarning("Allergy",
                        $"Patient's recorded allergies mention {family} — prescribed medicine \"{name}\" may be related. Please review."));
                }
            }
        }

        // 2) Interaction check: any two prescribed medicines whose families are a known pair.
        var distinctFamilies = familiesInPrescription.Select(x => x.Family!).Distinct().ToList();
        foreach (var (a, b, message) in InteractionPairs)
        {
            if (distinctFamilies.Contains(a, StringComparer.OrdinalIgnoreCase) &&
                distinctFamilies.Contains(b, StringComparer.OrdinalIgnoreCase))
            {
                warnings.Add(new DrugSafetyWarning("Interaction", message));
            }
        }

        return warnings;
    }

    private static string? FindFamily(string medicineName) =>
        DrugFamilies.FirstOrDefault(kv => kv.Value.Any(term => medicineName.Contains(term, StringComparison.OrdinalIgnoreCase))).Key;
}
