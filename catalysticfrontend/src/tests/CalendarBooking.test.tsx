import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import CalendarBooking from "../components/CalendarBooking";

afterEach(() => jest.restoreAllMocks());

test("a client can request a call without creating a booking", async () => {
  const fetchMock = jest.spyOn(window, "fetch").mockImplementation(async (input, init) => {
    const path = String(input);
    if (path === "/api/auth/csrf") return { ok: true, json: async () => ({ token: "csrf" }) } as Response;
    if (path === "/api/client/calendar/requests" && init?.method === "POST") return { ok: true } as Response;
    return { ok: true, json: async () => [] } as Response;
  });
  render(<CalendarBooking tasks={[]} />);
  fireEvent.change(screen.getByLabelText(/^message/i), { target: { value: "A call next week please" } });
  fireEvent.click(screen.getByRole("button", { name: /send call request/i }));
  expect(await screen.findByText(/not a confirmed booking/i)).toBeInTheDocument();
  expect(fetchMock).toHaveBeenCalledWith("/api/client/calendar/requests", expect.objectContaining({
    method: "POST", body: JSON.stringify({ message: "A call next week please" }),
  }));
});

test("an administrator can schedule an appointment against a pending request", async () => {
  const fetchMock = jest.spyOn(window, "fetch").mockImplementation(async (input, init) => {
    const path = String(input);
    if (path === "/api/auth/csrf") return { ok: true, json: async () => ({ token: "csrf" }) } as Response;
    if (path.endsWith("/events") && init?.method === "POST") return { ok: true } as Response;
    return { ok: true, json: async () => ({ events: [], requests: [{ id: 3, message: "Please call", status: "Pending", createdAt: "2026-01-01T10:00:00Z" }] }) } as Response;
  });
  render(<CalendarBooking clientId={4} tasks={[]} />);
  await screen.findByRole("option", { name: "Please call" });
  fireEvent.change(screen.getByLabelText(/related pending call request/i), { target: { value: "3" } });
  fireEvent.change(screen.getByLabelText(/^title/i), { target: { value: "Review" } });
  fireEvent.change(screen.getByLabelText(/^starts/i), { target: { value: "2099-01-01T10:00" } });
  fireEvent.change(screen.getByLabelText(/^ends/i), { target: { value: "2099-01-01T11:00" } });
  fireEvent.click(screen.getByRole("button", { name: /schedule appointment/i }));
  await waitFor(() => expect(fetchMock).toHaveBeenCalledWith("/api/admin/clients/4/calendar/events", expect.objectContaining({ method: "POST", body: expect.stringContaining('"callRequestId":3') })));
  expect(await screen.findByText("Appointment scheduled.")).toBeInTheDocument();
});

test("a scheduling conflict is shown and leaves the administrator's input intact", async () => {
  jest.spyOn(window, "fetch").mockImplementation(async (input, init) => {
    const path = String(input);
    if (path === "/api/auth/csrf") return { ok: true, json: async () => ({ token: "csrf" }) } as Response;
    if (init?.method === "POST") return { ok: false, json: async () => ({ error: "That time overlaps an existing appointment." }) } as Response;
    return { ok: true, json: async () => ({ events: [], requests: [] }) } as Response;
  });
  render(<CalendarBooking clientId={4} tasks={[]} />);
  fireEvent.change(screen.getByLabelText(/^title/i), { target: { value: "Review" } });
  fireEvent.change(screen.getByLabelText(/^starts/i), { target: { value: "2099-01-01T10:00" } });
  fireEvent.change(screen.getByLabelText(/^ends/i), { target: { value: "2099-01-01T11:00" } });
  fireEvent.click(screen.getByRole("button", { name: /schedule appointment/i }));
  expect(await screen.findByRole("alert")).toHaveTextContent(/overlaps an existing appointment/i);
  expect(screen.getByLabelText(/^title/i)).toHaveValue("Review");
  expect(screen.getByRole("button", { name: /schedule appointment/i })).toBeEnabled();
});
