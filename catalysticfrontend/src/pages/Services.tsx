import { Link } from "react-router-dom";
import PageIntro from "../components/PageIntro";

function Services() {
  const services = [
    {
      name: "Single consultation",
      text: "A standalone conversation to explore your priorities and what support could look like.",
    },
    {
      name: "One-month programme",
      text: "A flexible month of structured support focused on your personal goals.",
    },
    {
      name: "Three-month commitment",
      text: "A longer commitment with space to build momentum and review your progress.",
    },
  ];
  return (
    <>
      <PageIntro
        eyebrow="Our services"
        title="Support that meets you where you are."
        description="Choose a starting point that feels right for your circumstances and goals."
      />
      <section className="section">
        <div className="container">
          <div className="service-grid">
            {services.map((service) => (
              <article className="service-card" key={service.name}>
                <span className="service-label">PERSONAL DEVELOPMENT</span>
                <h2>{service.name}</h2>
                <p>{service.text}</p>
                <span className="service-note">
                  Pricing and details to be confirmed
                </span>
                <Link className="text-link" to="/contact">
                  Enquire about this service
                </Link>
              </article>
            ))}
          </div>
          <p className="disclaimer">
            Asistym provides wellness and personal development support. It
            does not provide medical diagnosis or treatment.
          </p>
        </div>
      </section>
    </>
  );
}


export default Services;
