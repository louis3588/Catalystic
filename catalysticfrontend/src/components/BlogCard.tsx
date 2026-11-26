import { Link } from "react-router-dom";
import BlogLabels, { BlogLabel } from "./BlogLabels";

type BlogCardProps = { id: number; title: string; content: string; releaseTime: string; labels: BlogLabel[] };

export default function BlogCard({ id, title, content, releaseTime, labels }: BlogCardProps) {
  return <article className="blog-card"><div className="blog-card-top"><span>FROM ASISTYM</span><time dateTime={releaseTime}>{new Date(releaseTime).toLocaleDateString("en-GB", { timeZone: "UTC", day: "2-digit", month: "2-digit", year: "numeric" })}</time></div><h2><Link to={`/blog/${id}`}>{title}</Link></h2><BlogLabels labels={labels} /><p>{content.length > 220 ? `${content.slice(0, 220)}…` : content}</p><Link className="blog-card-link" to={`/blog/${id}`}>Read article <span aria-hidden="true">↗</span></Link></article>;
}
