import { fireEvent, render, screen } from "@testing-library/react";
import Contact from "../pages/static/Contact";

afterEach(() => jest.restoreAllMocks());

test("a failed CSRF request preserves the enquiry and a retry can succeed", async () => {
  const fetchMock = jest.spyOn(window, "fetch")
    .mockResolvedValueOnce({ ok: false } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => ({ token: "csrf" }) } as Response)
    .mockResolvedValueOnce({ ok: true } as Response);
  render(<Contact />);
  fireEvent.change(screen.getByLabelText(/^name/i), { target: { value: "Test Visitor" } });
  fireEvent.change(screen.getByLabelText(/^email address/i), { target: { value: "visitor@example.invalid" } });
  fireEvent.change(screen.getByLabelText(/^message/i), { target: { value: "A test enquiry" } });
  fireEvent.click(screen.getByLabelText(/i agree/i));
  fireEvent.click(screen.getByRole("button", { name: /send enquiry/i }));

  expect(await screen.findByText(/could not be sent/i)).toBeInTheDocument();
  expect(screen.getByLabelText(/^message/i)).toHaveValue("A test enquiry");
  expect(fetchMock.mock.calls.filter(([url]) => url === "/api/enquiries")).toHaveLength(0);
  fireEvent.click(screen.getByRole("button", { name: /send enquiry/i }));
  expect(await screen.findByText(/enquiry has been received/i)).toBeInTheDocument();
  expect(screen.getByLabelText(/^message/i)).toHaveValue("");
  const request = fetchMock.mock.calls.find(([url]) => url === "/api/enquiries");
  expect(request?.[1]?.headers).toEqual(expect.objectContaining({ "X-CSRF-TOKEN": "csrf" }));
  expect(JSON.parse(String(request?.[1]?.body))).toEqual(expect.objectContaining({ consent: true, message: "A test enquiry" }));
});
