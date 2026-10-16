import type { Budget, BudgetInsert } from "@/schemas/budgets";
import { getAuthHeaders } from "@/utils/api.helpers";

const API_URL = import.meta.env.VITE_API_URL;


export async function getUserBudgets(): Promise<Budget[]> {
    const res = await fetch(`${API_URL}/budgets/getuserbudgets`, {
        headers: getAuthHeaders(),
    });
    if (!res.ok) throw new Error("Failed to fetch budgets");
    return await res.json();
}

// Get  budget by ID
export async function getBudget(id: number): Promise<Budget> {
    const res = await fetch(`${API_URL}/budgets/getbudgetbyid/${id}`, {
        headers: getAuthHeaders(),
    });
    if (!res.ok) throw new Error("Failed to fetch budget");
    return await res.json();
}

// Create new budget
export async function createBudget(data: BudgetInsert): Promise<Budget> {
    const res = await fetch(`${API_URL}/budgets/createbudget`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });
    if (!res.ok) {
        const error = await res.text();
        throw new Error(error || "Failed to create budget");
    }
    return await res.json();
}

// Delete budget
export async function deleteBudget(id: number): Promise<void> {
    const res = await fetch(`${API_URL}/budgets/deletebudget/${id}`, {
        method: "DELETE",
        headers: getAuthHeaders(),
    });
    if (!res.ok) throw new Error("Failed to delete budget");
}