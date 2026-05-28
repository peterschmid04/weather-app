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

class HttpError extends Error {
  constructor(status, message, body) {
    super(message);
    this.name = "HttpError";
    this.status = status;
    this.body = body;
  }
}

export default function WeatherApp() {
  const [inputCity, setInputCity] = useState("Loßburg");
  const [city, setCity] = useState("Loßburg");
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
  const [themeName, setThemeName] = useState("graphite");
  const activeWeatherRequestRef = useRef(0);
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
    return now.toLocaleDateString("de-DE", { weekday: "long" });
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
      const requestId = activeWeatherRequestRef.current + 1;
      activeWeatherRequestRef.current = requestId;
      const isCurrentRequest = () => activeWeatherRequestRef.current === requestId;

      try {
        const data = await authFetchJson(buildApiUrl(`/weather?city=${encodeURIComponent(nextCity)}`));
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

        setForecastData(
          forecast.slice(0, 6).map((day) => ({
            day: day.day,
            image: getWeatherImage(day.id),
            description: day.description,
            minTemp: day.tempMin,
            maxTemp: day.tempMax,
          }))
        );

        const nextHighlights = [
          ...(typeof uvData?.uvIndex === "number" ? [{ title: "UV-Index", value: uvData.uvIndex, unit: "" }] : []),
          { title: "Wind", value: data.windSpeed, unit: "km/h", status: getStatusWind(data.windSpeed) },
          { title: "Sonnenaufgang & Sonnenuntergang", up: `${data.sunrise}`, down: `${data.sunset}` },
          { title: "Luftfeuchtigkeit", value: data.humidity, unit: "%", status: getStatusHumidity(data.humidity) },
          { title: "Sichtweite", value: data.visibilityKm, unit: "km", status: getStatusVisibility(data.visibilityKm) },
        ];

        if (typeof airQualityData?.aqi === "number") {
          nextHighlights.push({
            title: "Luftqualität",
            value: airQualityData.aqi,
            unit: "",
            status: getStatusAirquality(airQualityData.aqi),
          });
        }

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
              setError("Zu viele Anfragen. Bitte kurz warten.");
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

  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentTime(getCurrentTime());
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

  useEffect(() => {
    if (!isAuthenticated) {
      return;
    }

    fetchWeatherData("Loßburg");
  }, [isAuthenticated, fetchWeatherData]);

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
        historyRefreshKey={historyRefreshKey}
        onSelectCity={loadCityFromSavedItem}
        themeName={themeName}
        onThemeChange={setThemeName}
        onHistoryChanged={() => setHistoryRefreshKey((current) => current + 1)}
        onFavoritesChanged={() => setFavoritesRefreshKey((current) => current + 1)}
      />
      <Stations authFetchJson={authFetchJson} />

      {!weather && (
        <button className="errorLogout" onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}>
          Abmelden
        </button>
      )}
    </div>
  );
}
