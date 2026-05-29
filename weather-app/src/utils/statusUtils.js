// Converts numeric weather values into compact German status labels for the
// highlight cards. The raw values still come from the backend/API.
export const getStatusWind = (value) => {
  switch (true) {
    case value <= 5:
      return "Schwach";
    case value <= 10:
      return "Mäßig";
    case value <= 20:
      return "Stark";
    case value <= 30:
      return "Sehr stark";
    default:
      return "Extrem";
  }
};

export const getStatusVisibility = (value) => {
  switch (true) {
    case value <= 1:
      return "Niedrig";
    case value <= 3:
      return "Mäßig";
    case value <= 5:
      return "Hoch";
    case value <= 10:
      return "Sehr hoch";
    default:
      return "Extrem";
  }
};

export const getStatusHumidity = (value) => {
  switch (true) {
    case value <= 30:
      return "Trocken";
    case value <= 60:
      return "Mäßig";
    case value <= 80:
      return "Feucht";
    case value <= 100:
      return "Sehr feucht";
    default:
      return "Extrem";
  }
};

export const getStatusAirquality = (value) => {
  switch (true) {
    case value === 1:
      return "Gut";
    case value === 2:
      return "Okay";
    case value === 3:
      return "Mäßig";
    case value === 4:
      return "Schlecht";
    case value === 5:
      return "Sehr schlecht";
    default:
      return "Unbekannt";
  }
};
