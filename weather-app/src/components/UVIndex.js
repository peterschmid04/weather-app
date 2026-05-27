// UVIndex.js
import React from "react";
import "./UVIndex.css";

// UVIndex component displays a semicircular UV index gauge
const UVIndex = ({ value }) => {
  const maxAngle = 180; // Maximum angle for the semicircle
  // Calculate the angle for the gauge based on the UV index value (max UV index = 17)
  const angle = (value / 17) * maxAngle;

  return (
    <div className="uv-container">
      <div className="half-mask-container">
        <div
          className="circle1"
          style={{
            // Use a conic-gradient mask to visually represent the UV index value
            mask: `conic-gradient(from -90deg, #ffee37 0deg, #ff8c00 ${angle * 0.5}deg, #b40101 ${angle}deg, transparent ${angle}deg)`,
          }}
        >
          {/* Inner circle for styling */}
          <div className="circle2"></div>
        </div>
        {/* Display the UV index value */}
        <div className="uv-value">{value}</div>
      </div>
    </div>
  );
};

export default UVIndex;