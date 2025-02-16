import axios from "axios";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

// Fetch the center coordinates for the initial map load
export const fetchCenterCoordinates = async () => {
    try {
        const response = await axios.get(`${API_BASE_URL}/area`);
        return response.data;
    } catch (error) {
        console.error("Error fetching center coordinates:", error);
        return null;
    }
};

// Fetch all hives and extract their latitude/longitude
export const fetchHives = async () => {
    try {
        const response = await axios.get(`${API_BASE_URL}/hive`);

        return response.data.map(hive => ({
            id: hive.HiveID,
            lat: hive.Telemetry?.Location?.Latitude ?? null,
            lon: hive.Telemetry?.Location?.Longitude ?? null,
        })).filter(hive => hive.lat !== null && hive.lon !== null); // Remove invalid locations

    } catch (error) {
        console.error("Error fetching hives:", error);
        return [];
    }
};

// Move all hives to a new location
export const moveHives = async (lat, lon, ids) => {
    try {
        await axios.patch(`${API_BASE_URL}/hive`, { 
            Hives: ids, 
            Destination: {
                Latitude: lat,
                Longitude: lon
            } 
        });
        console.log(`Moved hives to: ${lat}, ${lon}`);
    } catch (error) {
        console.error("Error moving hives:", error);
        throw error;
    }
};
