import "./Forecast.css";
import React from "react";
import { translateWeatherDescription, translateWeekday } from "../utils/localizationUtils";

const convertTemperature = (temp, isCelsius) => isCelsius ? temp : (temp * 9 / 5 + 32).toFixed(1);

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
            <img src={item.image} alt={translateWeatherDescription(item.description)} />
            <p className="min-temp">{convertTemperature(item.minTemp, isCelsius)}&deg;{isCelsius ? "C" : "F"} /</p>
            <p className="max-temp">{convertTemperature(item.maxTemp, isCelsius)}&deg;{isCelsius ? "C" : "F"}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
