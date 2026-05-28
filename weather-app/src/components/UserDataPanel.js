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

const normalizeCity = (value) => value.trim().toLocaleLowerCase("de-DE");
const normalizeCountry = (value) => (value.trim() || "DE").toUpperCase();

const isSameFavorite = (favorite, payload) =>
  normalizeCity(favorite.cityName) === normalizeCity(payload.cityName) &&
  normalizeCountry(favorite.countryCode) === normalizeCountry(payload.countryCode);

const getFriendlyErrorMessage = (error, fallback) => {
  if (error?.status === 409) {
    return "Diese Stadt ist bereits in deinen Favoriten.";
  }

  if (error?.status === 404) {
    return "Der Eintrag wurde nicht gefunden oder ist schon gelöscht.";
  }

  if (error?.status === 401) {
    return "Deine Anmeldung ist abgelaufen. Bitte melde dich erneut an.";
  }

  if (error?.status === 429) {
    return "Zu viele Anfragen. Bitte kurz warten und erneut versuchen.";
  }

  return fallback;
};

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
    setHistory(Array.isArray(data) ? data.slice(0, 3) : []);
  }, [authFetchJson]);

  const loadFavorites = useCallback(async () => {
    const data = await authFetchJson("http://localhost:5122/favorites/");
    setFavorites(Array.isArray(data) ? data : []);
  }, [authFetchJson]);

  useEffect(() => {
    let cancelled = false;

    Promise.allSettled([loadHistory(), loadFavorites()]).then(() => {
      if (cancelled) {
        return;
      }

      setMessage((current) => (current.startsWith("Daten konnten gerade nicht") ? "" : current));
    });

    return () => {
      cancelled = true;
    };
  }, [loadHistory, loadFavorites, historyRefreshKey]);

  const saveCurrentFavorite = async () => {
    if (!currentWeather?.city) {
      setMessage("Bitte zuerst Wetter für eine Stadt laden.");
      return;
    }

    try {
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
    } catch (error) {
      setMessage(getFriendlyErrorMessage(error, "Favorit konnte nicht gespeichert werden. Bitte versuche es erneut."));
    }
  };

  const saveFavorite = async (event) => {
    event.preventDefault();
    if (!favoriteForm.cityName.trim()) {
      setMessage("Bitte Stadt für den Favoriten eintragen.");
      return;
    }

    const payload = {
      cityName: favoriteForm.cityName.trim(),
      countryCode: normalizeCountry(favoriteForm.countryCode),
      latitude: null,
      longitude: null,
    };

    const selectedFavorite = favorites.find((favorite) => favorite.id === editingFavoriteId);
    if (selectedFavorite && isSameFavorite(selectedFavorite, payload)) {
      setFavoriteForm(emptyFavorite);
      setEditingFavoriteId("");
      setMessage("Keine Änderung notwendig.");
      return;
    }

    const url = editingFavoriteId
      ? `http://localhost:5122/favorites/${editingFavoriteId}`
      : "http://localhost:5122/favorites/";

    try {
      await authFetchJson(url, {
        method: editingFavoriteId ? "PUT" : "POST",
        body: JSON.stringify(payload),
      });

      setFavoriteForm(emptyFavorite);
      setEditingFavoriteId("");
      await loadFavorites();
      setMessage(editingFavoriteId ? "Favorit aktualisiert." : "Favorit gespeichert.");
    } catch (error) {
      setMessage(getFriendlyErrorMessage(error, "Favorit konnte nicht gespeichert werden. Bitte prüfe die Eingabe."));
    }
  };

  const editFavorite = (favorite) => {
    setEditingFavoriteId(favorite.id);
    setFavoriteForm({
      cityName: favorite.cityName,
      countryCode: favorite.countryCode,
    });
  };

  const deleteFavorite = async (favoriteId) => {
    const previousFavorites = favorites;
    setFavorites((current) => current.filter((favorite) => favorite.id !== favoriteId));

    try {
      await authFetchJson(`http://localhost:5122/favorites/${favoriteId}`, { method: "DELETE" });
      await loadFavorites().catch(() => {});
      setMessage("Favorit gelöscht.");
    } catch (error) {
      if (error?.status !== 404) {
        setFavorites(previousFavorites);
      }
      setMessage(getFriendlyErrorMessage(error, "Favorit konnte nicht gelöscht werden. Bitte versuche es erneut."));
    }
  };

  const deleteHistory = async (historyId) => {
    const previousHistory = history;
    setHistory((current) => current.filter((item) => item.id !== historyId));

    try {
      await authFetchJson(`http://localhost:5122/history/${historyId}`, { method: "DELETE" });
      await loadHistory().catch(() => {});
      setMessage("Verlaufseintrag gelöscht.");
    } catch (error) {
      if (error?.status !== 404) {
        setHistory(previousHistory);
      }
      setMessage(getFriendlyErrorMessage(error, "Verlaufseintrag konnte nicht gelöscht werden."));
    }
  };

  const changeTheme = async (nextTheme) => {
    if (nextTheme === themeName) {
      return;
    }

    const previousTheme = themeName;
    onThemeChange(nextTheme);

    try {
      const data = await authFetchJson("http://localhost:5122/theme/", {
        method: "PUT",
        body: JSON.stringify({ themeName: nextTheme }),
      });
      onThemeChange(data.themeName || nextTheme);
      setMessage("Theme gespeichert.");
    } catch (_) {
      onThemeChange(previousTheme);
      setMessage("Theme konnte nicht gespeichert werden. Bitte prüfe die Verbindung und versuche es erneut.");
    }
  };

  const loadCity = (cityName) => {
    onSelectCity(cityName);
  };

  return (
    <section className="user-data">
      <div className="user-data-header">
        <div>
          <h2>Verlauf und Favoriten</h2>
        </div>
        {message && <span>{message}</span>}
      </div>

      <div className="theme-picker" aria-label="Theme wählen">
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
                  Löschen
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
            <button type="submit">{editingFavoriteId ? "Ändern" : "Speichern"}</button>
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
                    Löschen
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
