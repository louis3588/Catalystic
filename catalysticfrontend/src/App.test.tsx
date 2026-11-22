import { fireEvent, render, screen, within } from "@testing-library/react";
import App from "./App";

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

test("public navigation opens placeholder pages and closes the mobile menu", () => {
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
    screen.getByRole("heading", { name: /blog is taking shape/i }),
  ).toBeInTheDocument();
  expect(screen.getByRole("button", { name: /menu/i })).toHaveAttribute(
    "aria-expanded",
    "false",
  );
});

test("the contact page explains that the form is not yet available", () => {
  window.history.pushState({}, "", "/contact");
  render(<App />);

  expect(
    screen.getByRole("heading", { name: /contact form coming soon/i }),
  ).toBeInTheDocument();
  expect(
    screen.queryByRole("button", { name: /send/i }),
  ).not.toBeInTheDocument();
});

test.each([
  ["/about", /progress starts with a conversation/i],
  ["/services", /support that meets you where you are/i],
  ["/blog", /ideas worth taking with you/i],
  ["/shop", /resources for the road ahead/i],
  ["/missing-page", /let’s get you back on track/i],
])("%s renders its public page", (path, heading) => {
  window.history.pushState({}, "", path);
  render(<App />);

  expect(screen.getByRole("heading", { level: 1, name: heading })).toBeInTheDocument();
});
