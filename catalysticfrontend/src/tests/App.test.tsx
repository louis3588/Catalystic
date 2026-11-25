import { fireEvent, render, screen, within } from "@testing-library/react";
import App from "../App";

beforeEach(() => {
  window.history.pushState({}, "", "/");
  window.scrollTo = jest.fn();
});

test("the homepage introduces the programme and seven pillars", () => {
  render(<App />);

  expect(
    screen.getByRole("heading", { name: /asistym for you/i }),
  ).toBeInTheDocument();
  expect(
    screen.getByRole("heading", { name: /seven pillars/i }),
  ).toBeInTheDocument();
  expect(
    screen.getByRole("heading", { name: "Financial" }),
  ).toBeInTheDocument();
});

test("public navigation opens the blog and closes the mobile menu", () => {
  window.fetch = jest.fn().mockResolvedValue({ ok: true, json: async () => [] });
  render(<App />);

  fireEvent.click(screen.getByRole("button", { name: /menu/i }));
  expect(screen.getByRole("button", { name: /close/i })).toHaveAttribute(
    "aria-expanded",
    "true",
  );
  fireEvent.click(
    within(
      screen.getByRole("navigation", { name: /main navigation/i }),
    ).getByRole("link", { name: "Blog" }),
  );
  expect(
    screen.getByRole("heading", { name: /ideas worth taking with you/i }),
  ).toBeInTheDocument();
  expect(screen.getByRole("button", { name: /menu/i })).toHaveAttribute(
    "aria-expanded",
    "false",
  );
});

test("the contact page collects an enquiry and requires consent", () => {
  window.history.pushState({}, "", "/contact");
  render(<App />);

  expect(
    screen.getByRole("button", { name: /send enquiry/i }),
  ).toBeInTheDocument();
  expect(screen.getByLabelText(/i agree/i)).toBeRequired();
});

test("submits a contact enquiry and confirms receipt", async () => {
  window.history.pushState({}, "", "/contact");
  const originalFetch = window.fetch;
  const mockFetch = jest.fn()
    .mockResolvedValueOnce({ ok: true, json: async () => ({ token: "test-token" }) } as Response)
    .mockResolvedValueOnce({ ok: true } as Response);
  window.fetch = mockFetch;
  render(<App />);

  fireEvent.change(screen.getByLabelText(/^name/i), { target: { value: "Test Visitor" } });
  fireEvent.change(screen.getByLabelText(/^email address/i), { target: { value: "test@example.com" } });
  fireEvent.change(screen.getByLabelText(/^message/i), { target: { value: "Hello" } });
  fireEvent.click(screen.getByLabelText(/i agree/i));
  fireEvent.click(screen.getByRole("button", { name: /send enquiry/i }));

  expect(await screen.findByText(/your enquiry has been received/i)).toBeInTheDocument();
  expect(mockFetch).toHaveBeenCalledWith("/api/enquiries", expect.objectContaining({ method: "POST" }));
  window.fetch = originalFetch;
});

test("administrator can replace an initial password without creating a session first", async () => {
  window.history.pushState({}, "", "/admin/enquiries");
  const originalFetch = window.fetch;
  const mockFetch = jest.fn()
    .mockResolvedValueOnce({ ok: false, status: 401 } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => ({ token: "test-token" }) } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => ({ passwordChangeRequired: true }) } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => ({ token: "test-token" }) } as Response)
    .mockResolvedValueOnce({ ok: true } as Response);
  window.fetch = mockFetch;
  render(<App />);

  fireEvent.change(await screen.findByLabelText(/^email$/i), { target: { value: "admin@example.com" } });
  fireEvent.change(screen.getByLabelText(/^password$/i), { target: { value: "temporary-password" } });
  fireEvent.click(screen.getByRole("button", { name: /sign in/i }));
  fireEvent.change(await screen.findByLabelText(/^new password$/i), { target: { value: "new password for test" } });
  fireEvent.change(screen.getByLabelText(/^confirm password$/i), { target: { value: "new password for test" } });
  fireEvent.click(screen.getByRole("button", { name: /set password/i }));

  expect(await screen.findByText(/password set/i)).toBeInTheDocument();
  window.fetch = originalFetch;
});

test("an administrator can see new enquiries and sign out", async () => {
  window.history.pushState({}, "", "/admin");
  const originalFetch = window.fetch;
  window.fetch = jest.fn()
    .mockResolvedValueOnce({ ok: true, json: async () => [{
      id: 1,
      name: "Test Visitor",
      email: "visitor@example.com",
      goals: "",
      message: "A message",
      status: "New",
      createdAt: "2026-01-01T12:00:00Z",
    }] } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => ({ token: "test-token" }) } as Response)
    .mockResolvedValueOnce({ ok: true } as Response);

  render(<App />);
  expect(await screen.findByRole("heading", { name: /hello daniel/i })).toBeInTheDocument();
  expect(screen.getByText("1")).toBeInTheDocument();
  expect(screen.getByRole("link", { name: /reply to visitor@example.com/i })).toHaveAttribute("href", expect.stringContaining("mailto:"));
  fireEvent.click(screen.getByRole("button", { name: /sign out/i }));
  expect(await screen.findByRole("heading", { name: /administrator sign in/i })).toBeInTheDocument();
  window.fetch = originalFetch;
});

test("an administrator can select a client and save a private note", async () => {
  window.history.pushState({}, "", "/admin/clients");
  const originalFetch = window.fetch;
  const client = { id: 10, firstName: "Alex", lastName: "Example", email: "alex@example.com", isActive: true };
  const detail = { client, privateNotes: [], sharedNotes: [], tasks: [] };
  window.fetch = jest.fn()
    .mockResolvedValueOnce({ ok: true, json: async () => [client] } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => detail } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => ({ token: "csrf" }) } as Response)
    .mockResolvedValueOnce({ ok: true } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => detail } as Response);

  render(<App />);
  fireEvent.click(await screen.findByRole("button", { name: /alex example/i }));
  fireEvent.change(await screen.findByLabelText(/^private note$/i), { target: { value: "Private observation" } });
  fireEvent.click(screen.getByRole("button", { name: /save private note/i }));
  expect(await screen.findByText(/private note saved/i)).toBeInTheDocument();
  window.fetch = originalFetch;
});

test("an invited client can choose a password and the credential is removed from the address bar", async () => {
  window.history.pushState({}, "", "/onboarding#test-invitation");
  const originalFetch = window.fetch;
  window.fetch = jest.fn()
    .mockResolvedValueOnce({ ok: true, json: async () => ({ token: "csrf" }) } as Response)
    .mockResolvedValueOnce({ ok: true } as Response);

  render(<App />);
  expect(window.location.hash).toBe("");
  fireEvent.change(screen.getByLabelText(/^new password$/i), { target: { value: "valid password example" } });
  fireEvent.change(screen.getByLabelText(/^confirm password$/i), { target: { value: "valid password example" } });
  fireEvent.click(screen.getByRole("button", { name: /set up account/i }));
  expect(await screen.findByText(/your account is ready/i)).toBeInTheDocument();
  window.fetch = originalFetch;
});

test("administrator explicitly approves and creates an invitation", async () => {
  window.history.pushState({}, "", "/admin/clients");
  const originalFetch = window.fetch;
  window.fetch = jest.fn()
    .mockResolvedValueOnce({ ok: true, json: async () => [] } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => ({ token: "csrf" }) } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => ({ token: "invitation-token" }) } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => [] } as Response);

  render(<App />);
  fireEvent.change(await screen.findByLabelText(/^first name/i), { target: { value: "Alex" } });
  fireEvent.change(screen.getByLabelText(/^email$/i), { target: { value: "alex@example.com" } });
  fireEvent.click(screen.getByLabelText(/personally approved/i));
  fireEvent.click(screen.getByRole("button", { name: /create invitation/i }));
  expect((await screen.findByLabelText(/one-time invitation link/i) as HTMLInputElement).value).toContain("/onboarding#invitation-token");
  window.fetch = originalFetch;
});

test.each([
  ["/about", /progress starts with a conversation/i],
  ["/services", /support that meets you where you are/i],
  ["/blog", /ideas worth taking with you/i],
  ["/shop", /resources for the road ahead/i],
  ["/missing-page", /let’s get you back on track/i],
])("%s renders its public page", (path, heading) => {
  if (path === "/blog") window.fetch = jest.fn().mockResolvedValue({ ok: true, json: async () => [] });
  window.history.pushState({}, "", path);
  render(<App />);

  expect(screen.getByRole("heading", { level: 1, name: heading })).toBeInTheDocument();
});
