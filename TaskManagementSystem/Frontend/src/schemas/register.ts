import { z } from "zod";

export const userRegisterSchema = z
    .object({
        username: z
            .string()
            .min(2, "Username must be at least 2 characters")
            .max(50, "Username must be less than 50 characters"),
        email: z.string().email("Invalid email address"),
        password: z
            .string()
            .min(8, "Password must be at least 8 characters")
            .regex(/[A-Z]/, "Password must contain an uppercase letter")
            .regex(/[a-z]/, "Password must contain a lowercase letter")
            .regex(/\d/, "Password must contain a number")
            .regex(/[^\w]/, "Password must contain a special character"),
        confirmPassword: z.string(),
        firstname: z.string().min(2, "First name is required"),
        lastname: z.string().min(2, "Last name is required"),
    })
    .refine((data) => data.password === data.confirmPassword, {
        message: "Passwords don't match",
        path: ["confirmPassword"],
    });

export type UserRegisterDTO = z.infer<typeof userRegisterSchema>;