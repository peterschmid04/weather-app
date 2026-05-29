import React from 'react';
import "./Highlights.css";
import iconSunrise from "../images/icons/sun/sunrise-svgrepo-com.svg";
import iconSunset from "../images/icons/sun/sunset-svgrepo-com.svg";
import UVIndex from './UVIndex.js'; // Import the UVIndex component

export default function Highlights({ highlights }) {
    // Render the highlights section with today's weather highlights
    return (
        <div className="highlights-container">
            <h2>Heutige Highlights</h2>
            <div className="highlights">
                {/* Map through the highlights array and render each highlight */}
                {highlights && highlights.map((item, index) => (
                    <div key={index} className="highlight-box">
                        <h3>{item.title}</h3>

                        {/* Render sunrise and sunset times if the title matches */}
                        {item.title === "Sonnenaufgang & Sonnenuntergang" ? (
                            <div className="sunrise-sunset">
                                <p><img src={iconSunrise} alt="Sonnenaufgang" /> {item.up}</p>
                                <p><img src={iconSunset} alt="Sonnenuntergang" /> {item.down}</p>
                            </div>
                        ) : item.title === "UV-Index" && typeof item.value === "number" ? (
                            // Render the UVIndex component for UV Index highlight
                            <UVIndex value={item.value} />
                        ) : (
                            // Render generic highlight information
                            <div>
                                <p>
                                    <strong>{typeof item.value === "number" ? item.value : "—"}</strong>
                                    {item.unit ? ` ${item.unit}` : ""}
                                </p>
                                <p>{item.status}</p>
                            </div>
                        )}
                    </div>
                ))}
            </div>
        </div>
    );
}
