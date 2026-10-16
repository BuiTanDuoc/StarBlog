import { BrowserRouter, Routes, Route } from "react-router";
import Layout from "@/components/layout/Layout.tsx";
import HomePage from "@/components/pages/HomePage.tsx";
import ExpensesPage from "@/components/pages/ExpensesPage.tsx";
import ExpensePage from "@/components/pages/ExpensePage.tsx";
import BudgetPage from "@/components/pages/Budgets/BudgetPage.tsx";  // ← Add this
import UsersPage from "@/components/pages/UsersPage.tsx";
import UserPage from "@/components/pages/UserPage.tsx";
import LoginPage from "@/components/pages/LoginPage.tsx";
import RegisterPage from "@/components/pages/RegisterPage.tsx";
import UnauthorizedPage from "@/components/pages/UnauthorizedPage.tsx";
import ProtectedRoute from "@/components/ProtectedRoute.tsx";
import { AuthProvider } from "@/context/AuthProvider.tsx";

function App() {
    return (
        <AuthProvider>
            <BrowserRouter>
                <Routes>
                    <Route element={<Layout />}>
                        <Route index element={<HomePage />} />
                        <Route path="login" element={<LoginPage />} />
                        <Route path="register" element={<RegisterPage />} />
                        <Route path="unauthorized" element={<UnauthorizedPage />} />

                        {/* Protected routes - require authentication */}
                        <Route element={<ProtectedRoute />}>
                            <Route path="expenses">
                                <Route index element={<ExpensesPage />} />
                                <Route path=":expenseId" element={<ExpensePage />} />
                                <Route path="new" element={<ExpensePage />} />
                            </Route>

                            {/* Budget routes */}
                            <Route path="budgets">
                                <Route path="new" element={<BudgetPage />} />
                            </Route>
                        </Route>

                        {/* Admin-only routes */}
                        <Route element={<ProtectedRoute requireAdmin />}>
                            <Route path="users">
                                <Route index element={<UsersPage />} />
                                <Route path=":userId" element={<UserPage />} />
                                <Route path="new" element={<UserPage />} />
                            </Route>
                        </Route>
                    </Route>
                </Routes>
            </BrowserRouter>
        </AuthProvider>
    );
}

export default App;