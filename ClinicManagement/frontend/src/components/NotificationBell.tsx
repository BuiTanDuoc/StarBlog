import { useEffect, useRef, useState } from "react";
import { HubConnectionBuilder, LogLevel, type HubConnection } from "@microsoft/signalr";
import { useTranslation } from "react-i18next";
import { api, ApiError, API_URL, getStoredTokens } from "../lib/api";
import type { PaginatedList } from "../lib/types";
import { translateNotificationType, formatDateTime } from "../lib/format";

interface NotificationDto {
  id: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  createdAt: string;
}

const POLL_INTERVAL_MS = 60_000;

export function NotificationBell() {
  const { t } = useTranslation();
  const [isOpen, setIsOpen] = useState(false);
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  const unreadCount = notifications.filter((n) => !n.isRead).length;

  async function loadNotifications() {
    setIsLoading(true);
    setError(null);
    try {
      const res = await api.get<PaginatedList<NotificationDto>>("/api/notifications?pageSize=15");
      setNotifications(res.items);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : t("notifications.failedToLoad"));
    } finally {
      setIsLoading(false);
    }
  }

  const loadNotificationsRef = useRef(loadNotifications);
  loadNotificationsRef.current = loadNotifications;

  useEffect(() => {
    loadNotifications();
    const interval = setInterval(loadNotifications, POLL_INTERVAL_MS);
    return () => clearInterval(interval);
  }, []);

  // Live push: the backend broadcasts "ReceiveNotification" over SignalR the moment something
  // happens (new appointment, payment, etc). We don't trust its payload shape for rendering —
  // it lacks an id/isRead — so a push just triggers an immediate REST refresh instead of a
  // 60s-stale one.
  useEffect(() => {
    const tokens = getStoredTokens();
    if (!tokens) return;

    const connection: HubConnection = new HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/notifications`, {
        accessTokenFactory: () => getStoredTokens()?.accessToken ?? "",
      })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on("ReceiveNotification", () => loadNotificationsRef.current());
    connection.start().catch(() => {
      // Real-time push is a nice-to-have — the polling interval above still covers us.
    });

    return () => {
      connection.stop();
    };
  }, []);

  useEffect(() => {
    function handleClickOutside(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setIsOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  async function markAsRead(id: string) {
    setNotifications((prev) => prev.map((n) => (n.id === id ? { ...n, isRead: true } : n)));
    try {
      await api.put(`/api/notifications/${id}/read`);
    } catch {
      // Non-critical — a stale read state resolves itself on the next poll.
    }
  }

  async function markAllAsRead() {
    setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
    try {
      await api.put("/api/notifications/read-all");
    } catch {
      // Non-critical — resolves itself on the next poll.
    }
  }

  return (
    <div className="relative" ref={containerRef}>
      <button
        onClick={() => setIsOpen((v) => !v)}
        className="relative flex h-9 w-9 items-center justify-center rounded-full text-ink-muted hover:bg-canvas"
        aria-label={t("notifications.title")}
      >
        <span className="text-lg leading-none">◔</span>
        {unreadCount > 0 && (
          <span className="absolute -right-0.5 -top-0.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-danger px-1 text-[10px] font-medium text-white">
            {unreadCount > 9 ? "9+" : unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <div className="absolute end-0 z-20 mt-2 w-80 rounded-lg border border-line bg-surface shadow-card">
          <div className="flex items-center justify-between border-b border-line px-4 py-3">
            <p className="text-sm font-medium text-ink">{t("notifications.title")}</p>
            {unreadCount > 0 && (
              <button className="text-xs font-medium text-harbor-500 hover:underline" onClick={markAllAsRead}>
                {t("notifications.markAllRead")}
              </button>
            )}
          </div>

          <div className="max-h-96 overflow-y-auto">
            {isLoading && <p className="px-4 py-6 text-center font-mono text-xs text-ink-muted">{t("common.loading")}</p>}
            {error && <p className="px-4 py-4 text-xs text-danger">{error}</p>}
            {!isLoading && notifications.length === 0 && (
              <p className="px-4 py-6 text-center text-sm text-ink-faint">{t("notifications.allCaughtUp")}</p>
            )}
            {notifications.map((n) => (
              <button
                key={n.id}
                onClick={() => !n.isRead && markAsRead(n.id)}
                className={`block w-full border-b border-line px-4 py-3 text-start last:border-0 hover:bg-canvas/60 ${
                  n.isRead ? "" : "bg-harbor-50/40"
                }`}
              >
                <div className="flex items-center justify-between gap-2">
                  <p className="text-xs font-medium uppercase tracking-[0.04em] text-ink-faint">
                    {translateNotificationType(n.type, t)}
                  </p>
                  {!n.isRead && <span className="h-1.5 w-1.5 shrink-0 rounded-full bg-harbor-500" />}
                </div>
                <p className="mt-1 text-sm font-medium text-ink">{n.title}</p>
                <p className="mt-0.5 text-xs text-ink-muted">{n.message}</p>
                <p className="mt-1 text-[11px] text-ink-faint">{formatDateTime(n.createdAt)}</p>
              </button>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
