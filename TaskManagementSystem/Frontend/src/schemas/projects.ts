import { z } from "zod";

export const projectReadOnlySchema = z.object({
    id: z.number(),
    name: z.string(),
    description: z.string().nullable(),
    ownerId: z.number(),
    ownerName: z.string(),
    memberCount: z.number(),
    taskCount: z.number(),
    createdDate: z.string(),
});

export type Project = z.infer<typeof projectReadOnlySchema>;

export const projectInsertSchema = z.object({
    name: z.string().min(1, "Project name is required"),
    description: z.string().optional(),
});

export type ProjectInsert = z.infer<typeof projectInsertSchema>;
export type ProjectInsertDTO = ProjectInsert; // ✅ ADD ALIAS

export const projectUpdateSchema = z.object({
    name: z.string().optional(),
    description: z.string().optional(),
});

export type ProjectUpdate = z.infer<typeof projectUpdateSchema>;
export type ProjectUpdateDTO = ProjectUpdate; // ✅ ADD ALIAS

export const projectMemberSchema = z.object({
    id: z.number(),
    userId: z.number(),
    username: z.string(),
    email: z.string(),
    role: z.string(),
});

export type ProjectMember = z.infer<typeof projectMemberSchema>;

export const addProjectMemberSchema = z.object({
    userId: z.number().min(1, "User is required"),
    role: z.enum(["Owner", "Member", "Viewer"]),
});

export type AddProjectMember = z.infer<typeof addProjectMemberSchema>;