import { inputClass, labelClass } from "../lib/ui";

export interface TemplateField {
  key: string;
  label: string;
  type: "text" | "number" | "textarea" | "date";
}

/** Parses a MedicalRecordTemplate.FieldsSchemaJson string, tolerating garbage input (never throws). */
export function parseTemplateSchema(json: string | null | undefined): TemplateField[] {
  if (!json) return [];
  try {
    const parsed = JSON.parse(json);
    if (!Array.isArray(parsed)) return [];
    return parsed.filter(
      (f): f is TemplateField => typeof f?.key === "string" && typeof f?.label === "string",
    );
  } catch {
    return [];
  }
}

interface DynamicTemplateFieldsProps {
  schema: TemplateField[];
  values: Record<string, string>;
  onChange: (values: Record<string, string>) => void;
}

/**
 * Renders one input per field in a template's schema (see MedicalRecordTemplate
 * on the backend). Used both when a doctor fills in a record against a
 * template and, read-only-ish, wherever a template needs previewing.
 */
export function DynamicTemplateFields({ schema, values, onChange }: DynamicTemplateFieldsProps) {
  if (schema.length === 0) return null;

  function setField(key: string, value: string) {
    onChange({ ...values, [key]: value });
  }

  return (
    <div className="grid grid-cols-1 gap-4 rounded-sm border border-line p-4 sm:grid-cols-2">
      {schema.map((field) => (
        <div key={field.key} className={field.type === "textarea" ? "sm:col-span-2" : undefined}>
          <label className={labelClass}>{field.label}</label>
          {field.type === "textarea" ? (
            <textarea
              className={`${inputClass} min-h-[60px]`}
              value={values[field.key] ?? ""}
              onChange={(e) => setField(field.key, e.target.value)}
            />
          ) : (
            <input
              type={field.type === "number" ? "number" : field.type === "date" ? "date" : "text"}
              className={inputClass}
              value={values[field.key] ?? ""}
              onChange={(e) => setField(field.key, e.target.value)}
            />
          )}
        </div>
      ))}
    </div>
  );
}
