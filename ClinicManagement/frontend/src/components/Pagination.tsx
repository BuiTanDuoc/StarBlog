interface PaginationProps {
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  onPageChange: (page: number) => void;
}

import { useTranslation } from "react-i18next";

export function Pagination({
  pageNumber,
  totalPages,
  totalCount,
  hasPreviousPage,
  hasNextPage,
  onPageChange,
}: PaginationProps) {
  const { t } = useTranslation();
  if (totalCount === 0) return null;

  return (
    <div className="flex items-center justify-between border-t border-line px-5 py-3.5">
      <p className="text-xs text-ink-faint">
        {t("pagination.page")} <span className="font-medium text-ink-muted">{pageNumber}</span> {t("pagination.of")}{" "}
        <span className="font-medium text-ink-muted">{Math.max(totalPages, 1)}</span> · {t("pagination.total", { count: totalCount })}
      </p>
      <div className="flex gap-2">
        <button
          className="rounded-sm border border-line px-3 py-1.5 text-xs font-medium text-ink transition-colors hover:bg-canvas disabled:cursor-not-allowed disabled:opacity-40"
          onClick={() => onPageChange(pageNumber - 1)}
          disabled={!hasPreviousPage}
        >
          {t("common.previous")}
        </button>
        <button
          className="rounded-sm border border-line px-3 py-1.5 text-xs font-medium text-ink transition-colors hover:bg-canvas disabled:cursor-not-allowed disabled:opacity-40"
          onClick={() => onPageChange(pageNumber + 1)}
          disabled={!hasNextPage}
        >
          {t("common.next")}
        </button>
      </div>
    </div>
  );
}
