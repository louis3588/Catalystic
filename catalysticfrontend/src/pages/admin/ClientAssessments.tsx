import { useCallback, useEffect, useState } from "react";
import { CartesianGrid, Legend, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";

const metrics = [
  { key: "physical", label: "Physical", colour: "#136b42" },
  { key: "dietary", label: "Dietary", colour: "#a54e18" },
  { key: "social", label: "Social", colour: "#2459a6" },
  { key: "emotional", label: "Emotional", colour: "#a52d52" },
  { key: "intellectual", label: "Intellectual", colour: "#693a99" },
  { key: "habitual", label: "Habitual", colour: "#82630b" },
  { key: "financial", label: "Financial", colour: "#156e79" },
  { key: "preos", label: "PREOS", colour: "#525464" },
] as const;

type Assessment = { id: number; assessmentDate: string; mean: number; total: number } & Record<typeof metrics[number]["key"], number>;

export default function ClientAssessments({ clientId }: { clientId: number }) {
  const [data, setData] = useState<Assessment[]>([]);
  const [state, setState] = useState<"loading" | "ready" | "error">("loading");

  const load = useCallback(async (signal?: AbortSignal) => {
    try {
      const response = await fetch(`/api/admin/clients/${clientId}/assessments/`, { credentials: "same-origin", cache: "no-store", signal });
      if (!response.ok) throw new Error();
      const result: Assessment[] = await response.json();
      if (!signal?.aborted) { setData(result); setState("ready"); }
    } catch { if (!signal?.aborted) setState("error"); }
  }, [clientId]);

  useEffect(() => {
    const controller = new AbortController();
    setData([]);
    setState("loading");
    void load(controller.signal);
    return () => controller.abort();
  }, [load]);

  return <section className="assessment-section" aria-labelledby="assessments-title">
    <h3 id="assessments-title">Wellness assessments</h3>
    <p>Subjective coaching indicators, including the administrator-only PREOS rating.</p>
    {state === "loading" && <p role="status">Loading assessments…</p>}
    {state === "error" && <p role="alert">Unable to load assessments. <button type="button" onClick={() => { setState("loading"); void load(); }}>Try again</button></p>}
    {state === "ready" && data.length === 0 && <p>No assessments recorded yet.</p>}
    {state === "ready" && data.length > 0 && <>
      <div className="assessment-charts">
        <div className="assessment-chart-card"><h4>Scores over time</h4><p>Each pillar and PREOS, out of 10.</p>
          <div className="assessment-chart"><ResponsiveContainer width="100%" height="100%"><LineChart data={data} margin={{ top: 10, right: 14, bottom: 4, left: -20 }}>
            <CartesianGrid stroke="#d9e6d7" strokeDasharray="3 3" /><XAxis dataKey="assessmentDate" tick={{ fontSize: 11 }} minTickGap={20} /><YAxis domain={[0, 10]} tickCount={6} /><Tooltip position={{ y: 0 }} /><Legend />
            {metrics.map(({ key, label, colour }) => <Line key={key} type="linear" dataKey={key} name={label} stroke={colour} strokeWidth={2} dot={{ r: 3 }} isAnimationActive={false} />)}
          </LineChart></ResponsiveContainer></div>
        </div>
        <div className="assessment-chart-card"><h4>Average score over time</h4><p>Mean of the seven pillars and PREOS, out of 10.</p>
          <div className="assessment-chart"><ResponsiveContainer width="100%" height="100%"><LineChart data={data} margin={{ top: 10, right: 14, bottom: 4, left: -20 }}>
            <CartesianGrid stroke="#d9e6d7" strokeDasharray="3 3" /><XAxis dataKey="assessmentDate" tick={{ fontSize: 11 }} minTickGap={20} /><YAxis domain={[0, 10]} tickCount={6} /><Tooltip position={{ y: 0 }} /><Line type="linear" dataKey="mean" name="Average" stroke="#28744a" strokeWidth={2} dot={{ r: 3 }} isAnimationActive={false} />
          </LineChart></ResponsiveContainer></div>
        </div>
      </div>
      <details className="assessment-table"><summary>View assessment data</summary><div className="assessment-table-scroll"><table><caption>Assessment scores and averages</caption><thead><tr><th scope="col">Date</th>{metrics.map(m => <th scope="col" key={m.key}>{m.label}</th>)}<th scope="col">Mean /10</th></tr></thead><tbody>{data.map(a => <tr key={a.id}><th scope="row">{a.assessmentDate}</th>{metrics.map(m => <td key={m.key}>{a[m.key]}</td>)}<td>{a.mean}</td></tr>)}</tbody></table></div></details>
    </>}
    <p>New assessments are created after a client submits a weekly review and the discussion and PREOS rating are completed.</p>
  </section>;
}
