import "./LoginOptions.css";

// Connection names come from .env through Docker Compose. Empty connection
// values disable the corresponding provider button instead of breaking login.
const authConnections = [
  { label: "Google", envKey: "REACT_APP_AUTH0_CONNECTION_GOOGLE" },
  { label: "Apple", envKey: "REACT_APP_AUTH0_CONNECTION_APPLE" },
  { label: "Facebook", envKey: "REACT_APP_AUTH0_CONNECTION_FACEBOOK" },
  { label: "GitHub", envKey: "REACT_APP_AUTH0_CONNECTION_GITHUB" },
];

export default function LoginOptions({ loginWithRedirect }) {
  // Auth0 SPA login uses Authorization Code Flow with PKCE. The frontend sends
  // only public SPA values; secrets remain in Auth0/provider dashboards.
  const authParams = {
    audience: process.env.REACT_APP_AUTH0_AUDIENCE,
    scope: process.env.REACT_APP_AUTH0_SCOPE,
    redirect_uri: window.location.origin,
  };

  const loginUniversal = (screenHint) =>
    loginWithRedirect({
      authorizationParams: {
        ...authParams,
        ...(screenHint ? { screen_hint: screenHint } : {}),
      },
    });

  const loginWithConnection = (connection) =>
    loginWithRedirect({
      authorizationParams: {
        ...authParams,
        connection,
      },
    });

  return (
    <main className="login-page">
      <section className="login-panel">
        <h1>Wetter-Dashboard</h1>
        <div className="login-actions">
          <button type="button" onClick={() => loginUniversal()}>
            Anmelden
          </button>
          <button type="button" onClick={() => loginUniversal("signup")}>
            Registrieren
          </button>
        </div>
        <p className="login-separator">oder anmelden mit</p>
        <div className="login-provider-grid">
          {authConnections.map((provider) => {
            const connection = process.env[provider.envKey];
            return (
              <button
                key={provider.label}
                type="button"
                disabled={!connection}
                onClick={() => loginWithConnection(connection)}
              >
                {provider.label}
              </button>
            );
          })}
        </div>
      </section>
    </main>
  );
}
