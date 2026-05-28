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

const citySuggestions = [
  { cityName: "Aachen", countryCode: "DE", query: "Aachen" },
  { cityName: "Augsburg", countryCode: "DE", query: "Augsburg" },
  { cityName: "Amsterdam", countryCode: "NL", query: "Amsterdam" },
  { cityName: "Athen", countryCode: "GR", query: "Athen", aliases: ["Athens"] },
  { cityName: "Barcelona", countryCode: "ES", query: "Barcelona" },
  { cityName: "Basel", countryCode: "CH", query: "Basel" },
  { cityName: "Berlin", countryCode: "DE", query: "Berlin" },
  { cityName: "Bielefeld", countryCode: "DE", query: "Bielefeld" },
  { cityName: "Bochum", countryCode: "DE", query: "Bochum" },
  { cityName: "Bonn", countryCode: "DE", query: "Bonn" },
  { cityName: "Bremen", countryCode: "DE", query: "Bremen" },
  { cityName: "Bremerhaven", countryCode: "DE", query: "Bremerhaven" },
  { cityName: "Brüssel", countryCode: "BE", query: "Brüssel", aliases: ["Bruessel", "Brussels"] },
  { cityName: "Budapest", countryCode: "HU", query: "Budapest" },
  { cityName: "Chemnitz", countryCode: "DE", query: "Chemnitz" },
  { cityName: "Dortmund", countryCode: "DE", query: "Dortmund" },
  { cityName: "Dresden", countryCode: "DE", query: "Dresden" },
  { cityName: "Duisburg", countryCode: "DE", query: "Duisburg" },
  { cityName: "Düsseldorf", countryCode: "DE", query: "Düsseldorf", aliases: ["Duesseldorf"] },
  { cityName: "Erfurt", countryCode: "DE", query: "Erfurt" },
  { cityName: "Erlangen", countryCode: "DE", query: "Erlangen" },
  { cityName: "Essen", countryCode: "DE", query: "Essen" },
  { cityName: "Florenz", countryCode: "IT", query: "Florenz", aliases: ["Florence"] },
  { cityName: "Frankfurt am Main", countryCode: "DE", query: "Frankfurt am Main" },
  { cityName: "Frankfurt (Oder)", countryCode: "DE", query: "Frankfurt Oder" },
  { cityName: "Freiburg im Breisgau", countryCode: "DE", query: "Freiburg im Breisgau" },
  { cityName: "Freiburg", countryCode: "DE", query: "Freiburg" },
  { cityName: "Genf", countryCode: "CH", query: "Genf", aliases: ["Geneva"] },
  { cityName: "Genua", countryCode: "IT", query: "Genua", aliases: ["Genoa"] },
  { cityName: "Graz", countryCode: "AT", query: "Graz" },
  { cityName: "Hagen", countryCode: "DE", query: "Hagen" },
  { cityName: "Halle", countryCode: "DE", query: "Halle" },
  { cityName: "Hamburg", countryCode: "DE", query: "Hamburg" },
  { cityName: "Hamm", countryCode: "DE", query: "Hamm" },
  { cityName: "Hannover", countryCode: "DE", query: "Hannover" },
  { cityName: "Heidelberg", countryCode: "DE", query: "Heidelberg" },
  { cityName: "Heilbronn", countryCode: "DE", query: "Heilbronn" },
  { cityName: "Innsbruck", countryCode: "AT", query: "Innsbruck" },
  { cityName: "Karlsruhe", countryCode: "DE", query: "Karlsruhe" },
  { cityName: "Kassel", countryCode: "DE", query: "Kassel" },
  { cityName: "Kiel", countryCode: "DE", query: "Kiel" },
  { cityName: "Koblenz", countryCode: "DE", query: "Koblenz" },
  { cityName: "Köln", countryCode: "DE", query: "Köln", aliases: ["Koeln", "Cologne"] },
  { cityName: "Kopenhagen", countryCode: "DK", query: "Kopenhagen", aliases: ["Copenhagen"] },
  { cityName: "Krakau", countryCode: "PL", query: "Krakau", aliases: ["Cracow", "Krakow"] },
  { cityName: "Leipzig", countryCode: "DE", query: "Leipzig" },
  { cityName: "Lissabon", countryCode: "PT", query: "Lissabon", aliases: ["Lisbon"] },
  { cityName: "Liverpool", countryCode: "GB", query: "Liverpool" },
  { cityName: "London", countryCode: "GB", query: "London" },
  { cityName: "Lübeck", countryCode: "DE", query: "Lübeck", aliases: ["Luebeck"] },
  { cityName: "Ludwigsburg", countryCode: "DE", query: "Ludwigsburg" },
  { cityName: "Madrid", countryCode: "ES", query: "Madrid" },
  { cityName: "Mailand", countryCode: "IT", query: "Mailand", aliases: ["Milan"] },
  { cityName: "Mainz", countryCode: "DE", query: "Mainz" },
  { cityName: "Mannheim", countryCode: "DE", query: "Mannheim" },
  { cityName: "Moskau", countryCode: "RU", query: "Moskau", aliases: ["Moscow"] },
  { cityName: "München", countryCode: "DE", query: "München", aliases: ["Muenchen", "Munich"] },
  { cityName: "Münster", countryCode: "DE", query: "Münster", aliases: ["Muenster"] },
  { cityName: "Neapel", countryCode: "IT", query: "Neapel", aliases: ["Naples"] },
  { cityName: "New York", countryCode: "US", query: "New York" },
  { cityName: "Nürnberg", countryCode: "DE", query: "Nürnberg", aliases: ["Nuernberg", "Nuremberg"] },
  { cityName: "Offenburg", countryCode: "DE", query: "Offenburg" },
  { cityName: "Oldenburg", countryCode: "DE", query: "Oldenburg" },
  { cityName: "Osnabrück", countryCode: "DE", query: "Osnabrück", aliases: ["Osnabrueck"] },
  { cityName: "Paris", countryCode: "FR", query: "Paris" },
  { cityName: "Palermo", countryCode: "IT", query: "Palermo" },
  { cityName: "Peking", countryCode: "CN", query: "Peking", aliases: ["Beijing"] },
  { cityName: "Pforzheim", countryCode: "DE", query: "Pforzheim" },
  { cityName: "Porto", countryCode: "PT", query: "Porto" },
  { cityName: "Potsdam", countryCode: "DE", query: "Potsdam" },
  { cityName: "Prag", countryCode: "CZ", query: "Prag", aliases: ["Prague"] },
  { cityName: "Regensburg", countryCode: "DE", query: "Regensburg" },
  { cityName: "Reutlingen", countryCode: "DE", query: "Reutlingen" },
  { cityName: "Rom", countryCode: "IT", query: "Rom", aliases: ["Rome"] },
  { cityName: "Rostock", countryCode: "DE", query: "Rostock" },
  { cityName: "Saarbrücken", countryCode: "DE", query: "Saarbrücken", aliases: ["Saarbruecken"] },
  { cityName: "Salzburg", countryCode: "AT", query: "Salzburg" },
  { cityName: "Schiltach", countryCode: "DE", query: "Schiltach" },
  { cityName: "Stuttgart", countryCode: "DE", query: "Stuttgart" },
  { cityName: "Turin", countryCode: "IT", query: "Turin" },
  { cityName: "Ulm", countryCode: "DE", query: "Ulm" },
  { cityName: "Venedig", countryCode: "IT", query: "Venedig", aliases: ["Venice"] },
  { cityName: "Warschau", countryCode: "PL", query: "Warschau", aliases: ["Warsaw"] },
  { cityName: "Wien", countryCode: "AT", query: "Wien", aliases: ["Vienna"] },
  { cityName: "Wiesbaden", countryCode: "DE", query: "Wiesbaden" },
  { cityName: "Wuppertal", countryCode: "DE", query: "Wuppertal" },
  { cityName: "Zürich", countryCode: "CH", query: "Zürich", aliases: ["Zuerich", "Zurich"] },
];

const normalizeForSearch = (value) =>
  value
    .trim()
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/[()]/g, "")
    .replace(/\s+/g, " ")
    .toLocaleLowerCase("de-DE");

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

const getSuggestionSearchTerms = (suggestion) => [
  suggestion.cityName,
  suggestion.query,
  suggestion.countryCode,
  ...(suggestion.aliases || []),
];

const suggestionMatchesInput = (suggestion, normalizedInput) =>
  getSuggestionSearchTerms(suggestion).some((term) => {
    const normalizedTerm = normalizeForSearch(term);
    return normalizedTerm.startsWith(normalizedInput);
  });

export const getCitySuggestions = (value, limit = 10) => {
  const normalizedInput = normalizeForSearch(value);

  if (!normalizedInput) {
    return [];
  }

  return citySuggestions
    .filter((suggestion) => suggestionMatchesInput(suggestion, normalizedInput))
    .slice(0, limit);
};
