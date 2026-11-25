import { Link } from "react-router-dom";
import PageIntro from "../../components/PageIntro";

function About() {
  return (
    <>
      <PageIntro
        eyebrow="About Asistym"
        title="Progress starts with a conversation."
        description="A practical, personal approach to building a life that feels more balanced and intentional."
      />
      <section className="section">
        <div className="container content-grid">
          <div>
            <span className="eyebrow">
              <span aria-hidden="true" />
              The approach
            </span>
            <h2>
              Support for the
              <br />
              <em>whole picture.</em>
            </h2>
          </div>
          <div className="prose">
            <p>
              Asistym is built around a simple idea: meaningful change begins
              with understanding where you are and deciding where you want to
              go.
            </p>
            <p>
              Through conversations, reflection and practical goals, the aim is
              to help you develop the confidence and habits to move forward
              independently.
            </p>
            <div className="notice">
              <strong>Practitioner introduction</strong>
              <p>An approved biography and photograph will be added here.</p>
            </div>
            <Link className="button" to="/contact">
              Get in touch
            </Link>
          </div>
        </div>
      </section>
    </>
  );
}


export default About;
