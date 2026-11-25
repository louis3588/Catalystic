import { fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import Blog from "../pages/static/Blog";
import AdminBlog from "../pages/admin/AdminBlog";

afterEach(() => jest.restoreAllMocks());

test("public article renders content as text", async () => {
  jest.spyOn(window, "fetch").mockResolvedValueOnce({ ok: true, json: async () => ({ id: 1, title: "A post", content: "<script>alert(1)</script>", releaseTime: "2026-01-01T12:00:00Z" }) } as Response);
  render(<MemoryRouter initialEntries={["/blog/1"]}><Routes><Route path="/blog/:id" element={<Blog />} /></Routes></MemoryRouter>);
  expect(await screen.findByText("<script>alert(1)</script>")).toBeInTheDocument();
  expect(document.querySelector("script")).toBeNull();
});

test("admin can create a scheduled post", async () => {
  const fetchMock = jest.spyOn(window, "fetch")
    .mockResolvedValueOnce({ ok: true, json: async () => [] } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => ({ token: "csrf" }) } as Response)
    .mockResolvedValueOnce({ ok: true } as Response)
    .mockResolvedValueOnce({ ok: true, json: async () => [] } as Response);
  render(<MemoryRouter><AdminBlog /></MemoryRouter>);
  fireEvent.change(await screen.findByLabelText(/^title/i), { target: { value: "A title" } });
  fireEvent.change(screen.getByLabelText(/article content/i), { target: { value: "Some content" } });
  fireEvent.change(screen.getByLabelText(/visibility/i), { target: { value: "published" } });
  fireEvent.change(screen.getByLabelText(/publication date/i), { target: { value: "2030-01-01T12:00" } });
  fireEvent.click(screen.getByRole("button", { name: /save post/i }));
  expect(await screen.findByText(/blog post saved/i)).toBeInTheDocument();
  expect(fetchMock.mock.calls.some(([url, options]) => url === "/api/admin/blog/" && options?.method === "POST")).toBe(true);
});
