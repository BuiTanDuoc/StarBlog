const Footer = () => {
    const currentYear = new Date().getFullYear();

    return (
        <footer className="bg-[#0b0f1a] text-gray-400 mt-10 border-t border-gray-800/50">
            <div className="container mx-auto px-6 py-6"> {/* Μειωμένο padding */}
                <div className="flex flex-col md:flex-row justify-between items-center gap-6">

                    {/* Left: Brand & Tagline */}
                    <div className="flex items-center gap-3">
                        <div className="w-6 h-6 bg-blue-600 rounded flex items-center justify-center">
                            <span className="text-white font-bold text-sm">T</span>
                        </div>
                        <span className="text-white font-semibold tracking-tight">TaskFlow</span>
                        <span className="hidden sm:inline text-gray-600">|</span>
                        <p className="text-xs hidden sm:inline">Modern collaboration for students</p>
                    </div>

                    {/* Center: Quick Links (Horizontal πλέον) */}
                    <nav className="flex gap-6 text-xs font-medium">
                        <a href="#" className="hover:text-white transition-colors">Projects</a>
                        <a href="#" className="hover:text-white transition-colors">About</a>
                        <a href="#" className="hover:text-white transition-colors">Privacy</a>
                        <a href="#" className="hover:text-white transition-colors">Contact</a>
                    </nav>

                    {/* Right: Copyright & Status */}
                    <div className="flex items-center gap-4 text-[10px] uppercase tracking-widest text-gray-500">
                        <p>&copy; {currentYear}</p>
                        <div className="flex items-center gap-1.5">
                            <span className="w-1.5 h-1.5 bg-green-500 rounded-full animate-pulse"></span>
                            <span className="hidden lg:inline">Live</span>
                        </div>
                    </div>

                </div>
            </div>
        </footer>
    );
};

export default Footer;