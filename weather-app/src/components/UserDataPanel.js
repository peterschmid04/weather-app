import { useCallback, useEffect, useState } from "react";
import "./UserDataPanel.css";
import { formatCityLocation } from "../utils/localizationUtils";
import { buildApiUrl } from "../utils/apiUtils";

const emptyFavorite = {
  cityName: "",
  countryCode: "DE",
};

const emptyShareForm = {
  stationId: "",
  email: "",
  permission: "write_measurements",
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
  stationsRefreshKey,
  selectedStationId,
  onSelectedStationChange,
  onSelectCity,
  themeName,
  onThemeChange,
  onFavoritesChanged,
}) {
  const [stations, setStations] = useState([]);
  const [shares, setShares] = useState({ outgoing: [], incoming: [] });
  const [shareForm, setShareForm] = useState(emptyShareForm);
  const [favorites, setFavorites] = useState([]);
  const [favoriteForm, setFavoriteForm] = useState(emptyFavorite);
  const [editingFavoriteId, setEditingFavoriteId] = useState("");
  const [message, setMessage] = useState("");

  const loadStations = useCallback(async () => {
    const data = await authFetchJson(buildApiUrl("/stations/"));
    const nextStations = Array.isArray(data) ? data : [];
    setStations(nextStations);

    if (!selectedStationId || !nextStations.some((station) => station.id === selectedStationId)) {
      onSelectedStationChange?.(nextStations[0]?.id || "");
    }

    setShareForm((current) => {
      if (current.stationId && nextStations.some((station) => station.id === current.stationId && station.isOwner)) {
        return current;
      }

      const nextOwnStation = nextStations.find((station) => station.isOwner);
      return { ...current, stationId: nextOwnStation?.id || "" };
    });
  }, [authFetchJson, onSelectedStationChange, selectedStationId]);

  const loadShares = useCallback(async () => {
    const data = await authFetchJson(buildApiUrl("/station-shares/"));
    setShares({
      outgoing: Array.isArray(data?.outgoing) ? data.outgoing : [],
      incoming: Array.isArray(data?.incoming) ? data.incoming : [],
    });
  }, [authFetchJson]);

  const loadFavorites = useCallback(async () => {
    const data = await authFetchJson(buildApiUrl("/favorites/"));
    setFavorites(Array.isArray(data) ? data : []);
  }, [authFetchJson]);

  useEffect(() => {
    let cancelled = false;

    Promise.allSettled([loadStations(), loadFavorites(), loadShares()]).then(() => {
      if (cancelled) {
        return;
      }

      setMessage((current) => (current.startsWith("Daten konnten gerade nicht") ? "" : current));
    });

    return () => {
      cancelled = true;
    };
  }, [loadStations, loadFavorites, loadShares, stationsRefreshKey]);

  const saveCurrentFavorite = async () => {
    if (!currentWeather?.city) {
      setMessage("Bitte zuerst Wetter für eine Stadt laden.");
      return;
    }

    try {
      await authFetchJson(buildApiUrl("/favorites/"), {
        method: "POST",
        body: JSON.stringify({
          cityName: currentWeather.city,
          countryCode: currentCountry || "DE",
          latitude: currentWeather.lat ?? null,
          longitude: currentWeather.lon ?? null,
        }),
      });
      await loadFavorites();
      onFavoritesChanged?.();
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
      ? buildApiUrl(`/favorites/${editingFavoriteId}`)
      : buildApiUrl("/favorites/");

    try {
      await authFetchJson(url, {
        method: editingFavoriteId ? "PUT" : "POST",
        body: JSON.stringify(payload),
      });

      setFavoriteForm(emptyFavorite);
      setEditingFavoriteId("");
      await loadFavorites();
      onFavoritesChanged?.();
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
      await authFetchJson(buildApiUrl(`/favorites/${favoriteId}`), { method: "DELETE" });
      await loadFavorites().catch(() => {});
      onFavoritesChanged?.();
      setMessage("Favorit gelöscht.");
    } catch (error) {
      if (error?.status !== 404) {
        setFavorites(previousFavorites);
      }
      setMessage(getFriendlyErrorMessage(error, "Favorit konnte nicht gelöscht werden. Bitte versuche es erneut."));
    }
  };

  const changeTheme = async (nextTheme) => {
    if (nextTheme === themeName) {
      return;
    }

    const previousTheme = themeName;
    onThemeChange(nextTheme);

    try {
      const data = await authFetchJson(buildApiUrl("/theme/"), {
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

  const saveShare = async (event) => {
    event.preventDefault();
    if (!shareForm.stationId) {
      setMessage("Bitte zuerst eine eigene Wetterstation auswählen.");
      return;
    }

    if (!shareForm.email.trim()) {
      setMessage("Bitte E-Mail-Adresse für die Freigabe eintragen.");
      return;
    }

    try {
      await authFetchJson(buildApiUrl("/station-shares/"), {
        method: "POST",
        body: JSON.stringify({
          weatherStationId: shareForm.stationId,
          email: shareForm.email.trim(),
          permission: shareForm.permission,
        }),
      });
      setShareForm((current) => ({ ...current, email: "" }));
      await loadShares();
      setMessage("Freigabe erstellt. Die andere Person kann sie nach dem Login annehmen.");
    } catch (error) {
      if (error?.status === 409) {
        setMessage("Diese Wetterstation ist bereits an diese E-Mail-Adresse freigegeben.");
        return;
      }

      setMessage(getFriendlyErrorMessage(error, error.message || "Freigabe konnte nicht erstellt werden."));
    }
  };

  const acceptShare = async (shareId) => {
    try {
      await authFetchJson(buildApiUrl(`/station-shares/${shareId}/accept`), { method: "POST" });
      await Promise.all([loadShares(), loadStations()]);
      setMessage("Freigabe angenommen.");
    } catch (error) {
      setMessage(getFriendlyErrorMessage(error, error.message || "Freigabe konnte nicht angenommen werden."));
    }
  };

  const deleteShare = async (shareId) => {
    try {
      await authFetchJson(buildApiUrl(`/station-shares/${shareId}`), { method: "DELETE" });
      await Promise.all([loadShares(), loadStations()]);
      setMessage("Freigabe entfernt.");
    } catch (error) {
      setMessage(getFriendlyErrorMessage(error, error.message || "Freigabe konnte nicht entfernt werden."));
    }
  };

  const ownStations = stations.filter((station) => station.isOwner);

  return (
    <section className="user-data">
      <div className="user-data-header">
        <div>
          <h2>Stationen und Favoriten</h2>
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
            <h3>Wetterstationen teilen</h3>
          </div>

          <form className="share-form" onSubmit={saveShare}>
            <select
              value={shareForm.stationId}
              onChange={(event) => setShareForm((current) => ({ ...current, stationId: event.target.value }))}
            >
              <option value="">Eigene Station auswählen</option>
              {ownStations.map((station) => (
                <option key={station.id} value={station.id}>
                  {station.name}
                </option>
              ))}
            </select>
            <input
              value={shareForm.email}
              onChange={(event) => setShareForm((current) => ({ ...current, email: event.target.value }))}
              placeholder="E-Mail des Auth0-Nutzers"
              type="email"
            />
            <select
              value={shareForm.permission}
              onChange={(event) => setShareForm((current) => ({ ...current, permission: event.target.value }))}
            >
              <option value="write_measurements">Messwerte eintragen</option>
              <option value="read">Nur ansehen</option>
            </select>
            <button type="submit">Teilen</button>
          </form>

          <div className="share-sections">
            <div className="share-section">
              <h4>Eingehende Freigaben</h4>
              {shares.incoming.length === 0 && <p className="empty">Keine Einladung erhalten.</p>}
              {shares.incoming.map((share) => (
                <article key={share.id} className="share-item">
                  <strong>{share.stationName}</strong>
                  <small>Von {share.ownerName}</small>
                  <small>Status: {share.status === "accepted" ? "angenommen" : "offen"}</small>
                  <div className="row-actions">
                    {share.status !== "accepted" && (
                      <button type="button" className="quiet" onClick={() => acceptShare(share.id)}>
                        Annehmen
                      </button>
                    )}
                    <button type="button" className="quiet" onClick={() => deleteShare(share.id)}>
                      Löschen
                    </button>
                  </div>
                </article>
              ))}
            </div>

            <div className="share-section">
              <h4>Von mir geteilt</h4>
              {shares.outgoing.length === 0 && <p className="empty">Noch nichts geteilt.</p>}
              {shares.outgoing.map((share) => (
                <article key={share.id} className="share-item">
                  <strong>{share.stationName}</strong>
                  <small>{share.sharedWithEmail}</small>
                  <small>Status: {share.status === "accepted" ? "angenommen" : "offen"}</small>
                  <button type="button" className="quiet" onClick={() => deleteShare(share.id)}>
                    Freigabe löschen
                  </button>
                </article>
              ))}
            </div>
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
                  {formatCityLocation(favorite.cityName, favorite.countryCode)}
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
