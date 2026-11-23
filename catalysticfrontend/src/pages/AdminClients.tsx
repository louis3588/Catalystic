import { FormEvent, useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { sendRequest } from "../api";

type Client = {
  id: number;
  firstName: string;
  lastName: string | null;
  email: string;
  isActive: boolean;
};
type Note = {
  id: number;
  content: string;
  createdAt?: string;
  sharedAt?: string;
};
type Task = {
  id: number;
  title: string;
  description: string;
  pillar: string;
  dueAt: string | null;
  completedAt: string | null;
};
type Detail = {
  client: Client;
  privateNotes: Note[];
  sharedNotes: Note[];
  tasks: Task[];
};

const pillars = [
  "Physical",
  "Dietary",
  "Social",
  "Emotional",
  "Intellectual",
  "Habitual",
  "Financial",
];

export default function AdminClients() {
  const [clients, setClients] = useState<Client[]>([]);
  const [detail, setDetail] = useState<Detail | null>(null);
  const [state, setState] = useState<"loading" | "ready" | "login" | "error">(
    "loading",
  );
  const [feedback, setFeedback] = useState("");
  const [busy, setBusy] = useState(false);
  const [invitationLink, setInvitationLink] = useState("");

  useEffect(() => {
    const controller = new AbortController();
    fetch("/api/admin/clients/", {
      credentials: "same-origin",
      cache: "no-store",
      signal: controller.signal,
    })
      .then(async (response) => {
        if (response.status === 401) {
          setState("login");
          return;
        }
        if (!response.ok) throw new Error();
        setClients(await response.json());
        setState("ready");
      })
      .catch(() => {
        if (!controller.signal.aborted) setState("error");
      });
    return () => controller.abort();
  }, []);

  const selectClient = useCallback(async (id: number) => {
    setDetail(null);
    setFeedback("Loading client…");
    try {
      const response = await fetch(`/api/admin/clients/${id}`, {
        credentials: "same-origin",
        cache: "no-store",
      });
      if (!response.ok) throw new Error();
      setDetail(await response.json());
      setFeedback("");
    } catch {
      setFeedback("Unable to load this client.");
    }
  }, []);

  async function submit(
    event: FormEvent<HTMLFormElement>,
    kind: "private-notes" | "shared-notes" | "tasks",
  ) {
    event.preventDefault();
    if (!detail || busy) return;
    const form = event.currentTarget;
    const data = new FormData(form);
    const body =
      kind === "tasks"
        ? {
            title: data.get("title"),
            description: data.get("description"),
            pillar: data.get("pillar"),
            dueAt: data.get("dueAt")
              ? new Date(`${data.get("dueAt")}T00:00:00Z`).toISOString()
              : null,
          }
        : { content: data.get("content") };

    setBusy(true);
    setFeedback("");
    try {
      const response = await sendRequest(
        `/api/admin/clients/${detail.client.id}/${kind}`,
        "POST",
        body,
      );
      if (!response.ok) throw new Error();
      form.reset();
      await selectClient(detail.client.id);
      setFeedback(
        kind === "private-notes"
          ? "Private note saved."
          : kind === "shared-notes"
            ? "Note shared with the client."
            : "Task assigned.",
      );
    } catch {
      setFeedback("Unable to save. Please try again.");
    } finally {
      setBusy(false);
    }
  }

  async function changeAccess() {
    if (!detail || busy) return;
    const clientId = detail.client.id;
    const active = !detail.client.isActive;
    setBusy(true);
    setFeedback("");

    try {
      const response = await sendRequest(
        `/api/admin/clients/${clientId}/access`,
        "PATCH",
        { active },
      );
      if (!response.ok) throw new Error();
      setClients((previous) =>
        previous.map((client) =>
          client.id === clientId ? { ...client, isActive: active } : client,
        ),
      );
      setDetail((previous) =>
        previous && previous.client.id === clientId
          ? { ...previous, client: { ...previous.client, isActive: active } }
          : previous,
      );
      setFeedback(
        active
          ? "Client access activated."
          : "Client access deactivated and existing sessions revoked.",
      );
    } catch {
      setFeedback(
        "Unable to change access. Activation requires a fully provisioned account.",
      );
    } finally {
      setBusy(false);
    }
  }

  async function inviteClient(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (busy) return;

    const form = event.currentTarget;
    const data = new FormData(form);
    setBusy(true);
    setFeedback("");
    setInvitationLink("");

    try {
      const response = await sendRequest("/api/admin/invitations", "POST", {
        firstName: data.get("firstName"),
        lastName: data.get("lastName"),
        email: data.get("email"),
        manuallyApproved: data.has("manuallyApproved"),
      });

      if (!response.ok) throw new Error();
      const result: { token: string } = await response.json();
      setInvitationLink(`${window.location.origin}/onboarding#${result.token}`);
      form.reset();
      setFeedback(
        "Invitation created. Share the link securely with the intended client.",
      );

      const clientsResponse = await fetch("/api/admin/clients/", {
        credentials: "same-origin",
        cache: "no-store",
      });
      if (clientsResponse.ok) setClients(await clientsResponse.json());
    } catch {
      setFeedback(
        "Unable to create the invitation. The email may already have an account or invitation.",
      );
    } finally {
      setBusy(false);
    }
  }

  return (
    <section className="section">
      <div className="container">
        <Link className="text-link" to="/admin">
          Back to enquiries
        </Link>
        <h1>Clients</h1>
        {state === "loading" && <p>Loading clients…</p>}
        {state === "login" && (
          <p>
            Please <Link to="/admin">sign in as an administrator</Link> to view
            clients.
          </p>
        )}
        {state === "error" && (
          <p role="alert">
            Unable to load clients. Check your connection and permissions.
          </p>
        )}
        {state === "ready" && (
          <>
            <section className="enquiry-item">
              <h2>Invite a client</h2>
              <p>
                For now, invitations require your manual approval. Payment is
                not checked by the system.
              </p>
              <form
                className="enquiry-form"
                onSubmit={(event) => void inviteClient(event)}
              >
                <label>
                  First name <input name="firstName" maxLength={64} required />
                </label>
                <label>
                  Last name (optional) <input name="lastName" maxLength={64} />
                </label>
                <label>
                  Email{" "}
                  <input name="email" type="email" maxLength={254} required />
                </label>
                <label className="check-label">
                  <input name="manuallyApproved" type="checkbox" required /> I
                  have personally approved this client for onboarding.
                </label>
                <button className="button book-button" disabled={busy}>
                  Create invitation
                </button>
              </form>
              {invitationLink && (
                <div className="invitation-result">
                  <label>
                    One-time invitation link{" "}
                    <input
                      value={invitationLink}
                      readOnly
                      onFocus={(event) => event.currentTarget.select()}
                    />
                  </label>
                  <p>
                    This link is shown once, expires after 48 hours, and grants
                    access to whoever uses it. Verify the recipient and share it
                    securely.
                  </p>
                  {(window.location.hostname === "localhost" ||
                    window.location.hostname === "127.0.0.1") && (
                    <p>
                      This local link only works on this device. Email delivery
                      is not configured.
                    </p>
                  )}
                  <button type="button" onClick={() => setInvitationLink("")}>
                    Hide link
                  </button>
                </div>
              )}
            </section>
            <p>
              Client accounts appear here after they have been provisioned.
              Showing up to 100 clients.
            </p>
            {clients.length === 0 ? (
              <p>No client accounts yet.</p>
            ) : (
              <ul className="client-list">
                {clients.map((client) => (
                  <li key={client.id}>
                    <button
                      type="button"
                      onClick={() => void selectClient(client.id)}
                    >
                      {client.firstName} {client.lastName} —{" "}
                      {client.isActive ? "Active" : "Inactive"}
                    </button>
                  </li>
                ))}
              </ul>
            )}
            <p role="status">{feedback}</p>
            {detail && (
              <div className="client-detail">
                <h2>
                  {detail.client.firstName} {detail.client.lastName}
                </h2>
                <p>{detail.client.email}</p>
                <p>
                  Account access:{" "}
                  {detail.client.isActive ? "Active" : "Inactive"}
                </p>
                <button
                  type="button"
                  className="button button-outline client-access-button"
                  disabled={busy}
                  onClick={() => void changeAccess()}
                >
                  {detail.client.isActive
                    ? "Deactivate access"
                    : "Activate access"}
                </button>
                <section className="enquiry-item">
                  <h3>Private practitioner notes</h3>
                  <p>Only administrators can see these notes.</p>
                  {detail.privateNotes.map((note) => (
                    <p className="enquiry-message" key={note.id}>
                      {note.content}
                    </p>
                  ))}
                  <form
                    className="enquiry-form"
                    onSubmit={(event) => void submit(event, "private-notes")}
                  >
                    <label>
                      Private note{" "}
                      <textarea
                        name="content"
                        rows={3}
                        maxLength={4000}
                        required
                      />
                    </label>
                    <button className="button book-button" disabled={busy}>
                      Save private note
                    </button>
                  </form>
                </section>
                <section className="enquiry-item">
                  <h3>Shared notes</h3>
                  <p>Sharing makes the note visible to this client.</p>
                  {detail.sharedNotes.map((note) => (
                    <p className="enquiry-message" key={note.id}>
                      {note.content}
                    </p>
                  ))}
                  <form
                    className="enquiry-form"
                    onSubmit={(event) => void submit(event, "shared-notes")}
                  >
                    <label>
                      Note to share{" "}
                      <textarea
                        name="content"
                        rows={3}
                        maxLength={4000}
                        required
                      />
                    </label>
                    <button className="button book-button" disabled={busy}>
                      Share note
                    </button>
                  </form>
                </section>
                <section className="enquiry-item">
                  <h3>Tasks and homework</h3>
                  {detail.tasks.map((task) => (
                    <div key={task.id}>
                      <strong>{task.title}</strong>
                      <p>
                        {task.pillar} ·{" "}
                        {task.completedAt ? "Completed" : "Assigned"}
                      </p>
                      {task.dueAt && (
                        <p>
                          Due:{" "}
                          {new Date(task.dueAt).toLocaleDateString("en-GB", {
                            timeZone: "UTC",
                          })}{" "}
                          (UTC)
                        </p>
                      )}
                      <p className="enquiry-message">{task.description}</p>
                    </div>
                  ))}
                  <form
                    className="enquiry-form"
                    onSubmit={(event) => void submit(event, "tasks")}
                  >
                    <label>
                      Title <input name="title" maxLength={160} required />
                    </label>
                    <label>
                      Description{" "}
                      <textarea name="description" rows={3} maxLength={4000} />
                    </label>
                    <label>
                      Pillar{" "}
                      <select name="pillar">
                        {pillars.map((pillar) => (
                          <option key={pillar}>{pillar}</option>
                        ))}
                      </select>
                    </label>
                    <label>
                      Due date in UTC (optional){" "}
                      <input name="dueAt" type="date" />
                    </label>
                    <button className="button book-button" disabled={busy}>
                      Assign task
                    </button>
                  </form>
                </section>
              </div>
            )}
          </>
        )}
      </div>
    </section>
  );
}
