import { useAuth } from "@/hooks/useAuth";
import { useNavigate } from "react-router";
import { useEffect } from "react";
import { Button } from "@/components/ui/button";

const HomePage = () => {
    const { isAuthenticated, username, user } = useAuth();
    const navigate = useNavigate();

    useEffect(() => {
        document.title = "TaskFlow - Home";
    }, []);

    // const handleLogout = () => {
    //     logoutUser();
    //     navigate("/login");
    // };

    return (
        <div className="py-12 container mx-auto px-4">
            {isAuthenticated ? (
                <div className="max-w-2xl mx-auto">
                    <div className="bg-gradient-to-r from-blue-50 to-blue-100 rounded-lg shadow-lg p-8 border border-blue-200">
                        <h1 className="text-4xl font-bold text-gray-800 mb-4">
                            Welcome back, <span className="text-blue-600">{username || user?.username}!</span>
                        </h1>
                        <p className="text-gray-700 text-lg mb-8">
                            Manage your team projects efficiently with TaskFlow. Create projects, assign tasks, and collaborate with your classmates.
                        </p>

                        <div className="grid grid-cols-1 md:grid-cols-2  mb-8">
                            <div className="bg-white rounded-lg p-6 shadow border-l-4 border-blue-600">
                                <h3 className="text-xl font-semibold text-gray-800 mb-2">📊 Projects</h3>
                                <p className="text-gray-600 mb-4">Create and manage your classroom projects</p>
                                <Button
                                    onClick={() => navigate("/projects")}
                                    className="w-full bg-blue-600 hover:bg-blue-700 text-white"
                                >
                                    View Projects
                                </Button>
                            </div>

                            {/*<div className="bg-white rounded-lg p-6 shadow border-l-4 border-green-600">*/}
                            {/*    <h3 className="text-xl font-semibold text-gray-800 mb-2">✨ Features</h3>*/}
                            {/*    <p className="text-gray-600 mb-4">Task assignment, team collaboration, progress tracking</p>*/}
                            {/*    <Button*/}
                            {/*        onClick={() => navigate("/projects")}*/}
                            {/*        variant="outline"*/}
                            {/*        className="w-full border-green-600 text-green-600 hover:bg-green-50"*/}
                            {/*    >*/}
                            {/*        Get Started*/}
                            {/*    </Button>*/}
                            {/*</div>*/}
                        </div>

                        {/*<Button*/}
                        {/*    onClick={handleLogout}*/}
                        {/*    variant="destructive"*/}
                        {/*    className=""*/}
                        {/*>*/}
                        {/*    Logout*/}
                        {/*</Button>*/}
                    </div>
                </div>
            ) : (
                <div className="max-w-2xl mx-auto">
                    <div className="bg-gradient-to-r from-blue-50 to-blue-100 rounded-lg shadow-lg p-12 text-center border border-blue-200">
                        <h1 className="text-4xl font-bold text-gray-800 mb-4">
                            Welcome to <span className="text-blue-600">TaskFlow</span>
                        </h1>
                        <p className="text-xl text-gray-700 mb-8">
                            The simple way for teams to manage projects and collaborate effectively.
                        </p>

                        <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-8">
                            <div className="bg-white rounded-lg p-6 shadow">
                                <div className="text-3xl mb-3">📋</div>
                                <h3 className="font-semibold text-gray-800 mb-2">Easy Projects</h3>
                                <p className="text-sm text-gray-600">Create and manage projects</p>
                            </div>
                            <div className="bg-white rounded-lg p-6 shadow">
                                <div className="text-3xl mb-3">👥</div>
                                <h3 className="font-semibold text-gray-800 mb-2">Team Collab</h3>
                                <p className="text-sm text-gray-600">Work with your teammates</p>
                            </div>
                            <div className="bg-white rounded-lg p-6 shadow">
                                <div className="text-3xl mb-3">✅</div>
                                <h3 className="font-semibold text-gray-800 mb-2">Track Progress</h3>
                                <p className="text-sm text-gray-600">Monitor project status</p>
                            </div>
                        </div>

                        <div className="flex flex-col sm:flex-row gap-4 justify-center">
                            <Button
                                onClick={() => navigate("/login")}
                                className="bg-blue-600 hover:bg-blue-700 text-white px-8 py-3 text-lg"
                            >
                                Login
                            </Button>
                            <Button
                                onClick={() => navigate("/register")}
                                variant="outline"
                                className="border-blue-600 text-blue-600 px-8 py-3 text-lg hover:bg-blue-50"
                            >
                                Register
                            </Button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default HomePage;