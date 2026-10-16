import { useTranslation } from "react-i18next";
import { Modal } from "./Modal";
import { dangerButtonClass, secondaryButtonClass } from "../lib/ui";

interface ConfirmDialogProps {
  title: string;
  message: string;
  confirmLabel?: string;
  isBusy?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export function ConfirmDialog({
  title,
  message,
  confirmLabel,
  isBusy = false,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  const { t } = useTranslation();
  return (
    <Modal title={title} onClose={onCancel} width="md">
      <p className="text-sm text-ink-muted">{message}</p>
      <div className="mt-6 flex justify-end gap-2">
        <button className={secondaryButtonClass} onClick={onCancel} disabled={isBusy}>
          {t("common.cancel")}
        </button>
        <button className={dangerButtonClass} onClick={onConfirm} disabled={isBusy}>
          {isBusy ? t("common.saving") : confirmLabel ?? t("common.delete")}
        </button>
      </div>
    </Modal>
  );
}
