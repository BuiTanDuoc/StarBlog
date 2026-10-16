import { Label } from "@/components/ui/label.tsx";
import { Input } from "@/components/ui/input.tsx";
import { Button } from "@/components/ui/button.tsx";
import { type UserLogin, userLoginSchema } from "@/schemas/login.ts";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { useNavigate, Link } from "react-router";
import { useAuth } from "@/hooks/useAuth.ts";

//The login form that uses react-hook-form for form management and Zod for validation,
// then authenticates the user via the auth context.
export default function LoginPage() {
    const navigate = useNavigate();
    const { loginUser } = useAuth();  // Get loginUser function from auth context

    // useForm is the main react-hook-form hook - manages form state and validation
    const {
        register,        // Function to register input fields
        handleSubmit,    // Wraps your submit handler with validation
        formState: { errors, isSubmitting },  // Form state (errors, loading, etc.)
    } = useForm<UserLogin>({  // TypeScript generic specifies the form data shape
        resolver: zodResolver(userLoginSchema),  // zodResolver connects Zod validation to react-hook-form
        defaultValues: {  // Initial form values
            username: "",
            password: "",
            keepLoggedIn: false,
        }
    });

    // This function only runs if validation passes
    // react-hook-form automatically validates before calling this
    const onSubmit = async (data: UserLogin) => {
        try {
            await loginUser(data);  // Call context function to login
            toast.success("Login successful");  // Show success notification
            navigate("/expenses");  // Redirect to expenses page
        } catch (err) {
            // Type guard: check if err is an Error instance to safely access .message
            toast.error(
                err instanceof Error ? err.message : "Login failed"
            );
        }
    };

    return (
        <div className="p-8">
            <h1 className="text-2xl text-center mb-6">Login</h1>
            {/* handleSubmit(onSubmit) creates a wrapper that validates before calling onSubmit */}
            <form
                onSubmit={handleSubmit(onSubmit)}
                className="max-w-sm mx-auto p-8 border rounded-md space-y-4"
                autoComplete="off"
            >
                <div>
                    <Label htmlFor="username">Username</Label>
                    {/* Spread operator {...register("username")} adds onChange, onBlur, ref, name props */}
                    <Input id="username" {...register("username")} />
                    {/* Conditional rendering: only show error if it exists */}
                    {errors.username && (
                        <div className="text-red-600 text-sm">
                            {errors.username.message}
                        </div>
                    )}
                </div>

                <div>
                    <Label htmlFor="password">Password</Label>
                    <Input type="password" id="password" {...register("password")} />
                    {errors.password && (
                        <div className="text-red-600 text-sm">
                            {errors.password.message}
                        </div>
                    )}
                </div>

                <div className="flex items-center space-x-2">
                    <input
                        type="checkbox"
                        id="keepLoggedIn"
                        {...register("keepLoggedIn")}
                        className="h-4 w-4 rounded border-gray-300"
                    />
                    <Label htmlFor="keepLoggedIn" className="cursor-pointer font-normal">
                        Keep me logged in
                    </Label>
                </div>

                {/* Button disabled during submission to prevent double-submit */}
                <Button disabled={isSubmitting} className="w-full">
                    {/* Conditional text based on submission state */}
                    {isSubmitting ? "Logging in..." : "Login"}
                </Button>

                <div className="text-center text-sm text-gray-600">
                    Don't have an account?{" "}
                    <Link to="/register" className="text-blue-600 hover:underline">
                        Register here
                    </Link>
                </div>
            </form>
        </div>
    );
}