const weekdayMap = {
  Sunday: "Sonntag",
  Monday: "Montag",
  Tuesday: "Dienstag",
  Wednesday: "Mittwoch",
  Thursday: "Donnerstag",
  Friday: "Freitag",
  Saturday: "Samstag",
};

const weatherDescriptionMap = {
  "clear sky": "klarer Himmel",
  "sky is clear": "klarer Himmel",
  "few clouds": "leicht bewölkt",
  "scattered clouds": "aufgelockerte Bewölkung",
  "broken clouds": "wechselnd bewölkt",
  "overcast clouds": "bedeckt",
  "light rain": "leichter Regen",
  "moderate rain": "mäßiger Regen",
  "heavy intensity rain": "starker Regen",
  "very heavy rain": "sehr starker Regen",
  "extreme rain": "extremer Regen",
  "freezing rain": "gefrierender Regen",
  "light intensity shower rain": "leichter Schauerregen",
  "shower rain": "Schauerregen",
  "heavy intensity shower rain": "starker Schauerregen",
  "ragged shower rain": "unregelmäßiger Schauerregen",
  thunderstorm: "Gewitter",
  "thunderstorm with light rain": "Gewitter mit leichtem Regen",
  "thunderstorm with rain": "Gewitter mit Regen",
  "thunderstorm with heavy rain": "Gewitter mit starkem Regen",
  "light thunderstorm": "leichtes Gewitter",
  "heavy thunderstorm": "starkes Gewitter",
  "ragged thunderstorm": "unregelmäßiges Gewitter",
  snow: "Schnee",
  "light snow": "leichter Schnee",
  "heavy snow": "starker Schnee",
  sleet: "Schneeregen",
  "light shower sleet": "leichter Schneeregenschauer",
  "shower sleet": "Schneeregenschauer",
  "light rain and snow": "leichter Regen und Schnee",
  "rain and snow": "Regen und Schnee",
  mist: "Dunst",
  smoke: "Rauch",
  haze: "Dunstschleier",
  fog: "Nebel",
  sand: "Sand",
  dust: "Staub",
  ash: "Asche",
  squall: "Sturmböen",
  tornado: "Tornado",
};

const cityNameMap = {
  freiburg: "Freiburg",
  "freiburg im breisgau": "Freiburg im Breisgau",
  vienna: "Wien",
  munich: "München",
  cologne: "Köln",
  nuremberg: "Nürnberg",
  zurich: "Zürich",
  geneva: "Genf",
  basel: "Basel",
  prague: "Prag",
  brussels: "Brüssel",
  copenhagen: "Kopenhagen",
  warsaw: "Warschau",
  cracow: "Krakau",
  krakow: "Krakau",
  milan: "Mailand",
  venice: "Venedig",
  rome: "Rom",
  florence: "Florenz",
  naples: "Neapel",
  turin: "Turin",
  genoa: "Genua",
  lisbon: "Lissabon",
  athens: "Athen",
  moscow: "Moskau",
};

export const translateWeekday = (value) => weekdayMap[value] || value;

export const translateWeatherDescription = (value) => {
  if (!value) {
    return "";
  }

  return weatherDescriptionMap[value.toLowerCase()] || value;
};

export const getGermanCityName = (value) => {
  if (!value) {
    return "";
  }

  const trimmedValue = value.trim();
  return cityNameMap[trimmedValue.toLocaleLowerCase("de-DE")] || trimmedValue;
};

export const formatCityLocation = (cityName, countryCode) => {
  const city = getGermanCityName(cityName);
  const country = (countryCode || "").toString().trim().toUpperCase();
  return country ? `${city}, ${country}` : city;
};
