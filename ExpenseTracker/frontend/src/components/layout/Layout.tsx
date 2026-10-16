// src/components/Layout.tsx
import Header from "./Header";
import Footer from "./Footer";
import {Outlet} from "react-router";


const Layout = () => {
    return (
        <>
            <Header />
            <main className="container mx-auto pt-24 pb-12 min-h-[90vh]">
                <Outlet />
            </main>
            <Footer />
        </>
    );
};

export default Layout;
