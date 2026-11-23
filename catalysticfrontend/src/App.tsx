import { useEffect } from "react";
import { BrowserRouter, Route, Routes, useLocation } from "react-router-dom";
import { Header, Footer } from "./components/SiteLayout";
import Home from "./pages/Home";
import About from "./pages/About";
import Services from "./pages/Services";
import Contact from "./pages/Contact";
import Blog from "./pages/Blog";
import Shop from "./pages/Shop";
import NotFound from "./pages/NotFound";
import AdminEnquiries from "./pages/AdminEnquiries";
import AdminClients from "./pages/AdminClients";
import Onboarding from "./pages/Onboarding";
import "./App.css";

function RouteChange() {
  const { pathname } = useLocation();

  useEffect(() => {
    window.scrollTo(0, 0);
  }, [pathname]);

  return null;
}

function App() {
  return (
    <BrowserRouter>
      <RouteChange />
      <a className="skip-link" href="#main-content">
        Skip to content
      </a>
      <Header />
      <main id="main-content">
        <Routes>
          <Route path="/" element={<Home />} />
          <Route path="/about" element={<About />} />
          <Route path="/services" element={<Services />} />
          <Route path="/contact" element={<Contact />} />
          <Route path="/blog" element={<Blog />} />
          <Route path="/shop" element={<Shop />} />
          <Route path="/admin/enquiries" element={<AdminEnquiries />} />
          <Route path="/admin" element={<AdminEnquiries />} />
          <Route path="/admin/clients" element={<AdminClients />} />
          <Route path="/onboarding" element={<Onboarding />} />
          <Route path="*" element={<NotFound />} />
        </Routes>
      </main>
      <Footer />
    </BrowserRouter>
  );
}

export default App;
