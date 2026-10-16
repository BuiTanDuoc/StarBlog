import { Input } from "@/components/ui/input";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
    userInsertSchema,
    userUpdateSchema,
    type UserInsert,
    type UserUpdate,
} from "@/schemas/users";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { useEffect } from "react";
import { createUser, getUser, updateUser } from "@/services/api.users";
import { useNavigate, useParams } from "react-router";
import { toast } from "sonner";

const UserPage = () => {
    const { userId } = useParams();
    const isEdit = Boolean(userId);
    const navigate = useNavigate();

    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting },
        reset,
    } = useForm<UserInsert | UserUpdate>({
        resolver: zodResolver(isEdit ? userUpdateSchema : userInsertSchema),
        defaultValues: isEdit ? {
            username: "",
            email: "",
            firstname: "",
            lastname: "",
        } : {
            username: "",
            email: "",
            firstname: "",
            lastname: "",
            password: "",
            userRole: "User",
        },
    });

    useEffect(() => {
        if (!isEdit || !userId) return;
        getUser(Number(userId))
            .then((data) => {
                reset({
                    username: data.username,
                    email: data.email,
                    firstname: data.firstname,
                    lastname: data.lastname,
                });
            })
            .catch((err) => {
                console.error("Error getting user:", err);
                toast.error("Failed to load user");
            });
    }, [isEdit, userId, reset]);

    const onSubmit = async (data: UserInsert | UserUpdate) => {
        try {
            if (isEdit && userId) {
                await updateUser(Number(userId), data as UserUpdate);
                toast.success("User updated successfully");
            } else {
                await createUser(data as UserInsert);
                toast.success("User created successfully");
            }
            navigate("/users");
        } catch (error) {
            toast.error(error instanceof Error ? error.message : "Something went wrong");
        }
    };

    return (
        <div className="p-8">
            <h1 className="text-2xl text-center mb-6">
                {isEdit ? "Edit User" : "Create New User"}
            </h1>
            <form
                onSubmit={handleSubmit(onSubmit)}
                className="max-w-md mx-auto p-8 border rounded-md space-y-4"
                autoComplete="off"
            >
                <div>
                    <Label htmlFor="username">Username</Label>
                    <Input id="username" {...register("username")} />
                    {errors.username && (
                        <div className="text-red-600 text-sm">{errors.username.message}</div>
                    )}
                </div>

                <div>
                    <Label htmlFor="email">Email</Label>
                    <Input id="email" type="email" {...register("email")} />
                    {errors.email && (
                        <div className="text-red-600 text-sm">{errors.email.message}</div>
                    )}
                </div>

                <div>
                    <Label htmlFor="firstname">First Name</Label>
                    <Input id="firstname" {...register("firstname")} />
                    {errors.firstname && (
                        <div className="text-red-600 text-sm">{errors.firstname.message}</div>
                    )}
                </div>

                <div>
                    <Label htmlFor="lastname">Last Name</Label>
                    <Input id="lastname" {...register("lastname")} />
                    {errors.lastname && (
                        <div className="text-red-600 text-sm">{errors.lastname.message}</div>
                    )}
                </div>

                {/* Only show password field when creating a new user */}
                {!isEdit && (
                    <div>
                        <Label htmlFor="password">Password</Label>
                        <Input
                            id="password"
                            type="password"
                            {...register("password")}
                        />
                        {/* Type guard: only access errors.password in create mode */}
                        {'password' in errors && errors.password && (
                            <div className="text-red-600 text-sm">{errors.password.message}</div>
                        )}
                    </div>
                )}

                {/* Only show user role field when creating a new user */}
                {!isEdit && (
                    <div>
                        <Label htmlFor="userRole">Role</Label>
                        <Input id="userRole" {...register("userRole")} />
                        {/* Type guard: only access errors.userRole in create mode */}
                        {'userRole' in errors && errors.userRole && (
                            <div className="text-red-600 text-sm">{errors.userRole.message}</div>
                        )}
                    </div>
                )}

                <div className="flex gap-2 pt-4">
                    <Button
                        type="button"
                        variant="outline"
                        onClick={() => navigate("/users")}
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

export default UserPage;