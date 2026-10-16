// src/components/Footer.tsx
const Footer = () => {
    const currentYear = new Date().getFullYear();

    return (
        <footer className="bg-[#222831] text-gray-200">
            <div className="container mx-auto py-6 text-center text-sm">
                © {currentYear} <span className="font-medium text-white">Petros Lyros</span>. All Rights Reserved.
            </div>
        </footer>
    );
};

export default Footer;
