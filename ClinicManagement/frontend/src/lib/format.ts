import type { TFunction } from "i18next";

/**
 * These Record maps are kept for the set of valid backend enum keys (used for
 * iterating options, defaults, etc). For user-facing labels, use the
 * translateXxx(value, t) helpers below instead of indexing these maps directly.
 */
export const GENDER_LABELS: Record<string, string> = {
  Male: "Male",
  Female: "Female",
  Other: "Other",
};

export const BLOOD_GROUP_LABELS: Record<string, string> = {
  Unknown: "Unknown",
  APositive: "A+",
  ANegative: "A−",
  BPositive: "B+",
  BNegative: "B−",
  ABPositive: "AB+",
  ABNegative: "AB−",
  OPositive: "O+",
  ONegative: "O−",
};

export const DOCTOR_STATUS_LABELS: Record<string, string> = {
  Active: "Active",
  OnLeave: "On leave",
  Suspended: "Suspended",
  Inactive: "Inactive",
};

export const APPOINTMENT_STATUS_LABELS: Record<string, string> = {
  Pending: "Pending",
  Confirmed: "Confirmed",
  CheckedIn: "Checked in",
  InProgress: "In progress",
  Completed: "Completed",
  Cancelled: "Cancelled",
  NoShow: "No show",
};

export const APPOINTMENT_STATUS_STYLES: Record<string, string> = {
  Pending: "bg-warning-light text-warning",
  Confirmed: "bg-harbor-50 text-harbor-500",
  CheckedIn: "bg-gold-light text-gold-dark",
  InProgress: "bg-gold-light text-gold-dark",
  Completed: "bg-success-light text-success",
  Cancelled: "bg-danger-light text-danger",
  NoShow: "bg-danger-light text-danger",
};

export const DOCTOR_STATUS_STYLES: Record<string, string> = {
  Active: "bg-success-light text-success",
  OnLeave: "bg-warning-light text-warning",
  Suspended: "bg-danger-light text-danger",
  Inactive: "bg-line text-ink-muted",
};

export const EMPLOYEE_STATUS_LABELS: Record<string, string> = {
  Active: "Active",
  OnLeave: "On leave",
  Terminated: "Terminated",
};

export const EMPLOYEE_STATUS_STYLES: Record<string, string> = {
  Active: "bg-success-light text-success",
  OnLeave: "bg-warning-light text-warning",
  Terminated: "bg-danger-light text-danger",
};

export const INVOICE_STATUS_LABELS: Record<string, string> = {
  Draft: "Draft",
  Issued: "Issued",
  PartiallyPaid: "Partially paid",
  Paid: "Paid",
  Overdue: "Overdue",
  Void: "Void",
};

export const INVOICE_STATUS_STYLES: Record<string, string> = {
  Draft: "bg-line text-ink-muted",
  Issued: "bg-harbor-50 text-harbor-500",
  PartiallyPaid: "bg-warning-light text-warning",
  Paid: "bg-success-light text-success",
  Overdue: "bg-danger-light text-danger",
  Void: "bg-line text-ink-faint",
};

export const PAYMENT_METHOD_LABELS: Record<string, string> = {
  Cash: "Cash",
  Card: "Card",
  Wallet: "Wallet",
  BankTransfer: "Bank transfer",
};

export const NOTIFICATION_TYPE_LABELS: Record<string, string> = {
  NewAppointment: "New appointment",
  AppointmentCancelled: "Appointment cancelled",
  AppointmentRescheduled: "Appointment rescheduled",
  PaymentReceived: "Payment received",
  SubscriptionExpiring: "Subscription expiring",
  SubscriptionExpired: "Subscription expired",
  General: "Notice",
};

export const AUDIT_ACTION_LABELS: Record<string, string> = {
  Create: "Create",
  Update: "Update",
  Delete: "Delete",
  Login: "Login",
  Logout: "Logout",
};

export const AUDIT_ACTION_STYLES: Record<string, string> = {
  Create: "bg-success-light text-success",
  Update: "bg-harbor-50 text-harbor-500",
  Delete: "bg-danger-light text-danger",
  Login: "bg-line text-ink-muted",
  Logout: "bg-line text-ink-muted",
};

export const PLAN_TIER_LABELS: Record<string, string> = {
  Free: "Free",
  Starter: "Starter",
  Professional: "Professional",
  Enterprise: "Enterprise",
};

export const SUBSCRIPTION_STATUS_LABELS: Record<string, string> = {
  Trialing: "Trialing",
  Active: "Active",
  PastDue: "Past due",
  Expired: "Expired",
  Cancelled: "Cancelled",
  PendingPayment: "Awaiting payment",
};

export const SUBSCRIPTION_STATUS_STYLES: Record<string, string> = {
  Trialing: "bg-harbor-50 text-harbor-500",
  Active: "bg-success-light text-success",
  PastDue: "bg-warning-light text-warning",
  Expired: "bg-danger-light text-danger",
  Cancelled: "bg-line text-ink-muted",
  PendingPayment: "bg-warning-light text-warning",
};

export const WEEKDAYS = [
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
  "Sunday",
];

/** Translation helpers — pass the `t` function from useTranslation(). Falls back to the English label if a key is missing. */
export function translateGender(value: string, t: TFunction): string {
  return t(`enums.gender.${value}`, GENDER_LABELS[value] ?? value);
}
export function translateBloodGroup(value: string, t: TFunction): string {
  return t(`enums.bloodGroup.${value}`, BLOOD_GROUP_LABELS[value] ?? value);
}
export function translateDoctorStatus(value: string, t: TFunction): string {
  return t(`enums.doctorStatus.${value}`, DOCTOR_STATUS_LABELS[value] ?? value);
}
export function translateAppointmentStatus(value: string, t: TFunction): string {
  return t(`enums.appointmentStatus.${value}`, APPOINTMENT_STATUS_LABELS[value] ?? value);
}
export function translateEmployeeStatus(value: string, t: TFunction): string {
  return t(`enums.employeeStatus.${value}`, EMPLOYEE_STATUS_LABELS[value] ?? value);
}
export function translateInvoiceStatus(value: string, t: TFunction): string {
  return t(`enums.invoiceStatus.${value}`, INVOICE_STATUS_LABELS[value] ?? value);
}
export function translatePaymentMethod(value: string, t: TFunction): string {
  return t(`enums.paymentMethod.${value}`, PAYMENT_METHOD_LABELS[value] ?? value);
}
export function translateNotificationType(value: string, t: TFunction): string {
  return t(`enums.notificationType.${value}`, NOTIFICATION_TYPE_LABELS[value] ?? value);
}
export function translateAuditAction(value: string, t: TFunction): string {
  return t(`enums.auditAction.${value}`, AUDIT_ACTION_LABELS[value] ?? value);
}
export function translatePlanTier(value: string, t: TFunction): string {
  return t(`enums.planTier.${value}`, PLAN_TIER_LABELS[value] ?? value);
}
export function translateSubscriptionStatus(value: string, t: TFunction): string {
  return t(`enums.subscriptionStatus.${value}`, SUBSCRIPTION_STATUS_LABELS[value] ?? value);
}
export function translateWeekday(value: string, t: TFunction): string {
  return t(`enums.weekday.${value}`, value);
}

const currencyFormatter = new Intl.NumberFormat(undefined, { style: "currency", currency: "USD" });
export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export function formatCurrency(value: number): string {
  return currencyFormatter.format(value);
}

function dateLocale(): string {
  return document.documentElement.lang === "ar" ? "ar-EG" : "en-US";
}

export function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString(dateLocale(), { year: "numeric", month: "short", day: "numeric" });
}

export function formatDateTime(iso: string): string {
  return new Date(iso).toLocaleString(dateLocale(), {
    month: "short",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  });
}

export function formatTime(iso: string): string {
  return new Date(iso).toLocaleTimeString(dateLocale(), { hour: "numeric", minute: "2-digit" });
}

export function calculateAge(dateOfBirthIso: string): number {
  const dob = new Date(dateOfBirthIso);
  const now = new Date();
  let age = now.getFullYear() - dob.getFullYear();
  const hasHadBirthdayThisYear =
    now.getMonth() > dob.getMonth() || (now.getMonth() === dob.getMonth() && now.getDate() >= dob.getDate());
  if (!hasHadBirthdayThisYear) age -= 1;
  return age;
}

/** <input type="datetime-local"> works in local time with no timezone suffix — this bridges to/from ISO. */
export function toDateTimeLocalValue(iso: string): string {
  const d = new Date(iso);
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

export function fromDateTimeLocalValue(value: string): string {
  return new Date(value).toISOString();
}

/** <input type="date"> value (YYYY-MM-DD) from an ISO date-of-birth style string, no time-zone drift. */
export function toDateInputValue(iso: string): string {
  return iso.slice(0, 10);
}
