import { Button } from "@/components/ui/button";
import { useAuth } from "@/hooks/useAuth";
import { useNavigate } from "react-router";
import { LogOut, LogIn } from "lucide-react";

export function AuthButton() {
    const { isAuthenticated, user, logoutUser } = useAuth();
    const navigate = useNavigate();

    const handleLogin = () => {
        navigate("/login");
    };

    const handleLogout = () => {
        logoutUser();
        navigate("/");
    };

    // Display "Hello Username" with logout button if authenticated, otherwise "Login" button
    return isAuthenticated && user ? (
        <div className="flex items-center gap-3">
            <span className="text-white text-sm font-medium hidden sm:inline">
                {user.username}
            </span>
            <Button
                onClick={handleLogout}
                variant="destructive"
                className="bg-red-500 hover:bg-red-700 text-white transition-all flex items-center gap-2 border-none"
            >
                <LogOut size={18} />
                <span className="hidden sm:inline">Logout</span>
            </Button>
        </div>
    ) : (
        <Button
            onClick={handleLogin}
            className="bg-white text-blue-600 hover:bg-gray-100 font-semibold flex items-center gap-2 transition-all"
        >
            <LogIn size={18} />
            <span className="hidden sm:inline">Login</span>
        </Button>
    );
}

export default AuthButton;