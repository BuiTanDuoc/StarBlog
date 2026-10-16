import { AuthButton } from "@/components/AuthButton";
import { useAuth } from "@/hooks/useAuth";
import { Link } from "react-router";

//The navigation header that shows different menu items based on user
// authentication status and role (admin vs regular user).
const Header = () => {
    // Custom hook that accesses our AuthContext to get user authentication state
    //  reads data from AuthProvider
    const { isAuthenticated, username, userRole } = useAuth();

    return (
        <header className="bg-[#222831] w-full fixed top-0 z-50 shadow-md">
            <div className="container mx-auto px-6 py-4 flex items-center justify-between">
                {/* Logo + Title */}
                {/* Link is React Router's version of <a> tag - doesn't reload the page */}
                <Link to="/" className="flex items-center gap-3">
                    <img
                        className="h-10"
                        src="https://cdn-icons-png.flaticon.com/512/2331/2331970.png"
                        alt="Expense Tracker Logo"
                    />
                    <h1 className="text-white text-2xl font-semibold tracking-wide">
                        Expense Tracker
                    </h1>
                </Link>

                {/* Navigation */}
                <div className="flex items-center gap-6">
                    {/* Conditional rendering: only show nav if user is logged in */}
                    {/* && is "short-circuit evaluation" - if left side is false, right side never runs */}
                    {isAuthenticated && (
                        <>
                            <nav className="flex gap-6 text-gray-200 font-medium">
                                <Link to="/expenses" className="hover:text-white transition">
                                    Expenses
                                </Link>
                                {/* Nested conditional: only admins see Users link */}
                                {userRole === "Admin" && (
                                    <Link to="/users" className="hover:text-white transition">
                                        Users
                                    </Link>
                                )}
                            </nav>
                            <span className="text-gray-300">
                                Welcome, {username}
                                {/* Show admin badge if user is admin */}
                                {userRole === "Admin" && (
                                    <span className="ml-2 text-xs bg-yellow-500 text-black px-2 py-1 rounded">
                                        Admin
                                    </span>
                                )}
                            </span>
                        </>
                    )}
                    {/* AuthButton shows Login or Logout based on auth state */}
                    <AuthButton />
                </div>
            </div>
        </header>
    );
};

export default Header;