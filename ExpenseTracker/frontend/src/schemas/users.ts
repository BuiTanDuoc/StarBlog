import { z } from "zod";

// matches backend UserReadOnlyDTO
export const userSchema = z.object({
    id: z.coerce.number().int(),
    username: z
        .string()
        .min(2, { message: "Username must be at least 2 characters" })
        .max(50, { message: "Username cannot exceed 50 characters" }),
    email: z
        .string()
        .email({ message: "Invalid email format" }),
    firstname: z
        .string()
        .min(2, { message: "First name must be at least 2 characters" })
        .max(50, { message: "First name cannot exceed 50 characters" }),
    lastname: z
        .string()
        .min(2, { message: "Last name must be at least 2 characters" })
        .max(50, { message: "Last name cannot exceed 50 characters" }),
    userRole: z.string(),
    token: z.string().optional(),
});

// backend UserInsertDTO - for creating users
export const userInsertSchema = z.object({
    username: z
        .string()
        .min(2, { message: "Username must be at least 2 characters" })
        .max(50, { message: "Username cannot exceed 50 characters" }),
    email: z
        .string()
        .trim()
        .toLowerCase()
        .email({ message: "Invalid email format" }),
    firstname: z
        .string()
        .min(2, { message: "First name must be at least 2 characters" })
        .max(50, { message: "First name cannot exceed 50 characters" }),
    lastname: z
        .string()
        .min(2, { message: "Last name must be at least 2 characters" })
        .max(50, { message: "Last name cannot exceed 50 characters" }),
    password: z
        .string()
        .min(8, { message: "Password must be at least 8 characters" })
        .regex(/^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*\W).*$/, {
            message: "Password must contain uppercase, lowercase, number, and special character",
        }),
    userRole: z.string().default("User"),
});

// backend UpdateUserDTO - matches your backend exactly (no password, no userRole)
export const userUpdateSchema = z.object({
    username: z
        .string()
        .min(2, { message: "Username must be at least 2 characters" })
        .max(50, { message: "Username cannot exceed 50 characters" }),
    email: z
        .string()
        .trim()
        .toLowerCase()
        .email({ message: "Invalid email format" }),
    firstname: z
        .string()
        .min(2, { message: "First name must be at least 2 characters" })
        .max(50, { message: "First name cannot exceed 50 characters" }),
    lastname: z
        .string()
        .min(2, { message: "Last name must be at least 2 characters" })
        .max(50, { message: "Last name cannot exceed 50 characters" }),
});

// For registration (no userRole - backend assigns it)
export const userRegisterSchema = z.object({
    username: z
        .string()
        .min(2, { message: "Username must be at least 2 characters" })
        .max(50, { message: "Username cannot exceed 50 characters" }),
    email: z
        .string()
        .trim()
        .toLowerCase()
        .email({ message: "Invalid email format" }),
    firstname: z
        .string()
        .min(2, { message: "First name must be at least 2 characters" })
        .max(50, { message: "First name cannot exceed 50 characters" }),
    lastname: z
        .string()
        .min(2, { message: "Last name must be at least 2 characters" })
        .max(50, { message: "Last name cannot exceed 50 characters" }),
    password: z
        .string()
        .min(8, { message: "Password must be at least 8 characters" })
        .regex(/^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*\W).*$/, {
            message: "Password must contain uppercase, lowercase, number, and special character",
        }),
    confirmPassword: z
        .string()
        .min(1, { message: "Please confirm your password" }),
})
    .refine((data) => data.password === data.confirmPassword, {
        message: "Passwords do not match",
        path: ["confirmPassword"],
    });

export type UserReadOnly = z.infer<typeof userSchema>;
export type UserInsert = z.infer<typeof userInsertSchema>;
export type UserUpdate = z.infer<typeof userUpdateSchema>;
export type UserRegister = z.infer<typeof userRegisterSchema>;