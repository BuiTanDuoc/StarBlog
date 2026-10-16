import { useNavigate } from "react-router";
import { AuthButton } from "@/components/AuthButton";
import { useAuth } from "@/hooks/useAuth";
import { Menu, X } from "lucide-react";
import { useState } from "react";

const Header = () => {
    const { isAuthenticated } = useAuth();
    const navigate = useNavigate();
    const [menuOpen, setMenuOpen] = useState(false);

    return (
        <header className="fixed top-0 w-full bg-gradient-to-r from-blue-600 to-blue-800 text-white shadow-lg z-50">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div className="flex items-center justify-between h-16">
                    {/* Logo */}
                    <div
                        onClick={() => navigate("/")}
                        className="text-2xl font-bold cursor-pointer hover:opacity-80 transition-opacity"
                    >
                        📋 TaskFlow
                    </div>

                    {/* Desktop Navigation */}
                    <nav className="hidden md:flex items-center gap-6">
                        {isAuthenticated && (
                            <button
                                onClick={() => navigate("/projects")}
                                className="hover:text-blue-200 transition-colors font-medium"
                            >
                                Projects
                            </button>
                        )}
                    </nav>

                    {/* Auth Button */}
                    <AuthButton />

                    {/* Mobile Menu Button */}
                    <button
                        onClick={() => setMenuOpen(!menuOpen)}
                        className="md:hidden p-2 hover:bg-blue-700 rounded-lg transition-colors"
                    >
                        {menuOpen ? <X size={24} /> : <Menu size={24} />}
                    </button>
                </div>

                {/* Mobile Navigation */}
                {menuOpen && (
                    <nav className="md:hidden pb-4 space-y-2 border-t border-blue-400">
                        {isAuthenticated && (
                            <button
                                onClick={() => {
                                    navigate("/projects");
                                    setMenuOpen(false);
                                }}
                                className="block w-full text-left px-4 py-2 hover:bg-blue-700 rounded-lg transition-colors"
                            >
                                Projects
                            </button>
                        )}
                    </nav>
                )}
            </div>
        </header>
    );
};

export default Header;