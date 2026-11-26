import { render, screen } from "@testing-library/react";
import ClientAssessments from "../pages/admin/ClientAssessments";

jest.mock("recharts", () => {
  const React = require("react");
  const Container = ({ children }: { children: React.ReactNode }) => <div>{children}</div>;
  const Empty = () => null;
  return { ResponsiveContainer: Container, LineChart: Container, BarChart: Container, CartesianGrid: Empty, Legend: Empty, Line: Empty, Bar: Empty, Tooltip: Empty, XAxis: Empty, YAxis: Empty };
});

afterEach(() => jest.restoreAllMocks());

test("shows recorded assessments in an accessible table", async () => {
  const result = [{ id: 1, assessmentDate: "2030-01-01", physical: 6, dietary: 5, social: 4, emotional: 7, intellectual: 8, habitual: 3, financial: 5, preos: 6, total: 44, mean: 5.5 }];
  jest.spyOn(window, "fetch").mockResolvedValueOnce({ ok: true, json: async () => result } as Response);
  render(<ClientAssessments clientId={1} />);
  expect(await screen.findByText("2030-01-01")).toBeInTheDocument();
  expect(screen.getByText("5.5")).toBeInTheDocument();
});
