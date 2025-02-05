# Android Motorcycle Tracking System  

This project is an **Android-based motorcycle Tracking system** built using the **Automate** app. It allows users to import and run a `.flo` flow directly in the Automate application to track motorcycle movement and environmental conditions.  

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

- **JSON Data Transmission:**  
  The collected data is formatted into a **JSON request** that can be sent to an API server (future implementation with **.NET API**).  

- **Customizable & Extendable:**  
  Users can freely access and modify the **Automate** flow to fit their needs.  

## Requirements  

- **Automate App** (Android)  
- Location services **must be enabled** for proper functionality  
- **Paid Version IS Required**: The flow may exceed the free version's allowed number of elements. It is possible to lower the amount of elements thus reducing the need for a paid version.  

## Future Development  

- **API Implementation:** A **.NET-based API** will be developed to receive and process tracking data.  
- **Additional Features:** Possible improvements include notification alerts, remote shutdown, and real-time tracking UI.  
