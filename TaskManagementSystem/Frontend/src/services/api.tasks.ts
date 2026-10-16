import type { Task, TaskInsert, TaskUpdate, PaginatedResult, TaskCommentReadOnlyDTO, TaskCommentInsert } from "@/schemas/tasks";
import { getAuthHeaders, handleResponse, API_URL } from "./api.common";

export async function getProjectTasks(projectId: number): Promise<Task[]> {
    const res = await fetch(`${API_URL}/api/projects/${projectId}/tasks/all`, {
        method: "GET",
        headers: getAuthHeaders(),
    });

    return handleResponse<Task[]>(res);
}

export async function getPaginatedTasks(projectId: number, pageNumber = 1, pageSize = 10): Promise<PaginatedResult> {
    const res = await fetch(
        `${API_URL}/api/projects/${projectId}/tasks/paginated?pageNumber=${pageNumber}&pageSize=${pageSize}`,
        {
            method: "GET",
            headers: getAuthHeaders(),
        }
    );

    return handleResponse<PaginatedResult>(res);
}

export async function getTask(projectId: number, taskId: number): Promise<Task> {
    const res = await fetch(`${API_URL}/api/projects/${projectId}/tasks/${taskId}`, {
        method: "GET",
        headers: getAuthHeaders(),
    });

    return handleResponse<Task>(res);
}

export async function createTask(projectId: number, data: TaskInsert): Promise<Task> {
    const res = await fetch(`${API_URL}/api/projects/${projectId}/tasks/create`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });

    return handleResponse<Task>(res);
}

export async function updateTask(projectId: number, taskId: number, data: TaskUpdate): Promise<Task> {
    const res = await fetch(`${API_URL}/api/projects/${projectId}/tasks/${taskId}`, {
        method: "PUT",
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });

    return handleResponse<Task>(res);
}

export async function updateTaskStatus(projectId: number, taskId: number, status: string): Promise<Task> {
    const res = await fetch(
        `${API_URL}/api/projects/${projectId}/tasks/${taskId}/status?newStatus=${status}`,
        {
            method: "PATCH",
            headers: getAuthHeaders(),
        }
    );

    return handleResponse<Task>(res);
}

export async function assignTask(projectId: number, taskId: number, userId: number): Promise<Task> {
    const res = await fetch(
        `${API_URL}/api/projects/${projectId}/tasks/${taskId}/assign?assignToUserId=${userId}`,
        {
            method: "PUT",
            headers: getAuthHeaders(),
        }
    );

    return handleResponse<Task>(res);
}

export async function unassignTask(projectId: number, taskId: number): Promise<Task> {
    const res = await fetch(`${API_URL}/api/projects/${projectId}/tasks/${taskId}/unassign`, {
        method: "PUT",
        headers: getAuthHeaders(),
    });

    return handleResponse<Task>(res);
}

export async function deleteTask(projectId: number, taskId: number): Promise<void> {
    const res = await fetch(`${API_URL}/api/projects/${projectId}/tasks/${taskId}`, {
        method: "DELETE",
        headers: getAuthHeaders(),
    });

    await handleResponse<void>(res);
}

export async function searchTasks(projectId: number, query: string): Promise<Task[]> {
    const res = await fetch(`${API_URL}/api/projects/${projectId}/tasks/search?query=${encodeURIComponent(query)}`, {
        method: "GET",
        headers: getAuthHeaders(),
    });

    return handleResponse<Task[]>(res);
}

// ✅ ADD THIS - Add task comment
export async function addComment(projectId: number, taskId: number, data: TaskCommentInsert): Promise<TaskCommentReadOnlyDTO> {
    const res = await fetch(`${API_URL}/api/projects/${projectId}/tasks/${taskId}/comments`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });

    return handleResponse<TaskCommentReadOnlyDTO>(res);
}

// ✅ ADD THIS - Get task comments (if needed separately)
export async function getTaskComments(projectId: number, taskId: number): Promise<TaskCommentReadOnlyDTO[]> {
    const res = await fetch(`${API_URL}/api/projects/${projectId}/tasks/${taskId}/comments`, {
        method: "GET",
        headers: getAuthHeaders(),
    });

    return handleResponse<TaskCommentReadOnlyDTO[]>(res);
}

// ✅ ADD THIS - Delete task comment
export async function deleteComment(projectId: number, taskId: number, commentId: number): Promise<void> {
    const res = await fetch(
        `${API_URL}/api/projects/${projectId}/tasks/${taskId}/comments/${commentId}`,
        {
            method: "DELETE",
            headers: getAuthHeaders(),
        }
    );

    await handleResponse<void>(res);
}