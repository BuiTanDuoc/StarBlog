import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router";
import { toast } from "sonner";
import { getProjectMembers, removeProjectMember, getProject, addProjectMember } from "@/services/api.projects";
import { searchUsers } from "@/services/api.auth";
import type { ProjectMember, Project } from "@/schemas/projects";
import type { UserReadOnly } from "@/schemas/users";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { X, UserPlus } from "lucide-react";

const ProjectMembersPage = () => {
    const { projectId } = useParams();
    const navigate = useNavigate();
    const [members, setMembers] = useState<ProjectMember[]>([]);
    const [project, setProject] = useState<Project | null>(null);
    const [loading, setLoading] = useState(false);

    // Add member states
    const [showAddMember, setShowAddMember] = useState(false);
    const [searchQuery, setSearchQuery] = useState("");
    const [searchResults, setSearchResults] = useState<UserReadOnly[]>([]);
    const [searching, setSearching] = useState(false);
    const [selectedRole, setSelectedRole] = useState<"Owner" | "Member" | "Viewer">("Member");

    useEffect(() => {
        if (!projectId) return;
        loadData();
    }, [projectId]);

    const loadData = async () => {
        if (!projectId) return;
        try {
            setLoading(true);
            const [membersData, projectData] = await Promise.all([
                getProjectMembers(Number(projectId)),
                getProject(Number(projectId)),
            ]);
            setMembers(membersData);
            setProject(projectData);
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Failed to load data");
        } finally {
            setLoading(false);
        }
    };

    // Search for users to add
    const handleSearchUsers = async () => {
        if (!searchQuery.trim()) {
            setSearchResults([]);
            return;
        }

        try {
            setSearching(true);
            console.log("Searching for users with query:", searchQuery); // 🔍 DEBUG

            const results = await searchUsers(searchQuery);
            console.log("Search results:", results); // 🔍 DEBUG

            // Filter out users already in project
            const memberUserIds = members.map(m => m.userId);
            const filtered = results.filter(user => !memberUserIds.includes(user.id));

            setSearchResults(filtered);

            if (filtered.length === 0) {
                toast.info(results.length === 0 ? "No users found" : "All matching users are already members");
            }
        } catch (error) {
            console.error("Search error:", error); // 🔍 DEBUG
            toast.error(error instanceof Error ? error.message : "Search failed");
        } finally {
            setSearching(false);
        }
    };

    // Add member to project
    const handleAddMember = async (userId: number) => {
        if (!projectId) return;

        try {
            await addProjectMember(Number(projectId), {
                userId,
                role: selectedRole,
            });

            toast.success("Member added successfully");
            setSearchQuery("");
            setSearchResults([]);
            await loadData();
        } catch (error) {
            console.error("Add member error:", error); // 🔍 DEBUG
            toast.error(error instanceof Error ? error.message : "Failed to add member");
        }
    };

    const handleRemoveMember = async (memberId: number) => {
        if (!projectId || !window.confirm("Remove member?")) return;
        try {
            await removeProjectMember(Number(projectId), memberId);
            setMembers(members.filter((m) => m.id !== memberId));
            toast.success("Member removed");
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Failed to remove member");
        }
    };

    return (
        <div className="py-8 container mx-auto px-4">
            <div className="flex justify-between items-center mb-8">
                <h1 className="text-3xl font-bold text-gray-800 dark:text-white">
                    {project?.name} - Members
                </h1>
                <div className="flex gap-2">
                    <Button
                        onClick={() => setShowAddMember(!showAddMember)}
                        className="bg-green-600 hover:bg-green-700 dark:bg-green-500 dark:hover:bg-green-600 text-white flex gap-2"
                    >
                        <UserPlus size={18} />
                        Add Member
                    </Button>
                    <Button
                        onClick={() => navigate(`/projects/${projectId}`)}
                        variant="outline"
                        className="dark:bg-slate-700 dark:border-slate-600 dark:text-white"
                    >
                        Back to Project
                    </Button>
                </div>
            </div>

            {/* Add Member Section */}
            {showAddMember && (
                <div className="mb-8 bg-white dark:bg-slate-800 rounded-lg shadow-lg p-6 border border-gray-200 dark:border-slate-700">
                    <div className="flex justify-between items-center mb-4">
                        <h2 className="text-xl font-bold text-gray-800 dark:text-white">Add New Member</h2>
                        <button
                            onClick={() => setShowAddMember(false)}
                            className="text-gray-500 hover:text-gray-700 dark:text-gray-400 dark:hover:text-gray-200"
                        >
                            <X size={24} />
                        </button>
                    </div>

                    <div className="space-y-4">
                        <div>
                            <label className="block text-gray-700 dark:text-gray-300 font-medium mb-2">
                                Search Users
                            </label>
                            <div className="flex gap-2">
                                <Input
                                    placeholder="Search by username (e.g., john, vasil)..."
                                    value={searchQuery}
                                    onChange={(e) => setSearchQuery(e.target.value)}
                                    onKeyPress={(e) => e.key === "Enter" && handleSearchUsers()}
                                    className="flex-1 border-gray-300 dark:bg-slate-700 dark:border-slate-600 dark:text-white"
                                />
                                <Button
                                    onClick={handleSearchUsers}
                                    disabled={searching || !searchQuery.trim()}
                                    className="bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600 text-white"
                                >
                                    {searching ? "..." : "Search"}
                                </Button>
                            </div>
                            <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                                Tip: Type at least 1 character and press Enter or click Search
                            </p>
                        </div>

                        <div>
                            <label className="block text-gray-700 dark:text-gray-300 font-medium mb-2">
                                Role
                            </label>
                            <select
                                value={selectedRole}
                                onChange={(e) => setSelectedRole(e.target.value as "Owner" | "Member" | "Viewer")}
                                className="w-full border border-gray-300 dark:bg-slate-700 dark:border-slate-600 dark:text-white rounded-md px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
                            >
                                <option value="Member">Member</option>
                                <option value="Viewer">Viewer</option>
                                <option value="Owner">Owner</option>
                            </select>
                        </div>

                        {/* Search Results */}
                        {searchResults.length > 0 && (
                            <div className="mt-4 space-y-2">
                                <p className="text-sm font-medium text-gray-700 dark:text-gray-300">
                                    Available Users ({searchResults.length})
                                </p>
                                <div className="space-y-2 max-h-60 overflow-y-auto">
                                    {searchResults.map((user) => (
                                        <div
                                            key={user.id}
                                            className="flex justify-between items-center bg-gray-50 dark:bg-slate-700 p-3 rounded-lg border border-gray-200 dark:border-slate-600"
                                        >
                                            <div>
                                                <p className="font-medium text-gray-800 dark:text-white">{user.username}</p>
                                                <p className="text-xs text-gray-600 dark:text-gray-400">{user.email}</p>
                                            </div>
                                            <Button
                                                onClick={() => handleAddMember(user.id)}
                                                size="sm"
                                                className="bg-green-600 hover:bg-green-700 dark:bg-green-500 dark:hover:bg-green-600 text-white"
                                            >
                                                Add
                                            </Button>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}

                        {searchQuery && searchResults.length === 0 && !searching && (
                            <div className="mt-4 p-3 bg-yellow-50 dark:bg-yellow-900 border border-yellow-200 dark:border-yellow-700 rounded-lg">
                                <p className="text-sm text-yellow-800 dark:text-yellow-200">
                                    No available users found. Either the user doesn't exist or they're already a member.
                                </p>
                            </div>
                        )}
                    </div>
                </div>
            )}

            {/* Members List */}
            {loading ? (
                <div className="flex items-center justify-center py-12">
                    <div className="text-center">
                        <div className="w-12 h-12 rounded-full border-4 border-blue-600 border-t-transparent animate-spin mx-auto mb-4"></div>
                        <p className="text-gray-600 dark:text-gray-300">Loading members...</p>
                    </div>
                </div>
            ) : members.length === 0 ? (
                <div className="text-center py-12 bg-gray-50 dark:bg-slate-700 rounded-lg border border-gray-200 dark:border-slate-600">
                    <p className="text-gray-600 dark:text-gray-300">No members yet. Add your first member!</p>
                </div>
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    {members.map((member) => (
                        <div
                            key={member.id}
                            className="bg-white dark:bg-slate-800 rounded-lg shadow p-6 border border-gray-200 dark:border-slate-700"
                        >
                            <div className="flex justify-between items-start mb-4">
                                <div>
                                    <h3 className="text-lg font-semibold text-gray-800 dark:text-white">
                                        {member.username}
                                    </h3>
                                    <p className="text-sm text-gray-600 dark:text-gray-400">{member.email}</p>
                                </div>
                                <span className="bg-blue-100 dark:bg-blue-900 text-blue-700 dark:text-blue-200 px-3 py-1 rounded-full text-xs font-medium">
                                    {member.role}
                                </span>
                            </div>

                            {member.role !== "Owner" && (
                                <Button
                                    onClick={() => handleRemoveMember(member.id)}
                                    variant="destructive"
                                    className="w-full"
                                >
                                    Remove
                                </Button>
                            )}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export default ProjectMembersPage;