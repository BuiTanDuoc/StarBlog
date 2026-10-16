import type { UserReadOnly, UserInsert, UserUpdate, UserRegister } from "@/schemas/users";
import { getAuthHeaders } from "@/utils/api.helpers";

const API_URL = import.meta.env.VITE_API_URL;

export interface PaginatedResult<T> {
    data: T[];
    totalRecords: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
}

// Fetch paginated users
export async function getPaginatedUsers(
    pageNumber = 1,
    pageSize = 10
): Promise<PaginatedResult<UserReadOnly>> {
    const res = await fetch(
        `${API_URL}/Users/Getallusers?pageNumber=${pageNumber}&pageSize=${pageSize}`,
        {
            headers: getAuthHeaders(),
        }
    );
    if (!res.ok) throw new Error("Failed to fetch paginated users");
    return await res.json();
}

// Get single user by ID
export async function getUser(id: number): Promise<UserReadOnly> {
    const res = await fetch(`${API_URL}/users/getuserbyid/${id}`, {
        headers: getAuthHeaders(),
    });
    if (!res.ok) throw new Error("Failed to fetch user");
    return await res.json();
}

// Register new user (public - no auth required)
export async function registerUser(data: UserRegister): Promise<UserReadOnly> {
    const res = await fetch(`${API_URL}/users/registeruser`, {
        method: "POST",
        headers: { "Content-Type": "application/json" }, // No auth header for registration
        body: JSON.stringify({
            username: data.username,
            email: data.email,
            firstname: data.firstname,
            lastname: data.lastname,
            password: data.password,
            confirmPassword: data.confirmPassword,
        }),
    });

    if (!res.ok) {
        let detail = "Registration failed";
        try {
            const errorData = await res.json();
            if (typeof errorData?.title === "string") detail = errorData.title;
            if (errorData?.errors) {
                const errors = Object.values(errorData.errors).flat();
                detail = errors.join(", ");
            }
        } catch (error) {
            console.error("Error parsing error response:", error);
        }
        throw new Error(detail);
    }

    return await res.json();
}

// Create new user (Admin only - requires auth)
export async function createUser(data: UserInsert): Promise<UserReadOnly> {
    const res = await fetch(`${API_URL}/users/registeruser`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });
    if (!res.ok) throw new Error("Failed to create user");
    return await res.json();
}

// Update existing user
export async function updateUser(id: number, data: UserUpdate): Promise<UserReadOnly> {
    const res = await fetch(`${API_URL}/users/updateuser/${id}`, {
        method: "PUT",
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });
    if (!res.ok) throw new Error("Failed to update user");
    return await res.json();
}

// Delete user
export async function deleteUser(id: number): Promise<void> {
    const res = await fetch(`${API_URL}/users/delete/${id}`, {
        method: "DELETE",
        headers: getAuthHeaders(),
    });
    if (!res.ok) throw new Error("Failed to delete user");
}