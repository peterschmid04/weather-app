import React, { useCallback, useEffect, useMemo, useState } from "react";
import "./Sidebar.css";
import { formatCityLocation, getCitySuggestions, translateWeatherDescription } from "../utils/localizationUtils";
import { buildApiUrl } from "../utils/apiUtils";

const convertTemperature = (temp, isCelsius) => isCelsius ? temp : ((temp * 9 / 5) + 32).toFixed(1);

export default function Sidebar({
  city,
  setCity,
  handleSubmit,
  weather,
  currentDay,
  currentTime,
  isCelsius,
  searchMessage,
  authFetchJson,
  historyRefreshKey,
  favoritesRefreshKey,
  onSelectCity,
  onHistoryChanged,
}) {
  const [showSavedItems, setShowSavedItems] = useState(false);
  const [quickHistory, setQuickHistory] = useState([]);
  const [quickFavorites, setQuickFavorites] = useState([]);
  const description = translateWeatherDescription(weather?.description);
  const citySuggestions = useMemo(() => getCitySuggestions(city), [city]);

  const loadSavedItems = useCallback(async () => {
    if (!authFetchJson) {
      return;
    }

    const [historyResult, favoritesResult] = await Promise.allSettled([
      authFetchJson(buildApiUrl("/history/")),
      authFetchJson(buildApiUrl("/favorites/")),
    ]);

    if (historyResult.status === "fulfilled") {
      setQuickHistory(Array.isArray(historyResult.value) ? historyResult.value.slice(0, 3) : []);
    }

    if (favoritesResult.status === "fulfilled") {
      setQuickFavorites(Array.isArray(favoritesResult.value) ? favoritesResult.value.slice(0, 5) : []);
    }
  }, [authFetchJson]);

  useEffect(() => {
    loadSavedItems();
  }, [loadSavedItems, historyRefreshKey, favoritesRefreshKey]);

  const selectCity = (nextCity, displayCity = nextCity) => {
    setShowSavedItems(false);
    onSelectCity?.(nextCity, displayCity);
  };

  const deleteHistory = async (historyId) => {
    setQuickHistory((current) => current.filter((item) => item.id !== historyId));

    try {
      await authFetchJson(buildApiUrl(`/history/${historyId}`), { method: "DELETE" });
      onHistoryChanged?.();
    } catch (_) {
      loadSavedItems();
    }
  };

  return (
    <form className="sidebar" onSubmit={handleSubmit}>
      <div className="search-area">
        {searchMessage && <p className="search-message">{searchMessage}</p>}
        <input
          type="text"
          className="cityInput"
          placeholder="Stadt suchen..."
          value={city}
          onChange={(event) => setCity(event.target.value)}
          onFocus={() => {
            setShowSavedItems(true);
            loadSavedItems();
          }}
          onBlur={() => setTimeout(() => setShowSavedItems(false), 120)}
        />

        {showSavedItems && (
          <div className="quick-search-panel" onMouseDown={(event) => event.preventDefault()}>
            {citySuggestions.length > 0 && (
              <div className="quick-section">
                <p className="quick-section-title">Vorschläge</p>
                {citySuggestions.map((suggestion) => (
                  <button
                    key={`${suggestion.cityName}-${suggestion.countryCode}`}
                    type="button"
                    className="quick-item"
                    onClick={() => selectCity(suggestion.query, suggestion.cityName)}
                  >
                    {formatCityLocation(suggestion.cityName, suggestion.countryCode)}
                  </button>
                ))}
              </div>
            )}

            <div className="quick-section">
              <p className="quick-section-title">Letzte Suchen</p>
              {quickHistory.length === 0 && <span className="quick-empty">Noch kein Verlauf</span>}
              {quickHistory.map((item) => (
                <div key={item.id} className="quick-history-row">
                  <button type="button" className="quick-item" onClick={() => selectCity(item.queryText || item.cityName)}>
                    {formatCityLocation(item.queryText || item.cityName, item.countryCode)}
                  </button>
                  <button type="button" className="quick-delete" onClick={() => deleteHistory(item.id)}>
                    Löschen
                  </button>
                </div>
              ))}
            </div>
          </div>
        )}

        <div className="quick-favorites-card">
          <p className="quick-section-title">Favoriten</p>
          {quickFavorites.length === 0 && <span className="quick-empty">Noch keine Favoriten</span>}
          {quickFavorites.map((favorite) => (
            <button key={favorite.id} type="button" className="quick-item" onClick={() => selectCity(favorite.cityName)}>
              {formatCityLocation(favorite.cityName, favorite.countryCode)}
            </button>
          ))}
        </div>
      </div>

      {weather && (
        <div className="sidebar-weather">
          <img className="image" src={weather.image} alt={weather.description} />
          <div className="temp">
            {convertTemperature(weather.temp, isCelsius)}&deg;{isCelsius ? "C" : "F"}
          </div>
          <div className="date">
            <p className="currentDay">{currentDay}</p>
            <p className="time">{currentTime}</p>
          </div>
          <div className="description">
            <img className="icon" src={weather.icon} alt="Wettersymbol" />
            <p>{description}</p>
          </div>
        </div>
      )}
    </form>
  );
}
