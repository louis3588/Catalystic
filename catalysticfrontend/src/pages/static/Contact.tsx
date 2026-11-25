import { FormEvent, useState } from "react";
import PageIntro from "../../components/PageIntro";
import { sendRequest } from "../../api";

function Contact() {
  const [state, setState] = useState<"idle" | "sending" | "sent" | "error">("idle");

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = event.currentTarget;
    const data = new FormData(form);
    setState("sending");

    try {
      const response = await sendRequest("/api/enquiries", "POST", {
        name: data.get("name"),
        email: data.get("email"),
        contactPreference: "Email",
        goals: "",
        message: data.get("message"),
        consent: data.has("consent"),
        website: data.get("website"),
      });

      if (!response.ok) throw new Error("Submission failed");
      form.reset();
      setState("sent");
    } catch {
      setState("error");
    }
  }

  return (
    <>
      <PageIntro eyebrow="Get in touch" title="Let’s start a conversation." description="Tell us a little about what you would like to work towards." />
      <section className="section">
        <div className="container content-grid contact-grid">
          <div>
            <h2>No perfect words <em>required.</em></h2>
            <p className="muted">Your enquiry will be reviewed by the practitioner, who can contact you by email. Please avoid sharing sensitive personal or medical details at this stage.</p>
          </div>
          <form className="enquiry-form" onSubmit={submit}>
            <label>Name <input name="name" autoComplete="name" maxLength={120} required /></label>
            <label>Email address <input name="email" type="email" autoComplete="email" maxLength={254} required /></label>
            <p>Preferred contact method: Email</p>
            <label>Message <textarea name="message" maxLength={2000} rows={5} required /></label>
            <div className="honeypot" aria-hidden="true"><label>Website <input name="website" tabIndex={-1} autoComplete="off" /></label></div>
            <label className="check-label"><input type="checkbox" name="consent" required /> I agree to Asistym using these details to respond to my enquiry.</label>
            <button className="button book-button" type="submit" disabled={state === "sending"}>{state === "sending" ? "Sending…" : "Send enquiry"}</button>
            <p role="status" aria-live="polite">{state === "sent" ? "Thank you. Your enquiry has been received." : state === "error" ? "Your enquiry could not be sent. Please try again later." : ""}</p>
          </form>
        </div>
      </section>
    </>
  );
}

export default Contact;
