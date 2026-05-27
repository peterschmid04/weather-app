import { useCallback, useEffect, useState } from "react";
import "./UserDataPanel.css";

const emptyFavorite = {
  cityName: "",
  countryCode: "DE",
};

const themes = [
  { id: "graphite", label: "Graphit" },
  { id: "sky", label: "Himmel" },
  { id: "forest", label: "Wald" },
  { id: "sunset", label: "Sonnenuntergang" },
];

export default function UserDataPanel({
  authFetchJson,
  currentWeather,
  currentCountry,
  historyRefreshKey,
  onSelectCity,
  themeName,
  onThemeChange,
}) {
  const [history, setHistory] = useState([]);
  const [favorites, setFavorites] = useState([]);
  const [favoriteForm, setFavoriteForm] = useState(emptyFavorite);
  const [editingFavoriteId, setEditingFavoriteId] = useState("");
  const [message, setMessage] = useState("");

  const loadHistory = useCallback(async () => {
    const data = await authFetchJson("http://localhost:5122/history/");
    setHistory(data);
  }, [authFetchJson]);

  const loadFavorites = useCallback(async () => {
    const data = await authFetchJson("http://localhost:5122/favorites/");
    setFavorites(data);
  }, [authFetchJson]);

  useEffect(() => {
    Promise.all([loadHistory(), loadFavorites()]).catch(() =>
      setMessage("Verlauf oder Favoriten konnten nicht geladen werden.")
    );
  }, [loadHistory, loadFavorites, historyRefreshKey]);

  const saveCurrentFavorite = async () => {
    if (!currentWeather?.city) {
      setMessage("Bitte zuerst Wetter fuer eine Stadt laden.");
      return;
    }

    await authFetchJson("http://localhost:5122/favorites/", {
      method: "POST",
      body: JSON.stringify({
        cityName: currentWeather.city,
        countryCode: currentCountry || "DE",
        latitude: currentWeather.lat ?? null,
        longitude: currentWeather.lon ?? null,
      }),
    });
    await loadFavorites();
    setMessage("Aktuelle Stadt als Favorit gespeichert.");
  };

  const saveFavorite = async (event) => {
    event.preventDefault();
    if (!favoriteForm.cityName.trim()) {
      setMessage("Bitte Stadt fuer den Favoriten eintragen.");
      return;
    }

    const payload = {
      cityName: favoriteForm.cityName.trim(),
      countryCode: favoriteForm.countryCode.trim() || "DE",
      latitude: null,
      longitude: null,
    };

    const url = editingFavoriteId
      ? `http://localhost:5122/favorites/${editingFavoriteId}`
      : "http://localhost:5122/favorites/";

    await authFetchJson(url, {
      method: editingFavoriteId ? "PUT" : "POST",
      body: JSON.stringify(payload),
    });

    setFavoriteForm(emptyFavorite);
    setEditingFavoriteId("");
    await loadFavorites();
    setMessage(editingFavoriteId ? "Favorit aktualisiert." : "Favorit gespeichert.");
  };

  const editFavorite = (favorite) => {
    setEditingFavoriteId(favorite.id);
    setFavoriteForm({
      cityName: favorite.cityName,
      countryCode: favorite.countryCode,
    });
  };

  const deleteFavorite = async (favoriteId) => {
    await authFetchJson(`http://localhost:5122/favorites/${favoriteId}`, { method: "DELETE" });
    await loadFavorites();
    setMessage("Favorit geloescht.");
  };

  const deleteHistory = async (historyId) => {
    await authFetchJson(`http://localhost:5122/history/${historyId}`, { method: "DELETE" });
    await loadHistory();
    setMessage("Verlaufseintrag geloescht.");
  };

  const changeTheme = async (nextTheme) => {
    const data = await authFetchJson("http://localhost:5122/theme/", {
      method: "PUT",
      body: JSON.stringify({ themeName: nextTheme }),
    });
    onThemeChange(data.themeName);
    setMessage("Theme gespeichert.");
  };

  const loadCity = (cityName) => {
    onSelectCity(cityName);
  };

  return (
    <section className="user-data">
      <div className="user-data-header">
        <div>
          <h2>Verlauf und Favoriten</h2>
          <p>Alles wird fuer deinen Auth0-Nutzer getrennt gespeichert.</p>
        </div>
        {message && <span>{message}</span>}
      </div>

      <div className="theme-picker" aria-label="Theme waehlen">
        {themes.map((theme) => (
          <button
            key={theme.id}
            type="button"
            className={themeName === theme.id ? "active" : ""}
            onClick={() => changeTheme(theme.id)}
          >
            {theme.label}
          </button>
        ))}
      </div>

      <div className="user-data-grid">
        <div className="saved-panel">
          <div className="panel-title">
            <h3>Suchverlauf</h3>
          </div>
          <div className="saved-list">
            {history.length === 0 && <p className="empty">Noch keine Suche gespeichert.</p>}
            {history.map((item) => (
              <article key={item.id}>
                <button type="button" onClick={() => loadCity(item.cityName)}>
                  {item.cityName}, {item.countryCode}
                </button>
                <small>{new Date(item.searchedAtUtc).toLocaleString("de-DE")}</small>
                <button type="button" className="quiet" onClick={() => deleteHistory(item.id)}>
                  Loeschen
                </button>
              </article>
            ))}
          </div>
        </div>

        <div className="saved-panel">
          <div className="panel-title">
            <h3>Favoriten</h3>
            <button type="button" onClick={saveCurrentFavorite}>
              Aktuelle Stadt
            </button>
          </div>

          <form className="favorite-form" onSubmit={saveFavorite}>
            <input
              value={favoriteForm.cityName}
              onChange={(event) => setFavoriteForm((current) => ({ ...current, cityName: event.target.value }))}
              placeholder="Stadt"
            />
            <input
              value={favoriteForm.countryCode}
              onChange={(event) => setFavoriteForm((current) => ({ ...current, countryCode: event.target.value.toUpperCase() }))}
              placeholder="DE"
              maxLength="2"
            />
            <button type="submit">{editingFavoriteId ? "Aendern" : "Speichern"}</button>
          </form>

          <div className="saved-list">
            {favorites.length === 0 && <p className="empty">Noch keine Favoriten gespeichert.</p>}
            {favorites.map((favorite) => (
              <article key={favorite.id}>
                <button type="button" onClick={() => loadCity(favorite.cityName)}>
                  {favorite.cityName}, {favorite.countryCode}
                </button>
                <div className="row-actions">
                  <button type="button" className="quiet" onClick={() => editFavorite(favorite)}>
                    Bearbeiten
                  </button>
                  <button type="button" className="quiet" onClick={() => deleteFavorite(favorite.id)}>
                    Loeschen
                  </button>
                </div>
              </article>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}
