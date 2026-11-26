import { FormEvent, useState } from "react";
import { Link } from "react-router-dom";
import { sendRequest } from "../../api";

const metrics = ["physical", "dietary", "social", "emotional", "intellectual", "habitual", "financial"];

export default function WeeklyReview() {
  const [message, setMessage] = useState("");
  const [busy, setBusy] = useState(false);
  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (busy) return;
    const form = event.currentTarget;
    const fields = new FormData(form);
    const body: Record<string, string | number> = { nextReviewDate: String(fields.get("nextReviewDate")) };
    metrics.forEach(metric => { body[metric] = Number(fields.get(metric)); });
    setBusy(true);
    try {
      const response = await sendRequest("/api/client/weekly-reviews/", "POST", body);
      if (response.status === 409) { setMessage("A review is already awaiting completion, or one was submitted today."); return; }
      if (response.status === 401 || response.status === 403) { setMessage("Please sign in to your client account first."); return; }
      if (!response.ok) throw new Error();
      form.reset();
      setMessage("Your review has been submitted. Your practitioner will contact you to discuss it.");
    } catch { setMessage("Unable to submit your review. Choose a date within the next seven days and try again."); }
    finally { setBusy(false); }
  }
  return <section className="section"><div className="container"><Link to="/client">← Client portal</Link><h1>Weekly review</h1><p>Rate each area from 0 to 10. These are subjective coaching indicators.</p>
    <form className="enquiry-form" onSubmit={event => void submit(event)}>{metrics.map(metric => <label key={metric} className="review-label">{metric}<input name={metric} type="number" min="0" max="10" step="0.01" required /></label>)}
      <label>Next review date (within the next seven days, UTC) <input name="nextReviewDate" type="date" required /></label>
      <button className="button book-button" disabled={busy}>{busy ? "Submitting…" : "Submit weekly review"}</button><p role="status">{message}</p>
    </form>
  </div></section>;
}
