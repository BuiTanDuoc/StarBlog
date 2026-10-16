import { useRef, useState } from "react";
import { useTranslation } from "react-i18next";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { useApiData } from "../../lib/useApiData";
import { api, ApiError, downloadFile, uploadFile } from "../../lib/api";
import { formatDateTime, formatFileSize } from "../../lib/format";
import { primaryButtonClass } from "../../lib/ui";

interface FileAttachmentDto {
  id: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  uploadedBy?: string | null;
  createdAt: string;
}

interface AttachmentsPanelProps {
  entityName: string;
  entityId: string;
  canManage: boolean;
}

const ACCEPTED_TYPES =
  "application/pdf,image/jpeg,image/png,image/webp,application/msword,application/vnd.openxmlformats-officedocument.wordprocessingml.document";

export function AttachmentsPanel({ entityName, entityId, canManage }: AttachmentsPanelProps) {
  const { t } = useTranslation();
  const { data, isLoading, error, refetch } = useApiData<FileAttachmentDto[]>(
    `/api/files/by-entity?entityName=${entityName}&entityId=${entityId}`,
  );
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [downloadError, setDownloadError] = useState<string | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<FileAttachmentDto | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);

  async function handleFileSelected(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    setIsUploading(true);
    setUploadError(null);

    const formData = new FormData();
    formData.append("File", file);
    formData.append("EntityName", entityName);
    formData.append("EntityId", entityId);

    try {
      await uploadFile("/api/files", formData);
      refetch();
    } catch (err) {
      setUploadError(err instanceof ApiError ? err.message : t("medicalRecords.failedToUpload"));
    } finally {
      setIsUploading(false);
      if (fileInputRef.current) fileInputRef.current.value = "";
    }
  }

  async function handleDownload(f: FileAttachmentDto) {
    setDownloadError(null);
    try {
      await downloadFile(`/api/files/${f.id}/download`, f.fileName);
    } catch (err) {
      setDownloadError(err instanceof ApiError ? err.message : t("medicalRecords.failedToDownload"));
    }
  }

  async function handleDelete() {
    if (!deleteTarget) return;
    setIsDeleting(true);
    try {
      await api.delete(`/api/files/${deleteTarget.id}`);
      setDeleteTarget(null);
      refetch();
    } catch (err) {
      setUploadError(err instanceof ApiError ? err.message : t("medicalRecords.failedToDeleteFile"));
    } finally {
      setIsDeleting(false);
    }
  }

  return (
    <div className="space-y-3">
      {canManage && (
        <div className="flex items-center gap-3">
          <button
            className={primaryButtonClass}
            onClick={() => fileInputRef.current?.click()}
            disabled={isUploading}
          >
            {isUploading ? t("medicalRecords.uploading") : `+ ${t("medicalRecords.uploadFile")}`}
          </button>
          <input
            ref={fileInputRef}
            type="file"
            accept={ACCEPTED_TYPES}
            className="hidden"
            onChange={handleFileSelected}
          />
          <p className="text-xs text-ink-faint">{t("medicalRecords.acceptedFileTypes")}</p>
        </div>
      )}

      {uploadError && <p className="text-sm text-danger">{uploadError}</p>}
      {downloadError && <p className="text-sm text-danger">{downloadError}</p>}
      {isLoading && <p className="px-1 py-8 text-center font-mono text-sm text-ink-muted">{t("medicalRecords.loadingFiles")}</p>}
      {error && <p className="text-sm text-danger">{error}</p>}
      {data && data.length === 0 && !isLoading && (
        <p className="py-10 text-center text-sm text-ink-faint">{t("medicalRecords.noFilesYet")}</p>
      )}

      {data && data.length > 0 && (
        <div className="overflow-hidden rounded-lg border border-line bg-surface shadow-card">
          <table className="w-full text-start text-sm">
            <thead>
              <tr className="border-b border-line text-xs font-medium uppercase tracking-[0.06em] text-ink-faint">
                <th className="px-5 py-3">{t("medicalRecords.file")}</th>
                <th className="px-5 py-3">{t("medicalRecords.size")}</th>
                <th className="px-5 py-3">{t("medicalRecords.uploadedBy")}</th>
                <th className="px-5 py-3">{t("common.date")}</th>
                <th className="px-5 py-3 text-end">{t("common.actions")}</th>
              </tr>
            </thead>
            <tbody>
              {data.map((f) => (
                <tr key={f.id} className="border-b border-line last:border-0 hover:bg-canvas/60">
                  <td className="px-5 py-3 font-medium text-ink">{f.fileName}</td>
                  <td className="px-5 py-3 text-ink-muted">{formatFileSize(f.sizeBytes)}</td>
                  <td className="px-5 py-3 text-ink-muted">{f.uploadedBy || "—"}</td>
                  <td className="px-5 py-3 text-ink-faint">{formatDateTime(f.createdAt)}</td>
                  <td className="px-5 py-3 text-end">
                    <button
                      className="text-xs font-medium text-harbor-500 hover:underline"
                      onClick={() => handleDownload(f)}
                    >
                      {t("common.download")}
                    </button>
                    {canManage && (
                      <button
                        className="ms-3 text-xs font-medium text-danger hover:underline"
                        onClick={() => setDeleteTarget(f)}
                      >
                        {t("common.delete")}
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {deleteTarget && (
        <ConfirmDialog
          title={t("medicalRecords.deleteFile")}
          message={t("medicalRecords.deleteFileConfirm", { name: deleteTarget.fileName })}
          isBusy={isDeleting}
          onConfirm={handleDelete}
          onCancel={() => setDeleteTarget(null)}
        />
      )}
    </div>
  );
}
