import { useEffect, useRef, useState } from "react";
import { CheckCircle2, CalendarDays } from "lucide-react";

type CalendarTask = { id: number; title: string; dueAt: string | null; completedAt: string | null; description?: string };

export default function TaskCalendar({ tasks }: { tasks: CalendarTask[] }) {
  const now = new Date();
  const [month, setMonth] = useState(() => new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), 1)));
  const [open, setOpen] = useState<number | null>(null);
  const [pinned, setPinned] = useState<number | null>(null);
  const container = useRef<HTMLElement>(null);
  const year = month.getUTCFullYear();
  const monthIndex = month.getUTCMonth();
  const days = new Date(Date.UTC(year, monthIndex + 1, 0)).getUTCDate();
  const offset = (new Date(Date.UTC(year, monthIndex, 1)).getUTCDay() + 6) % 7;
  const dated = tasks.filter(task => task.dueAt && !task.completedAt);

  useEffect(() => {
    function close(event: MouseEvent) { if (!(event.target as Element).closest?.(".calendar-event-wrap")) { setOpen(null); setPinned(null); } }
    document.addEventListener("click", close);
    return () => document.removeEventListener("click", close);
  }, []);

  return <section ref={container} className="task-calendar" aria-label="Calendar">
    <div className="calendar-heading"><div><span className="calendar-kicker">YOUR SCHEDULE</span><h3>Calendar</h3></div><div><button type="button" aria-label="Previous month" onClick={() => { setOpen(null); setMonth(new Date(Date.UTC(year, monthIndex - 1, 1))); }}>←</button> <strong>{month.toLocaleDateString("en-GB", { month: "long", year: "numeric", timeZone: "UTC" })}</strong> <button type="button" aria-label="Next month" onClick={() => { setOpen(null); setMonth(new Date(Date.UTC(year, monthIndex + 1, 1))); }}>→</button></div></div>
    <p>Upcoming tasks and reviews. Appointments and calendar export are coming later.</p>
    <div className="calendar-scroll"><div className="calendar-grid">{["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"].map(day => <strong key={day}>{day}</strong>)}{Array.from({ length: offset }, (_, i) => <div className="calendar-day calendar-empty" key={`empty-${i}`} />)}{Array.from({ length: days }, (_, i) => {
      const date = `${year}-${String(monthIndex + 1).padStart(2, "0")}-${String(i + 1).padStart(2, "0")}`;
      const events = dated.filter(task => task.dueAt?.slice(0, 10) === date);
      return <div className="calendar-day" key={date}><span className="calendar-day-number">{i + 1}</span>{events.map(event => <div className="calendar-event-wrap" key={event.id} onMouseEnter={() => { if (pinned === null) setOpen(event.id); }} onMouseLeave={() => { if (pinned === null) setOpen(null); }} onBlur={e => { if (!e.currentTarget.contains(e.relatedTarget) && pinned === null) setOpen(null); }}>
        <button type="button" className="calendar-event" aria-expanded={open === event.id} onFocus={() => { if (pinned === null) setOpen(event.id); }} onClick={() => { const next = pinned === event.id ? null : event.id; setPinned(next); setOpen(next); }} onKeyDown={e => { if (e.key === "Escape") { setPinned(null); setOpen(null); } }}>{event.title}</button>
        {open === event.id && <div className={`calendar-popover ${event.id === -1 ? "calendar-popover-review" : ""}`} role="group" aria-label={`${event.title} details`}><CheckCircle2 className="calendar-watermark" aria-hidden="true" /><span>{event.id === -1 ? "WEEKLY REVIEW" : "TASK"}</span><strong>{event.title}</strong><p><CalendarDays size={15} aria-hidden="true" /> {new Date(`${date}T12:00:00Z`).toLocaleDateString("en-GB", { timeZone: "UTC" })} · All day</p>{event.description && <p>{event.description}</p>}<button type="button" disabled title="Calendar export is not available yet">Export event · Coming soon</button></div>}
      </div>)}</div>;
    })}</div></div>
  </section>;
}
