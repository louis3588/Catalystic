import { Link } from "react-router-dom";
import PageIntro from "../components/PageIntro";

function NotFound() {
  return (
    <>
      <PageIntro
        eyebrow="Page not found"
        title="Let’s get you back on track."
        description="We couldn’t find the page you were looking for."
      />
      <section className="section">
        <div className="container">
          <Link className="button" to="/">
            Back to home
          </Link>
        </div>
      </section>
    </>
  );
}


export default NotFound;
