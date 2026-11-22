import { Link } from "react-router-dom";
import PageIntro from "../components/PageIntro";

function Contact() {
  return (
    <>
      <PageIntro
        eyebrow="Get in touch"
        title="Let’s start a conversation."
        description="Every journey begins somewhere. This is a space to talk about what you’d like to work towards."
      />
      <section className="section">
        <div className="container content-grid">
          <div>
            <span className="eyebrow">
              <span aria-hidden="true" />
              Your next step
            </span>
            <h2>
              No perfect words
              <br />
              <em>required.</em>
            </h2>
            <p className="muted">
              You don’t need a complete plan to reach out. Start with what’s on
              your mind.
            </p>
          </div>
          <div className="notice large-notice">
            <span className="notice-icon" aria-hidden="true">
              ✳
            </span>
            <h2>Contact form coming soon.</h2>
            <p>
              We’re preparing a private enquiry form so your message can reach
              the practitioner directly. Please check back soon.
            </p>
            <p>
              For your privacy, please don’t share sensitive personal
              information until the enquiry service is available.
            </p>
            <Link className="text-link" to="/services">
              Explore services
            </Link>
          </div>
        </div>
      </section>
    </>
  );
}


export default Contact;
