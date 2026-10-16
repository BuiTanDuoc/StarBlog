import type { Expense, ExpenseInsert } from "@/schemas/expenses";
import { getAuthHeaders } from "@/utils/api.helpers";

const API_URL = import.meta.env.VITE_API_URL;

export interface PaginatedResult<T> {
    data: T[];
    totalRecords: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
}

export async function getPaginatedExpenses(
    pageNumber = 1,
    pageSize = 10
): Promise<PaginatedResult<Expense>> {
    const res = await fetch(
        `${API_URL}/expenses/getpaginateduserexpenses?pageNumber=${pageNumber}&pageSize=${pageSize}`,
        {
            headers: getAuthHeaders(),
        }
    );
    if (!res.ok) throw new Error("Failed to fetch paginated expenses");
    return await res.json();
}

export async function getExpense(id: number): Promise<Expense> {
    const res = await fetch(`${API_URL}/expenses/getexpensebyid/${id}`, {
        headers: getAuthHeaders(),
    });
    if (!res.ok) throw new Error("Failed to fetch expense");
    return await res.json();
}

export async function createExpense(data: ExpenseInsert): Promise<Expense> {
    const res = await fetch(`${API_URL}/expenses/createexpense`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });
    if (!res.ok) throw new Error("Failed to create expense");
    return await res.json();
}

export async function updateExpense(id: number, data: ExpenseInsert): Promise<Expense> {
    const res = await fetch(`${API_URL}/expenses/updateexpense/${id}`, {
        method: "PUT",
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });
    if (!res.ok) throw new Error("Failed to update expense");
    return await res.json();
}

export async function deleteExpense(id: number): Promise<void> {
    const res = await fetch(`${API_URL}/expenses/deleteexpense/${id}`, {
        method: "DELETE",
        headers: getAuthHeaders(),
    });
    if (!res.ok) throw new Error("Failed to delete expense");
}