export type BlogLabel = { id: number; name: string; colour: string };

export default function BlogLabels({ labels, onRemove }: { labels: BlogLabel[]; onRemove?: (id: number) => void }) {
  return <div className="blog-labels">{labels.map(label => <span className="blog-label" key={label.id} style={{ borderColor: label.colour }}><span className="blog-label-dot" style={{ backgroundColor: label.colour }} aria-hidden="true" />{label.name}{onRemove && <button type="button" aria-label={`Remove ${label.name}`} onClick={() => onRemove(label.id)}>×</button>}</span>)}</div>;
}
