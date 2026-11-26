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
  fireEvent.change(screen.getByLabelText(/next review date/i), { target: { value: "2030-01-02" } });
  fireEvent.click(screen.getByRole("button", { name: /submit weekly review/i }));
  expect(await screen.findByText(/review has been submitted/i)).toBeInTheDocument();
  const [, options] = fetchMock.mock.calls.find(([url]) => url === "/api/client/weekly-reviews/")!;
  const body = JSON.parse(String(options?.body));
  expect(body.financial).toBe(6);
  expect(body.nextReviewDate).toBe("2030-01-02");
  expect(body).not.toHaveProperty("preos");
});
