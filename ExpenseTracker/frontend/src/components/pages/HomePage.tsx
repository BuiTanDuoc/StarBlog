import { Link } from "react-router";
import { useEffect } from "react";
import { useAuth } from "@/hooks/useAuth";

//The landing page that shows different navigation links based on whether the user is logged in and their role.
const HomePage = () => {
    // Get authentication state from context
    const { isAuthenticated, userRole } = useAuth();

    // Array of link objects with conditional visibility logic
    // This pattern makes it easy to add/remove/modify links
    const links = [
        {path: "/expenses", label: "Expenses", show: isAuthenticated  },// Only show if logged in
        {path: "/users", label: "Users (Admin)", show: isAuthenticated && userRole === "Admin" }, // Only show if logged in AND admin
        {path: "/login", label: "Login", show: !isAuthenticated  },// Only show if NOT logged in
        {path: "/register", label: "Register", show: !isAuthenticated  }// Only show if NOT logged in
    ];

    // useEffect runs side effects after render
    // Empty dependency array [] means it only runs once on component mount
    useEffect(() => {
        document.title = "Expense Tracker";  // Set browser tab title
    }, []);  // Empty array = run once on mount

    return (
        <>
            <h1 className="text-2xl text-center my-12">Home Page</h1>
            <div className="flex flex-col items-start max-w-sm mx-auto gap-4">
                {/* Array method chaining: filter then map */}
                {links
                    .filter((link) => link.show)  // Only keep links where show === true
                    .map((link) => (              // Transform each link into a Link component
                        <Link
                            key={link.path}  // React needs unique key for list items
                            to={link.path}
                            className="bg-gray-200 w-full px-4 py-2 rounded hover:bg-gray-300 transition"
                        >
                            {link.label}
                        </Link>
                    ))}
            </div>
        </>
    );
};

export default HomePage;