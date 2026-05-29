import React from 'react';
import "./Highlights.css";
import iconSunrise from "../images/icons/sun/sunrise-svgrepo-com.svg";
import iconSunset from "../images/icons/sun/sunset-svgrepo-com.svg";
import UVIndex from './UVIndex.js';

// Highlights renders the six compact metric cards under the forecast.
// UV and sunrise/sunset have custom layouts; all other cards use the same
// generic value/status renderer.
export default function Highlights({ highlights }) {
    return (
        <div className="highlights-container">
            <h2>Heutige Highlights</h2>
            <div className="highlights">
                {highlights && highlights.map((item, index) => (
                    <div key={index} className="highlight-box">
                        <h3>{item.title}</h3>

                        {item.title === "Sonnenaufgang & Sonnenuntergang" ? (
                            <div className="sunrise-sunset">
                                <p><img src={iconSunrise} alt="Sonnenaufgang" /> {item.up}</p>
                                <p><img src={iconSunset} alt="Sonnenuntergang" /> {item.down}</p>
                            </div>
                        ) : item.title === "UV-Index" && typeof item.value === "number" ? (
                            <UVIndex value={item.value} />
                        ) : (
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
