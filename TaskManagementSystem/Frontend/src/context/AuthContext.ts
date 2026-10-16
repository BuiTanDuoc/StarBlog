import { createContext } from "react";
import type { UserLoginDTO } from "@/schemas/login";
import type { UserReadOnly } from "@/schemas/users";

export type AuthContextType = {
    isAuthenticated: boolean;
    user: UserReadOnly | null;
    username: string | null;  // ✅ ADD THIS for backward compatibility
    token: string | null;
    loginUser: (fields: UserLoginDTO) => Promise<void>;
    logoutUser: () => void;
    loading: boolean;
};

export const AuthContext = createContext<AuthContextType | undefined>(undefined);