# MotoDataLogger (API FINISHED 1.0v)

**MotoDataLogger** is an open source **Android-based motorcycle tracking system** powered by the **Automate** app. It captures real-time sensor data from an Android device and structures it into a JSON request for future API integration. The idea is to be able to use an older or unused android device for tracking motorcycle or any other vehicle. The system implements the alarm system which will warn user if any of the sensors values changed beyond threshold.

## 📌 Features

✔ **Real-time Data Logging** – Collects essential ride data, including: 
  - 📍 GPS location & speed  
  - ☀ Ambient light levels  
  - 📊 Acceleration & orientation  
  - 🔋 Battery status  
  - 🌡 Temperature & humidity  
  - 🧲 Magnetic field readings
✔ **API and Web UI** – Receives and displays collected data.

<img width="1550" height="971" alt="dashboard" src="https://github.com/user-attachments/assets/49a02667-c7e0-4ab4-95bf-c7c39c42a42b" />


✔ **Traffic monitoring and blocking** – Functionality to inspect IP addresses that access the system and whitelist/blacklist these IP addresses using Web UI.

✔ **JSON Data Generation** – Prepares structured JSON output for seamless API integration.

✔ **Flexible & Customizable** – Modify the **Automate** flow directly on your phone, no coding required!  

## 🔧 Requirements  

- **Android device** with the [Automate](https://llamalab.com/automate/) app  
- **Location services enabled** for GPS tracking. Might need rooted device depending on your case. Main problem may be that the automate app will not be able to send data to the api, if the device is locked. There is a workaround for non-rooted with which I had success. I will write down this workaround later on, when I have time.
- **Paid version is a must** (Automate app flow file may exceed free version limits)  

## 🚀 Future Enhancements  

- **A separate app instead of an automate flow?** If anyone is interested and has skills in android/ios development contact me. Currently, I dont have time nor resources to develop Android or IOS application.  

## Example JSON

Gather data from Android device using Automate flow to send to API

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
