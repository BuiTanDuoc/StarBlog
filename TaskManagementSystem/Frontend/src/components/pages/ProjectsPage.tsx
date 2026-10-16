import { useEffect, useState } from "react";
import { useNavigate } from "react-router";
import { toast } from "sonner";
import { getProjects, deleteProject } from "@/services/api.projects";
import type { Project } from "@/schemas/projects";
import { Button } from "@/components/ui/button";

const ProjectsPage = () => {
    const [projects, setProjects] = useState<Project[]>([]);
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();

    useEffect(() => {
        document.title = "TaskFlow - Projects";
        loadProjects();
    }, []);

    const loadProjects = async () => {
        try {
            setLoading(true);
            const data = await getProjects();
            setProjects(data);
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Failed to load projects");
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = async (id: number) => {
        if (!window.confirm("Are you sure you want to delete this project?")) {
            return;
        }

        try {
            await deleteProject(id);
            setProjects(projects.filter((p) => p.id !== id));
            toast.success("Project deleted successfully");
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Failed to delete project");
        }
    };

    return (
        <div className="py-8">
            <div className="flex justify-between items-center mb-8">
                <h1 className="text-3xl font-bold text-gray-800">Projects</h1>
                <Button
                    onClick={() => navigate("/projects/new")}
                    className="bg-blue-600 hover:bg-blue-700 text-white"
                >
                    + New Project
                </Button>
            </div>

            {loading && (
                <div className="flex items-center justify-center py-12">
                    <div className="text-center">
                        <div className="w-12 h-12 rounded-full border-4 border-blue-600 border-t-transparent animate-spin mx-auto mb-4"></div>
                        <p className="text-gray-600">Loading projects...</p>
                    </div>
                </div>
            )}

            {!loading && projects.length === 0 && (
                <div className="text-center py-12 bg-gray-50 rounded-lg border border-gray-200">
                    <p className="text-gray-600 text-lg">No projects yet. Create one to get started!</p>
                    <Button
                        onClick={() => navigate("/projects/new")}
                        className="mt-4 bg-blue-600 hover:bg-blue-700 text-white"
                    >
                        Create First Project
                    </Button>
                </div>
            )}

            {!loading && projects.length > 0 && (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                    {projects.map((project) => (
                        <div
                            key={project.id}
                            className="bg-white rounded-lg shadow hover:shadow-lg transition p-6 border border-gray-200"
                        >
                            <h3 className="text-xl font-semibold text-gray-800 mb-2">{project.name}</h3>
                            <p className="text-gray-600 text-sm mb-4 line-clamp-2">{project.description}</p>

                            {/*<div className="flex gap-2 mb-4 text-sm">*/}
                            {/*    <span className="bg-blue-100 text-blue-700 px-3 py-1 rounded-full">*/}
                            {/*        👥 {project.memberCount} members*/}
                            {/*    </span>*/}
                            {/*    <span className="bg-green-100 text-green-700 px-3 py-1 rounded-full">*/}
                            {/*        ✓ {project.taskCount} tasks*/}
                            {/*    </span>*/}
                            {/*</div>*/}

                            <div className="flex gap-2">
                                <Button
                                    onClick={() => navigate(`/projects/${project.id}/tasks`)}
                                    className="flex-1 bg-blue-600 hover:bg-blue-700 text-white text-sm"
                                >
                                    Tasks
                                </Button>
                                <Button
                                    onClick={() => navigate(`/projects/${project.id}`)}
                                    variant="outline"
                                    className="flex-1 text-sm"
                                >
                                    Edit
                                </Button>
                                <Button
                                    onClick={() => handleDelete(project.id)}
                                    variant="destructive"
                                    className="flex-1 text-sm"
                                >
                                    Delete
                                </Button>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export default ProjectsPage;