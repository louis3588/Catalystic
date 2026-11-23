import { useState } from "react";
import { Link, NavLink } from "react-router-dom";

const navigation = [
  { label: "Home", path: "/" },
  { label: "About", path: "/about" },
  { label: "Services", path: "/services" },
  { label: "Blog", path: "/blog" },
  { label: "Contact Us", path: "/contact" },
];

function Brand() {
  return (
    <Link className="brand" to="/" aria-label="Asistym home">
      <span className="brand-mark" aria-hidden="true">
        <span />
      </span>
      <span>
        Asistym<span className="brand-dot">.</span>
      </span>
    </Link>
  );
}

function Header() {
  const [menuOpen, setMenuOpen] = useState(false);

  return (
    <header className="site-header">
      <div className="header-inner container">
        <Brand />
        <button
          className="menu-toggle"
          type="button"
          aria-expanded={menuOpen}
          aria-controls="main-navigation"
          onClick={() => setMenuOpen(!menuOpen)}
        >
          {menuOpen ? "Close" : "Menu"}{" "}
          <span aria-hidden="true">{menuOpen ? "×" : "☰"}</span>
        </button>
        <nav
          id="main-navigation"
          className={menuOpen ? "main-nav is-open" : "main-nav"}
          aria-label="Main navigation"
        >
          {navigation.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              end={item.path === "/"}
              onClick={() => setMenuOpen(false)}
            >
              {item.label}
            </NavLink>
          ))}
          <Link
            className="button button-small"
            to="/shop"
            onClick={() => setMenuOpen(false)}
          >
            Explore the shop
          </Link>
          <Link
              className="button button-small"
              to="/client"
              onClick={() => setMenuOpen(false)}
          >
            Client Login
          </Link>
        </nav>
      </div>
    </header>
  );
}

function Footer() {
  return (
    <footer className="site-footer">
      <div className="container footer-main">
        <div className="footer-intro">
          <Brand />
          <p>
            A more intentional approach to men’s wellbeing, built around the
            life you want to lead.
          </p>
        </div>
        <div className="footer-links">
          <div>
            <h2>Explore</h2>
            <Link to="/about">About</Link>
            <Link to="/services">Services</Link>
            <Link to="/blog">Blog</Link>
          </div>
          <div>
            <h2>Connect</h2>
            <Link to="/contact">Get in touch</Link>
            <Link to="/shop">Shop</Link>
          </div>
        </div>
      </div>
      <div className="container footer-bottom">
        <span>© {new Date().getFullYear()} Asistym.</span>
        <span>Progress, on your terms.</span>
      </div>
    </footer>
  );
}


export { Header, Footer };
