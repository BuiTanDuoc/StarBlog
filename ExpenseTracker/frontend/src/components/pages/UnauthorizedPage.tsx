import { Button } from "@/components/ui/button";
import { useNavigate } from "react-router";
import { ShieldAlert } from "lucide-react";

//A simple error page that displays when users try to access admin-only routes without proper permissions.
const UnauthorizedPage = () => {
    const navigate = useNavigate();

    return (
        <div className="flex flex-col items-center justify-center min-h-[60vh] p-8">
            {/* lucide-react icon component - scalable SVG icons */}
            <ShieldAlert className="w-24 h-24 text-red-500 mb-6" />
            <h1 className="text-3xl font-bold mb-4">Access Denied</h1>
            <p className="text-gray-600 mb-8 text-center max-w-md">
                You don't have permission to access this page.
                This area is restricted to administrators only.
            </p>
            <div className="flex gap-4">
                {/* navigate(-1) goes back one page in browser history, like clicking the back button */}
                <Button onClick={() => navigate(-1)} variant="outline">
                    Go Back
                </Button>
                {/* navigate("/") goes to the home route */}
                <Button onClick={() => navigate("/")}>
                    Go to Home
                </Button>
            </div>
        </div>
    );
};

export default UnauthorizedPage;