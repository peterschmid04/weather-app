import React from "react";
import "./UVIndex.css";

// Semicircular UV index gauge used by the highlights section.
const UVIndex = ({ value }) => {
  const maxAngle = 180;
  // Scale OpenWeatherMap UV values into the half-circle mask.
  const angle = (value / 17) * maxAngle;

  return (
    <div className="uv-container">
      <div className="half-mask-container">
        <div
          className="circle1"
          style={{
            mask: `conic-gradient(from -90deg, #ffee37 0deg, #ff8c00 ${angle * 0.5}deg, #b40101 ${angle}deg, transparent ${angle}deg)`,
          }}
        >
          <div className="circle2"></div>
        </div>
        <div className="uv-value">{value}</div>
      </div>
    </div>
  );
};

export default UVIndex;
