export async function csrfToken(): Promise<string> {
  const response = await fetch("/api/auth/csrf", { credentials: "same-origin", cache: "no-store" });

  if (!response.ok) {
    throw new Error("Unable to connect to the service.");
  }

  const data: { token: string } = await response.json();
  return data.token;
}

export async function sendRequest(path: string, method: string, body: unknown): Promise<Response> {
  const token = await csrfToken();

  return fetch(path, {
    method,
    credentials: "same-origin",
    headers: { "Content-Type": "application/json", "X-CSRF-TOKEN": token },
    body: JSON.stringify(body),
  });
}
