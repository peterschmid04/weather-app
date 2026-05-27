import { useCallback, useEffect, useMemo, useState } from "react";
import "./Stations.css";

const emptyStation = {
  name: "",
  cityName: "",
  countryCode: "DE",
  description: "",
  latitude: "",
  longitude: "",
};

const emptyMeasurement = {
  temperatureC: "",
  humidityPercent: "",
  pressureHpa: "",
  windSpeedKmh: "",
  windDirectionDegrees: "",
  rainfallMm: "",
  notes: "",
};

export default function Stations({ authFetchJson }) {
  const [stations, setStations] = useState([]);
  const [selectedStationId, setSelectedStationId] = useState("");
  const [stationForm, setStationForm] = useState(emptyStation);
  const [measurementForm, setMeasurementForm] = useState(emptyMeasurement);
  const [measurements, setMeasurements] = useState([]);
  const [message, setMessage] = useState("");

  const selectedStation = useMemo(
    () => stations.find((station) => station.id === selectedStationId),
    [stations, selectedStationId]
  );

  const loadStations = useCallback(async () => {
    const data = await authFetchJson("http://localhost:5122/stations/");
    setStations(data);
    if (!selectedStationId && data.length > 0) {
      setSelectedStationId(data[0].id);
    }
  }, [authFetchJson, selectedStationId]);

  const loadMeasurements = useCallback(
    async (stationId) => {
      if (!stationId) {
        setMeasurements([]);
        return;
      }
      const data = await authFetchJson(`http://localhost:5122/stations/${stationId}/measurements`);
      setMeasurements(data);
    },
    [authFetchJson]
  );

  useEffect(() => {
    loadStations().catch(() => setMessage("Stationen konnten nicht geladen werden."));
  }, [loadStations]);

  useEffect(() => {
    loadMeasurements(selectedStationId).catch(() => setMessage("Messwerte konnten nicht geladen werden."));
  }, [loadMeasurements, selectedStationId]);

  const updateStationForm = (field, value) =>
    setStationForm((current) => ({ ...current, [field]: value }));

  const updateMeasurementForm = (field, value) =>
    setMeasurementForm((current) => ({ ...current, [field]: value }));

  const numberOrNull = (value) => (value === "" ? null : Number(value));

  const createStation = async (event) => {
    event.preventDefault();
    setMessage("");
    const created = await authFetchJson("http://localhost:5122/stations/", {
      method: "POST",
      body: JSON.stringify({
        ...stationForm,
        latitude: numberOrNull(stationForm.latitude),
        longitude: numberOrNull(stationForm.longitude),
      }),
    });
    setStationForm(emptyStation);
    setSelectedStationId(created.id);
    await loadStations();
    setMessage("Station gespeichert.");
  };

  const createMeasurement = async (event) => {
    event.preventDefault();
    if (!selectedStationId) {
      setMessage("Bitte zuerst eine Station auswaehlen.");
      return;
    }
    setMessage("");
    await authFetchJson(`http://localhost:5122/stations/${selectedStationId}/measurements`, {
      method: "POST",
      body: JSON.stringify({
        temperatureC: numberOrNull(measurementForm.temperatureC),
        humidityPercent: numberOrNull(measurementForm.humidityPercent),
        pressureHpa: numberOrNull(measurementForm.pressureHpa),
        windSpeedKmh: numberOrNull(measurementForm.windSpeedKmh),
        windDirectionDegrees:
          measurementForm.windDirectionDegrees === "" ? null : Number.parseInt(measurementForm.windDirectionDegrees, 10),
        rainfallMm: numberOrNull(measurementForm.rainfallMm),
        notes: measurementForm.notes,
      }),
    });
    setMeasurementForm(emptyMeasurement);
    await Promise.all([loadStations(), loadMeasurements(selectedStationId)]);
    setMessage("Messwert gespeichert.");
  };

  return (
    <section className="stations">
      <div className="stations-header">
        <h2>Eigene Orte</h2>
        {message && <span>{message}</span>}
      </div>

      <form className="station-form" onSubmit={createStation}>
        <input value={stationForm.name} onChange={(event) => updateStationForm("name", event.target.value)} placeholder="Stationsname" required />
        <input value={stationForm.cityName} onChange={(event) => updateStationForm("cityName", event.target.value)} placeholder="Ort" required />
        <input value={stationForm.countryCode} onChange={(event) => updateStationForm("countryCode", event.target.value)} placeholder="DE" maxLength="2" required />
        <input value={stationForm.latitude} onChange={(event) => updateStationForm("latitude", event.target.value)} placeholder="Breitengrad" type="number" step="0.000001" />
        <input value={stationForm.longitude} onChange={(event) => updateStationForm("longitude", event.target.value)} placeholder="Laengengrad" type="number" step="0.000001" />
        <input value={stationForm.description} onChange={(event) => updateStationForm("description", event.target.value)} placeholder="Beschreibung" />
        <button type="submit">Ort speichern</button>
      </form>

      <div className="station-content">
        <div className="station-list">
          {stations.map((station) => (
            <button
              key={station.id}
              type="button"
              className={station.id === selectedStationId ? "active" : ""}
              onClick={() => setSelectedStationId(station.id)}
            >
              <strong>{station.name}</strong>
              <span>{station.cityName}, {station.countryCode}</span>
              {station.latestMeasurement && <small>{station.latestMeasurement.temperatureC ?? "-"} C</small>}
            </button>
          ))}
        </div>

        <form className="measurement-form" onSubmit={createMeasurement}>
          <h3>{selectedStation ? selectedStation.name : "Messwerte"}</h3>
          <input value={measurementForm.temperatureC} onChange={(event) => updateMeasurementForm("temperatureC", event.target.value)} placeholder="Temperatur C" type="number" step="0.1" />
          <input value={measurementForm.humidityPercent} onChange={(event) => updateMeasurementForm("humidityPercent", event.target.value)} placeholder="Luftfeuchte %" type="number" step="0.1" />
          <input value={measurementForm.pressureHpa} onChange={(event) => updateMeasurementForm("pressureHpa", event.target.value)} placeholder="Luftdruck hPa" type="number" step="0.1" />
          <input value={measurementForm.windSpeedKmh} onChange={(event) => updateMeasurementForm("windSpeedKmh", event.target.value)} placeholder="Wind km/h" type="number" step="0.1" />
          <input value={measurementForm.windDirectionDegrees} onChange={(event) => updateMeasurementForm("windDirectionDegrees", event.target.value)} placeholder="Windrichtung Grad" type="number" />
          <input value={measurementForm.rainfallMm} onChange={(event) => updateMeasurementForm("rainfallMm", event.target.value)} placeholder="Regen mm" type="number" step="0.1" />
          <input value={measurementForm.notes} onChange={(event) => updateMeasurementForm("notes", event.target.value)} placeholder="Notiz" />
          <button type="submit">Messwert speichern</button>
        </form>

        <div className="measurement-list">
          {measurements.slice(0, 5).map((measurement) => (
            <article key={measurement.id}>
              <strong>{new Date(measurement.measuredAtUtc).toLocaleString("de-DE")}</strong>
              <span>{measurement.temperatureC ?? "-"} C | {measurement.humidityPercent ?? "-"} % | {measurement.pressureHpa ?? "-"} hPa</span>
              {measurement.notes && <small>{measurement.notes}</small>}
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}
