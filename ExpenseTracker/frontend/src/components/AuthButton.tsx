import {Button} from "@/components/ui/button.tsx";
import {useAuth} from "@/hooks/useAuth.ts";
import {useNavigate} from "react-router";


//A smart button component that renders either "Login" or "Logout" based
// on authentication state and handles the respective actions.
export function AuthButton() {
    // Destructure only what we need from auth context
    const {isAuthenticated, logoutUser} = useAuth()
    const navigate = useNavigate()

    const handleLogin = () => {
        navigate("/login")  // Navigate to login page
    }

    const handleLogout = () => {
        logoutUser();  // Call logout function from context (clears token, resets state)
    }

    // Ternary operator for conditional rendering
    // Pattern: condition ? <ComponentA /> : <ComponentB />
    return isAuthenticated ? (
        // If logged in: show Logout button
        <Button onClick={handleLogout}>
            Logout
        </Button>
    ) : (
        // If NOT logged in: show Login button
        <Button onClick={handleLogin}>
            Login
        </Button>
    )
}