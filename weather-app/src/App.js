import { useCallback, useEffect, useState } from "react";
import "./App.css";
import Forecast from "./components/Forecast";
import Highlights from "./components/Highlights";
import LoginOptions from "./components/LoginOptions";
import Sidebar from "./components/Sidebar";
import Stations from "./components/Stations";
import UserDataPanel from "./components/UserDataPanel";
import { getWeatherImage, getWeatherIcons } from "./utils/weatherUtils";
import { getStatusWind, getStatusVisibility, getStatusHumidity, getStatusAirquality } from "./utils/statusUtils";
import { useAuth0 } from "@auth0/auth0-react";

const API_BASE = "http://localhost:5122";

class HttpError extends Error {
  constructor(status, message, body) {
    super(message);
    this.name = "HttpError";
    this.status = status;
    this.body = body;
  }
}

export default function WeatherApp() {
  const [inputCity, setInputCity] = useState("Lossburg");
  const [city, setCity] = useState("Lossburg");
  const [weather, setWeather] = useState(null);
  const [country, setCountry] = useState("");
  const [highlights, setHighlights] = useState([]);
  const [forecastData, setForecastData] = useState([]);
  const [error, setError] = useState("");
  const [timezoneOffset, setTimezoneOffset] = useState(0);
  const [isCelsius, setIsCelsius] = useState(true);
  const [historyRefreshKey, setHistoryRefreshKey] = useState(0);
  const [themeName, setThemeName] = useState("graphite");
  const { isAuthenticated, loginWithRedirect, logout, getAccessTokenSilently } = useAuth0();

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
    return now.toLocaleDateString("en-En", { weekday: "long" });
  };

  const [currentTime, setCurrentTime] = useState(getCurrentTime());
  const [currentDay] = useState(getCurrentDay());

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
    [getAccessTokenSilently]
  );

  const fetchWeatherData = useCallback(
    async (nextCity) => {
      try {
        const data = await authFetchJson(`${API_BASE}/weather?city=${encodeURIComponent(nextCity)}`);
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

        const uvData = await authFetchJson(`${API_BASE}/uv?lat=${data.lat}&lon=${data.lon}`);
        const airQualityData = await authFetchJson(`${API_BASE}/airquality?lat=${data.lat}&lon=${data.lon}`);
        const forecast = await authFetchJson(`${API_BASE}/forecast?lat=${data.lat}&lon=${data.lon}`);

        setForecastData(
          forecast.slice(0, 6).map((day) => ({
            day: day.day,
            image: getWeatherImage(day.id),
            description: day.description,
            minTemp: day.tempMin,
            maxTemp: day.tempMax,
          }))
        );

        setHighlights([
          { title: "UV Index", value: uvData.uvIndex, unit: "" },
          {
            title: "Wind Status",
            value: data.windSpeed,
            unit: "km/h",
            status: getStatusWind(data.windSpeed),
          },
          { title: "Sunrise & Sunset", up: `${data.sunrise}`, down: `${data.sunset}` },
          {
            title: "Humidity",
            value: data.humidity,
            unit: "%",
            status: getStatusHumidity(data.humidity),
          },
          {
            title: "Visibility",
            value: data.visibilityKm,
            unit: "km",
            status: getStatusVisibility(data.visibilityKm),
          },
          {
            title: "Air Quality",
            value: airQualityData.aqi,
            unit: "",
            status: getStatusAirquality(airQualityData.aqi),
          },
        ]);
      } catch (err) {
        setWeather(null);
        setHighlights([]);
        setForecastData([]);

        if (err instanceof HttpError) {
          switch (err.status) {
            case 401:
              setError("Nicht eingeloggt oder Sitzung abgelaufen. Bitte erneut anmelden.");
              return;
            case 403:
              setError("Keine Berechtigung fuer diese Region.");
              return;
            case 404:
              setError("Stadt wurde nicht gefunden.");
              return;
            case 409:
              setError("Dieser Eintrag existiert bereits.");
              return;
            case 429:
              setError("Zu viele Anfragen. Bitte kurz warten.");
              return;
            case 500:
              setError("Serverfehler. Bitte spaeter erneut versuchen.");
              return;
            default:
              setError(`Fehler (${err.status}): ${err.message}`);
              return;
          }
        }

        setError("Netzwerkfehler oder unerwarteter Fehler.");
      }
    },
    [authFetchJson]
  );

  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentTime(getCurrentTime());
    }, 60000);
    return () => clearInterval(interval);
  }, []);

  const handleSubmit = (event) => {
    event.preventDefault();
    if (inputCity.trim()) {
      fetchWeatherData(inputCity.trim());
    } else {
      setError("Bitte eine Stadt eingeben.");
    }
  };

  const loadCityFromSavedItem = useCallback(
    (nextCity) => {
      setInputCity(nextCity);
      fetchWeatherData(nextCity);
    },
    [fetchWeatherData]
  );

  const getCountryFlagEmoji = (countryCode) =>
    countryCode
      .toUpperCase()
      .replace(/./g, (char) => String.fromCodePoint(127397 + char.charCodeAt()));

  useEffect(() => {
    if (!isAuthenticated) {
      return;
    }

    fetchWeatherData("Lossburg");
  }, [isAuthenticated, fetchWeatherData]);

  useEffect(() => {
    if (!isAuthenticated) {
      return;
    }

    authFetchJson(`${API_BASE}/theme/`)
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
        setCity={setInputCity}
        handleSubmit={handleSubmit}
        weather={weather}
        currentDay={currentDay}
        currentTime={currentTime}
        isCelsius={isCelsius}
      />

      {weather && (
        <>
          <div className="header">
            <span className="location-title">
              {city}, {getCountryFlagEmoji(country)} UTC{timezoneOffsetFormatted}
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
                Logout
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
        historyRefreshKey={historyRefreshKey}
        onSelectCity={loadCityFromSavedItem}
        themeName={themeName}
        onThemeChange={setThemeName}
      />
      <Stations authFetchJson={authFetchJson} />

      {!weather && (
        <button className="errorLogout" onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}>
          Logout
        </button>
      )}
    </div>
  );
}
