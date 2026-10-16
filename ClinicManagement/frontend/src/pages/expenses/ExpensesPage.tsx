import { useState } from "react";
import { useTranslation } from "react-i18next";
import { AppShell } from "../../components/AppShell";
import { Pagination } from "../../components/Pagination";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { ExpenseFormModal, type ExpenseDto } from "./ExpenseFormModal";
import { useApiData } from "../../lib/useApiData";
import { useAuth } from "../../lib/auth";
import { api, ApiError } from "../../lib/api";
import type { PaginatedList } from "../../lib/types";
import { translatePaymentMethod, formatCurrency, formatDate } from "../../lib/format";
import { inputClass, primaryButtonClass } from "../../lib/ui";

interface Branch {
  id: string;
  name: string;
}

const PAGE_SIZE = 20;

export function ExpensesPage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const roles = user?.roles ?? [];

  const [pageNumber, setPageNumber] = useState(1);
  const [categoryFilter, setCategoryFilter] = useState("");
  const [formState, setFormState] = useState<{ mode: "create" } | { mode: "edit"; expense: ExpenseDto } | null>(
    null,
  );
  const [deleteTarget, setDeleteTarget] = useState<ExpenseDto | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const { data: branches } = useApiData<Branch[]>("/api/branches");

  const query = new URLSearchParams({
    pageNumber: String(pageNumber),
    pageSize: String(PAGE_SIZE),
    ...(categoryFilter ? { category: categoryFilter } : {}),
  });
  const { data, isLoading, error, refetch } = useApiData<PaginatedList<ExpenseDto>>(
    `/api/expenses?${query.toString()}`,
  );

  async function handleDelete() {
    if (!deleteTarget) return;
    setIsDeleting(true);
    setDeleteError(null);
    try {
      await api.delete(`/api/expenses/${deleteTarget.id}`);
      setDeleteTarget(null);
      refetch();
    } catch (err) {
      setDeleteError(err instanceof ApiError ? err.message : t("expenses.failedToDelete"));
    } finally {
      setIsDeleting(false);
    }
  }

  return (
    <AppShell title={t("nav.expenses")} roleLabel={roles[0] ?? ""}>
      <div className="space-y-4">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <input
            className={`${inputClass} max-w-xs`}
            placeholder={t("expenses.filterByCategory")}
            value={categoryFilter}
            onChange={(e) => {
              setCategoryFilter(e.target.value);
              setPageNumber(1);
            }}
          />
          <button className={primaryButtonClass} onClick={() => setFormState({ mode: "create" })}>
            + {t("expenses.addExpense")}
          </button>
        </div>

        <div className="overflow-hidden rounded-lg border border-line bg-surface shadow-card">
          {isLoading && <p className="px-5 py-8 text-center font-mono text-sm text-ink-muted">{t("expenses.loadingExpenses")}</p>}
          {error && <p className="px-5 py-4 text-sm text-danger">{error}</p>}

          {data && data.items.length === 0 && !isLoading && (
            <p className="px-5 py-10 text-center text-sm text-ink-faint">{t("expenses.noMatch")}</p>
          )}

          {data && data.items.length > 0 && (
            <table className="w-full text-start text-sm">
              <thead>
                <tr className="border-b border-line text-xs font-medium uppercase tracking-[0.06em] text-ink-faint">
                  <th className="px-5 py-3">{t("common.date")}</th>
                  <th className="px-5 py-3">{t("expenses.category")}</th>
                  <th className="px-5 py-3">{t("common.description")}</th>
                  <th className="px-5 py-3">{t("invoices.method")}</th>
                  <th className="px-5 py-3">{t("common.amount")}</th>
                  <th className="px-5 py-3 text-end">{t("common.actions")}</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((exp) => (
                  <tr key={exp.id} className="border-b border-line last:border-0 hover:bg-canvas/60">
                    <td className="px-5 py-3 text-ink-muted">{formatDate(exp.expenseDate)}</td>
                    <td className="px-5 py-3 font-medium text-ink">{exp.category}</td>
                    <td className="px-5 py-3 text-ink-faint">{exp.description || "—"}</td>
                    <td className="px-5 py-3 text-ink-muted">{translatePaymentMethod(exp.method, t)}</td>
                    <td className="px-5 py-3 tabular-nums text-ink-muted">{formatCurrency(exp.amount)}</td>
                    <td className="px-5 py-3 text-end">
                      <button
                        className="text-xs font-medium text-harbor-500 hover:underline"
                        onClick={() => setFormState({ mode: "edit", expense: exp })}
                      >
                        {t("common.edit")}
                      </button>
                      <button
                        className="ms-3 text-xs font-medium text-danger hover:underline"
                        onClick={() => setDeleteTarget(exp)}
                      >
                        {t("common.delete")}
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          {data && (
            <Pagination
              pageNumber={data.pageNumber}
              totalPages={data.totalPages}
              totalCount={data.totalCount}
              hasPreviousPage={data.hasPreviousPage}
              hasNextPage={data.hasNextPage}
              onPageChange={setPageNumber}
            />
          )}
        </div>
      </div>

      {formState && (
        <ExpenseFormModal
          expense={formState.mode === "edit" ? formState.expense : undefined}
          branches={branches ?? []}
          onClose={() => setFormState(null)}
          onSaved={() => {
            setFormState(null);
            refetch();
          }}
        />
      )}

      {deleteTarget && (
        <ConfirmDialog
          title={t("expenses.deleteExpense")}
          message={t("expenses.deleteConfirmMessage", { category: deleteTarget.category, amount: formatCurrency(deleteTarget.amount) })}
          isBusy={isDeleting}
          onConfirm={handleDelete}
          onCancel={() => {
            setDeleteTarget(null);
            setDeleteError(null);
          }}
        />
      )}
      {deleteError && <p className="mt-2 text-sm text-danger">{deleteError}</p>}
    </AppShell>
  );
}
