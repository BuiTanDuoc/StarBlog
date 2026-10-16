export const API_URL = import.meta.env.VITE_API_URL || "http://localhost:5001";

export function getAuthHeaders(): HeadersInit {
    const token = getCookie("jwt_token");
    return {
        "Content-Type": "application/json",
        ...(token && { Authorization: `Bearer ${token}` }),
    };
}

export async function handleResponse<T>(res: Response): Promise<T> {
    let data: any;

    try {
        data = await res.json();
    } catch (e) {
        throw new Error(`Failed to parse response: ${res.statusText}`);
    }

    if (!res.ok) {
        throw new Error(
            data?.message ||
            data?.error ||
            `HTTP Error ${res.status}: ${res.statusText}`
        );
    }

    // ✅ Response is already in correct format
    return data;
}

// Helper to get/set cookies
export function getCookie(name: string): string | null {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop()?.split(";").shift() || null;
    return null;
}