import { useCallback, useEffect, useState } from "react";
import { api, ApiError } from "./api";

interface UseApiDataResult<T> {
  data: T | null;
  isLoading: boolean;
  error: string | null;
  /** Re-runs the same GET request — handy after a create/update/delete elsewhere on the page. */
  refetch: () => void;
}

export function useApiData<T>(path: string): UseApiDataResult<T> {
  const [data, setData] = useState<T | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [refreshNonce, setRefreshNonce] = useState(0);

  const refetch = useCallback(() => setRefreshNonce((n) => n + 1), []);

  useEffect(() => {
    let cancelled = false;

    setIsLoading(true);
    setError(null);

    api
      .get<T>(path)
      .then((result) => {
        if (!cancelled) setData(result);
      })
      .catch((err) => {
        if (!cancelled) setError(err instanceof ApiError ? err.message : "Failed to load data.");
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false);
      });

    return () => {
      cancelled = true;
    };
  }, [path, refreshNonce]);

  return { data, isLoading, error, refetch };
}
