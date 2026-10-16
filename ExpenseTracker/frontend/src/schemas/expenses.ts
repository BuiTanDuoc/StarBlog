import { z } from "zod";

// Nested category schema (matches ExpenseCategoryReadOnlyDTO)
export const expenseCategorySchema = z.object({
    id: z.coerce.number().int(),
    name: z.string().min(1, { message: "Category name is required" }),
});

//  ExpenseReadOnlyDTO
export const expenseSchema = z.object({
    id: z.coerce.number().int(),
    title: z.string().min(1, { message: "Title is required" }),
    amount: z.coerce.number().min(0, { message: "Amount must be non-negative" }),
    date: z.coerce.date(),
    category: expenseCategorySchema.optional(), // nullable
});

// Insert/Update schema (ExpenseInsertDTO)
export const expenseInsertSchema = z.object({
    title: z.string().min(1, { message: "Title is required" }),
    amount: z.coerce.number().min(0, { message: "Amount must be non-negative" }),
    date: z.coerce.date(),
    categoryName: z.string().min(1, { message: "Category name is required" }),
});



export type Expense = z.infer<typeof expenseSchema>;
export type ExpenseInsert = z.infer<typeof expenseInsertSchema>;
export type ExpenseCategory = z.infer<typeof expenseCategorySchema>;
