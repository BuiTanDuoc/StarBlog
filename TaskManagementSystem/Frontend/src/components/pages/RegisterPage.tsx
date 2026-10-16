import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { userRegisterSchema, type UserRegisterDTO } from "@/schemas/register";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { useNavigate } from "react-router";
import { register as registerUser } from "@/services/api.auth";
import { useEffect } from "react";

export default function RegisterPage() {
    const navigate = useNavigate();

    useEffect(() => {
        document.title = "TaskFlow - Register";
    }, []);

    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting },
    } = useForm<UserRegisterDTO>({
        resolver: zodResolver(userRegisterSchema),
        defaultValues: {
            username: "",
            email: "",
            password: "",
            confirmPassword: "",
            firstname: "",
            lastname: "",
        },
    });

    const getErrorMessage = (fieldName: keyof UserRegisterDTO): string | undefined => {
        const error = errors[fieldName];
        if (error && typeof error === "object" && "message" in error) {
            return error.message as string;
        }
        return undefined;
    };

    const onSubmit = async (data: UserRegisterDTO) => {
        try {
            await registerUser(data);
            toast.success("Registration successful! Please login.");
            navigate("/login");
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : "Registration failed";
            toast.error(errorMessage);
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-blue-100 px-4">
            <div className="w-full max-w-md">
                <div className="bg-white rounded-lg shadow-xl p-8">
                    <div className="text-center mb-8">
                        <h1 className="text-4xl font-bold text-blue-600 mb-2">📋 TaskFlow</h1>
                        <p className="text-gray-600">Create your account</p>
                    </div>

                    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                        <div className="grid grid-cols-2 gap-3">
                            <div>
                                <Label htmlFor="firstname" className="text-gray-700 font-medium">
                                    First Name
                                </Label>
                                <Input
                                    id="firstname"
                                    placeholder="John"
                                    {...register("firstname")}
                                    className="mt-1 border-gray-300 text-sm"
                                    disabled={isSubmitting}
                                />
                                {getErrorMessage("firstname") && (
                                    <p className="text-red-600 text-xs mt-1">{getErrorMessage("firstname")}</p>
                                )}
                            </div>

                            <div>
                                <Label htmlFor="lastname" className="text-gray-700 font-medium">
                                    Last Name
                                </Label>
                                <Input
                                    id="lastname"
                                    placeholder="Doe"
                                    {...register("lastname")}
                                    className="mt-1 border-gray-300 text-sm"
                                    disabled={isSubmitting}
                                />
                                {getErrorMessage("lastname") && (
                                    <p className="text-red-600 text-xs mt-1">{getErrorMessage("lastname")}</p>
                                )}
                            </div>
                        </div>

                        <div>
                            <Label htmlFor="username" className="text-gray-700 font-medium">
                                Username
                            </Label>
                            <Input
                                id="username"
                                placeholder="johndoe"
                                {...register("username")}
                                className="mt-1 border-gray-300"
                                disabled={isSubmitting}
                            />
                            {getErrorMessage("username") && (
                                <p className="text-red-600 text-sm mt-1">{getErrorMessage("username")}</p>
                            )}
                        </div>

                        <div>
                            <Label htmlFor="email" className="text-gray-700 font-medium">
                                Email
                            </Label>
                            <Input
                                id="email"
                                type="email"
                                placeholder="john@example.com"
                                {...register("email")}
                                className="mt-1 border-gray-300"
                                disabled={isSubmitting}
                            />
                            {getErrorMessage("email") && (
                                <p className="text-red-600 text-sm mt-1">{getErrorMessage("email")}</p>
                            )}
                        </div>

                        <div>
                            <Label htmlFor="password" className="text-gray-700 font-medium">
                                Password
                            </Label>
                            <Input
                                type="password"
                                id="password"
                                placeholder="Enter password"
                                {...register("password")}
                                className="mt-1 border-gray-300"
                                disabled={isSubmitting}
                            />
                            {getErrorMessage("password") && (
                                <p className="text-red-600 text-sm mt-1">{getErrorMessage("password")}</p>
                            )}
                        </div>

                        <div>
                            <Label htmlFor="confirmPassword" className="text-gray-700 font-medium">
                                Confirm Password
                            </Label>
                            <Input
                                type="password"
                                id="confirmPassword"
                                placeholder="Confirm password"
                                {...register("confirmPassword")}
                                className="mt-1 border-gray-300"
                                disabled={isSubmitting}
                            />
                            {getErrorMessage("confirmPassword") && (
                                <p className="text-red-600 text-sm mt-1">{getErrorMessage("confirmPassword")}</p>
                            )}
                        </div>

                        <Button
                            type="submit"
                            disabled={isSubmitting}
                            className="w-full bg-blue-600 text-white hover:bg-blue-700 py-2 font-semibold mt-6"
                        >
                            {isSubmitting ? "Registering..." : "Register"}
                        </Button>
                    </form>

                    <div className="mt-6 text-center text-sm text-gray-600">
                        <p>Already have an account?</p>
                        <button
                            onClick={() => navigate("/login")}
                            className="text-blue-600 hover:text-blue-700 font-semibold mt-2"
                        >
                            Login here
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}