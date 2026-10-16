import { z } from "zod";

export const userLoginSchema = z.object({
    username: z
        .string()
        .min(2, { message: "Username must be at least 2 characters" })
        .max(50, { message: "Username cannot exceed 50 characters" }),

    password: z
        .string()
        .min(8, { message: "Password must be at least 8 characters" })
        .regex(
            /^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*\W).*$/,
            { message: "Password must contain uppercase, lowercase, number, and special character" }
        ),

    keepLoggedIn: z.boolean(),
});

export type UserLogin = z.infer<typeof userLoginSchema>;
