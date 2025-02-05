# Android Motorcycle Tracking System  

This project is an **Android-based motorcycle tracking system** built using the **Automate** app. It allows users to import and run a `.flo` flow directly in Automate to track motorcycle movement and environmental conditions.  

Currently, the app **only generates a JSON request** containing the collected sensor data. API transmission is **not yet implemented** but will be added in future commits.  

## Why Automate?  

The **Automate** app was chosen for its **robustness, reliability, and flexibility**. It allows modifications and enhancements **directly on your phone**, eliminating the need to rebuild the entire application on a workstation.  

## Features  

- **Real-time Data Collection:**  
  The system gathers sensor data, including:  
  - GPS movement  
  - Ambient light  
  - Acceleration  
  - Temperature & humidity  
  - Magnetic field  
  - Device orientation  
  - Battery level  

- **JSON Data Generation:**  
  The collected data is structured into a **JSON request** for potential transmission to an API server (planned **.NET API** integration).  

- **Customizable & Extendable:**  
  Users can freely access and modify the **Automate** flow to fit their specific needs.  

## Requirements  

- **Automate App** (Android)  
- Location services **must be enabled** for proper functionality  
- **Paid Version Required**: The flow may exceed the free version's element limit. However, optimizing the flow to reduce elements **could** eliminate the need for a paid version.  

## Future Development  

- **API Integration:** A **.NET-based API** will be developed to receive and process tracking data.  
- **Additional Features:** Potential enhancements include:  
  - Notification alerts  
  - Remote shutdown  
  - Real-time tracking UI  
