import { FormEvent, useCallback, useEffect, useRef, useState } from "react";
import { sendRequest } from "../api";
import TaskCalendar, { ScheduledEvent } from "./TaskCalendar";

type Task = { id: number; title: string; description?: string; dueAt: string | null; completedAt: string | null };
type Request = { id: number; message: string; status: string; createdAt: string };

export default function CalendarBooking({ clientId, tasks, onPendingCallsChange }: { clientId?: number; tasks: Task[]; onPendingCallsChange?: (count: number) => void }) {
  const admin = clientId !== undefined;
  const base = admin ? `/api/admin/clients/${clientId}/calendar` : "/api/client/calendar";
  const [events, setEvents] = useState<ScheduledEvent[]>([]);
  const [requests, setRequests] = useState<Request[]>([]);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const [busy, setBusy] = useState(false);
  const pendingChanged = useRef(onPendingCallsChange);
  pendingChanged.current = onPendingCallsChange;

  const load = useCallback(async () => {
    try {
      const response = await fetch(`${base}/`, { credentials: "same-origin", cache: "no-store" });
      if (!response.ok) throw new Error();
      const data = await response.json();
      if (admin) {
        if (!Array.isArray(data.events) || !Array.isArray(data.requests)) throw new Error();
        setEvents(data.events); setRequests(data.requests);
        pendingChanged.current?.(data.requests.filter((request: Request) => request.status === "Pending").length);
      }
      else {
        if (!Array.isArray(data)) throw new Error();
        setEvents(data);
        const requestResponse = await fetch(`${base}/requests`, { credentials: "same-origin", cache: "no-store" });
        if (!requestResponse.ok) throw new Error();
        setRequests(await requestResponse.json());
      }
      setError("");
    } catch { setError("Unable to load your calendar. Please try again."); }
  }, [admin, base]);

  useEffect(() => { void load(); }, [load]);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (busy) return;
    const form = event.currentTarget;
    const data = new FormData(form);
    const requestId = data.get("requestId");
    let body: unknown = { message: data.get("message") };
    if (admin) {
      const start = new Date(String(data.get("startsAt")));
      const end = new Date(String(data.get("endsAt")));
      if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) { setError("Enter a valid start and end time."); return; }
      body = { type: data.get("type"), title: data.get("title"), description: data.get("description"), startsAt: start.toISOString(), endsAt: end.toISOString(), callRequestId: requestId ? Number(requestId) : null };
    }
    setBusy(true); setError(""); setNotice("");
    try {
      const response = await sendRequest(`${base}/${admin ? "events" : "requests"}`, "POST", body);
      if (!response.ok) {
        const result = await response.json().catch(() => ({}));
        throw new Error(result.error || "Unable to save. Please try again.");
      }
      form.reset(); await load();
      setNotice(admin ? "Appointment scheduled." : "Your call request has been sent for review. It is not a confirmed booking.");
    } catch (failure) { setError(failure instanceof Error ? failure.message : "Unable to save."); }
    finally { setBusy(false); }
  }

  async function decline(id: number) {
    setBusy(true); setError("");
    try {
      const response = await sendRequest(`${base}/requests/${id}/decline`, "POST", {});
      if (!response.ok) throw new Error();
      await load(); setNotice("Call request declined.");
    } catch { setError("Unable to update the request. Please refresh and try again."); }
    finally { setBusy(false); }
  }

  return <section className="calendar-booking" id="calendar-booking">
    <TaskCalendar tasks={tasks} events={events} exportBase={base} />
    {error && <p role="alert">{error}</p>}{notice && <p role="status">{notice}</p>}
    {admin ? <>
      <h3>Call requests</h3>
      {requests.length === 0 ? <p>No call requests yet.</p> : <ul>{requests.map(request => <li key={request.id}><strong>{request.status}</strong> · {new Date(request.createdAt).toLocaleDateString("en-GB", { timeZone: "Europe/London" })}<p>{request.message}</p>{request.status === "Pending" && <button type="button" disabled={busy} onClick={() => void decline(request.id)}>Decline request</button>}</li>)}</ul>}
      <form className="enquiry-form" onSubmit={event => void submit(event)}>
        <h3>Schedule an appointment</h3>
        <p>Times use your device’s time zone. Confirm the date and time with the client.</p>
        <label>Type <select name="type"><option value="Call">Call</option><option value="Meeting">Meeting</option></select></label>
        <label>Related pending call request (optional) <select name="requestId"><option value="">None</option>{requests.filter(request => request.status === "Pending").map(request => <option key={request.id} value={request.id}>{request.message.slice(0, 80)}</option>)}</select></label>
        <label>Title <input name="title" maxLength={160} required /></label>
        <label>Details <textarea name="description" maxLength={4000} rows={3} /></label>
        <label>Starts <input name="startsAt" type="datetime-local" required /></label>
        <label>Ends <input name="endsAt" type="datetime-local" required /></label>
        <button className="button book-button" disabled={busy}>Schedule appointment</button>
      </form>
    </> : <form className="enquiry-form" onSubmit={event => void submit(event)}>
      <h3>Request a call</h3><p>Tell us when you would like to talk. Your practitioner will review your request; no time is booked until confirmed.</p>
      <label>Message <textarea name="message" maxLength={1000} rows={3} required /></label>
      <button className="button book-button" disabled={busy}>Send call request</button>
      {requests.length > 0 && <ul>{requests.map(request => <li key={request.id}>{request.message} — {request.status}</li>)}</ul>}
    </form>}
  </section>;
}
