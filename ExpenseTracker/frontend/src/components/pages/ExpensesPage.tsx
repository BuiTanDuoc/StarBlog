import {
    Table,
    TableBody,
    TableCell,
    TableFooter,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { useEffect, useState } from "react";
import { deleteExpense, getPaginatedExpenses } from "@/services/api.expenses";
import { getUserBudgets, deleteBudget } from "@/services/api.budgets";
import type { Expense } from "@/schemas/expenses";
import type { Budget } from "@/schemas/budgets";
import { Button } from "@/components/ui/button";
import { Pencil, Trash2, Plus } from "lucide-react";
import { useNavigate } from "react-router";
import { toast } from "sonner";

const ExpensesPage = () => {
    const [expenses, setExpenses] = useState<Expense[]>([]);
    const [budgets, setBudgets] = useState<Budget[]>([]);
    const [total, setTotal] = useState(0);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();

    // Helper to format date
    const formatDate = (date: Date) => {
        return new Date(date).toLocaleDateString();
    };

    // Load both expenses and budgets
    const loadData = async () => {
        try {
            const [expensesData, budgetsData] = await Promise.all([
                getPaginatedExpenses(),
                getUserBudgets(),
            ]);

            const allExpenses = expensesData.data.map(e => ({ ...e, date: new Date(e.date) }));
            setExpenses(allExpenses);

            // Calculate total spent
            const sum = allExpenses.reduce((acc, expense) => acc + expense.amount, 0);
            setTotal(sum);

            // Update budgets with spent amounts
            const updatedBudgets = budgetsData.map(budget => {
                const spent = allExpenses
                    .filter(e => e.category?.id === budget.categoryId)
                    .filter(e => e.date >= new Date(budget.startDate) && e.date <= new Date(budget.endDate))
                    .reduce((sum, e) => sum + e.amount, 0);
                return { ...budget, spentAmount: spent };
            });

            setBudgets(updatedBudgets);
            setLoading(false);
        } catch (err) {
            console.error(err);
            toast.error("Failed to load data");
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData();
    }, []);

    const handleDeleteExpense = async (id: number) => {
        if (!confirm("Are you sure you want to delete this expense?")) return;

        try {
            await deleteExpense(id);
            toast.success("Expense deleted successfully");
            loadData(); // reload everything to update totals and budgets
        } catch (error) {
            console.error(error);
            toast.error("Failed to delete expense");
        }
    };

    const handleDeleteBudget = async (id: number) => {
        if (!confirm("Are you sure you want to delete this budget?")) return;

        try {
            await deleteBudget(id);
            toast.success("Budget deleted successfully");
            loadData(); // reload budgets after deletion
        } catch (error) {
            console.error(error);
            toast.error("Failed to delete budget");
        }
    };

    if (loading) {
        return <div className="p-8 text-center">Loading...</div>;
    }

    return (
        <div className="p-8 space-y-8">
            {/* ============= EXPENSES SECTION ============= */}
            <div>
                <div className="flex justify-between items-center mb-6">
                    <h1 className="text-2xl">My Expenses</h1>
                    <Button onClick={() => navigate("/expenses/new")}>
                        <Plus className="w-4 h-4 mr-2" />
                        Add Expense
                    </Button>
                </div>

                <Table>
                    {/*<TableCaption>A list of your recent expenses.</TableCaption>*/}
                    <TableHeader className="bg-gray-100">
                        <TableRow>
                            <TableHead className="w-[100px]">Title</TableHead>
                            <TableHead>Date</TableHead>
                            <TableHead>Category</TableHead>
                            <TableHead>Amount</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {expenses.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={5} className="text-center py-8 text-gray-500">
                                    No expenses found. Click "Add Expense" to create one.
                                </TableCell>
                            </TableRow>
                        ) : (
                            expenses.map((expense) => (
                                <TableRow key={expense.id}>
                                    <TableCell className="font-medium">{expense.title}</TableCell>
                                    <TableCell>{expense.date.toLocaleDateString()}</TableCell>
                                    <TableCell>{expense.category?.name || "-"}</TableCell>
                                    <TableCell>${expense.amount.toFixed(2)}</TableCell>
                                    <TableCell className="text-right space-x-2">
                                        <Button
                                            onClick={() => navigate(`/expenses/${expense.id}`)}
                                            variant="outline"
                                            size="sm"
                                        >
                                            <Pencil className="w-4 h-4" />
                                        </Button>
                                        <Button
                                            onClick={() => handleDeleteExpense(expense.id)}
                                            variant="destructive"
                                            size="sm"
                                        >
                                            <Trash2 className="w-4 h-4" />
                                        </Button>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                    <TableFooter>
                        <TableRow>
                            <TableCell colSpan={4}>Total</TableCell>
                            <TableCell className="text-right">${total.toFixed(2)}</TableCell>
                        </TableRow>
                    </TableFooter>
                </Table>
            </div>

            {/* ============= BUDGETS SECTION ============= */}
            <div>
                <div className="flex justify-between items-center mb-6">
                    <h2 className="text-2xl">My Budgets</h2>
                    <Button onClick={() => navigate("/budgets/new")} variant="outline">
                        <Plus className="w-4 h-4 mr-2" />
                        Add Budget
                    </Button>
                </div>

                <Table>
                    {/*<TableCaption>Your budget limits by category.</TableCaption>*/}
                    <TableHeader className="bg-gray-100">
                        <TableRow>
                            <TableHead>Category</TableHead>
                            <TableHead>Limit</TableHead>
                            <TableHead>Spent</TableHead>
                            <TableHead>Remaining</TableHead>
                            <TableHead>Period</TableHead>
                            <TableHead className="text-right">Actions</TableHead>
                        </TableRow>
                    </TableHeader>
                    <TableBody>
                        {budgets.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={6} className="text-center py-8 text-gray-500">
                                    No budgets found. Click "Add Budget" to create one.
                                </TableCell>
                            </TableRow>
                        ) : (
                            budgets.map((budget) => {
                                const spent = budget.spentAmount ?? 0;
                                const remaining = budget.limitAmount - spent;
                                const isOverBudget = spent > budget.limitAmount;
                                const percentage = (spent / budget.limitAmount) * 100;
                                const isNearLimit = percentage > 80 && !isOverBudget;

                                return (
                                    <TableRow
                                        key={budget.id}
                                        className={isOverBudget ? "bg-red-50" : isNearLimit ? "bg-yellow-50" : ""}
                                    >
                                        <TableCell className="font-medium">{budget.categoryName}</TableCell>
                                        <TableCell>${budget.limitAmount.toFixed(2)}</TableCell>
                                        <TableCell>
                                            <span className={isOverBudget ? "text-red-600 font-semibold" : ""}>
                                                ${spent.toFixed(2)}
                                            </span>
                                        </TableCell>
                                        <TableCell>
                                            <span className={
                                                isOverBudget
                                                    ? "text-red-600"
                                                    : isNearLimit
                                                        ? "text-yellow-600"
                                                        : "text-green-600"
                                            }>
                                                ${remaining.toFixed(2)}
                                            </span>
                                        </TableCell>
                                        <TableCell className="text-sm text-gray-600">
                                            {formatDate(budget.startDate)} - {formatDate(budget.endDate)}
                                        </TableCell>
                                        <TableCell className="text-right">
                                            <Button
                                                onClick={() => handleDeleteBudget(budget.id)}
                                                variant="destructive"
                                                size="sm"
                                            >
                                                <Trash2 className="w-4 h-4" />
                                            </Button>
                                        </TableCell>
                                    </TableRow>
                                );
                            })
                        )}
                    </TableBody>
                </Table>
            </div>
        </div>
    );
};

export default ExpensesPage;
