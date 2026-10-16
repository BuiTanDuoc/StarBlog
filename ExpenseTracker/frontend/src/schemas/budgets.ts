import { z } from "zod";

//  BudgetReadOnlyDTO
export const budgetSchema = z.object({
    id: z.coerce.number().int(),
    userId: z.coerce.number().int(),
    categoryId: z.coerce.number().int(),
    categoryName: z.string(),
    limitAmount: z.coerce.number().min(0, { message: "Limit must be non-negative" }),
    spentAmount: z.coerce.number().nullable(),
    startDate: z.coerce.date(),
    endDate: z.coerce.date(),
});

// Matches BudgetInsertDTO
export const budgetInsertSchema = z.object({
    categoryId: z.coerce.number().int().min(1, { message: "Category is required" }),
    limitAmount: z.coerce.number().min(0.01, { message: "Limit must be greater than 0" }),
    startDate: z.coerce.date(),
    endDate: z.coerce.date(),
}).refine((data) => data.endDate > data.startDate, {
    message: "End date must be after start date",
    path: ["endDate"],
});

export type Budget = z.infer<typeof budgetSchema>;
export type BudgetInsert = z.infer<typeof budgetInsertSchema>;