# MotoDataLogger

**MotoDataLogger** is an **Android-based motorcycle tracking system** powered by the **Automate** app. It captures real-time sensor data from an Android device and structures it into a JSON request for future API integration.  

## 📌 Features  

✔ **Real-time Data Logging** – Collects essential ride data, including:  
  - 📍 GPS location & speed  
  - ☀ Ambient light levels  
  - 📊 Acceleration & orientation  
  - 🔋 Battery status  
  - 🌡 Temperature & humidity  
  - 🧲 Magnetic field readings  

✔ **JSON Data Generation** – Prepares structured JSON output for seamless API integration.  

✔ **Flexible & Customizable** – Modify the **Automate** flow directly on your phone, no coding required!  

## 🔧 Requirements  

- **Android device** with the [Automate](https://llamalab.com/automate/) app  
- **Location services enabled** for GPS tracking  
- **Paid version recommended** (Automate app flow file may exceed free version limits)  

## 🚀 Future Enhancements  

- **.NET API integration** for real-time data transmission
- **Live tracking dashboard**

- **JSON Data Generation:**
  The collected data is structured into a **JSON request** for potential transmission to an API server (planned **.NET API** integration).  

## Example JSON

Gather data from Android device to send to API

```json
{
  "Location": {
    "Latitude": "56.7069466",
    "Longitude": "29.2551711",
    "Altitude": "126.3020230117178",
    "Angle": "",
    "Speed": "",
    "Accuracy": "15.092000007629395",
    "Time": "1.738715070128E9",
    "Provider": "network"
  },
  "Light_Sensitivity": "9.99000072479248",
  "Magnetic_Field": "56.768226623535156",
  "Angle_Data": {
    "Azimuth": "163.20204162597656",
    "Pitch": "-5.505209445953369",
    "Roll": "-0.3244052827358246"
  },
  "Battery_Level": "32",
  "Battery_Charging_Time_Left": "NotCharging",
  "Timestamp": "2025-02-05 02:27:28"
}
