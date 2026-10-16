import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router";
import { toast } from "sonner";
import { getProjectTasks, deleteTask, searchTasks } from "@/services/api.tasks";
import type { Task } from "@/schemas/tasks";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

const TasksPage = () => {
    const { projectId } = useParams();
    const [tasks, setTasks] = useState<Task[]>([]);
    const [loading, setLoading] = useState(false);
    const [searchQuery, setSearchQuery] = useState("");
    const navigate = useNavigate();

    useEffect(() => {
        document.title = "TaskFlow - Tasks";
        loadTasks();
    }, [projectId]);

    const loadTasks = async () => {
        if (!projectId) return;
        try {
            setLoading(true);
            const data = await getProjectTasks(Number(projectId));
            setTasks(data);
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Failed to load tasks");
        } finally {
            setLoading(false);
        }
    };

    const handleSearch = async () => {
        if (!searchQuery.trim() || !projectId) return;
        try {
            const results = await searchTasks(Number(projectId), searchQuery);
            setTasks(results);
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Search failed");
        }
    };

    const handleDelete = async (taskId: number) => {
        if (!window.confirm("Delete this task?") || !projectId) return;
        try {
            await deleteTask(Number(projectId), taskId);
            setTasks(tasks.filter((t) => t.id !== taskId));
            toast.success("Task deleted");
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Failed to delete task");
        }
    };

    const getPriorityColor = (priority: string) => {
        const colors: Record<string, string> = {
            Critical: "text-red-600 bg-red-100",
            High: "text-orange-600 bg-orange-100",
            Medium: "text-yellow-600 bg-yellow-100",
            Low: "text-blue-600 bg-blue-100",
        };
        return colors[priority] || colors.Medium;
    };

    const getStatusColor = (status: string) => {
        const colors: Record<string, string> = {
            Done: "text-green-600 bg-green-100",
            Review: "text-purple-600 bg-purple-100",
            InProgress: "text-blue-600 bg-blue-100",
            Backlog: "text-gray-600 bg-gray-100",
        };
        return colors[status] || colors.Backlog;
    };

    return (
        <div className="py-8">
            <div className="flex justify-between items-center mb-8">
                <h1 className="text-3xl font-bold text-gray-800">Tasks</h1>
                <Button
                    onClick={() => navigate(`/projects/${projectId}/tasks/new`)}
                    className="bg-blue-600 hover:bg-blue-700 text-white"
                >
                    + New Task
                </Button>
            </div>

            {/* Search Bar */}
            <div className="flex gap-2 mb-8">
                <Input
                    placeholder="Search tasks..."
                    value={searchQuery}
                    onChange={(e) => setSearchQuery(e.target.value)}
                    onKeyPress={(e) => e.key === "Enter" && handleSearch()}
                    className="flex-1 border-gray-300"
                />
                <Button
                    onClick={handleSearch}
                    className="bg-blue-600 hover:bg-blue-700 text-white"
                >
                    Search
                </Button>
                <Button
                    onClick={loadTasks}
                    variant="outline"
                >
                    Reset
                </Button>
            </div>

            {loading && (
                <div className="flex items-center justify-center py-12">
                    <div className="text-center">
                        <div className="w-12 h-12 rounded-full border-4 border-blue-600 border-t-transparent animate-spin mx-auto mb-4"></div>
                        <p className="text-gray-600">Loading tasks...</p>
                    </div>
                </div>
            )}

            {!loading && tasks.length === 0 && (
                <div className="text-center py-12 bg-gray-50 rounded-lg border border-gray-200">
                    <p className="text-gray-600 text-lg">No tasks yet. Create one to get started!</p>
                </div>
            )}

            {!loading && tasks.length > 0 && (
                <div className="overflow-x-auto border border-gray-300 rounded-lg shadow">
                    <table className="w-full">
                        <thead className="bg-gray-100 border-b border-gray-300">
                        <tr>
                            <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Title</th>
                            <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Status</th>
                            <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Priority</th>
                            <th className="px-6 py-3 text-left text-sm font-semibold text-gray-700">Assigned To</th>
                            <th className="px-6 py-3 text-center text-sm font-semibold text-gray-700">Actions</th>
                        </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-200">
                        {tasks.map((task) => (
                            <tr key={task.id} className="hover:bg-gray-50 transition">
                                <td className="px-6 py-3 text-sm font-medium text-gray-900">{task.title}</td>
                                <td className="px-6 py-3 text-sm">
                                        <span className={`px-3 py-1 rounded-full text-xs font-medium ${getStatusColor(task.status)}`}>
                                            {task.status}
                                        </span>
                                </td>
                                <td className="px-6 py-3 text-sm">
                                        <span className={`px-3 py-1 rounded-full text-xs font-medium ${getPriorityColor(task.priority)}`}>
                                            {task.priority}
                                        </span>
                                </td>
                                <td className="px-6 py-3 text-sm text-gray-600">
                                    {task.assignedToUsername || "Unassigned"}
                                </td>
                                <td className="px-6 py-3 text-center space-x-2">
                                    <Button
                                        onClick={() => navigate(`/projects/${projectId}/tasks/${task.id}`)}
                                        size="sm"
                                        className="bg-blue-500 hover:bg-blue-600 text-white"
                                    >
                                        View
                                    </Button>
                                    <Button
                                        onClick={() => handleDelete(task.id)}
                                        size="sm"
                                        variant="destructive"
                                    >
                                        Delete
                                    </Button>
                                </td>
                            </tr>
                        ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
};

export default TasksPage;