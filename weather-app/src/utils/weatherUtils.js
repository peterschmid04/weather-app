import getCloudy from "../images/cloudy.svg";
import getRain from "../images/rain.svg";
import getSnow from "../images/snow.svg";
import getThunderstorm from "../images/thunderstorm.svg";
import getClear from "../images/clear.svg";
import getFog from "../images/fog.svg";
import getClouds from "../images/clouds.svg";

import iconCloudy from "../images/icons/cloudy.svg";
import iconRain from "../images/icons/rain.svg";
import iconSnow from "../images/icons/snow.svg";
import iconThunderstorm from "../images/icons/thunderstorm.svg";
import iconClear from "../images/icons/clear.svg";
import iconFog from "../images/icons/fog.svg";
import iconClouds from "../images/icons/cloudy.svg";

// Returns main weather image based on weather condition id
export const getWeatherImage = (weatherId) => {
    switch (true) {
        case weatherId >= 200 && weatherId < 300:
            return getThunderstorm; 
        case weatherId >= 300 && weatherId < 400:
            return getClouds; 
        case weatherId >= 500 && weatherId < 600:
            return getRain; 
        case weatherId >= 600 && weatherId < 700:
            return getSnow;    
        case weatherId >= 700 && weatherId < 800:
            return getFog; 
        case weatherId === 800:
            return getClear; 
        case weatherId >= 801 && weatherId < 810:
            return getCloudy; 
        default:
            return getClouds; 
    }
};

// Returns weather icon based on weather condition id
export const getWeatherIcons = (weatherId) => {
    switch (true) {
        case weatherId >= 200 && weatherId < 300:
            return iconThunderstorm; 
        case weatherId >= 300 && weatherId < 400:
            return iconClouds;
        case weatherId >= 500 && weatherId < 600:
            return iconRain;
        case weatherId >= 600 && weatherId < 700:
            return iconSnow;
        case weatherId >= 700 && weatherId < 800:
            return iconFog;
        case weatherId === 800:
            return iconClear;
        case weatherId >= 801 && weatherId < 810:
            return iconCloudy; 
        default:
            return iconClouds;
    }
};