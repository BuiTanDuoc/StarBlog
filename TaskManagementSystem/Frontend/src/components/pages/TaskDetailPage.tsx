import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router";
import { toast } from "sonner";
import { getTask, addComment, deleteComment, updateTaskStatus } from "@/services/api.tasks";
import type { Task, TaskCommentReadOnlyDTO } from "@/schemas/tasks";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useAuth } from "@/hooks/useAuth";
import { Trash2, Send } from "lucide-react";

const TaskDetailPage = () => {
    const { projectId, taskId } = useParams();
    const { username } = useAuth();
    const [task, setTask] = useState<Task | null>(null);
    const [loading, setLoading] = useState(false);
    const [commentText, setCommentText] = useState("");
    const [submittingComment, setSubmittingComment] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        if (!projectId || !taskId) return;
        loadTask();
    }, [projectId, taskId]);

    const loadTask = async () => {
        if (!projectId || !taskId) return;
        try {
            setLoading(true);
            const data = await getTask(Number(projectId), Number(taskId));
            console.log("Loaded task:", data); // 🔍 Debug
            console.log("Comments:", data.comments); // 🔍 Debug
            setTask(data);
        } catch (error) {
            console.error("Load task error:", error); // 🔍 Debug
            toast.error(error instanceof Error ? error.message : "Failed to load task");
        } finally {
            setLoading(false);
        }
    };

    const handleAddComment = async () => {
        if (!projectId || !taskId || !commentText.trim()) return;
        try {
            setSubmittingComment(true);
            await addComment(Number(projectId), Number(taskId), { content: commentText });
            setCommentText("");
            await loadTask();
            toast.success("Comment added");
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Failed to add comment");
        } finally {
            setSubmittingComment(false);
        }
    };

    const handleDeleteComment = async (commentId: number) => {
        if (!projectId || !taskId || !window.confirm("Delete this comment?")) return;
        try {
            await deleteComment(Number(projectId), Number(taskId), commentId);
            await loadTask();
            toast.success("Comment deleted");
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Failed to delete comment");
        }
    };

    const handleStatusChange = async (newStatus: string) => {
        if (!projectId || !taskId) return;
        try {
            await updateTaskStatus(Number(projectId), Number(taskId), newStatus);
            await loadTask();
            toast.success("Status updated");
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Failed to update status");
        }
    };

    if (loading) {
        return (
            <div className="flex items-center justify-center min-h-screen">
                <div className="text-center">
                    <div className="w-12 h-12 rounded-full border-4 border-blue-600 border-t-transparent animate-spin mx-auto mb-4"></div>
                    <p className="text-gray-600 dark:text-gray-300">Loading task...</p>
                </div>
            </div>
        );
    }

    if (!task) {
        return (
            <div className="text-center py-12">
                <p className="text-gray-600 dark:text-gray-300">Task not found</p>
                <Button
                    onClick={() => navigate(`/projects/${projectId}/tasks`)}
                    className="mt-4 bg-blue-600 hover:bg-blue-700 text-white"
                >
                    Back to Tasks
                </Button>
            </div>
        );
    }

    const getPriorityColor = (priority: string) => {
        const colors: Record<string, string> = {
            Critical: "bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-200",
            High: "bg-orange-100 text-orange-700 dark:bg-orange-900 dark:text-orange-200",
            Medium: "bg-yellow-100 text-yellow-700 dark:bg-yellow-900 dark:text-yellow-200",
            Low: "bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-200",
        };
        return colors[priority] || colors.Medium;
    };

    const getStatusColor = (status: string) => {
        const colors: Record<string, string> = {
            Done: "bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-200",
            Review: "bg-purple-100 text-purple-700 dark:bg-purple-900 dark:text-purple-200",
            InProgress: "bg-blue-100 text-blue-700 dark:bg-blue-900 dark:text-blue-200",
            Backlog: "bg-gray-100 text-gray-700 dark:bg-gray-800 dark:text-gray-200",
        };
        return colors[status] || colors.Backlog;
    };

    return (
        <div className="py-8 container mx-auto px-4">
            <Button
                onClick={() => navigate(`/projects/${projectId}/tasks`)}
                variant="outline"
                className="mb-6"
            >
                ← Back to Tasks
            </Button>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Main Content */}
                <div className="lg:col-span-2 space-y-6">
                    {/* Task Header */}
                    <div className="bg-white dark:bg-slate-800 rounded-lg shadow-lg p-8 border border-gray-200 dark:border-slate-700">
                        <h1 className="text-3xl font-bold text-gray-800 dark:text-white mb-4">{task.title}</h1>
                        <p className="text-gray-600 dark:text-gray-300 mb-6 whitespace-pre-wrap">{task.description || "No description"}</p>

                        <div className="flex gap-4 mb-6 flex-wrap">
                            <select
                                value={task.status}
                                onChange={(e) => handleStatusChange(e.target.value)}
                                className={`px-4 py-2 border rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 font-medium ${getStatusColor(task.status)} cursor-pointer`}
                            >
                                <option value="Backlog">Backlog</option>
                                <option value="InProgress">In Progress</option>
                                <option value="Review">Review</option>
                                <option value="Done">Done</option>
                            </select>

                            <Button
                                onClick={() => navigate(`/projects/${projectId}/tasks/${taskId}/edit`)}
                                className="bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 text-white"
                            >
                                Edit Task
                            </Button>
                        </div>
                    </div>

                    {/* Comments Section */}
                    <div className="bg-white dark:bg-slate-800 rounded-lg shadow-lg p-8 border border-gray-200 dark:border-slate-700">
                        <h2 className="text-2xl font-bold text-gray-800 dark:text-white mb-6">
                            💬 Comments ({task.comments?.length || 0})
                        </h2>

                        {/* Add Comment */}
                        <div className="mb-8 pb-6 border-b border-gray-200 dark:border-slate-700">
                            <div className="flex gap-2">
                                <Input
                                    placeholder="Add a comment..."
                                    value={commentText}
                                    onChange={(e) => setCommentText(e.target.value)}
                                    onKeyPress={(e) => e.key === "Enter" && handleAddComment()}
                                    className="flex-1 border-gray-300 dark:bg-slate-700 dark:border-slate-600 dark:text-white"
                                    disabled={submittingComment}
                                />
                                <Button
                                    onClick={handleAddComment}
                                    disabled={submittingComment || !commentText.trim()}
                                    className="bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 text-white flex gap-2"
                                >
                                    <Send size={16} />
                                    {submittingComment ? "..." : "Post"}
                                </Button>
                            </div>
                        </div>

                        {/* Comments List */}
                        <div className="space-y-4">
                            {!task.comments || task.comments.length === 0 ? (
                                <div className="text-center py-8 bg-gray-50 dark:bg-slate-700 rounded-lg">
                                    <p className="text-gray-500 dark:text-gray-400">No comments yet. Be the first to comment!</p>
                                </div>
                            ) : (
                                task.comments.map((comment: TaskCommentReadOnlyDTO) => (
                                    <div key={comment.id} className="bg-gray-50 dark:bg-slate-700 rounded-lg p-4 border border-gray-200 dark:border-slate-600">
                                        <div className="flex justify-between items-start mb-3">
                                            <div className="flex-1">
                                                <p className="font-semibold text-gray-800 dark:text-white">
                                                    {comment.username}
                                                    {comment.username === username && (
                                                        <span className="ml-2 text-xs bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-200 px-2 py-1 rounded">
                                                            You
                                                        </span>
                                                    )}
                                                </p>
                                                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                                                    {new Date(comment.createdDate).toLocaleString()}
                                                </p>
                                            </div>
                                            {comment.username === username && (
                                                <Button
                                                    onClick={() => handleDeleteComment(comment.id)}
                                                    size="sm"
                                                    variant="destructive"
                                                    className="flex gap-1"
                                                >
                                                    <Trash2 size={14} />
                                                    Delete
                                                </Button>
                                            )}
                                        </div>
                                        <p className="text-gray-700 dark:text-gray-300 whitespace-pre-wrap break-words">
                                            {comment.content}
                                        </p>
                                    </div>
                                ))
                            )}
                        </div>
                    </div>
                </div>

                {/* Sidebar - Task Details */}
                <div className="space-y-6">
                    <div className="bg-white dark:bg-slate-800 rounded-lg shadow p-6 border border-gray-200 dark:border-slate-700">
                        <h3 className="font-semibold text-gray-800 dark:text-white mb-4">Task Details</h3>
                        <div className="space-y-4 text-sm">
                            <div>
                                <p className="text-gray-600 dark:text-gray-400 font-medium">Status</p>
                                <p className={`mt-1 px-3 py-1 rounded-full inline-block ${getStatusColor(task.status)}`}>
                                    {task.status}
                                </p>
                            </div>
                            <div>
                                <p className="text-gray-600 dark:text-gray-400 font-medium">Priority</p>
                                <p className={`mt-1 px-3 py-1 rounded-full inline-block ${getPriorityColor(task.priority)}`}>
                                    {task.priority}
                                </p>
                            </div>
                            <div>
                                <p className="text-gray-600 dark:text-gray-400 font-medium">Assigned To</p>
                                <p className="text-gray-800 dark:text-white mt-1">
                                    {task.assignedToUsername || "Unassigned"}
                                </p>
                            </div>
                            {task.dueDate && (
                                <div>
                                    <p className="text-gray-600 dark:text-gray-400 font-medium">Due Date</p>
                                    <p className="text-gray-800 dark:text-white mt-1">
                                        {new Date(task.dueDate).toLocaleDateString()}
                                    </p>
                                </div>
                            )}
                            <div>
                                <p className="text-gray-600 dark:text-gray-400 font-medium">Created</p>
                                <p className="text-gray-800 dark:text-white text-xs mt-1">
                                    {new Date(task.createdDate).toLocaleString()}
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default TaskDetailPage;