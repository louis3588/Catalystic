import { FormEvent, useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { sendRequest } from "../../api";
import CalendarBooking from "../../components/CalendarBooking";
import { CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";

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
  firstName?: string;
  nextReviewDate?: string | null;
  notes: SharedNote[];
  tasks: ClientTask[];
};

type View = "loading" | "login" | "ready" | "forbidden" | "error";

function formatDate(value: string): string {
  return new Intl.DateTimeFormat("en-GB", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    timeZone: "UTC",
  }).format(new Date(value));
}

export default function ClientPortal() {
  const [view, setView] = useState<View>("loading");
  const [work, setWork] = useState<ClientWork | null>(null);
  const [feedback, setFeedback] = useState("");
  const [busy, setBusy] = useState(false);
  const [scores, setScores] = useState<{ assessmentDate: string; score: number }[] | null>(null);
  const [scoresError, setScoresError] = useState(false);

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

  useEffect(() => {
    if (view !== "ready") { setScores(null); return; }
    const controller = new AbortController();
    setScoresError(false);
    fetch("/api/client/assessments", { credentials: "same-origin", cache: "no-store", signal: controller.signal })
      .then(async response => { if (!response.ok) throw new Error(); return response.json(); })
      .then(result => { if (!controller.signal.aborted) setScores(result); })
      .catch(() => { if (!controller.signal.aborted) setScoresError(true); });
    return () => controller.abort();
  }, [view]);

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
        <h1>{view === "ready" && work ? `Welcome${work.firstName ? `, ${work.firstName}` : ""}` : "Client portal"}</h1>
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
            <nav className="client-quick-links" aria-label="Client shortcuts"><Link to="/client/weekly-review">Start weekly review →</Link><a href="#calendar-booking">Request a call →</a></nav>
            <p role="status">{feedback}</p>
            {scoresError && <p role="alert">Unable to load your score history.</p>}
            {scores && <section className="client-score-section" aria-labelledby="score-history-heading">
              <div className="client-score-summary"><div><span>Current score</span><strong>{scores.length ? `${scores[scores.length - 1].score.toFixed(2)} / 10` : "Not yet recorded"}</strong></div><div><span>Average of earlier reviews</span><strong>{scores.length > 1 ? `${(scores.slice(0, -1).reduce((sum, item) => sum + item.score, 0) / (scores.length - 1)).toFixed(2)} / 10` : "No earlier reviews"}</strong></div></div>
              <h2 id="score-history-heading">Your score over time</h2><p>Mean of your seven wellness pillars. These are subjective coaching indicators.</p>
              {scores.length ? <div className="assessment-chart" role="img" aria-label={`Score history: ${scores.map(item => `${formatDate(item.assessmentDate)}: ${item.score.toFixed(2)} out of 10`).join(", ")}`}><ResponsiveContainer width="100%" height="100%"><LineChart data={scores} margin={{ top: 10, right: 14, bottom: 4, left: -20 }}><CartesianGrid stroke="#d9e6d7" strokeDasharray="3 3" /><XAxis dataKey="assessmentDate" tick={{ fontSize: 11 }} minTickGap={20} tickFormatter={formatDate} /><YAxis domain={[0, 10]} tickCount={6} /><Tooltip position={{ y: 0 }} labelFormatter={(value) => formatDate(String(value))} formatter={(value) => typeof value === "number" ? value.toFixed(2) : value} /><Line type="linear" dataKey="score" name="Current score" stroke="#28744a" strokeWidth={2} dot={{ r: 3 }} isAnimationActive={false} /></LineChart></ResponsiveContainer></div> : <p>No assessments recorded yet.</p>}
            </section>}
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
                        {task.dueAt && <p className="client-portal-meta">Due {formatDate(task.dueAt)}</p>}
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
            <CalendarBooking tasks={work.nextReviewDate ? [...work.tasks, { id: -1, title: "Weekly review", dueAt: work.nextReviewDate, completedAt: null }] : work.tasks} />
          </>
        )}
      </div>
    </section>
  );
}
