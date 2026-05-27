import "./LoginOptions.css";

const authConnections = [
  { label: "Google", envKey: "REACT_APP_AUTH0_CONNECTION_GOOGLE" },
  { label: "Apple", envKey: "REACT_APP_AUTH0_CONNECTION_APPLE" },
  { label: "Facebook", envKey: "REACT_APP_AUTH0_CONNECTION_FACEBOOK" },
  { label: "GitHub", envKey: "REACT_APP_AUTH0_CONNECTION_GITHUB" },
];

export default function LoginOptions({ loginWithRedirect }) {
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
        <h1>Weather Dashboard</h1>
        <div className="login-actions">
          <button type="button" onClick={() => loginUniversal()}>
            Login
          </button>
          <button type="button" onClick={() => loginUniversal("signup")}>
            Registrieren
          </button>
        </div>
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
