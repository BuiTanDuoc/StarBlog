import type { Project, ProjectInsert, ProjectUpdate, ProjectMember, AddProjectMember } from "@/schemas/projects";
import { getAuthHeaders, handleResponse, API_URL } from "./api.common";

export async function getProjects(): Promise<Project[]> {
    const res = await fetch(`${API_URL}/api/projects/user/all`, {
        method: "GET",
        headers: getAuthHeaders(),
    });

    return handleResponse<Project[]>(res);
}

export async function getProject(id: number): Promise<Project> {
    const res = await fetch(`${API_URL}/api/projects/${id}`, {
        method: "GET",
        headers: getAuthHeaders(),
    });

    return handleResponse<Project>(res);
}

export async function createProject(data: ProjectInsert): Promise<Project> {
    const res = await fetch(`${API_URL}/api/projects/create`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });

    return handleResponse<Project>(res);
}

export async function updateProject(id: number, data: ProjectUpdate): Promise<Project> {
    const res = await fetch(`${API_URL}/api/projects/${id}`, {
        method: "PUT",
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });

    return handleResponse<Project>(res);
}

export async function deleteProject(id: number): Promise<void> {
    const res = await fetch(`${API_URL}/api/projects/${id}`, {
        method: "DELETE",
        headers: getAuthHeaders(),
    });

    await handleResponse<void>(res);
}

// ✅ ADD THIS - Get project members
export async function getProjectMembers(projectId: number): Promise<ProjectMember[]> {
    const res = await fetch(`${API_URL}/api/projects/${projectId}/members`, {
        method: "GET",
        headers: getAuthHeaders(),
    });

    return handleResponse<ProjectMember[]>(res);
}

export async function addProjectMember(projectId: number, data: AddProjectMember): Promise<ProjectMember> {
    const res = await fetch(`${API_URL}/api/projects/${projectId}/members`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify(data),
    });

    return handleResponse<ProjectMember>(res);
}

export async function removeProjectMember(projectId: number, memberId: number): Promise<void> {
    const res = await fetch(`${API_URL}/api/projects/${projectId}/members/${memberId}`, {
        method: "DELETE",
        headers: getAuthHeaders(),
    });

    await handleResponse<void>(res);
}

export async function updateMemberRole(projectId: number, memberId: number, newRole: string): Promise<ProjectMember> {
    const res = await fetch(
        `${API_URL}/api/projects/${projectId}/members/${memberId}/role?newRole=${newRole}`,
        {
            method: "PUT",
            headers: getAuthHeaders(),
        }
    );

    return handleResponse<ProjectMember>(res);
}