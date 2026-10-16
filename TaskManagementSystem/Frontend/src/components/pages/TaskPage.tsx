import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { taskInsertSchema, taskUpdateSchema, type TaskInsert, type TaskUpdate } from "@/schemas/tasks";
import { useEffect, useState } from "react";
import { createTask, getTask, updateTask } from "@/services/api.tasks";
import { getProjectMembers } from "@/services/api.projects";
import { useNavigate, useParams } from "react-router";
import { toast } from "sonner";
import type { ProjectMember } from "@/schemas/projects";

const TaskPage = () => {
    const { projectId, taskId } = useParams();
    const isEdit = Boolean(taskId);
    const navigate = useNavigate();
    const [members, setMembers] = useState<ProjectMember[]>([]);

    const schema = isEdit ? taskUpdateSchema : taskInsertSchema;

    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting },
        reset,
    } = useForm<TaskInsert | TaskUpdate>({
        resolver: zodResolver(schema),
        defaultValues: {
            title: "",
            description: "",
            status: "Backlog" as const,
            priority: "Medium" as const,
            dueDate: "",
            assignedToUserId: undefined,
        },
    });

    const getErrorMessage = (fieldName: string): string | undefined => {
        const error = errors[fieldName as keyof typeof errors];
        if (error && typeof error === "object" && "message" in error) {
            return error.message as string;
        }
        return undefined;
    };

    // Load project members
    useEffect(() => {
        if (!projectId) return;

        const loadMembers = async () => {
            try {
                const data = await getProjectMembers(Number(projectId));
                setMembers(data);

                if (data.length === 0) {
                    toast.warning("No team members in this project. Add members to assign tasks.");
                }
            } catch (err) {
                console.error("Failed to load members:", err);
                toast.error(err instanceof Error ? err.message : "Failed to load team members");
            }
        };

        loadMembers();
    }, [projectId]);

    // Load task if editing
    useEffect(() => {
        if (!isEdit || !taskId || !projectId) return;
        getTask(Number(projectId), Number(taskId))
            .then((data) => {
                reset({
                    title: data.title,
                    description: data.description,
                    status: data.status as "Backlog" | "InProgress" | "Review" | "Done",
                    priority: data.priority as "Low" | "Medium" | "High" | "Critical",
                    dueDate: data.dueDate ? data.dueDate.split("T")[0] : "",
                    assignedToUserId: data.assignedToUserId || undefined,
                } as TaskInsert | TaskUpdate);
            })
            .catch((err) => {
                toast.error(err instanceof Error ? err.message : "Failed to load task");
            });
    }, [isEdit, taskId, projectId, reset]);

    const onSubmit = async (data: TaskInsert | TaskUpdate) => {
        if (!projectId) return;
        try {
            console.log("Form data:", data); // 🔍 Debug

            // ✅ Handle date conversion
            const dueDate = (data as any).dueDate
                ? new Date((data as any).dueDate).toISOString()
                : null;

            // ✅ Handle user assignment - must be number or null
            const assignedToUserId = (data as any).assignedToUserId
                ? Number((data as any).assignedToUserId)
                : null;

            const payload: TaskInsert | TaskUpdate = {
                ...data,
                dueDate,
                assignedToUserId,
            } as any;

            console.log("Payload:", payload); // 🔍 Debug

            if (isEdit && taskId) {
                await updateTask(Number(projectId), Number(taskId), payload as TaskUpdate);
                toast.success("Task updated successfully");
            } else {
                await createTask(Number(projectId), payload as TaskInsert);
                toast.success("Task created successfully");
            }
            navigate(`/projects/${projectId}/tasks`);
        } catch (error) {
            console.error("Task submission error:", error);  // 🔍 Debug
            toast.error(error instanceof Error ? error.message : "Something went wrong");
        }
    };

    useEffect(() => {
        document.title = isEdit ? "TaskFlow - Edit Task" : "TaskFlow - New Task";
    }, [isEdit]);

    return (
        <div className="max-w-2xl mx-auto py-8 container px-4">
            <div className="bg-white rounded-lg shadow-lg p-8 border border-gray-200">
                <h1 className="text-3xl font-bold mb-8 text-gray-800">
                    {isEdit ? "Edit Task" : "Create New Task"}
                </h1>

                <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
                    <div>
                        <Label htmlFor="title" className="text-gray-700 font-medium">
                            Task Title *
                        </Label>
                        <Input
                            id="title"
                            placeholder="e.g., Design Database Schema"
                            {...register("title")}
                            className="mt-2 border-gray-300"
                        />
                        {getErrorMessage("title") && (
                            <p className="text-red-600 text-sm mt-1">{getErrorMessage("title")}</p>
                        )}
                    </div>

                    <div>
                        <Label htmlFor="description" className="text-gray-700 font-medium">
                            Description
                        </Label>
                        <textarea
                            id="description"
                            placeholder="Task description..."
                            {...register("description")}
                            className="mt-2 w-full border border-gray-300 rounded-md px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                            rows={4}
                        />
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <Label htmlFor="status" className="text-gray-700 font-medium">
                                Status *
                            </Label>
                            <select
                                id="status"
                                {...register("status")}
                                className="mt-2 w-full border border-gray-300 rounded-md px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                            >
                                <option value="Backlog">Backlog</option>
                                <option value="InProgress">In Progress</option>
                                <option value="Review">Review</option>
                                <option value="Done">Done</option>
                            </select>
                        </div>

                        <div>
                            <Label htmlFor="priority" className="text-gray-700 font-medium">
                                Priority *
                            </Label>
                            <select
                                id="priority"
                                {...register("priority")}
                                className="mt-2 w-full border border-gray-300 rounded-md px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                            >
                                <option value="Low">Low</option>
                                <option value="Medium">Medium</option>
                                <option value="High">High</option>
                                <option value="Critical">Critical</option>
                            </select>
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <div>
                            <Label htmlFor="dueDate" className="text-gray-700 font-medium">
                                Due Date
                            </Label>
                            <Input
                                id="dueDate"
                                type="date"
                                {...register("dueDate")}
                                className="mt-2 border-gray-300"
                            />
                        </div>

                        <div>
                            <Label htmlFor="assignedToUserId" className="text-gray-700 font-medium">
                                Assign To
                            </Label>
                            <select
                                id="assignedToUserId"
                                {...register("assignedToUserId")}
                                className="mt-2 w-full border border-gray-300 rounded-md px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                            >
                                <option value="">Unassigned</option>
                                {members.map((member) => (
                                    <option key={member.userId} value={member.userId}>
                                        {member.username}
                                    </option>
                                ))}
                            </select>
                        </div>
                    </div>

                    <div className="flex gap-3 pt-6">
                        <Button
                            type="submit"
                            disabled={isSubmitting}
                            className="flex-1 bg-blue-600 hover:bg-blue-700 text-white font-semibold"
                        >
                            {isSubmitting ? "Saving..." : isEdit ? "Update" : "Create"}
                        </Button>
                        <Button
                            type="button"
                            onClick={() => navigate(`/projects/${projectId}/tasks`)}
                            variant="outline"
                            className="flex-1"
                        >
                            Cancel
                        </Button>
                    </div>
                </form>
            </div>
        </div>
    );
};

export default TaskPage;