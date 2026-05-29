import { useCallback, useEffect, useRef, useState } from "react";
import "./App.css";
import Forecast from "./components/Forecast";
import Highlights from "./components/Highlights";
import LoginOptions from "./components/LoginOptions";
import Sidebar from "./components/Sidebar";
import Stations from "./components/Stations";
import UserDataPanel from "./components/UserDataPanel";
import { getWeatherImage, getWeatherIcons } from "./utils/weatherUtils";
import { getStatusWind, getStatusVisibility, getStatusHumidity, getStatusAirquality } from "./utils/statusUtils";
import { formatCityLocation } from "./utils/localizationUtils";
import { buildApiUrl } from "./utils/apiUtils";
import { useAuth0 } from "@auth0/auth0-react";

// Main dashboard container. This component owns the authenticated API calls,
// weather state, saved-user-data refresh keys, unit toggle and selected theme.
const DEFAULT_CITY = "Schwenningen";
const FORECAST_CARD_COUNT = 5;

// HttpError keeps HTTP status codes available after fetch() so the UI can show
// specific German messages for 401, 403, 404, 409, 429 and 500.
class HttpError extends Error {
  constructor(status, message, body) {
    super(message);
    this.name = "HttpError";
    this.status = status;
    this.body = body;
  }
}

// Converts backend forecast DTOs into the compact card model used by Forecast.
const buildForecastCards = (forecast) => {
  return forecast.slice(0, FORECAST_CARD_COUNT).map((day) => ({
    day: day.day,
    image: typeof day.id === "number" ? getWeatherImage(day.id) : null,
    description: day.description || "Nicht verfügbar",
    minTemp: typeof day.tempMin === "number" ? day.tempMin : null,
    maxTemp: typeof day.tempMax === "number" ? day.tempMax : null,
  }));
};

export default function WeatherApp() {
  const [inputCity, setInputCity] = useState(DEFAULT_CITY);
  const [city, setCity] = useState(DEFAULT_CITY);
  const [weather, setWeather] = useState(null);
  const [country, setCountry] = useState("");
  const [highlights, setHighlights] = useState([]);
  const [forecastData, setForecastData] = useState([]);
  const [error, setError] = useState("");
  const [searchMessage, setSearchMessage] = useState("");
  const [timezoneOffset, setTimezoneOffset] = useState(0);
  const [isCelsius, setIsCelsius] = useState(true);
  const [historyRefreshKey, setHistoryRefreshKey] = useState(0);
  const [favoritesRefreshKey, setFavoritesRefreshKey] = useState(0);
  const [stationsRefreshKey, setStationsRefreshKey] = useState(0);
  const [selectedStationId, setSelectedStationId] = useState("");
  const [themeName, setThemeName] = useState("graphite");
  const activeWeatherRequestRef = useRef(0);
  const initialWeatherLoadedRef = useRef(false);
  const lastWeatherRequestRef = useRef({ city: "", startedAt: 0 });
  const lastRateLimitNoticeRef = useRef(0);
  const { isAuthenticated, loginWithRedirect, logout, getAccessTokenSilently, user } = useAuth0();

  const timezoneOffsetFormatted = timezoneOffset >= 0 ? `+${timezoneOffset}` : timezoneOffset;

  const getCurrentTime = () => {
    const now = new Date();
    return now.toLocaleTimeString("de-DE", {
      second: undefined,
      hour: "2-digit",
      minute: "2-digit",
      hourCycle: "h23",
    });
  };

  const getCurrentDay = () => {
    const now = new Date();
    return now.toLocaleDateString("de-DE", { weekday: "long" });
  };

  const [currentTime, setCurrentTime] = useState(getCurrentTime());
  const [currentDay, setCurrentDay] = useState(getCurrentDay());

  // Adds the Auth0 access token to every backend call and converts non-2xx
  // responses into HttpError instances.
  const authFetchJson = useCallback(
    async (url, options = {}) => {
      const token = await getAccessTokenSilently({
        authorizationParams: {
          audience: process.env.REACT_APP_AUTH0_AUDIENCE,
          scope: process.env.REACT_APP_AUTH0_SCOPE,
        },
      });

      const res = await fetch(url, {
        ...options,
        headers: {
          ...(options.headers || {}),
          Authorization: `Bearer ${token}`,
          Accept: "application/json",
          ...(user?.email ? { "X-Weather-App-Profile-Email": user.email } : {}),
          ...(options.body ? { "Content-Type": "application/json" } : {}),
        },
      });

      let body = null;
      try {
        body = await res.json();
      } catch (_) {
        body = null;
      }

      if (!res.ok) {
        const msg = body?.title || body?.error || body?.message || res.statusText || `HTTP ${res.status}`;
        throw new HttpError(res.status, msg, body);
      }

      return body;
    },
    [getAccessTokenSilently, user?.email]
  );

  // Loads current weather first, then loads UV, air quality and forecast in
  // parallel. A request id prevents older slow responses from overwriting newer
  // searches, and duplicate same-city requests are suppressed briefly.
  const fetchWeatherData = useCallback(
    async (nextCity) => {
      const trimmedCity = nextCity.trim();
      const normalizedCity = trimmedCity.toLocaleLowerCase("de-DE");
      const now = Date.now();
      if (
        lastWeatherRequestRef.current.city === normalizedCity &&
        now - lastWeatherRequestRef.current.startedAt < 2500
      ) {
        return;
      }

      lastWeatherRequestRef.current = { city: normalizedCity, startedAt: now };

      const requestId = activeWeatherRequestRef.current + 1;
      activeWeatherRequestRef.current = requestId;
      const isCurrentRequest = () => activeWeatherRequestRef.current === requestId;

      try {
        const data = await authFetchJson(buildApiUrl(`/weather?city=${encodeURIComponent(trimmedCity)}`));
        if (!isCurrentRequest()) {
          return;
        }

        if (!data) {
          throw new Error("No weather data found.");
        }

        setCountry(data.country);
        setTimezoneOffset(data.timezoneOffsetHours);
        setWeather({
          city: data.city,
          temp: data.temperatureC,
          humidity: data.humidity,
          visibility: data.visibilityKm,
          description: data.description,
          image: getWeatherImage(data.weatherId),
          icon: getWeatherIcons(data.weatherId),
          lat: data.lat,
          lon: data.lon,
        });

        setCity(data.city);
        setError("");
        setHistoryRefreshKey((current) => current + 1);

        const [uvResult, airQualityResult, forecastResult] = await Promise.allSettled([
          authFetchJson(buildApiUrl(`/uv?lat=${data.lat}&lon=${data.lon}`)),
          authFetchJson(buildApiUrl(`/airquality?lat=${data.lat}&lon=${data.lon}`)),
          authFetchJson(buildApiUrl(`/forecast?lat=${data.lat}&lon=${data.lon}`)),
        ]);

        if (!isCurrentRequest()) {
          return;
        }

        const uvData = uvResult.status === "fulfilled" ? uvResult.value : null;
        const airQualityData = airQualityResult.status === "fulfilled" ? airQualityResult.value : null;
        const forecast = forecastResult.status === "fulfilled" && Array.isArray(forecastResult.value)
          ? forecastResult.value
          : [];

        setForecastData(buildForecastCards(forecast));

        const nextHighlights = [
          {
            title: "UV-Index",
            value: typeof uvData?.uvIndex === "number" ? uvData.uvIndex : null,
            unit: "",
            status: typeof uvData?.uvIndex === "number" ? "" : "Nicht verfügbar",
          },
          { title: "Wind", value: data.windSpeed, unit: "km/h", status: getStatusWind(data.windSpeed) },
          { title: "Sonnenaufgang & Sonnenuntergang", up: `${data.sunrise}`, down: `${data.sunset}` },
          { title: "Luftfeuchtigkeit", value: data.humidity, unit: "%", status: getStatusHumidity(data.humidity) },
          { title: "Sichtweite", value: data.visibilityKm, unit: "km", status: getStatusVisibility(data.visibilityKm) },
          {
            title: "Luftqualität",
            value: typeof airQualityData?.aqi === "number" ? airQualityData.aqi : null,
            unit: "",
            status: typeof airQualityData?.aqi === "number"
              ? getStatusAirquality(airQualityData.aqi)
              : "Nicht verfügbar",
          },
        ];

        setHighlights(nextHighlights);
      } catch (err) {
        if (!isCurrentRequest()) {
          return;
        }

        setWeather(null);
        setHighlights([]);
        setForecastData([]);

        if (err instanceof HttpError) {
          switch (err.status) {
            case 401:
              setError("Nicht eingeloggt oder Sitzung abgelaufen. Bitte erneut anmelden.");
              return;
            case 403:
              setError("Keine Berechtigung für diese Region.");
              return;
            case 404:
              setError("Stadt wurde nicht gefunden.");
              return;
            case 409:
              setError("Dieser Eintrag existiert bereits.");
              return;
            case 429:
              if (Date.now() - lastRateLimitNoticeRef.current > 15000) {
                lastRateLimitNoticeRef.current = Date.now();
                setError("Zu viele Anfragen. Bitte kurz warten.");
              }
              return;
            case 500:
              setError("Serverfehler. Bitte später erneut versuchen.");
              return;
            default:
              setError(`Fehler (${err.status}): ${err.message}`);
              return;
          }
        }

        setError("Die Verbindung zum Backend oder Internet ist gerade nicht erreichbar. Bitte prüfe WLAN oder Netzwerk und versuche es danach erneut.");
      }
    },
    [authFetchJson]
  );

  // Keep the clock/day label fresh while the app stays open.
  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentTime(getCurrentTime());
      setCurrentDay(getCurrentDay());
    }, 60000);
    return () => clearInterval(interval);
  }, []);

  const handleSubmit = (event) => {
    event.preventDefault();
    if (inputCity.trim()) {
      setSearchMessage("");
      fetchWeatherData(inputCity.trim());
    } else {
      setSearchMessage("Bitte eine Stadt eingeben.");
    }
  };

  const loadCityFromSavedItem = useCallback(
    (nextCity, displayCity = nextCity) => {
      setInputCity(displayCity);
      setSearchMessage("");
      fetchWeatherData(nextCity);
    },
    [fetchWeatherData]
  );

  const updateInputCity = useCallback((nextCity) => {
    setInputCity(nextCity);
    if (nextCity.trim()) {
      setSearchMessage("");
    }
  }, []);

  // After login, load the user's default favorite city when one exists;
  // otherwise fall back to Schwenningen.
  useEffect(() => {
    if (!isAuthenticated) {
      initialWeatherLoadedRef.current = false;
      return;
    }

    if (initialWeatherLoadedRef.current) {
      return;
    }

    initialWeatherLoadedRef.current = true;
    let cancelled = false;

    const loadInitialWeather = async () => {
      let startCity = DEFAULT_CITY;

      try {
        const defaultFavorite = await authFetchJson(buildApiUrl("/favorites/default"));
        if (defaultFavorite?.cityName) {
          startCity = defaultFavorite.cityName;
        }
      } catch (error) {
        if (!(error instanceof HttpError && error.status === 404)) {
          setSearchMessage("Standard-Favorit konnte nicht geladen werden. Schwenningen wird geladen.");
        }
      }

      if (cancelled) {
        return;
      }

      setInputCity(startCity);
      fetchWeatherData(startCity);
    };

    loadInitialWeather();

    return () => {
      cancelled = true;
    };
  }, [isAuthenticated, authFetchJson, fetchWeatherData]);

  // Load the saved color theme for the current Auth0 user.
  useEffect(() => {
    if (!isAuthenticated) {
      return;
    }

    authFetchJson(buildApiUrl("/theme/"))
      .then((data) => setThemeName(data.themeName || "graphite"))
      .catch(() => setThemeName("graphite"));
  }, [isAuthenticated, authFetchJson]);

  if (!isAuthenticated) {
    return <LoginOptions loginWithRedirect={loginWithRedirect} />;
  }

  return (
    <div className={`weather-grid theme-${themeName}`}>
      <Sidebar
        city={inputCity}
        setCity={updateInputCity}
        handleSubmit={handleSubmit}
        weather={weather}
        currentDay={currentDay}
        currentTime={currentTime}
        isCelsius={isCelsius}
        searchMessage={searchMessage}
        authFetchJson={authFetchJson}
        historyRefreshKey={historyRefreshKey}
        favoritesRefreshKey={favoritesRefreshKey}
        onSelectCity={loadCityFromSavedItem}
        onHistoryChanged={() => setHistoryRefreshKey((current) => current + 1)}
        onFavoritesChanged={() => setFavoritesRefreshKey((current) => current + 1)}
      />

      {weather && (
        <>
          <div className="header">
            <span className="location-title">
              {formatCityLocation(city, country)} UTC{timezoneOffsetFormatted}
            </span>
            <div className="header-actions">
              <div className="toggle-buttons">
                <button type="button" onClick={() => setIsCelsius(true)} className={isCelsius ? "active" : ""}>
                  &deg;C
                </button>
                <button type="button" onClick={() => setIsCelsius(false)} className={!isCelsius ? "active" : ""}>
                  &deg;F
                </button>
              </div>
              <button className="logout" onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}>
                Abmelden
              </button>
            </div>
          </div>
          <Forecast forecastData={forecastData} isCelsius={isCelsius} />
          <Highlights highlights={highlights} />
        </>
      )}

      {error && <p className="errorDisplay">{error}</p>}

      <UserDataPanel
        authFetchJson={authFetchJson}
        currentWeather={weather}
        currentCountry={country}
        stationsRefreshKey={stationsRefreshKey}
        selectedStationId={selectedStationId}
        onSelectedStationChange={setSelectedStationId}
        onSelectCity={loadCityFromSavedItem}
        themeName={themeName}
        onThemeChange={setThemeName}
        onFavoritesChanged={() => setFavoritesRefreshKey((current) => current + 1)}
      />
      <Stations
        authFetchJson={authFetchJson}
        selectedStationId={selectedStationId}
        onSelectedStationChange={setSelectedStationId}
        onStationsChanged={() => setStationsRefreshKey((current) => current + 1)}
      />

      {!weather && (
        <button className="errorLogout" onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}>
          Abmelden
        </button>
      )}
    </div>
  );
}
