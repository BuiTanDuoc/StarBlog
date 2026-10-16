import { Navigate, Outlet, useLocation } from "react-router";
import { useAuth } from "@/hooks/useAuth.ts";

interface ProtectedRouteProps {
    requireAdmin?: boolean;
}

const ProtectedRoute = ({ requireAdmin = false }: ProtectedRouteProps) => {
    // ACCESS AUTH STATE
    const { isAuthenticated, userRole, loading } = useAuth();

    // SAVE CURRENT LOCATION: So we can redirect back after login
    const location = useLocation();

    // Wait for auth check to complete
    if (loading) {
        return <div className="p-8 text-center">Loading...</div>;
    }

    //  Not logged in at all
    if (!isAuthenticated) {
        // Navigate component = declarative redirect
        return <Navigate
            to="/login"
            state={{ from: location }}  // Save where they tried to go
            replace  // Don't add to history (prevents back button issues)
        />;
    }

    //  Logged in but not admin (when admin required)
    if (requireAdmin && userRole !== "Admin") {
        return <Navigate to="/unauthorized" replace />;
    }

    //IF ALL CHECKS PASSED Render child routes
    return <Outlet />;
};

export default ProtectedRoute;