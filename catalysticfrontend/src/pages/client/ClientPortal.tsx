import { FormEvent, useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { sendRequest } from "../../api";

type SharedNote = {
  id: number;
  content: string;
  sharedAt: string;
};

type ClientTask = {
  id: number;
  title: string;
  description: string;
  pillar: string;
  dueAt: string | null;
  completedAt: string | null;
};

type ClientWork = {
  notes: SharedNote[];
  tasks: ClientTask[];
};

type View = "loading" | "login" | "ready" | "forbidden" | "error";

function formatDate(value: string): string {
  return new Intl.DateTimeFormat("en-GB", {
    day: "numeric",
    month: "long",
    year: "numeric",
    timeZone: "UTC",
  }).format(new Date(value));
}

export default function ClientPortal() {
  const [view, setView] = useState<View>("loading");
  const [work, setWork] = useState<ClientWork | null>(null);
  const [feedback, setFeedback] = useState("");
  const [busy, setBusy] = useState(false);

  const loadWork = useCallback(async (signal?: AbortSignal) => {
    try {
      const response = await fetch("/api/client/work/", {
        credentials: "same-origin",
        cache: "no-store",
        signal,
      });

      if (response.status === 401) {
        setWork(null);
        setView("login");
      } else if (response.status === 403) {
        setWork(null);
        setView("forbidden");
      } else if (!response.ok) {
        setView("error");
      } else {
        setWork(await response.json() as ClientWork);
        setView("ready");
      }
    } catch {
      if (!signal?.aborted) setView("error");
    }
  }, []);

  useEffect(() => {
    const controller = new AbortController();
    void loadWork(controller.signal);
    return () => controller.abort();
  }, [loadWork]);

  async function signIn(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (busy) return;
    const form = event.currentTarget;
    const data = new FormData(form);
    setBusy(true);
    setFeedback("");

    try {
      const response = await sendRequest("/api/auth/login", "POST", {
        email: data.get("email"),
        password: data.get("password"),
      });
      if (!response.ok) {
        setFeedback("Sign-in failed. Please check your details and try again.");
        return;
      }

      const result: { passwordChangeRequired: boolean } = await response.json();
      form.reset();
      if (result.passwordChangeRequired) {
        setFeedback("This account needs its initial password changed. Please use the administrator sign-in.");
        return;
      }
      await loadWork();
    } catch {
      setFeedback("Unable to sign in. Please check your connection and try again.");
    } finally {
      setBusy(false);
    }
  }

  async function signOut() {
    if (busy) return;
    setBusy(true);
    setFeedback("");
    try {
      const response = await sendRequest("/api/auth/logout", "POST", {});
      if (!response.ok) throw new Error();
      setWork(null);
      setView("login");
      setFeedback("You have been signed out.");
    } catch {
      setFeedback("Unable to sign out. Please try again.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <section className="section client-portal">
      <div className="container">
        <p className="client-portal-eyebrow">Your space</p>
        <h1>Client portal</h1>
        <p className="client-portal-intro">Your shared notes and next steps, all in one place.</p>

        {view === "loading" && <p role="status">Loading your portal…</p>}
        {view === "error" && (
          <div role="alert">
            <p>We couldn’t load your portal. Please check your connection and try again.</p>
            <button type="button" onClick={() => { setView("loading"); void loadWork(); }}>Try again</button>
          </div>
        )}
        {view === "login" && (
          <form className="enquiry-form client-login" onSubmit={(event) => void signIn(event)}>
            <h2>Sign in</h2>
            <p>Use the account you set up with your invitation.</p>
            <label>Email <input name="email" type="email" autoComplete="username" required /></label>
            <label>Password <input name="password" type="password" autoComplete="current-password" required /></label>
            <button className="button book-button" disabled={busy}>{busy ? "Signing in…" : "Sign in"}</button>
            <p role="status">{feedback}</p>
          </form>
        )}
        {view === "forbidden" && (
          <div className="client-portal-card">
            <p>This portal is available to client accounts. Sign out to use a different account.</p>
            <button type="button" onClick={() => void signOut()} disabled={busy}>Sign out</button>
            <p role="status">{feedback}</p>
            <Link to="/admin">Administrator sign-in</Link>
          </div>
        )}
        {view === "ready" && work && (
          <>
            <div className="client-portal-toolbar">
              <p>Keep your focus on the steps you’ve agreed with your practitioner.</p>
              <button type="button" onClick={() => void signOut()} disabled={busy}>Sign out</button>
            </div>
            <p role="status">{feedback}</p>
            <div className="client-portal-grid">
              <section aria-labelledby="client-tasks-heading">
                <h2 id="client-tasks-heading">Your tasks</h2>
                {work.tasks.length === 0 ? <p className="client-portal-card">No tasks assigned yet.</p> : (
                  <ul className="client-portal-list">
                    {work.tasks.map((task) => (
                      <li className="client-portal-card" key={task.id}>
                        <p className="client-portal-meta">{task.pillar} · {task.completedAt ? "Completed" : "Assigned"}</p>
                        <h3>{task.title}</h3>
                        {task.description && <p className="client-portal-content">{task.description}</p>}
                        {task.dueAt && <p className="client-portal-meta">Due {formatDate(task.dueAt)} (UTC)</p>}
                      </li>
                    ))}
                  </ul>
                )}
              </section>
              <section aria-labelledby="client-notes-heading">
                <h2 id="client-notes-heading">Shared notes</h2>
                {work.notes.length === 0 ? <p className="client-portal-card">No notes shared with you yet.</p> : (
                  <ul className="client-portal-list">
                    {work.notes.map((note) => (
                      <li className="client-portal-card" key={note.id}>
                        <p className="client-portal-meta">Shared {formatDate(note.sharedAt)}</p>
                        <p className="client-portal-content">{note.content}</p>
                      </li>
                    ))}
                  </ul>
                )}
              </section>
            </div>
          </>
        )}
      </div>
    </section>
  );
}
