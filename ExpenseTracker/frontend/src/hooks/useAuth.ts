import {useContext} from "react";
import {AuthContext} from "@/context/AuthContext.ts";

// Custom hook pattern: prefix with "use" to indicate it's a React hook
export function useAuth() {
    // useContext is the React hook that reads from a Context
    // It returns whatever value the nearest Provider above it has
    const ctx = useContext(AuthContext);

    // Guard clause: ensure this hook is used within AuthProvider
    // This prevents runtime errors and gives developers a clear error message
    if (!ctx) {
        throw new Error("useAuth must be used within AuthProvider");
    }

    // Return the context value (auth state and functions)
    return ctx;
}