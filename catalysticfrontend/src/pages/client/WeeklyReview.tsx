import { FormEvent, useState } from "react";
import { Link } from "react-router-dom";
import { sendRequest } from "../../api";
import { Dumbbell, Apple, Users, Heart, Brain, Repeat, CirclePoundSterling } from "lucide-react";

const metrics = ["physical", "dietary", "social", "emotional", "intellectual", "habitual", "financial"];
const icons = [Dumbbell, Apple, Users, Heart, Brain, Repeat, CirclePoundSterling];

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
  const dates = Array.from({ length: 7 }, (_, index) => { const date = new Date(); date.setUTCHours(12, 0, 0, 0); date.setUTCDate(date.getUTCDate() + index + 1); return date; });
  return <section className="section weekly-review-page"><div className="container"><Link to="/client">← Client portal</Link><h1>Weekly review</h1><p>Your answers help you reflect on your week and give your practitioner a starting point for your next conversation.</p>
    <form className="enquiry-form" onSubmit={event => void submit(event)}><div className="review-grid">{metrics.map((metric, index) => { const Icon = icons[index]; return <label key={metric} className="review-card review-label"><Icon aria-hidden="true" /><span>{metric}</span><input name={metric} type="number" min="0" max="10" step="0.01" placeholder="0–10" required /></label>; })}</div>
      <fieldset className="review-dates"><legend>Pick your next review date, ensuring it is within the next 7 days</legend>{dates.map(date => { const value = date.toISOString().slice(0, 10); return <label key={value}><input type="radio" name="nextReviewDate" value={value} required /><span>{date.toLocaleDateString("en-GB", { weekday: "short", day: "2-digit", month: "short", timeZone: "UTC" })}</span></label>; })}</fieldset>
      <button className="button book-button" disabled={busy}>{busy ? "Submitting…" : "Submit weekly review"}</button><p role="status">{message}</p>
    </form>
  </div></section>;
}
