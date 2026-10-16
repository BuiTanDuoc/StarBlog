import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { api, clearTokens, getStoredTokens, storeTokens, ApiError } from "./api";

export interface AuthUser {
  userId: string;
  email: string;
  fullName: string;
  tenantId: string;
  roles: string[];
}

interface LoginResponse {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  userId: string;
  email: string;
  fullName: string;
  tenantId: string;
  roles: string[];
}

interface AuthContextValue {
  user: AuthUser | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<AuthUser>;
  registerTenant: (payload: {
    clinicName: string;
    slug: string;
    ownerFullName: string;
    ownerEmail: string;
    password: string;
  }) => Promise<AuthUser>;
  registerPatient: (payload: {
    tenantSlug: string;
    fullName: string;
    email: string;
    phone: string;
    password: string;
    dateOfBirth: string;
    gender: string;
  }) => Promise<AuthUser>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const USER_STORAGE_KEY = "clinic.user";

/** The dashboard each role lands on after login — first match wins for users with multiple roles. */
export function primaryRouteForRoles(roles: string[]): string {
  if (roles.includes("ClinicOwner")) return "/dashboard/owner";
  if (roles.includes("Doctor")) return "/dashboard/doctor";
  if (roles.includes("Receptionist")) return "/dashboard/receptionist";
  if (roles.includes("Patient")) return "/portal";
  // SuperAdmin has no dashboard endpoint of its own (the clinic-owner/doctor/receptionist
  // dashboards are role-locked at the API level, not covered by SuperAdmin's permission
  // bypass) — /welcome surfaces the platform-admin sections (Plans, Audit log) it does have.
  return "/welcome";
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const tokens = getStoredTokens();
    const rawUser = localStorage.getItem(USER_STORAGE_KEY);
    if (tokens && rawUser) {
      setUser(JSON.parse(rawUser) as AuthUser);
    }
    setIsLoading(false);
  }, []);

  function applyAuthResponse(response: LoginResponse): AuthUser {
    storeTokens({
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      accessTokenExpiresAt: response.accessTokenExpiresAt,
    });

    const authUser: AuthUser = {
      userId: response.userId,
      email: response.email,
      fullName: response.fullName,
      tenantId: response.tenantId,
      roles: response.roles,
    };
    localStorage.setItem(USER_STORAGE_KEY, JSON.stringify(authUser));
    setUser(authUser);
    return authUser;
  }

  async function login(email: string, password: string): Promise<AuthUser> {
    const response = await api.post<LoginResponse>("/api/auth/login", { email, password });
    return applyAuthResponse(response);
  }

  async function registerTenant(payload: {
    clinicName: string;
    slug: string;
    ownerFullName: string;
    ownerEmail: string;
    password: string;
  }): Promise<AuthUser> {
    const response = await api.post<LoginResponse>("/api/auth/register-tenant", payload);
    return applyAuthResponse(response);
  }

  async function registerPatient(payload: {
    tenantSlug: string;
    fullName: string;
    email: string;
    phone: string;
    password: string;
    dateOfBirth: string;
    gender: string;
  }): Promise<AuthUser> {
    const response = await api.post<LoginResponse>("/api/patient-portal/register", payload);
    return applyAuthResponse(response);
  }

  function logout() {
    const tokens = getStoredTokens();
    if (tokens?.refreshToken) {
      // Best-effort — don't block logout on this, and don't leak errors to the caller.
      api.post("/api/auth/revoke-token", { refreshToken: tokens.refreshToken }).catch(() => {});
    }
    clearTokens();
    localStorage.removeItem(USER_STORAGE_KEY);
    setUser(null);
    window.location.href = "/login";
  }

  return (
    <AuthContext.Provider value={{ user, isLoading, login, registerTenant, registerPatient, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}

export { ApiError };
