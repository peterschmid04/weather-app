import React from 'react';
import "./Sidebar.css";

// Converts temperature between Celsius and Fahrenheit
const convertTemperature = (temp, isCelsius) => {
    return isCelsius ? temp : ((temp * 9/5) + 32).toFixed(1);
};

/**
 * Sidebar component for weather app.
 * Displays search input and current weather info.
 */

export default function Sidebar({ city, setCity, handleSubmit, weather, currentDay, currentTime, isCelsius }) {
    return (
        // Form for searching cities
        <form className="sidebar" onSubmit={handleSubmit}>
            {/* Input field for city search */}
            <input
                type="text"
                className="cityInput"
                placeholder="Search for places..."
                value={city}
                onChange={(e) => setCity(e.target.value)}
            />
            {/* Show weather info if available */}
            {weather && (
                <div>
                    <div>
                        {/* Weather image */}
                        <img className="image" src={weather.image} alt={weather.description} />
                        {/* Temperature display */}
                        <div className="temp">
                            {convertTemperature(weather.temp, isCelsius)}°{isCelsius ? 'C' : 'F'}
                        </div>
                        {/* Date and time */}
                        <div className="date">
                            <p className="currentDay">{currentDay}</p>
                            <p className="time">{currentTime}</p>
                        </div>
                        {/* Weather description and icon */}
                        <div className="description">
                            <img className="icon" src={weather.icon} alt="Weather Icon" />
                            <p>{weather.description}</p>
                        </div>    
                    </div>
                </div>
            )}
        </form>
    );
}