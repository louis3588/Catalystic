import { useCallback, useEffect, useState } from "react";
import { sendRequest } from "../../api";

type Review = { id: number; reviewDate: string; nextReviewDate: string; status: string; physical: number; dietary: number; social: number; emotional: number; intellectual: number; habitual: number; financial: number };
const metrics = ["physical", "dietary", "social", "emotional", "intellectual", "habitual", "financial"] as const;

export default function WeeklyReviews({ clientId, onCompleted }: { clientId: number; onCompleted: () => void }) {
  const [reviews, setReviews] = useState<Review[] | null>(null);
  const [message, setMessage] = useState("");
  const [busy, setBusy] = useState(false);
  const load = useCallback(async () => {
    try {
      const response = await fetch(`/api/admin/clients/${clientId}/weekly-reviews/`, { credentials: "same-origin", cache: "no-store" });
      if (!response.ok) throw new Error();
      setReviews(await response.json());
    } catch { setMessage("Unable to load weekly reviews."); }
  }, [clientId]);
  useEffect(() => { void load(); }, [load]);
  async function action(id: number, actionName: "contacted" | "complete", preos?: number) {
    if (busy) return;
    setBusy(true); setMessage("");
    try {
      const response = await sendRequest(`/api/admin/clients/${clientId}/weekly-reviews/${id}/${actionName}`, "POST", actionName === "complete" ? { preos } : {});
      if (!response.ok) throw new Error();
      await load();
      setMessage(actionName === "complete" ? "Review completed and assessment recorded." : "Contact recorded.");
      if (actionName === "complete") onCompleted();
    } catch { setMessage("Unable to update the review. It may have changed or an assessment may already exist for that date."); }
    finally { setBusy(false); }
  }
  return <div><h3>Weekly reviews</h3>{reviews === null && !message && <p>Loading reviews…</p>}{reviews?.length === 0 && <p>No weekly reviews yet.</p>}
    {reviews?.map(review => <article className="client-note-card" key={review.id}><h4>{review.reviewDate}</h4><p>{review.status === "PendingContact" ? "Contact client to discuss" : review.status === "Contacted" ? "Contacted — PREOS needed" : "Completed"}</p><p>Next review: {review.nextReviewDate}</p><dl>{metrics.map(metric => <div key={metric} className="review-label"><dt>{metric}</dt><dd>{review[metric]} / 10</dd></div>)}</dl>
      {review.status === "PendingContact" && <button type="button" disabled={busy} onClick={() => void action(review.id, "contacted")}>Mark discussion completed</button>}
      {review.status === "Contacted" && <form onSubmit={event => { event.preventDefault(); const value = new FormData(event.currentTarget).get("preos"); void action(review.id, "complete", Number(value)); }}><label>PREOS <input name="preos" type="number" min="0" max="10" step="0.01" required /></label> <button disabled={busy}>Complete review</button></form>}
    </article>)}<p role="status">{message}</p></div>;
}
