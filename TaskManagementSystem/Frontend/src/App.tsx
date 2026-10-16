import { BrowserRouter, Routes, Route } from "react-router";
import Layout from "@/components/layout/Layout";
import HomePage from "@/components/pages/HomePage";
import LoginPage from "@/components/pages/LoginPage";
import RegisterPage from "@/components/pages/RegisterPage";
import ProjectsPage from "@/components/pages/ProjectsPage";
import ProjectPage from "@/components/pages/ProjectPage";
import TasksPage from "@/components/pages/TasksPage";
import TaskPage from "@/components/pages/TaskPage";
import ProjectMembersPage from "@/components/pages/ProjectMembersPage";
import ProtectedRoute from "@/components/ProtectedRoute";
import { AuthProvider } from "@/context/AuthProvider";
import { Toaster } from "sonner";

function App() {
    return (
        <>
            <AuthProvider>
                <BrowserRouter>
                    <Routes>
                        <Route element={<Layout />}>
                            <Route index element={<HomePage />} />
                            <Route path="login" element={<LoginPage />} />
                            <Route path="register" element={<RegisterPage />} />

                            {/* Protected Routes */}
                            <Route element={<ProtectedRoute />}>
                                <Route path="projects" element={<ProjectsPage />} />
                                <Route path="projects/new" element={<ProjectPage />} />
                                <Route path="projects/:projectId" element={<ProjectPage />} />
                                <Route path="projects/:projectId/tasks" element={<TasksPage />} />
                                <Route path="projects/:projectId/tasks/new" element={<TaskPage />} />
                                <Route path="projects/:projectId/tasks/:taskId" element={<TaskPage />} />
                                <Route path="projects/:projectId/members" element={<ProjectMembersPage />} />
                            </Route>
                        </Route>
                    </Routes>
                </BrowserRouter>
                <Toaster richColors />
            </AuthProvider>
        </>
    );
}

export default App;