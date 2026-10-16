import { Input } from "@/components/ui/input";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { budgetInsertSchema, type BudgetInsert } from "@/schemas/budgets";
import type { ExpenseCategory } from "@/schemas/expenses";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { useState, useEffect } from "react";
import { createBudget } from "@/services/api.budgets";
import { getPaginatedExpenses } from "@/services/api.expenses";
import { useNavigate } from "react-router";
import { toast } from "sonner";

const BudgetPage = () => {
    const navigate = useNavigate();
    const [categories, setCategories] = useState<ExpenseCategory[]>([]);
    const [loading, setLoading] = useState(true);

    const today = new Date();
    const nextMonth = new Date();
    nextMonth.setMonth(nextMonth.getMonth() + 1);

    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting },
    } = useForm<BudgetInsert>({
        resolver: zodResolver(budgetInsertSchema),
        defaultValues: {
            categoryId: 0,
            limitAmount: 0,
            startDate: today.toISOString().split('T')[0] as unknown as Date,
            endDate: nextMonth.toISOString().split('T')[0] as unknown as Date,
        },
    });

    // Fetch categories from user's expenses
    useEffect(() => {
        getPaginatedExpenses()
            .then(({ data }) => {
                // Extract unique categories from expenses
                const uniqueCategories = data
                    .map(expense => expense.category)
                    .filter((cat): cat is ExpenseCategory => cat !== null && cat !== undefined)
                    .filter((cat, index, self) =>
                        // Remove duplicates by ID
                        index === self.findIndex(c => c.id === cat.id)
                    );

                setCategories(uniqueCategories);
                setLoading(false);
            })
            .catch((err) => {
                console.error("Error loading categories:", err);
                toast.error("Failed to load categories");
                setLoading(false);
            });
    }, []);

    const onSubmit = async (data: BudgetInsert) => {
        try {
            await createBudget(data);
            toast.success("Budget created successfully");
            navigate("/expenses");
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Something went wrong");
        }
    };

    if (loading) {
        return <div className="p-8 text-center">Loading...</div>;
    }

    return (
        <div className="p-8">
            <h1 className="text-2xl text-center mb-6">Create New Budget</h1>
            <form
                onSubmit={handleSubmit(onSubmit)}
                className="max-w-sm mx-auto p-8 border rounded-md space-y-4"
                autoComplete="off"
            >
                <div>
                    <Label htmlFor="categoryId">Category</Label>
                    {categories.length === 0 ? (
                        <div className="text-sm text-gray-600 p-3 bg-gray-50 rounded border">
                            No categories found. Create an expense with a category first!
                        </div>
                    ) : (
                        <select
                            id="categoryId"
                            {...register("categoryId", { valueAsNumber: true })}
                            className="w-full px-3 py-2 border rounded-md"
                        >
                            <option value={0}>Select a category...</option>
                            {categories.map((cat) => (
                                <option key={cat.id} value={cat.id}>
                                    {cat.name}
                                </option>
                            ))}
                        </select>
                    )}
                    {errors.categoryId && (
                        <div className="text-red-600 text-sm">{errors.categoryId.message}</div>
                    )}
                </div>

                <div>
                    <Label htmlFor="limitAmount">Budget Limit ($)</Label>
                    <Input
                        id="limitAmount"
                        type="number"
                        step="0.01"
                        {...register("limitAmount", { valueAsNumber: true })}
                    />
                    {errors.limitAmount && (
                        <div className="text-red-600 text-sm">{errors.limitAmount.message}</div>
                    )}
                </div>

                <div>
                    <Label htmlFor="startDate">Start Date</Label>
                    <Input
                        id="startDate"
                        type="date"
                        {...register("startDate", {
                            setValueAs: (value) => new Date(value),
                        })}
                    />
                    {errors.startDate && (
                        <div className="text-red-600 text-sm">{errors.startDate.message}</div>
                    )}
                </div>

                <div>
                    <Label htmlFor="endDate">End Date</Label>
                    <Input
                        id="endDate"
                        type="date"
                        {...register("endDate", {
                            setValueAs: (value) => new Date(value),
                        })}
                    />
                    {errors.endDate && (
                        <div className="text-red-600 text-sm">{errors.endDate.message}</div>
                    )}
                </div>

                <div className="flex gap-2 pt-4">
                    <Button
                        type="button"
                        variant="outline"
                        onClick={() => navigate("/expenses")}
                        className="flex-1"
                    >
                        Cancel
                    </Button>
                    <Button
                        disabled={isSubmitting || categories.length === 0}
                        className="flex-1"
                    >
                        {isSubmitting ? "Creating..." : "Create Budget"}
                    </Button>
                </div>
            </form>
        </div>
    );
};

export default BudgetPage;