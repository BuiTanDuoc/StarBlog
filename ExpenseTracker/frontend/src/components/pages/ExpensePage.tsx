import { Input } from "@/components/ui/input";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { expenseInsertSchema, type ExpenseInsert } from "@/schemas/expenses";
import type { ExpenseCategory } from "@/schemas/expenses";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { useEffect, useState } from "react";
import { createExpense, getExpense, updateExpense, getPaginatedExpenses } from "@/services/api.expenses";
import { useNavigate, useParams } from "react-router";
import { toast } from "sonner";

const ExpensePage = () => {
    const { expenseId } = useParams();
    const isEdit = Boolean(expenseId);
    const navigate = useNavigate();
    const [categories, setCategories] = useState<ExpenseCategory[]>([]);
    const [useExisting, setUseExisting] = useState(true); // Toggle between dropdown and text input

    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting },
        reset,
        setValue,
        watch,
    } = useForm<ExpenseInsert>({
        resolver: zodResolver(expenseInsertSchema),
        defaultValues: {
            title: "",
            amount: 0,
            date: new Date().toISOString().split('T')[0] as unknown as Date,
            categoryName: "",
        },
    });

    // Watch categoryName to see if user typed something
    const categoryName = watch("categoryName");

    // Fetch existing categories on mount
    useEffect(() => {
        getPaginatedExpenses()
            .then(({ data }) => {
                const uniqueCategories = data
                    .map(expense => expense.category)
                    .filter((cat): cat is ExpenseCategory => cat !== null && cat !== undefined)
                    .filter((cat, index, self) =>
                        index === self.findIndex(c => c.id === cat.id)
                    );
                setCategories(uniqueCategories);
            })
            .catch((err) => {
                console.error("Error loading categories:", err);
            });
    }, []);

    // Load expense data for editing
    useEffect(() => {
        if (!isEdit || !expenseId) return;
        getExpense(Number(expenseId))
            .then((data) => {
                reset({
                    title: data.title,
                    amount: data.amount,
                    date: new Date(data.date).toISOString().split('T')[0] as unknown as Date,
                    categoryName: data.category?.name ?? "",
                });
            })
            .catch((err) => {
                console.error("Error getting expense:", err);
                toast.error("Failed to load expense");
            });
    }, [isEdit, expenseId, reset]);

    const onSubmit = async (data: ExpenseInsert) => {
        try {
            if (isEdit && expenseId) {
                await updateExpense(Number(expenseId), data);
                toast.success("Expense updated successfully");
            } else {
                await createExpense(data);
                toast.success("Expense created successfully");
            }
            navigate("/expenses");
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Something went wrong");
        }
    };

    return (
        <div className="p-8">
            <h1 className="text-2xl text-center mb-6">
                {isEdit ? "Edit Expense" : "Create New Expense"}
            </h1>
            <form
                onSubmit={handleSubmit(onSubmit)}
                className="max-w-sm mx-auto p-8 border rounded-md space-y-4"
                autoComplete="off"
            >
                <div>
                    <Label htmlFor="title">Title</Label>
                    <Input id="title" {...register("title")} />
                    {errors.title && (
                        <div className="text-red-600 text-sm">{errors.title.message}</div>
                    )}
                </div>

                <div>
                    <Label htmlFor="amount">Amount</Label>
                    <Input
                        id="amount"
                        type="number"
                        step="0.01"
                        {...register("amount", { valueAsNumber: true })}
                    />
                    {errors.amount && (
                        <div className="text-red-600 text-sm">{errors.amount.message}</div>
                    )}
                </div>

                <div>
                    <Label htmlFor="date">Date</Label>
                    <Input
                        id="date"
                        type="date"
                        {...register("date", {
                            setValueAs: (value) => new Date(value),
                        })}
                    />
                    {errors.date && (
                        <div className="text-red-600 text-sm">{errors.date.message}</div>
                    )}
                </div>

                {/* Category Section with Toggle */}
                <div>
                    <div className="flex justify-between items-center mb-2">
                        <Label htmlFor="categoryName">Category</Label>
                        {categories.length > 0 && (
                            <button
                                type="button"
                                onClick={() => setUseExisting(!useExisting)}
                                className="text-xs text-blue-600 hover:underline"
                            >
                                {useExisting ? "Create new category" : "Use existing category"}
                            </button>
                        )}
                    </div>

                    {useExisting && categories.length > 0 ? (
                        // Dropdown for existing categories
                        <select
                            id="categoryName"
                            value={categoryName}
                            onChange={(e) => setValue("categoryName", e.target.value)}
                            className="w-full px-3 py-2 border rounded-md"
                        >
                            <option value="">Select a category...</option>
                            {categories.map((cat) => (
                                <option key={cat.id} value={cat.name}>
                                    {cat.name}
                                </option>
                            ))}
                        </select>
                    ) : (
                        // Text input for new category
                        <Input
                            id="categoryName"
                            {...register("categoryName")}
                            placeholder="Enter new category name"
                        />
                    )}

                    {errors.categoryName && (
                        <div className="text-red-600 text-sm">
                            {errors.categoryName.message}
                        </div>
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
                    <Button disabled={isSubmitting} className="flex-1">
                        {isSubmitting ? "Submitting..." : isEdit ? "Update" : "Create"}
                    </Button>
                </div>
            </form>
        </div>
    );
};

export default ExpensePage;