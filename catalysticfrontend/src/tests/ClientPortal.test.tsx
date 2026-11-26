import { fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import ClientPortal from "../pages/client/ClientPortal";

afterEach(() => {
  jest.restoreAllMocks();
});

test("client signs in, sees assigned work, and signs out", async () => {
  const fetchMock = jest.spyOn(window, "fetch")
    .mockResolvedValueOnce({ status: 401 } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => ({ token: "csrf" }) } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => ({ passwordChangeRequired: false }) } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => ({
      notes: [{ id: 1, content: "Your shared note", sharedAt: "2026-01-01T12:00:00Z" }],
      tasks: [{ id: 2, title: "Make a plan", description: "Take a first step", pillar: "Physical", dueAt: null, completedAt: null }],
    }) } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => [] } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => ({ token: "csrf" }) } as Response)
    .mockResolvedValueOnce({ ok: true } as Response);

  render(<MemoryRouter><ClientPortal /></MemoryRouter>);
  fireEvent.change(await screen.findByLabelText(/^email/i), { target: { value: "client@example.invalid" } });
  fireEvent.change(screen.getByLabelText(/^password/i), { target: { value: "test password" } });
  fireEvent.click(screen.getByRole("button", { name: /^sign in$/i }));

  expect(await screen.findByText("Your shared note")).toBeInTheDocument();
  expect(screen.getByText("Make a plan")).toBeInTheDocument();
  expect(fetchMock).toHaveBeenCalledWith("/api/auth/login", expect.objectContaining({ method: "POST" }));

  fireEvent.click(screen.getByRole("button", { name: /sign out/i }));
  expect(await screen.findByText(/you have been signed out/i)).toBeInTheDocument();
  expect(screen.queryByText("Your shared note")).not.toBeInTheDocument();
});

test("shows empty states and handles a forbidden session", async () => {
  jest.spyOn(window, "fetch")
    .mockResolvedValueOnce({ ok: true, json: async () => ({ notes: [], tasks: [] }) } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => [] } as Response);
  const { unmount } = render(<MemoryRouter><ClientPortal /></MemoryRouter>);
  expect(await screen.findByText(/no tasks assigned yet/i)).toBeInTheDocument();
  expect(screen.getByText(/no notes shared with you yet/i)).toBeInTheDocument();
  unmount();

  jest.restoreAllMocks();
  jest.spyOn(window, "fetch").mockResolvedValueOnce({ status: 403 } as Response);
  render(<MemoryRouter><ClientPortal /></MemoryRouter>);
  expect(await screen.findByText(/available to client accounts/i)).toBeInTheDocument();
});
