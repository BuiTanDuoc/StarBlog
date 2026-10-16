import { createContext } from "react";
import type { UserLogin } from "@/schemas/login.ts";


// This defines what data and functions will be available to consumers
type AuthContextProps = {
    isAuthenticated: boolean;                    // Is user logged in?
    accessToken: string | null;                  // JWT token (null if not logged in)
    userId: string | null;                       // User ID from token
    username: string | null;                     // Username from token
    userRole: string | null;                     // User role (Admin or User)
    loginUser: (fields: UserLogin) => Promise<void>;  // Async login function
    logoutUser: () => void;                      // Logout function
    loading: boolean;                            // Is auth state being initialized?
}

// createContext creates a Context object
// undefined means: if someone tries to use this context without a Provider, throw error
// This is the "context object" - the actual data container
export const AuthContext = createContext<AuthContextProps | undefined>(undefined);