import { Link } from "react-router-dom";
import PageIntro from "./PageIntro";

function EmptyPage({ type }: { type: "blog" | "shop" }) {
  const blog = type === "blog";
  return (
    <>
      <PageIntro
        eyebrow={blog ? "The blog" : "The shop"}
        title={
          blog
            ? "Ideas worth taking with you."
            : "Resources for the road ahead."
        }
        description={
          blog
            ? "Thoughtful perspectives on wellbeing, growth and everyday life."
            : "Books and resources to support your journey, at your own pace."
        }
      />
      <section className="section">
        <div className="container">
          <div className="empty-state">
            <span className="empty-icon" aria-hidden="true">
              {blog ? "✳" : "▤"}
            </span>
            <span className="eyebrow">
              <span aria-hidden="true" />
              Coming soon
            </span>
            <h2>
              {blog
                ? "The blog is taking shape."
                : "The shop is on its way."}
            </h2>
            <p>
              {blog
                ? "Articles from Asistym will appear here when they’re ready to share."
                : "Products and resources will appear here when they become available."}
            </p>
            <Link className="button button-outline" to="/">
              Back to home
            </Link>
          </div>
        </div>
      </section>
    </>
  );
}


export default EmptyPage;
