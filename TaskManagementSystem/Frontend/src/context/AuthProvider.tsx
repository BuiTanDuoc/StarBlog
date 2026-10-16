import { useEffect, useState } from "react";
import type { UserLoginDTO } from "@/schemas/login";
import { login } from "@/services/api.auth";
import { deleteCookie, getCookie, setCookie } from "@/utils/cookies";
import { AuthContext } from "@/context/AuthContext";
import * as React from "react";
import type { UserReadOnly } from "@/schemas/users";

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [user, setUser] = useState<UserReadOnly | null>(null);
    const [username, setUsername] = useState<string | null>(null);
    const [token, setToken] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);

    // Check if user is already logged in when app starts
    useEffect(() => {
        const storedToken = getCookie("jwt_token");
        const storedUser = getCookie("user");

        if (storedToken && storedUser) {
            try {
                const parsedUser = JSON.parse(storedUser);
                setIsAuthenticated(true);
                setUser(parsedUser);
                setUsername(parsedUser.username);
                setToken(storedToken);
            } catch (error) {
                console.error("Failed to parse stored user:", error);
                deleteCookie("jwt_token");
                deleteCookie("user");
            }
        }

        setLoading(false);
    }, []);

    const loginUser = async (fields: UserLoginDTO) => {
        try {
            const response = await login(fields);
            const { token: newToken, username: newUsername, role, expiresAt } = response.data;

            const userData: UserReadOnly = {
                id: response.data.id || 1,
                username: newUsername,
                email: response.data.email || "",
                firstname: response.data.firstname || "",
                lastname: response.data.lastname || "",
                userRole: role,
            };

            // Store in cookies
            setCookie("jwt_token", newToken, {
                expires: new Date(expiresAt),
                secure: import.meta.env.PROD,
                sameSite: "Lax",
                path: "/",
            });

            setCookie("user", JSON.stringify(userData), {
                expires: new Date(expiresAt),
                secure: import.meta.env.PROD,
                sameSite: "Lax",
                path: "/",
            });

            // Update state
            setIsAuthenticated(true);
            setUser(userData);
            setUsername(newUsername);
            setToken(newToken);
        } catch (error) {
            console.error("AuthProvider loginUser error:", error);
            throw error;
        }
    };

    const logoutUser = () => {
        deleteCookie("jwt_token");
        deleteCookie("user");

        setIsAuthenticated(false);
        setUser(null);
        setUsername(null);
        setToken(null);
    };

    return (
        <AuthContext.Provider
            value={{
                isAuthenticated,
                user,
                username,
                token,
                loginUser,
                logoutUser,
                loading,
            }}
        >
            {!loading && children}
        </AuthContext.Provider>
    );
};