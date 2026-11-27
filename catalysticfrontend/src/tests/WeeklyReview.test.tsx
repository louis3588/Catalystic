import { fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import WeeklyReview from "../pages/client/WeeklyReview";

afterEach(() => jest.restoreAllMocks());

test("client submits seven ratings and a next review date", async () => {
  const fetchMock = jest.spyOn(window, "fetch")
    .mockResolvedValueOnce({ ok: true, json: async () => ({ token: "csrf" }) } as Response)
    .mockResolvedValueOnce({ ok: true } as Response);
  render(<MemoryRouter><WeeklyReview /></MemoryRouter>);
  for (const label of ["physical", "dietary", "social", "emotional", "intellectual", "habitual", "financial"]) {
    fireEvent.change(screen.getByLabelText(label), { target: { value: "6" } });
  }
  const nextDate = screen.getAllByRole("radio")[0] as HTMLInputElement;
  fireEvent.click(nextDate);
  fireEvent.click(screen.getByRole("button", { name: /submit weekly review/i }));
  expect(await screen.findByText(/review has been submitted/i)).toBeInTheDocument();
  const [, options] = fetchMock.mock.calls.find(([url]) => url === "/api/client/weekly-reviews/")!;
  const body = JSON.parse(String(options?.body));
  expect(body.financial).toBe(6);
  expect(body.nextReviewDate).toBe(nextDate.value);
  expect(body).not.toHaveProperty("preos");
});

test.each([
  [409, /already awaiting completion/i],
  [401, /sign in to your client account/i],
  [403, /sign in to your client account/i],
  [500, /unable to submit your review/i],
])("weekly review reports a %s response and keeps the entered scores", async (status, expected) => {
  jest.spyOn(window, "fetch")
    .mockResolvedValueOnce({ ok: true, json: async () => ({ token: "csrf" }) } as Response)
    .mockResolvedValueOnce({ ok: false, status } as Response);
  render(<MemoryRouter><WeeklyReview /></MemoryRouter>);
  for (const label of ["physical", "dietary", "social", "emotional", "intellectual", "habitual", "financial"]) {
    fireEvent.change(screen.getByLabelText(label), { target: { value: "6.5" } });
  }
  fireEvent.click(screen.getAllByRole("radio")[0]);
  fireEvent.click(screen.getByRole("button", { name: /submit weekly review/i }));
  expect(await screen.findByText(expected)).toBeInTheDocument();
  expect(screen.getByLabelText("physical")).toHaveValue(6.5);
  expect(screen.getByRole("button", { name: /submit weekly review/i })).toBeEnabled();
});
