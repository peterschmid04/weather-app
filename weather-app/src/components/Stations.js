import { useCallback, useEffect, useMemo, useState } from "react";
import "./Stations.css";
import { formatCityLocation } from "../utils/localizationUtils";
import { buildApiUrl } from "../utils/apiUtils";

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

export default function Stations({ authFetchJson, selectedStationId, onSelectedStationChange, onStationsChanged }) {
  const [stations, setStations] = useState([]);
  const [editingStationId, setEditingStationId] = useState("");
  const [stationForm, setStationForm] = useState(emptyStation);
  const [measurementForm, setMeasurementForm] = useState(emptyMeasurement);
  const [measurements, setMeasurements] = useState([]);
  const [message, setMessage] = useState("");

  const selectedStation = useMemo(
    () => stations.find((station) => station.id === selectedStationId),
    [stations, selectedStationId]
  );

  const loadStations = useCallback(async (preferredStationId = selectedStationId) => {
    const data = await authFetchJson(buildApiUrl("/stations/"));
    setStations(data);

    if (preferredStationId && data.some((station) => station.id === preferredStationId)) {
      onSelectedStationChange?.(preferredStationId);
      return data;
    }

    onSelectedStationChange?.(data[0]?.id || "");
    return data;
  }, [authFetchJson, onSelectedStationChange, selectedStationId]);

  const setSelectedStationId = useCallback(
    (stationId) => {
      onSelectedStationChange?.(stationId);
    },
    [onSelectedStationChange]
  );

  useEffect(() => {
    if (!selectedStationId) {
      return;
    }

    const selectedStationStillExists = stations.some((station) => station.id === selectedStationId);
    if (stations.length > 0 && !selectedStationStillExists) {
      onSelectedStationChange?.(stations[0].id);
    }
  }, [onSelectedStationChange, selectedStationId, stations]);

  const loadMeasurements = useCallback(
    async (stationId) => {
      if (!stationId) {
        setMeasurements([]);
        return;
      }
      const data = await authFetchJson(buildApiUrl(`/stations/${stationId}/measurements`));
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

  const resetStationForm = () => {
    setStationForm(emptyStation);
    setEditingStationId("");
  };

  const saveStation = async (event) => {
    event.preventDefault();
    setMessage("");
    if (!stationForm.name.trim()) {
      setMessage("Bitte einen Stationsnamen eintragen.");
      return;
    }

    const payload = {
      ...stationForm,
      cityName: stationForm.cityName.trim() || null,
      countryCode: stationForm.countryCode.trim() || null,
      latitude: numberOrNull(stationForm.latitude),
      longitude: numberOrNull(stationForm.longitude),
    };

    const url = editingStationId
      ? buildApiUrl(`/stations/${editingStationId}`)
      : buildApiUrl("/stations/");

    try {
      const saved = await authFetchJson(url, {
        method: editingStationId ? "PUT" : "POST",
        body: JSON.stringify(payload),
      });
      resetStationForm();
      await loadStations(saved.id);
      onStationsChanged?.();
      setMessage(editingStationId ? "Station aktualisiert." : "Station gespeichert.");
    } catch (error) {
      setMessage(error.message || "Station konnte nicht gespeichert werden.");
    }
  };

  const editStation = (station) => {
    setEditingStationId(station.id);
    setStationForm({
      name: station.name,
      cityName: station.cityName,
      countryCode: station.countryCode,
      description: station.description || "",
      latitude: station.latitude ?? "",
      longitude: station.longitude ?? "",
    });
  };

  const deleteStation = async (stationId) => {
    try {
      await authFetchJson(buildApiUrl(`/stations/${stationId}`), { method: "DELETE" });
      if (selectedStationId === stationId) {
        setSelectedStationId("");
        setMeasurements([]);
      }
      resetStationForm();
      await loadStations();
      onStationsChanged?.();
      setMessage("Station gelöscht.");
    } catch (error) {
      setMessage(error.message || "Station konnte nicht gelöscht werden.");
    }
  };

  const createMeasurement = async (event) => {
    event.preventDefault();
    if (!selectedStationId) {
      setMessage("Bitte zuerst eine Station auswählen.");
      return;
    }

    setMessage("");
    try {
      await authFetchJson(buildApiUrl(`/stations/${selectedStationId}/measurements`), {
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
    } catch (error) {
      setMessage(error.message || "Messwert konnte nicht gespeichert werden.");
    }
  };

  return (
    <section className="stations">
      <div className="stations-header">
        <div>
          <h2>Eigene Orte und Wetterstationen</h2>
          <p>Nur der Stationsname ist Pflicht. Ort und Land können später angepasst werden.</p>
        </div>
        {message && <span>{message}</span>}
      </div>

      <form className="station-form" onSubmit={saveStation}>
        <input value={stationForm.name} onChange={(event) => updateStationForm("name", event.target.value)} placeholder="Stationsname" required />
        <input value={stationForm.cityName} onChange={(event) => updateStationForm("cityName", event.target.value)} placeholder="Ort optional" />
        <input value={stationForm.countryCode} onChange={(event) => updateStationForm("countryCode", event.target.value.toUpperCase())} placeholder="DE" maxLength="2" />
        <input value={stationForm.latitude} onChange={(event) => updateStationForm("latitude", event.target.value)} placeholder="Breitengrad optional" type="number" step="0.000001" />
        <input value={stationForm.longitude} onChange={(event) => updateStationForm("longitude", event.target.value)} placeholder="Längengrad optional" type="number" step="0.000001" />
        <input value={stationForm.description} onChange={(event) => updateStationForm("description", event.target.value)} placeholder="Beschreibung optional" />
        <button type="submit">{editingStationId ? "Station ändern" : "Station speichern"}</button>
        {editingStationId && <button type="button" onClick={resetStationForm}>Abbrechen</button>}
      </form>

      <div className="station-content">
        <div className="station-list">
          {stations.length === 0 && <p className="empty">Noch keine eigene Wetterstation gespeichert.</p>}
          {stations.map((station) => (
            <article key={station.id} className={station.id === selectedStationId ? "active" : ""}>
              <button type="button" className="station-select" onClick={() => setSelectedStationId(station.id)}>
                <strong>{station.name}</strong>
                <span>{formatCityLocation(station.cityName, station.countryCode)}</span>
                {station.latestMeasurement && <small>{station.latestMeasurement.temperatureC ?? "-"} °C</small>}
              </button>
              <div className="station-actions">
                <button type="button" onClick={() => editStation(station)}>Bearbeiten</button>
                <button type="button" onClick={() => deleteStation(station.id)}>Löschen</button>
              </div>
            </article>
          ))}
        </div>

        <form className="measurement-form" onSubmit={createMeasurement}>
          <h3>{selectedStation ? selectedStation.name : "Messwerte"}</h3>
          <input value={measurementForm.temperatureC} onChange={(event) => updateMeasurementForm("temperatureC", event.target.value)} placeholder="Temperatur °C" type="number" step="0.1" />
          <input value={measurementForm.humidityPercent} onChange={(event) => updateMeasurementForm("humidityPercent", event.target.value)} placeholder="Luftfeuchte %" type="number" step="0.1" />
          <input value={measurementForm.pressureHpa} onChange={(event) => updateMeasurementForm("pressureHpa", event.target.value)} placeholder="Luftdruck hPa" type="number" step="0.1" />
          <input value={measurementForm.windSpeedKmh} onChange={(event) => updateMeasurementForm("windSpeedKmh", event.target.value)} placeholder="Wind km/h" type="number" step="0.1" />
          <input value={measurementForm.windDirectionDegrees} onChange={(event) => updateMeasurementForm("windDirectionDegrees", event.target.value)} placeholder="Windrichtung Grad" type="number" />
          <input value={measurementForm.rainfallMm} onChange={(event) => updateMeasurementForm("rainfallMm", event.target.value)} placeholder="Regen mm" type="number" step="0.1" />
          <input value={measurementForm.notes} onChange={(event) => updateMeasurementForm("notes", event.target.value)} placeholder="Notiz optional" />
          <button type="submit">Messwert speichern</button>
        </form>

        <div className="measurement-list">
          {measurements.length === 0 && <p className="empty">Noch keine Messwerte gespeichert.</p>}
          {measurements.slice(0, 5).map((measurement) => (
            <article key={measurement.id}>
              <strong>{new Date(measurement.measuredAtUtc).toLocaleString("de-DE")}</strong>
              <span>{measurement.temperatureC ?? "-"} °C | {measurement.humidityPercent ?? "-"} % | {measurement.pressureHpa ?? "-"} hPa</span>
              {measurement.notes && <small>{measurement.notes}</small>}
            </article>
          ))}
        </div>
      </div>
    </section>
  );
}
