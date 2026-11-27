import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import PageIntro from "../../components/PageIntro";
import BlogLabels, { BlogLabel } from "../../components/BlogLabels";
import BlogCard from "../../components/BlogCard";

type Post = { id: number; title: string; content: string; releaseTime: string; labels: BlogLabel[] };

export default function Blog() {
  const { id } = useParams();
  const [posts, setPosts] = useState<Post[] | null>(null);
  const [error, setError] = useState("");
  const [selectedLabel, setSelectedLabel] = useState("");

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
      {!id && posts && posts.length > 0 && <label className="blog-filter">Filter by label <select value={selectedLabel} onChange={event => setSelectedLabel(event.target.value)}><option value="">All labels</option>{Array.from(new Map(posts.flatMap(post => post.labels || []).map(label => [label.id, label])).values()).sort((a, b) => a.name.localeCompare(b.name)).map(label => <option key={label.id} value={label.id}>{label.name}</option>)}</select></label>}
      {posts?.length === 0 && <p>No posts have been published yet.</p>}
      {posts && selectedLabel && !posts.some(post => (post.labels || []).some(label => String(label.id) === selectedLabel)) && <p>No posts match this label.</p>}
      {posts?.filter(post => !selectedLabel || (post.labels || []).some(label => String(label.id) === selectedLabel)).map((post) => !id ? <BlogCard key={post.id} {...post} labels={post.labels || []} /> : <article className="enquiry-item" key={post.id}>
        <h2>{id ? post.title : <Link to={`/blog/${post.id}`}>{post.title}</Link>}</h2>
        <BlogLabels labels={post.labels || []} />
        <p>{new Date(post.releaseTime).toLocaleDateString("en-GB", { timeZone: "UTC", day: "numeric", month: "long", year: "numeric" })}</p>
        <p className="enquiry-message">{id ? post.content : `${post.content.slice(0, 220)}${post.content.length > 220 ? "…" : ""}`}</p>
      </article>)}
    </div></section>
  </>;
}
