import { FormEvent, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { sendRequest } from "../../api";

export default function Onboarding() {
  const [token, setToken] = useState("");
  const [feedback, setFeedback] = useState("");
  const [complete, setComplete] = useState(false);
  const [busy, setBusy] = useState(false);

  useEffect(() => {
    const fragment = window.location.hash.slice(1);
    if (fragment) {
      setToken(fragment);
      window.history.replaceState(
        window.history.state,
        "",
        window.location.pathname,
      );
    }
  }, []);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token || busy) return;
    const data = new FormData(event.currentTarget);
    const password = String(data.get("password") ?? "");

    if (password !== data.get("confirmation")) {
      setFeedback("The passwords do not match.");
      return;
    }

    setBusy(true);
    setFeedback("");

    try {
      const response = await sendRequest("/api/invitations/accept", "POST", {
        token,
        password,
      });
      if (!response.ok) throw new Error();
      setToken("");
      setComplete(true);
    } catch {
      setFeedback(
        "Unable to complete setup. The invitation may be invalid, expired or already used.",
      );
    } finally {
      setBusy(false);
    }
  }

  return (
    <section className="section">
      <div className="container">
        <h1>Set up your account</h1>
        {complete ? (
          <div role="status">
            <p>Your account is ready.</p>
            <Link to="/client">Sign in to your client portal</Link>
          </div>
        ) : (
          <form className="enquiry-form" onSubmit={submit}>
            <p>
              Choose a password of 12 to 256 characters. The invitation can only
              be used once.
            </p>
            <label>
              New password{" "}
              <input
                type="password"
                name="password"
                autoComplete="new-password"
                minLength={12}
                maxLength={256}
                required
              />
            </label>
            <label>
              Confirm password{" "}
              <input
                type="password"
                name="confirmation"
                autoComplete="new-password"
                minLength={12}
                maxLength={256}
                required
              />
            </label>
            <button
              className="button book-button"
              type="submit"
              disabled={busy || !token}
            >
              {busy ? "Setting up…" : "Set up account"}
            </button>
            <p role="status">
              {feedback ||
                (!token
                  ? "Open the complete invitation link to continue."
                  : "")}
            </p>
          </form>
        )}
      </div>
    </section>
  );
}
