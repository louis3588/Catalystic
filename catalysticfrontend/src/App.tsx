import { useEffect } from "react";
import { BrowserRouter, Route, Routes, useLocation } from "react-router-dom";
import { Header, Footer } from "./components/SiteLayout";
import Home from "./pages/static/Home";
import About from "./pages/static/About";
import Services from "./pages/static/Services";
import Contact from "./pages/static/Contact";
import Blog from "./pages/static/Blog";
import Shop from "./pages/static/Shop";
import NotFound from "./pages/static/NotFound";
import AdminEnquiries from "./pages/admin/AdminEnquiries";
import AdminClients from "./pages/admin/AdminClients";
import Onboarding from "./pages/client/Onboarding";
import ClientPortal from "./pages/client/ClientPortal";
import AdminBlog from "./pages/admin/AdminBlog";
import WeeklyReview from "./pages/client/WeeklyReview";
import "./App.css";
import "./styles/admin.css";
import "./styles/client.css";

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
          <Route path="/blog/:id" element={<Blog />} />
          <Route path="/shop" element={<Shop />} />
          <Route path="/admin/enquiries" element={<AdminEnquiries />} />
          <Route path="/admin" element={<AdminEnquiries />} />
          <Route path="/admin/clients" element={<AdminClients />} />
          <Route path="/admin/blog" element={<AdminBlog />} />
          <Route path="/onboarding" element={<Onboarding />} />
          <Route path="/client" element={<ClientPortal />} />
          <Route path="/client/weekly-review" element={<WeeklyReview />} />
          <Route path="*" element={<NotFound />} />
        </Routes>
      </main>
      <Footer />
    </BrowserRouter>
  );
}

export default App;
