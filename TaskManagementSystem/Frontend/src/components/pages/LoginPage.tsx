import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { userLoginSchema, type UserLoginDTO } from "@/schemas/login";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { useNavigate } from "react-router";
import { useAuth } from "@/hooks/useAuth";
import { useEffect } from "react";

export default function LoginPage() {
    const navigate = useNavigate();
    const { loginUser, isAuthenticated } = useAuth();

    useEffect(() => {
        if (isAuthenticated) {
            navigate("/projects");
        }
    }, [isAuthenticated, navigate]);

    useEffect(() => {
        document.title = "TaskFlow - Login";
    }, []);

    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting },
    } = useForm<UserLoginDTO>({
        resolver: zodResolver(userLoginSchema),
        defaultValues: {
            username: "",
            password: "",
        },
    });

    const getErrorMessage = (fieldName: keyof UserLoginDTO): string | undefined => {
        const error = errors[fieldName];
        if (error && typeof error === "object" && "message" in error) {
            return error.message as string;
        }
        return undefined;
    };

    const onSubmit = async (data: UserLoginDTO) => {
        try {
            await loginUser(data);
            toast.success("Login successful!");
            navigate("/projects");
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : "Login failed";
            toast.error(errorMessage);
        }
    };

    return (
        <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-blue-50 to-blue-100 px-4">
            <div className="w-full max-w-md">
                <div className="bg-white rounded-lg shadow-xl p-8">
                    <div className="text-center mb-8">
                        <h1 className="text-4xl font-bold text-blue-600 mb-2">📋 TaskFlow</h1>
                        <p className="text-gray-600">Team collaboration simplified</p>
                    </div>

                    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
                        <div>
                            <Label htmlFor="username" className="text-gray-700 font-medium">
                                Username
                            </Label>
                            <Input
                                id="username"
                                placeholder="Enter your username"
                                {...register("username")}
                                className="mt-2 border-gray-300"
                                disabled={isSubmitting}
                            />
                            {getErrorMessage("username") && (
                                <p className="text-red-600 text-sm mt-1">{getErrorMessage("username")}</p>
                            )}
                        </div>

                        <div>
                            <Label htmlFor="password" className="text-gray-700 font-medium">
                                Password
                            </Label>
                            <Input
                                type="password"
                                id="password"
                                placeholder="Enter your password"
                                {...register("password")}
                                className="mt-2 border-gray-300"
                                disabled={isSubmitting}
                            />
                            {getErrorMessage("password") && (
                                <p className="text-red-600 text-sm mt-1">{getErrorMessage("password")}</p>
                            )}
                        </div>

                        <Button
                            type="submit"
                            disabled={isSubmitting}
                            className="w-full bg-blue-600 text-white hover:bg-blue-700 py-2 font-semibold"
                        >
                            {isSubmitting ? "Logging in..." : "Login"}
                        </Button>
                    </form>

                    <div className="mt-6 text-center text-sm text-gray-600">
                        <p>Don't have an account?</p>
                        <button
                            onClick={() => navigate("/register")}
                            className="text-blue-600 hover:text-blue-700 font-semibold mt-2"
                        >
                            Register here
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}