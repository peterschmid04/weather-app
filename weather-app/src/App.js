import { useState, useEffect,useCallback} from "react";
import "./App.css";
import Forecast from "./components/Forecast";
import Highlights from "./components/Highlights";
import LoginOptions from "./components/LoginOptions";
import Sidebar from "./components/Sidebar";
import Stations from "./components/Stations";
import { getWeatherImage, getWeatherIcons } from "./utils/weatherUtils";
import {getStatusWind, getStatusVisibility, getStatusHumidity, getStatusAirquality} from "./utils/statusUtils";
import { useAuth0 } from "@auth0/auth0-react";
class HttpError extends Error {
  constructor(status, message, body) {
    super(message);
    this.name = "HttpError";
    this.status = status;
    this.body = body;
  }
}
// Main weather app component
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
  const { isAuthenticated,  loginWithRedirect, logout, getAccessTokenSilently } = useAuth0();

  const timezoneOffsetFormatted =
    timezoneOffset >= 0 ? `+${timezoneOffset}` : timezoneOffset;

  // Get current time formatted
  const getCurrentTime = () => {
    const now = new Date();
    return now.toLocaleTimeString("de-DE", {  
      second: undefined,
      hour: "2-digit",
      minute: "2-digit",
      hourCycle: "h23",
    });
  }

  // Get current day name
  const getCurrentDay = () => {
    const now = new Date();
    const options = { weekday: "long" };
    return now.toLocaleDateString("en-En", options);
  };

  const [currentTime, setCurrentTime] = useState(getCurrentTime());
  const [currentDay] = useState(getCurrentDay());

  // Function to fetch JSON data with Auth0 token
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
      try { body = await res.json(); } catch (_) {}

      if (!res.ok) {
        const msg =
          body?.title ||
          body?.error ||
          body?.message ||
          res.statusText ||
          `HTTP ${res.status}`;
        throw new HttpError(res.status, msg, body);
      }
      return body;
    },
    [getAccessTokenSilently]
  );

  // Fetch weather and related data for a city
  const fetchWeatherData = useCallback(
    async (city) => {
      try {
        const data = await authFetchJson(`http://localhost:5122/weather?city=${encodeURIComponent(city)}`);
        if (!data) throw new Error("No data found");
        if (data.Error) throw new Error(data.Error);
        
        setCountry(data.country);
        setTimezoneOffset(data.timezoneOffsetHours);

        setWeather({
          city: data.city,
          temp: (data.temperatureC),
          humidity: data.humidity,
          visibility: data.visibilityKm,
          description: data.description,
          image: getWeatherImage(data.weatherId),
          icon: getWeatherIcons(data.weatherId),
        });

        setCity(data.city); // Set city to actual found value
        setError("");

        const uvData       = await authFetchJson(`http://localhost:5122/uv?lat=${data.lat}&lon=${data.lon}`);
        const airQualityData = await authFetchJson(`http://localhost:5122/airquality?lat=${data.lat}&lon=${data.lon}`);
        const forecastData = await authFetchJson(`http://localhost:5122/forecast?lat=${data.lat}&lon=${data.lon}`);

        setForecastData(
          forecastData.slice(0, 6).map((day) => ({
            day: day.day,
            image: getWeatherImage(day.id),
            description: day.description,
            minTemp: day.tempMin,
            maxTemp: day.tempMax,
          }))
        );

        // Set highlights for sidebar
        setHighlights([
          {
            title: "UV Index",
            value: uvData.uvIndex,
            unit: "",
          },
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
            value: (data.visibilityKm),
            unit: "km",
            status: getStatusVisibility((data.visibilityKm)),
          },
          {
            title: "Air Quality",
            value: airQualityData.aqi,
            unit: "",
            status: getStatusAirquality(airQualityData.aqi),
          },
        ]);
        setError("");
       } catch (err) {
        setWeather(null);
        setHighlights([]);
        setForecastData([]);

        if (err instanceof HttpError) {
          switch (err.status) {
            case 401:
              setError("🔒 Not signed in or token expired — please log in again");
              return;

            case 403:
              setError("⛔ No permission (e.g. region not allowed).");
              return;

            case 404:
              setError("❓ City not found");
              return;

            case 500:
              setError("❌ unexpected server error");
              return;

            default:
              setError(`Error (${err.status}): ${err.message}`);
              return;
          }
        } else {
          setError("🌐 Network error or unexpected error.");
        }
      }
    },
    [authFetchJson]
  );

  // Update time every minute
  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentTime(getCurrentTime());
    }, 60000);
    return () => clearInterval(interval);
  }, []);

  // Handle city search submit
  const handleSubmit = (event) => {
    event.preventDefault();
    if (inputCity.trim()) {
      fetchWeatherData(inputCity);
    } else {
      setError("Please enter a city!");
    }
  };

  // Get country flag emoji from country code
  const getCountryFlagEmoji = (countryCode) => {
    return countryCode
      .toUpperCase()
      .replace(/./g, (char) =>
        String.fromCodePoint(127397 + char.charCodeAt())
      );
  };

  useEffect(() => {
    if (!isAuthenticated) {
      
      return;
    } else {
      fetchWeatherData("Lossburg");
    }
  }, [isAuthenticated, fetchWeatherData]);

  if (!isAuthenticated) {
    return <LoginOptions loginWithRedirect={loginWithRedirect} />;
  }

  return (
    <div className="weather-grid">
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
          <Forecast forecastData={forecastData} isCelsius={isCelsius} />
          <Highlights highlights={highlights} />
          <div className="header">
            {city}, {getCountryFlagEmoji(country)} UTC{timezoneOffsetFormatted}
            <button className="logout" onClick={() => logout({ logoutParams: { returnTo: window.location.origin } })}>Logout</button>
          </div>
          <div className="toggle-buttons">
            <button
              onClick={() => setIsCelsius(true)}
              className={isCelsius ? "active" : ""}
            >
              °C
            </button>
            <button
              onClick={() => setIsCelsius(false)}
              className={!isCelsius ? "active" : ""}
            >
              °F
            </button>
          </div>
        </>
      )}
      {error && <p className="errorDisplay">{error}</p> }
      <Stations authFetchJson={authFetchJson} />
    {!weather && <button className="errorLogout" onClick={() => logout({ logoutParams: { returnTo: window.location.origin }  })}>Logout</button>}  
    </div>
  );
}
