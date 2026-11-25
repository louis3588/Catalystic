import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import PageIntro from "../../components/PageIntro";

type Post = { id: number; title: string; content: string; releaseTime: string };

export default function Blog() {
  const { id } = useParams();
  const [posts, setPosts] = useState<Post[] | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    const controller = new AbortController();
    setPosts(null);
    setError("");
    fetch(id ? `/api/blog/${encodeURIComponent(id)}` : "/api/blog/", { signal: controller.signal })
      .then(async (response) => {
        if (!response.ok) throw new Error();
        const data = await response.json();
        setPosts(id ? [data] : data);
      })
      .catch(() => { if (!controller.signal.aborted) setError("Unable to load this blog content."); });
    return () => controller.abort();
  }, [id]);

  return <>
    <PageIntro eyebrow="The blog" title="Ideas worth taking with you." description="Thoughts and practical ideas for your wellbeing journey." />
    <section className="section"><div className="container">
      {id && <Link to="/blog">← All blog posts</Link>}
      {error && <p role="alert">{error}</p>}
      {!error && !posts && <p role="status">Loading blog posts…</p>}
      {posts?.length === 0 && <p>No posts have been published yet.</p>}
      {posts?.map((post) => <article className="enquiry-item" key={post.id}>
        <h2>{id ? post.title : <Link to={`/blog/${post.id}`}>{post.title}</Link>}</h2>
        <p>{new Date(post.releaseTime).toLocaleDateString("en-GB", { timeZone: "UTC", day: "numeric", month: "long", year: "numeric" })}</p>
        <p className="enquiry-message">{id ? post.content : `${post.content.slice(0, 220)}${post.content.length > 220 ? "…" : ""}`}</p>
        {!id && <Link to={`/blog/${post.id}`}>Read article</Link>}
      </article>)}
    </div></section>
  </>;
}
