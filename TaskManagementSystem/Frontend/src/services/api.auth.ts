import type { UserLoginDTO } from "@/schemas/login";
import type { UserRegisterDTO } from "@/schemas/register";
import { handleResponse, API_URL, getAuthHeaders } from "./api.common";
import type { UserReadOnly } from "@/schemas/users";

export type JwtTokenResponse = {
    token: string;
    username: string;
    id: number;
    email: string;
    firstname: string;
    lastname: string;
    role: string;
    expiresAt: string;
};

export type AuthResponse = {
    success?: boolean;
    data: JwtTokenResponse;
};

export type RegisterResponse = {
    success?: boolean;
    data: {
        id: number;
        username: string;
    };
};

export async function login({ username, password }: UserLoginDTO): Promise<AuthResponse> {
    const res = await fetch(`${API_URL}/api/auth/login/access-token`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password }),
    });

    const result = await handleResponse<JwtTokenResponse>(res);
    return {
        success: true,
        data: result,
    };
}

export async function register({ email, username, password, firstname, lastname }: UserRegisterDTO): Promise<RegisterResponse> {
    const registerData = {
        email,
        username,
        password,
        firstname,
        lastname,
    };

    const res = await fetch(`${API_URL}/api/users`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(registerData),
    });

    const result = await handleResponse<{ id: number; username: string }>(res);
    return {
        success: true,
        data: result,
    };
}

export async function searchUsers(query: string): Promise<UserReadOnly[]> {
    if (!query.trim()) {
        return [];
    }

    try {
        const res = await fetch(`${API_URL}/api/users/search?query=${encodeURIComponent(query)}`, {
            method: "GET",
            headers: getAuthHeaders(),
        });

        // ✅ Response is already an array
        const result = await handleResponse<UserReadOnly[]>(res);

        return Array.isArray(result) ? result : [];
    } catch (error) {
        console.error("Search users error:", error);
        throw error;
    }
}