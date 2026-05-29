import "./Forecast.css";
import React from "react";
import { translateWeatherDescription, translateWeekday } from "../utils/localizationUtils";

const convertTemperature = (temp, isCelsius) => isCelsius ? temp : Number((temp * 9 / 5 + 32).toFixed(1));

const formatTemperature = (temp, isCelsius) => {
  if (typeof temp !== "number") {
    return "-";
  }

  return `${convertTemperature(temp, isCelsius)}°${isCelsius ? "C" : "F"}`;
};

export default function Forecast({ forecastData, isCelsius }) {
  if (!forecastData || forecastData.length === 0) {
    return null;
  }

  return (
    <div className="forecast-container">
      <div className="forecast">
        {forecastData.map((item, index) => (
          <div key={index} className="forecast-box">
            <p className="day">{translateWeekday(item.day)}</p>
            <p className="forecast-description">{translateWeatherDescription(item.description)}</p>
            {item.image ? (
              <img src={item.image} alt={translateWeatherDescription(item.description)} />
            ) : (
              <div className="forecast-placeholder" aria-label="Keine Vorhersage verfügbar">
                -
              </div>
            )}
            <p className="min-temp">{formatTemperature(item.minTemp, isCelsius)} /</p>
            <p className="max-temp">{formatTemperature(item.maxTemp, isCelsius)}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
