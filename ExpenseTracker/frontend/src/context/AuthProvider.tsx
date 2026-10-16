import {useEffect, useState} from "react";
import type {UserLogin} from "@/schemas/login.ts";
import {login} from "@/services/api.login.ts";
import {deleteCookie, getCookie, setCookie} from "@/utils/cookies.ts";
import {jwtDecode, } from "jwt-decode";
import {AuthContext} from "@/context/AuthContext.ts";


type JwtPayload =  {
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": string; // userId
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": string;           // username
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": string;   // email
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": string;         // role
    exp: number;  // expiration timestamp
    iss: string;  // issuer
    aud: string;  // audience
}

export const AuthProvider = ({children}: {children: React.ReactNode}) => {
    const [accessToken, setAccessToken] = useState<string | null>(null);
    const [userRole, setUserRole] = useState<string | null>(null);
    const [userId, setUserId] = useState<string | null>(null);
    const [username, setUsername] = useState<string | null>(null);
    const [loading, setLoading] = useState<boolean>(true);

    // Check if user is already logged in
    useEffect(() => {
        const token = getCookie("access_token");
        setAccessToken(token ?? null);

        if (token) {
            try {
                //  Extract user info from token
                const decoded = jwtDecode<JwtPayload>(token);

                // Check if token is expired
                if (decoded.exp * 1000 < Date.now()) {
                    logoutUser();
                    return;
                }

                // EXTRACT CLAIMS: Parse .NET claim format
                setUserId(decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ?? null);
                setUsername(decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ?? null);
                setUserRole(decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ?? null);
            } catch (error) {
                console.error("Failed to decode token:", error);
                setUserId(null);
                setUsername(null);
                setUserRole(null);
            }
        } else {
            // No token found, user is not logged in
            setUserId(null);
            setUsername(null);
            setUserRole(null);
        }
        // Done checking, app can render now
        setLoading(false);
    }, []);

    const loginUser = async (fields: UserLogin) => {
        //  Send credentials to backend
        const res = await login(fields);

        // SAVE TOKEN: Store in cookie
        setCookie("access_token", res.token, {
            expires: fields.keepLoggedIn ? 7 : 1,  // 7 days if keepLoggedIn, else 1 day
            sameSite: 'Lax',
            secure: false,  // Set to true in production (https)
            path: "/"
        });

        setAccessToken(res.token);

        // DECODE AND STORE USER INFO
        try {
            const decoded = jwtDecode<JwtPayload>(res.token);
            setUserId(decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ?? null);
            setUsername(decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ?? null);
            setUserRole(decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ?? null);


        } catch (error) {
            console.error("Failed to decode token:", error);
            setUserId(null);
            setUsername(null);
            setUserRole(null);
        }
    };
        // LOGOUT FUNCTION: Clear everything
    const logoutUser = () => {
        deleteCookie("access_token");
        setAccessToken(null);
        setUserId(null);
        setUsername(null);
        setUserRole(null);
    };

    // PROVIDE TO ALL CHILDREN
    return (
        <AuthContext.Provider
            value={{
                isAuthenticated: !!accessToken,
                accessToken,
                userId,
                username,
                userRole,
                loginUser,
                logoutUser,
                loading,
            }}
        >
            {loading ? <div className="p-8 text-center">Loading...</div> : children}
        </AuthContext.Provider>
    );
};