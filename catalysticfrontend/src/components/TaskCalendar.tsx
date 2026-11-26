import { useState } from "react";

type CalendarTask = { id: number; title: string; dueAt: string | null; completedAt: string | null };

export default function TaskCalendar({ tasks }: { tasks: CalendarTask[] }) {
  const now = new Date();
  const [month, setMonth] = useState(() => new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), 1)));
  const year = month.getUTCFullYear();
  const monthIndex = month.getUTCMonth();
  const days = new Date(Date.UTC(year, monthIndex + 1, 0)).getUTCDate();
  const offset = (new Date(Date.UTC(year, monthIndex, 1)).getUTCDay() + 6) % 7;
  const dated = tasks.filter(task => task.dueAt && !task.completedAt);

  return <section className="task-calendar" aria-label="Task calendar">
    <div className="calendar-heading"><h3>Calendar</h3><div><button type="button" aria-label="Previous month" onClick={() => setMonth(new Date(Date.UTC(year, monthIndex - 1, 1)))}>←</button> <strong>{month.toLocaleDateString("en-GB", { month: "long", year: "numeric", timeZone: "UTC" })}</strong> <button type="button" aria-label="Next month" onClick={() => setMonth(new Date(Date.UTC(year, monthIndex + 1, 1)))}>→</button></div></div>
    <p>Task due dates are shown in UTC. Appointments and calendar sync are not available yet.</p>
    <div className="calendar-grid" aria-hidden="true">{["M", "T", "W", "T", "F", "S", "S"].map((day, i) => <strong key={i}>{day}</strong>)}{Array.from({ length: offset }, (_, i) => <span key={`empty-${i}`} />)}{Array.from({ length: days }, (_, i) => { const date = `${year}-${String(monthIndex + 1).padStart(2, "0")}-${String(i + 1).padStart(2, "0")}`; return <span className={dated.some(task => task.dueAt?.slice(0, 10) === date) ? "calendar-has-event" : ""} key={date}>{i + 1}</span>; })}</div>
    <h4>Due this month</h4>
    {dated.filter(task => task.dueAt?.slice(0, 7) === `${year}-${String(monthIndex + 1).padStart(2, "0")}`).length === 0 ? <p>No tasks due this month.</p> : <ul>{dated.filter(task => task.dueAt?.slice(0, 7) === `${year}-${String(monthIndex + 1).padStart(2, "0")}`).map(task => <li key={task.id}>{task.dueAt?.slice(0, 10)} · {task.title}</li>)}</ul>}
  </section>;
}
