import { Navigate } from "react-router-dom";
import { useTranslation } from "react-i18next";
import { useAuth } from "../lib/auth";

interface RoleRouteProps {
  children: React.ReactNode;
  allowedRoles?: string[];
}

export function RoleRoute({ children, allowedRoles }: RoleRouteProps) {
  const { user, isLoading } = useAuth();
  const { t } = useTranslation();

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-canvas">
        <p className="font-mono text-sm text-ink-muted">{t("common.loading")}</p>
      </div>
    );
  }

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (allowedRoles && !allowedRoles.some((role) => user.roles.includes(role))) {
    return <Navigate to="/welcome" replace />;
  }

  return <>{children}</>;
}
