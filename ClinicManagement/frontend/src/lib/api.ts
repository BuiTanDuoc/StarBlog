export const API_URL = import.meta.env.VITE_API_URL ?? "https://localhost:7168";

interface TokenPair {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
}

const STORAGE_KEY = "clinic.auth";

export function getStoredTokens(): TokenPair | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  return raw ? (JSON.parse(raw) as TokenPair) : null;
}

function storeTokens(tokens: TokenPair) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(tokens));
}

function clearTokens() {
  localStorage.removeItem(STORAGE_KEY);
}

export class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
    public errors: string[] = [],
  ) {
    super(message);
  }
}

/**
 * Every request goes through here. On a 401 (expired access token) it
 * transparently calls /api/auth/refresh-token once and retries — callers
 * never have to think about token lifetime. On refresh failure, tokens are
 * cleared and the error propagates so the caller can redirect to /login.
 */
async function request<T>(path: string, options: RequestInit = {}, isRetry = false): Promise<T> {
  const tokens = getStoredTokens();

  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(tokens ? { Authorization: `Bearer ${tokens.accessToken}` } : {}),
      ...options.headers,
    },
  });

  if (response.status === 401 && tokens && !isRetry) {
    const refreshed = await tryRefresh(tokens.refreshToken);
    if (refreshed) {
      return request<T>(path, options, true);
    }
    clearTokens();
    window.location.href = "/login";
    throw new ApiError("Session expired.", 401);
  }

  if (!response.ok) {
    const body = await safeJson(response);
    throw new ApiError(
      body?.errors?.[0] ?? `Request failed (${response.status})`,
      response.status,
      body?.errors ?? [],
    );
  }

  if (response.status === 204) return undefined as T;
  return (await safeJson(response)) as T;
}

async function tryRefresh(refreshToken: string): Promise<boolean> {
  try {
    const response = await fetch(`${API_URL}/api/auth/refresh-token`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    });
    if (!response.ok) return false;

    const data = await response.json();
    storeTokens({
      accessToken: data.accessToken,
      refreshToken: data.refreshToken,
      accessTokenExpiresAt: data.accessTokenExpiresAt,
    });
    return true;
  } catch {
    return false;
  }
}

async function safeJson(response: Response) {
  try {
    return await response.json();
  } catch {
    return null;
  }
}

export const api = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "POST", body: body ? JSON.stringify(body) : undefined }),
  put: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: "PUT", body: body ? JSON.stringify(body) : undefined }),
  delete: <T>(path: string) => request<T>(path, { method: "DELETE" }),
};

export async function uploadFile<T>(path: string, formData: FormData): Promise<T> {
  const tokens = getStoredTokens();
  const response = await fetch(`${API_URL}${path}`, {
    method: "POST",
    headers: tokens ? { Authorization: `Bearer ${tokens.accessToken}` } : {},
    body: formData,
  });

  if (!response.ok) {
    const body = await safeJson(response);
    throw new ApiError(
      body?.errors?.[0] ?? `Upload failed (${response.status})`,
      response.status,
      body?.errors ?? [],
    );
  }

  return (await safeJson(response)) as T;
}

export async function downloadFile(path: string, fileName: string): Promise<void> {
  const tokens = getStoredTokens();
  const response = await fetch(`${API_URL}${path}`, {
    headers: tokens ? { Authorization: `Bearer ${tokens.accessToken}` } : {},
  });
  if (!response.ok) throw new ApiError(`Failed to download file (${response.status})`, response.status);

  const blob = await response.blob();
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(url);
}

export { storeTokens, clearTokens };
