import React from "react";
import "./Sidebar.css";
import { translateWeatherDescription } from "../utils/localizationUtils";

const convertTemperature = (temp, isCelsius) => isCelsius ? temp : ((temp * 9 / 5) + 32).toFixed(1);

export default function Sidebar({ city, setCity, handleSubmit, weather, currentDay, currentTime, isCelsius, searchMessage }) {
  const description = translateWeatherDescription(weather?.description);

  return (
    <form className="sidebar" onSubmit={handleSubmit}>
      {searchMessage && <p className="search-message">{searchMessage}</p>}
      <input
        type="text"
        className="cityInput"
        placeholder="Stadt suchen..."
        value={city}
        onChange={(event) => setCity(event.target.value)}
      />

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
