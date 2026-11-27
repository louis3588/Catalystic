import { useEffect, useRef, useState } from "react";
import { CheckCircle2, CalendarDays } from "lucide-react";

type CalendarTask = { id: number; title: string; dueAt: string | null; completedAt: string | null; description?: string };
export type ScheduledEvent = { id: number; type: "Call" | "Meeting"; title: string; description: string; startsAt: string; endsAt: string };

function londonDate(value: string): string {
  const parts = new Intl.DateTimeFormat("en-GB", { timeZone: "Europe/London", year: "numeric", month: "2-digit", day: "2-digit" }).formatToParts(new Date(value));
  const part = (name: string) => parts.find(item => item.type === name)?.value;
  return `${part("year")}-${part("month")}-${part("day")}`;
}

export default function TaskCalendar({ tasks, events = [], exportBase }: { tasks: CalendarTask[]; events?: ScheduledEvent[]; exportBase?: string }) {
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
    <p>Upcoming tasks, reviews and appointments. Appointment times are shown in UK time.</p>
    <div className="calendar-scroll"><div className="calendar-grid">{["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"].map(day => <strong key={day}>{day}</strong>)}{Array.from({ length: offset }, (_, i) => <div className="calendar-day calendar-empty" key={`empty-${i}`} />)}{Array.from({ length: days }, (_, i) => {
      const date = `${year}-${String(monthIndex + 1).padStart(2, "0")}-${String(i + 1).padStart(2, "0")}`;
      const dayEvents = [
        ...dated.filter(task => task.dueAt?.slice(0, 10) === date).map(task => ({ ...task, type: task.id === -1 ? "Review" : "Task", startsAt: null as string | null, endsAt: null as string | null })),
        ...events.filter(item => londonDate(item.startsAt) === date).map(item => ({ ...item, dueAt: item.startsAt, completedAt: null })),
      ];
      return <div className="calendar-day" key={date}><span className="calendar-day-number">{i + 1}</span>{dayEvents.map(event => <div className="calendar-event-wrap" key={`${event.type}-${event.id}`} onMouseEnter={() => { if (pinned === null) setOpen(event.id); }} onMouseLeave={() => { if (pinned === null) setOpen(null); }} onBlur={e => { if (!e.currentTarget.contains(e.relatedTarget) && pinned === null) setOpen(null); }}>
        <button type="button" className={`calendar-event calendar-event-${event.type.toLowerCase()}`} aria-expanded={open === event.id} onFocus={() => { if (pinned === null) setOpen(event.id); }} onClick={() => { const next = pinned === event.id ? null : event.id; setPinned(next); setOpen(next); }} onKeyDown={e => { if (e.key === "Escape") { setPinned(null); setOpen(null); } }}>{event.title}</button>
        {open === event.id && <div className={`calendar-popover calendar-popover-${event.type.toLowerCase()}`} role="group" aria-label={`${event.title} details`}><CheckCircle2 className="calendar-watermark" aria-hidden="true" /><span>{event.type === "Review" ? "WEEKLY REVIEW" : event.type.toUpperCase()}</span><strong>{event.title}</strong><p><CalendarDays size={15} aria-hidden="true" /> {new Date(`${date}T12:00:00Z`).toLocaleDateString("en-GB", { timeZone: "UTC" })} · {event.startsAt ? `${new Date(event.startsAt).toLocaleTimeString("en-GB", { timeZone: "Europe/London", hour: "2-digit", minute: "2-digit" })}–${new Date(event.endsAt!).toLocaleTimeString("en-GB", { timeZone: "Europe/London", hour: "2-digit", minute: "2-digit" })}` : "All day"}</p>{event.description && <p>{event.description}</p>}{exportBase && event.id !== -1 ? <a className="calendar-export" href={`${exportBase}/events/${event.id}/file`}>Export event</a> : <button type="button" disabled title="Calendar export is not available for this event">Export event</button>}</div>}
      </div>)}</div>;
    })}</div></div>
  </section>;
}
