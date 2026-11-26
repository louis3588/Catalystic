import { FormEvent, useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { sendRequest } from "../../api";
import BlogLabels, { BlogLabel } from "../../components/BlogLabels";

type Post = { id: number; title: string; content: string; isPublished: boolean; releaseTime: string; labels: BlogLabel[] };

export default function AdminBlog() {
  const [posts, setPosts] = useState<Post[]>([]);
  const [view, setView] = useState<"loading" | "ready" | "denied" | "error">("loading");
  const [editing, setEditing] = useState<Post | null>(null);
  const [message, setMessage] = useState("");
  const [busy, setBusy] = useState(false);
  const [labels, setLabels] = useState<BlogLabel[]>([]);
  const [selected, setSelected] = useState<number[]>([]);
  const [editingLabel, setEditingLabel] = useState<BlogLabel | null>(null);

  const load = useCallback(async () => {
    try {
      const response = await fetch("/api/admin/blog/", { credentials: "same-origin", cache: "no-store" });
      if (response.status === 401 || response.status === 403) { setView("denied"); return; }
      if (!response.ok) throw new Error();
      setPosts(await response.json());
      const labelResponse = await fetch("/api/admin/blog/labels", { credentials: "same-origin", cache: "no-store" });
      if (!labelResponse.ok) throw new Error();
      setLabels(await labelResponse.json());
      setView("ready");
    } catch { setView("error"); }
  }, []);

  useEffect(() => { void load(); }, [load]);

  async function save(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (busy) return;
    const form = event.currentTarget;
    const data = new FormData(form);
    const release = String(data.get("releaseTime") || "");
    setBusy(true);
    setMessage("");
    try {
      const response = await sendRequest(editing ? `/api/admin/blog/${editing.id}` : "/api/admin/blog/", editing ? "PUT" : "POST", {
        title: data.get("title"), content: data.get("content"),
        isPublished: data.get("publication") === "published",
        releaseTime: release ? new Date(release).toISOString() : null,
        labelIds: selected,
      });
      if (!response.ok) throw new Error();
      form.reset();
      setEditing(null);
      setSelected([]);
      await load();
      setMessage("Blog post saved.");
    } catch { setMessage("Unable to save the blog post. Please try again."); }
    finally { setBusy(false); }
  }

  async function saveLabel(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (busy) return;
    const form = event.currentTarget;
    const data = new FormData(form);
    setBusy(true);
    try {
      const response = await sendRequest(editingLabel ? `/api/admin/blog/labels/${editingLabel.id}` : "/api/admin/blog/labels", editingLabel ? "PUT" : "POST", { name: data.get("name"), colour: data.get("colour") });
      if (!response.ok) throw new Error();
      form.reset(); setEditingLabel(null); await load(); setMessage("Label saved.");
    } catch { setMessage("Unable to save the label."); }
    finally { setBusy(false); }
  }

  async function remove(post: Post) {
    if (busy || !window.confirm(`Delete “${post.title}”?`)) return;
    setBusy(true);
    try {
      const response = await sendRequest(`/api/admin/blog/${post.id}`, "DELETE", {});
      if (!response.ok) throw new Error();
      if (editing?.id === post.id) setEditing(null);
      await load();
      setMessage("Blog post deleted.");
    } catch { setMessage("Unable to delete the blog post."); }
    finally { setBusy(false); }
  }

  return <section className="section admin-area"><div className="container">
    <Link to="/admin">← Admin dashboard</Link><h1>Manage blog</h1>
    {view === "loading" && <p role="status">Loading posts…</p>}
    {view === "denied" && <p>Administrator access is required. <Link to="/admin">Sign in</Link></p>}
    {view === "error" && <p role="alert">Unable to load posts.</p>}
    {view === "ready" && <>
      <details className="enquiry-item"><summary>Manage labels</summary>
        <form className="enquiry-form" key={editingLabel?.id ?? "new-label"} onSubmit={event => void saveLabel(event)}>
          <h2>{editingLabel ? "Edit label" : "Create label"}</h2>
          <label>Label name <input name="name" maxLength={64} defaultValue={editingLabel?.name} required /></label>
          <label>Label colour <input name="colour" type="color" defaultValue={editingLabel?.colour || "#28744a"} required /></label>
          <button className="button book-button" disabled={busy}>Save label</button>
          {editingLabel && <button type="button" onClick={() => setEditingLabel(null)}>Cancel editing label</button>}
        </form>
        {labels.map(label => <div className="blog-label-row" key={label.id}><BlogLabels labels={[label]} /><button type="button" onClick={() => setEditingLabel(label)}>Edit {label.name}</button></div>)}
      </details>
      <form key={editing?.id ?? "new"} className="enquiry-form" onSubmit={(event) => void save(event)}>
        <h2>{editing ? "Edit post" : "Create post"}</h2>
        <label>Title <input name="title" defaultValue={editing?.title} maxLength={128} required /></label>
        <label>Article content <textarea name="content" defaultValue={editing?.content} rows={12} maxLength={8192} required /></label>
        <fieldset><legend>Labels</legend><p>Use × to remove a label, then save the post to apply changes.</p><BlogLabels labels={labels.filter(label => selected.includes(label.id))} onRemove={id => setSelected(previous => previous.filter(value => value !== id))} />
          <label>Add label <select value="" onChange={event => { const id = Number(event.target.value); if (id) setSelected(previous => [...previous, id]); }}><option value="">Choose a label</option>{labels.filter(label => !selected.includes(label.id)).map(label => <option key={label.id} value={label.id}>{label.name}</option>)}</select></label>
        </fieldset>
        <label>Visibility <select name="publication" defaultValue={editing?.isPublished ? "published" : "draft"}><option value="draft">Draft</option><option value="published">Publish</option></select></label>
        <label>Publication date and time (your local time; optional) <input name="releaseTime" type="datetime-local" defaultValue={editing ? new Date(new Date(editing.releaseTime).getTime() - new Date(editing.releaseTime).getTimezoneOffset() * 60000).toISOString().slice(0, 16) : ""} /></label>
        <p>Leave the date empty to publish immediately when visibility is set to Publish. Future dates publish automatically when due.</p>
        <button className="button book-button" disabled={busy}>Save post</button>
        {editing && <button type="button" onClick={() => { setEditing(null); setSelected([]); }}>Cancel editing</button>}
      </form>
      <p role="status">{message}</p><h2>All posts</h2>
      {posts.length === 0 && <p>No posts yet.</p>}
      {posts.map((post) => <article className="enquiry-item" key={post.id}><h3>{post.title}</h3><BlogLabels labels={post.labels || []} /><p>{!post.isPublished ? "Draft" : new Date(post.releaseTime) > new Date() ? "Scheduled" : "Published"}</p><button type="button" onClick={() => { setEditing(post); setSelected((post.labels || []).map(label => label.id)); }}>Edit</button> <button type="button" disabled={busy} onClick={() => void remove(post)}>Delete</button></article>)}
    </>}
  </div></section>;
}
