import React, { useCallback, useEffect, useState } from "react";
import "./Sidebar.css";
import { translateWeatherDescription } from "../utils/localizationUtils";

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
}) {
  const [showSavedItems, setShowSavedItems] = useState(false);
  const [quickHistory, setQuickHistory] = useState([]);
  const [quickFavorites, setQuickFavorites] = useState([]);
  const description = translateWeatherDescription(weather?.description);

  const loadSavedItems = useCallback(async () => {
    if (!authFetchJson) {
      return;
    }

    const [historyResult, favoritesResult] = await Promise.allSettled([
      authFetchJson("http://localhost:5122/history/"),
      authFetchJson("http://localhost:5122/favorites/"),
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

  const selectCity = (nextCity) => {
    setShowSavedItems(false);
    onSelectCity(nextCity);
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
            <div className="quick-section">
              <p className="quick-section-title">Letzte Suchen</p>
              {quickHistory.length === 0 && <span className="quick-empty">Noch kein Verlauf</span>}
              {quickHistory.map((item) => (
                <button key={item.id} type="button" className="quick-item" onClick={() => selectCity(item.cityName)}>
                  {item.cityName}, {item.countryCode}
                </button>
              ))}
            </div>
          </div>
        )}

        <div className="quick-favorites-card">
          <p className="quick-section-title">Favoriten</p>
          {quickFavorites.length === 0 && <span className="quick-empty">Noch keine Favoriten</span>}
          {quickFavorites.map((favorite) => (
            <button key={favorite.id} type="button" className="quick-item" onClick={() => selectCity(favorite.cityName)}>
              {favorite.cityName}, {favorite.countryCode}
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
