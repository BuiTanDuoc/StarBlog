import { z } from "zod";

// Comment schema
export const taskCommentReadOnlySchema = z.object({
    id: z.number(),
    content: z.string(),
    userId: z.number(),
    username: z.string(),
    createdDate: z.string(),
});

export type TaskCommentReadOnlyDTO = z.infer<typeof taskCommentReadOnlySchema>;

export const taskReadOnlySchema = z.object({
    id: z.number(),
    title: z.string(),
    description: z.string().nullable(),
    status: z.string(),
    priority: z.string(),
    dueDate: z.string().nullable(),
    projectId: z.number(),
    assignedToUserId: z.number().nullable(),
    assignedToUsername: z.string().nullable(),
    comments: z.array(taskCommentReadOnlySchema),
    commentCount: z.number(),
    createdDate: z.string(),
});

export type Task = z.infer<typeof taskReadOnlySchema>;

export const taskInsertSchema = z.object({
    title: z.string().min(1, "Task title is required"),
    description: z.string().optional(),
    status: z.enum(["Backlog", "InProgress", "Review", "Done"]),
    priority: z.enum(["Low", "Medium", "High", "Critical"]),
    dueDate: z.string().optional(),
    assignedToUserId: z.number().optional(),
});

export type TaskInsert = z.infer<typeof taskInsertSchema>;
export type TaskInsertDTO = TaskInsert; // ✅ ADD ALIAS

export const taskUpdateSchema = z.object({
    title: z.string().optional(),
    description: z.string().optional(),
    status: z.enum(["Backlog", "InProgress", "Review", "Done"]).optional(),
    priority: z.enum(["Low", "Medium", "High", "Critical"]).optional(),
    dueDate: z.string().optional(),
    assignedToUserId: z.number().optional(),
});

export type TaskUpdate = z.infer<typeof taskUpdateSchema>;
export type TaskUpdateDTO = TaskUpdate; // ✅ ADD ALIAS

export const taskCommentInsertSchema = z.object({
    content: z.string().min(1, "Comment cannot be empty"),
});

export type TaskCommentInsert = z.infer<typeof taskCommentInsertSchema>;

export const paginatedResultSchema = z.object({
    data: z.array(taskReadOnlySchema),
    pageNumber: z.number(),
    pageSize: z.number(),
    totalRecords: z.number(),
    totalPages: z.number(),
});

export type PaginatedResult = z.infer<typeof paginatedResultSchema>;