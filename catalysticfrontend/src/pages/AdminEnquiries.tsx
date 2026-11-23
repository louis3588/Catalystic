import { FormEvent, useCallback, useEffect, useState } from "react";
import { sendRequest } from "../api";
import { Link } from "react-router-dom";

type Enquiry = { id: number; name: string; email: string; goals: string; message: string; status: string; createdAt: string };
const statuses = ["New", "Under review", "Consultation requested", "Accepted", "Declined", "Converted to client", "Closed"];

export default function AdminEnquiries() {
  const [items, setItems] = useState<Enquiry[]>([]);
  const [view, setView] = useState<"loading" | "login" | "ready" | "error">("loading");
  const [feedback, setFeedback] = useState("");
  const [bootstrap, setBootstrap] = useState<{ email: string; password: string } | null>(null);

  const load = useCallback(async () => {
    try {
      const response = await fetch("/api/admin/enquiries/", { credentials: "same-origin", cache: "no-store" });
      if (response.status === 401) { setView("login"); return; }
      if (!response.ok) throw new Error();
      setItems(await response.json());
      setView("ready");
    } catch {
      setView("error");
    }
  }, []);

  useEffect(() => { void load(); }, [load]);

  async function login(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    setFeedback("");
    try {
      const response = await sendRequest("/api/auth/login", "POST", { email: data.get("email"), password: data.get("password") });
      if (!response.ok) throw new Error();
      const result = await response.json();
      if (result.passwordChangeRequired) {
        setBootstrap({ email: String(data.get("email")), password: String(data.get("password")) });
        setFeedback("");
        return;
      }
      await load();
    } catch { setFeedback("Sign-in failed. Please check your details and try again."); }
  }

  async function setInitialPassword(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!bootstrap) return;

    const data = new FormData(event.currentTarget);
    const newPassword = String(data.get("newPassword") ?? "");
    const confirmation = String(data.get("confirmation") ?? "");

    if (newPassword !== confirmation) {
      setFeedback("The passwords do not match.");
      return;
    }

    try {
      const response = await sendRequest("/api/auth/bootstrap-password", "POST", { ...bootstrap, newPassword });
      if (!response.ok) throw new Error();
      setBootstrap(null);
      setFeedback("Password set. Please sign in with your new password.");
    } catch {
      setFeedback("Password setup failed. The one-time password may have expired.");
    }
  }

  async function update(id: number, status: string) {
    setFeedback("");
    try {
      const response = await sendRequest(`/api/admin/enquiries/${id}`, "PATCH", { status });
      if (!response.ok) throw new Error();
      await load();
      setFeedback("Status updated.");
    } catch { setFeedback("Could not update the enquiry. Please try again."); }
  }

  async function logout() {
    setFeedback("");

    try {
      const response = await sendRequest("/api/auth/logout", "POST", {});
      if (!response.ok) throw new Error();
      setItems([]);
      setView("login");
      setFeedback("You have been signed out.");
    } catch {
      setFeedback("Could not sign out. Please try again.");
    }
  }

  return (
    <section className="section"><div className="container">
      <h1>Enquiries</h1>
      {view === "loading" && <p>Loading enquiries…</p>}
      {view === "error" && <p role="alert">Unable to load enquiries. Check your connection and permissions.</p>}
      {view === "login" && !bootstrap && <form className="enquiry-form" onSubmit={login}>
        <h2>Administrator sign in</h2>
        <label>Email <input name="email" type="email" autoComplete="username" required /></label>
        <label>Password <input name="password" type="password" autoComplete="current-password" required /></label>
        <button className="button book-button" type="submit">Sign in</button>
      </form>}
      {view === "login" && bootstrap && <form className="enquiry-form" onSubmit={setInitialPassword}>
        <h2>Set your password</h2>
        <p>Choose a password between 12 and 256 characters. You will sign in again after setting it.</p>
        <label>New password <input name="newPassword" type="password" autoComplete="new-password" minLength={12} maxLength={256} required /></label>
        <label>Confirm password <input name="confirmation" type="password" autoComplete="new-password" minLength={12} maxLength={256} required /></label>
        <button className="button book-button" type="submit">Set password</button>
        <button type="button" onClick={() => { setBootstrap(null); setFeedback(""); }}>Back to sign in</button>
      </form>}
      <p role="status">{feedback}</p>
      {view === "ready" && <div className="admin-overview">
        <div>
          <span>ADMIN PORTAL</span>
          <h2>Enquiry inbox</h2>
          <p>New enquiries in the latest 100: {items.filter(item => item.status === "New").length}.</p>
          <p>Use the email link on an enquiry to respond through your email application.</p>
          <Link to="/admin/clients">View clients</Link>
        </div>
        <button className="button button-outline" type="button" onClick={() => void logout()}>Sign out</button>
      </div>}
      {view === "ready" && (items.length === 0 ? <p>No enquiries yet.</p> : items.map(item => (
        <article className="enquiry-item" key={item.id}>
          <h2>{item.name}</h2>
          <p><a href={`mailto:${encodeURIComponent(item.email)}?subject=${encodeURIComponent("Your Asistym enquiry")}`}>Reply to {item.email}</a> · {new Date(item.createdAt).toLocaleString()}</p>
          {item.goals && <p><strong>Goals:</strong> {item.goals}</p>}
          <p className="enquiry-message">{item.message}</p>
          <label>Status <select value={item.status} onChange={event => void update(item.id, event.target.value)}>{statuses.map(status => <option key={status}>{status}</option>)}</select></label>
        </article>
      )))}
    </div></section>
  );
}
