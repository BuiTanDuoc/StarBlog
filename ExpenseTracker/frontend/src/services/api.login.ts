import type { UserLogin } from "@/schemas/login.ts";

const API_URL = import.meta.env.VITE_API_URL;

// JwtTokenDTO from the backend
export type LoginResponse = {
    token: string;
    username: string;
    role: string;
    expiresAt: string;
}

export async function login({
                                username,
                                password,
                                keepLoggedIn
                            }: UserLogin): Promise<LoginResponse> {
    const res = await fetch(`${API_URL}/auth/login/access-token`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({
            username,
            password,
            keepLoggedIn
        }),
    });

    if (!res.ok) {
        let detail = "Login failed";
        try {
            const data = await res.json();
            // Handle different error response formats
            if (typeof data?.title === "string") detail = data.title;
            if (typeof data?.detail === "string") detail = data.detail;
            if (typeof data === "string") detail = data;
        } catch (error) {
            console.error("Error parsing error response:", error);
        }
        throw new Error(detail);
    }

    return await res.json();
}