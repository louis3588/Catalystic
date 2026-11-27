import { fireEvent, render, screen } from "@testing-library/react";
import TaskCalendar from "../components/TaskCalendar";

test("shows a task on its day and opens and closes its details", () => {
  const date = new Date();
  const dueAt = new Date(Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), 15)).toISOString();
  render(<TaskCalendar tasks={[{ id: 7, title: "Plan a walk", description: "Choose a route", dueAt, completedAt: null }]} />);
  const event = screen.getByRole("button", { name: "Plan a walk" });
  fireEvent.click(event);
  expect(screen.getByRole("group", { name: "Plan a walk details" })).toHaveTextContent("Choose a route");
  expect(screen.getByRole("button", { name: /export event/i })).toBeDisabled();
  fireEvent.keyDown(event, { key: "Escape" });
  expect(screen.queryByRole("group", { name: "Plan a walk details" })).not.toBeInTheDocument();
});

test("provides an authenticated export link for an appointment", () => {
  const date = new Date();
  const start = new Date(Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), 15, 10)).toISOString();
  const end = new Date(Date.UTC(date.getUTCFullYear(), date.getUTCMonth(), 15, 11)).toISOString();
  render(<TaskCalendar tasks={[]} events={[{ id: 9, type: "Call", title: "Review call", description: "Discuss the week", startsAt: start, endsAt: end }]} exportBase="/api/client/calendar" />);
  fireEvent.click(screen.getByRole("button", { name: "Review call" }));
  expect(screen.getByRole("link", { name: "Export event" })).toHaveAttribute("href", "/api/client/calendar/events/9/file");
});
