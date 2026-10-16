import { z } from "zod";

export const userReadOnlySchema = z.object({
    id: z.number(),
    username: z.string(),
    email: z.string(),
    firstname: z.string(),
    lastname: z.string(),
    userRole: z.string(),
});

export type UserReadOnly = z.infer<typeof userReadOnlySchema>;