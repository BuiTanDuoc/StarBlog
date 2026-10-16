import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { type UserRegister, userRegisterSchema } from "@/schemas/users";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { useNavigate, Link } from "react-router";
import { registerUser } from "@/services/api.users";

export default function RegisterPage() {
    const navigate = useNavigate();

    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting },
    } = useForm<UserRegister>({
        resolver: zodResolver(userRegisterSchema),
        defaultValues: {
            username: "",
            email: "",
            firstname: "",
            lastname: "",
            password: "",
            confirmPassword: "",
        },
    });

    const onSubmit = async (data: UserRegister) => {
        try {
            await registerUser(data);
            toast.success("Registration successful! Please login.");
            navigate("/login");
        } catch (err) {
            toast.error(err instanceof Error ? err.message : "Registration failed");
        }
    };

    return (
        <div className="p-8">
            <h1 className="text-2xl text-center mb-6">Create an Account</h1>
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

                <div>
                    <Label htmlFor="password">Password</Label>
                    <Input type="password" id="password" {...register("password")} />
                    {errors.password && (
                        <div className="text-red-600 text-sm">{errors.password.message}</div>
                    )}
                </div>

                <div>
                    <Label htmlFor="confirmPassword">Confirm Password</Label>
                    <Input
                        type="password"
                        id="confirmPassword"
                        {...register("confirmPassword")}
                    />
                    {errors.confirmPassword && (
                        <div className="text-red-600 text-sm">
                            {errors.confirmPassword.message}
                        </div>
                    )}
                </div>

                <Button disabled={isSubmitting} className="w-full">
                    {isSubmitting ? "Registering..." : "Register"}
                </Button>

                <div className="text-center text-sm text-gray-600">
                    Already have an account?{" "}
                    <Link to="/login" className="text-blue-600 hover:underline">
                        Login here
                    </Link>
                </div>
            </form>
        </div>
    );
}