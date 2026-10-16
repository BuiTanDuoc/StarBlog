import {
    Table,
    TableBody,
    TableCaption,
    TableCell,
    TableHead,
    TableHeader,
    TableRow,
} from "@/components/ui/table";
import { useEffect, useState } from "react";
import { deleteUser, getPaginatedUsers } from "@/services/api.users";
import type { UserReadOnly } from "@/schemas/users";
import { Button } from "@/components/ui/button";
import { Pencil, Trash2 } from "lucide-react";
import { useNavigate } from "react-router";
import { toast } from "sonner";

const UsersPage = () => {
    const [users, setUsers] = useState<UserReadOnly[]>([]);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();

    const handleDelete = async (id: number) => {
        // Add confirmation dialog
        if (!confirm("Are you sure you want to delete this user?")) return;

        try {
            await deleteUser(id);
            // Update the UI by filtering out the deleted user
            setUsers(prev => prev.filter(u => u.id !== id));
            toast.success("User deleted successfully");
        } catch (error) {
            console.error(error);
            toast.error("Failed to delete user");
        }
    };

    // In UsersPage.tsx
    useEffect(() => {
        getPaginatedUsers() // Uses defaults
            .then(({ data }) => {
                setUsers(data);
                setLoading(false);
            })
            .catch((err) => {
                console.error(err);
                toast.error("Failed to load users");
                setLoading(false);
            });
    }, []);

    if (loading) {
        return <div className="p-8 text-center">Loading...</div>;
    }

    return (
        <div className="p-8">
            <div className="flex justify-between items-center mb-6">
                <h1 className="text-2xl">Users</h1>
                {/*<Button onClick={() => navigate("/users/new")}>*/}
                {/*    <Plus className="w-4 h-4 mr-2" />*/}
                {/*    Add User*/}
                {/*</Button>*/}
            </div>

            <Table>
                <TableCaption>A list of all users in the system.</TableCaption>
                <TableHeader className="bg-gray-100">
                    <TableRow>
                        <TableHead className="w-[80px]">ID</TableHead>
                        <TableHead>Username</TableHead>
                        <TableHead>Email</TableHead>
                        <TableHead>First Name</TableHead>
                        <TableHead>Last Name</TableHead>
                        <TableHead>Role</TableHead>
                        <TableHead className="text-right">Actions</TableHead>
                    </TableRow>
                </TableHeader>
                <TableBody>
                    {users.map((user) => (
                        <TableRow key={user.id}>
                            <TableCell className="font-medium">{user.id}</TableCell>
                            <TableCell>{user.username}</TableCell>
                            <TableCell>{user.email}</TableCell>
                            <TableCell>{user.firstname}</TableCell>
                            <TableCell>{user.lastname}</TableCell>
                            <TableCell>{user.userRole}</TableCell>
                            <TableCell className="text-right space-x-2">
                                <Button
                                    onClick={() => navigate(`/users/${user.id}`)}
                                    variant="outline"
                                    size="sm"
                                >
                                    <Pencil className="w-4 h-4" />
                                </Button>
                                <Button
                                    onClick={() => handleDelete(user.id)}
                                    variant="destructive"
                                    size="sm"
                                >
                                    <Trash2 className="w-4 h-4" />
                                </Button>
                            </TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
        </div>
    );
};

export default UsersPage;