const express = require('express');
const router = express.Router();
const { generateToken, adminLogin, authenticateAdmin, authenticateTracker } = require('./auth');
const { addUser, deleteUser, getAllUsers, addTrackerData, getLastTrackerData, getAllTrackerData, addAccessLog, getAccessLogs, getAccessLogsByIP, getAccessLogsByPath, getAccessLogsByMethod, getAccessLogsWithFilters, getAccessLogStats, getAllKnownIPs, whitelistIP, removeWhitelistIP, forgetKnownIP, getAllBlacklistedIPs, addToBlacklist, removeFromBlacklist, getAlarmSetting, setAlarmSetting, isAlarmEnabled } = require('./db');
const crypto = require('crypto');
const fetch = require('node-fetch');
require('dotenv').config();

// Battery check function
function checkBatteryStatus() {
  getAllUsers((err, users) => {
    if (err || !users) return;
    users.forEach(user => {
      getLastTrackerData(user.id, (err, data) => {
        if (err || !data) return;
        const batteryLevel = parseInt(data.battery_level);
        const lastRequestTime = new Date(data.timestamp);
        const now = new Date();
        const timeDiffMinutes = (now - lastRequestTime) / (1000 * 60);
        
        if (batteryLevel < 30 && timeDiffMinutes <= 25) {
          const pushoverToken = process.env.PUSHOVER_APP_TOKEN;
          const pushoverUser = process.env.PUSHOVER_USER_KEY;
          const message = `Low battery alert: ${user.username} battery is at ${batteryLevel}%`;
          console.log('[DEBUG] Sending low battery notification:', message);
          fetch('https://api.pushover.net/1/messages.json', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: new URLSearchParams({
              token: pushoverToken,
              user: pushoverUser,
              message
            })
          });
        }
      });
    });
  });
}

// Check battery every 30 minutes
setInterval(checkBatteryStatus, 30 * 60 * 1000);

// Manual battery check endpoint (admin only)
router.get('/check-battery', authenticateAdmin, (req, res) => {
  checkBatteryStatus();
  res.json({ message: 'Battery check initiated' });
});

// Admin login
router.post('/admin/login', (req, res) => {
  const { username, password } = req.body;
  if (adminLogin(username, password)) {
    const token = generateToken({ username, role: 'admin' });
    return res.json({ token });
  }
  res.status(401).json({ error: 'Invalid admin credentials' });
});

// Get all users (admin only)
router.get('/users', authenticateAdmin, (req, res) => {
  getAllUsers((err, users) => {
    if (err) return res.status(500).json({ error: 'DB error' });
    res.json(users);
  });
});

// Add user (admin only)
router.post('/users', authenticateAdmin, (req, res) => {
  const { username, password } = req.body;
  const token = crypto.randomBytes(16).toString('hex');
  addUser(username, password, token, (err) => {
    if (err) return res.status(400).json({ error: 'User exists or DB error' });
    res.json({ username, token });
  });
});

// Delete user (admin only)
router.delete('/users/:username', authenticateAdmin, (req, res) => {
  deleteUser(req.params.username, (err) => {
    if (err) return res.status(400).json({ error: 'DB error' });
    res.json({ success: true });
  });
});

// Tracker data ingestion
router.post('/tracker/data', authenticateTracker, (req, res) => {
  const user = req.trackerUser;
  const data = req.body;
  console.log('[DEBUG] /tracker/data called with user:', user ? user.username : null);
  // Check for major changes
  getLastTrackerData(user.id, (err, lastData) => {
    if (err) {
      console.error('[DEBUG] Error fetching last tracker data:', err);
      return res.status(500).json({ error: 'DB error', debug: 'Error fetching last tracker data' });
    }
    let majorChange = false;
    let lowBattery = false;
    if (lastData) {
      // Thresholds (can be adjusted)
      const locThreshold = 0.0005; // ~50m
      const angleThreshold = 5; // degrees
      // Location change
      const latDiff = Math.abs(parseFloat(data.Location.Latitude) - parseFloat(lastData.location_latitude));
      const lonDiff = Math.abs(parseFloat(data.Location.Longitude) - parseFloat(lastData.location_longitude));
      console.log(`[DEBUG] latDiff: ${latDiff}, lonDiff: ${lonDiff}`);
      if (latDiff > locThreshold || lonDiff > locThreshold) {
        majorChange = true;
        console.log('[DEBUG] Major location change detected');
      }
      // Angle change
      const azDiff = Math.abs(parseFloat(data.Angle_Data.Azimuth) - parseFloat(lastData.angle_azimuth));
      const pitchDiff = Math.abs(parseFloat(data.Angle_Data.Pitch) - parseFloat(lastData.angle_pitch));
      const rollDiff = Math.abs(parseFloat(data.Angle_Data.Roll) - parseFloat(lastData.angle_roll));
      console.log(`[DEBUG] azDiff: ${azDiff}, pitchDiff: ${pitchDiff}, rollDiff: ${rollDiff}`);
      if (rollDiff > angleThreshold) {
        majorChange = true;
        console.log('[DEBUG] Major angle change detected');
      }
    } else {
      console.log('[DEBUG] No previous tracker data found for user');
    }
    // Battery check
    if (parseInt(data.Battery_Level) < 20) {
      lowBattery = true;
      console.log('[DEBUG] Low battery detected');
      
      // Send low battery notification if alarms are enabled
      isAlarmEnabled((err, alarmEnabled) => {
        if (err) {
          console.error('[ERROR] Failed to check alarm status for battery:', err);
          return;
        }
        
        if (alarmEnabled !== false) {
          const pushoverToken = process.env.PUSHOVER_APP_TOKEN;
          const pushoverUser = process.env.PUSHOVER_USER_KEY;
          const message = `⚠️ LOW BATTERY ALERT!\n\nUser: ${user.username}\nBattery Level: ${data.Battery_Level}%\nTime: ${new Date().toLocaleString()}\n\nDevice battery is critically low.`;
          console.log('[DEBUG] Sending low battery notification with alarm:', message);
          fetch('https://api.pushover.net/1/messages.json', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: new URLSearchParams({
              token: pushoverToken,
              user: pushoverUser,
              message,
              title: 'MotoDataLogger - Low Battery Alert',
              priority: 1, // High priority
              sound: 'falling' // Use falling sound for battery alerts
            })
          });
        }
      });
    }
    // Check if alarms are enabled before sending notifications
    isAlarmEnabled((err, alarmEnabled) => {
      if (err) {
        console.error('[ERROR] Failed to check alarm status:', err);
        // Continue without notification if we can't check alarm status
      }
      
      // Send pushover notification if major change detected and alarms are enabled
      if (majorChange && alarmEnabled !== false) {
        const pushoverToken = process.env.PUSHOVER_APP_TOKEN;
        const pushoverUser = process.env.PUSHOVER_USER_KEY;
        const message = `🚨 MAJOR CHANGE DETECTED!\n\nUser: ${user.username}\nTime: ${new Date().toLocaleString()}\n\nLocation or angle has changed significantly.`;
        console.log('[DEBUG] Sending pushover notification with alarm:', message);
        fetch('https://api.pushover.net/1/messages.json', {
          method: 'POST',
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
          body: new URLSearchParams({
            token: pushoverToken,
            user: pushoverUser,
            message,
            title: 'MotoDataLogger - Major Change Alert',
            priority: 2, // Emergency priority (bypasses quiet hours)
            sound: 'siren' // Use siren sound for major change alerts
          })
        });
      } else if (majorChange && alarmEnabled === false) {
        console.log('[DEBUG] Major change detected but alarms are disabled');
      }
    });
    addTrackerData(user.id, data, (err) => {
      if (err) {
        console.error('[DEBUG] Error adding tracker data:', err);
        return res.status(500).json({ error: 'DB error', debug: 'Error adding tracker data' });
      }
      console.log('[DEBUG] Tracker data added successfully');
      res.json({ success: true, majorChange, lowBattery });
    });
  });
});

// Get latest tracker data for dashboard (admin only)
router.get('/tracker/latest', authenticateAdmin, (req, res) => {
  // For demo, get the first user and their latest data
  const { getAllUsers, getLastTrackerData } = require('./db');
  getAllUsers((err, users) => {
    if (err || !users || users.length === 0) return res.status(404).json({ error: 'No users' });
    const user = users[0];
    getLastTrackerData(user.id, (err, data) => {
      if (err || !data) return res.status(404).json({ error: 'No tracker data' });
      // Parse raw_json for full tracker data
      let parsed = {};
      try { parsed = JSON.parse(data.raw_json); } catch (e) {}
      res.json(parsed);
    });
  });
});

// Get all location data for map (admin only)
router.get('/tracker/locations', authenticateAdmin, (req, res) => {
  const { getAllUsers, getAllTrackerData } = require('./db');
  getAllUsers((err, users) => {
    if (err || !users || users.length === 0) return res.status(404).json({ error: 'No users' });
    const user = users[0];
    getAllTrackerData(user.id, (err, data) => {
      if (err || !data || data.length === 0) return res.status(404).json({ error: 'No location data' });
      
      // Parse and format location data
      const locations = data.map(item => {
        let parsed = {};
        try { parsed = JSON.parse(item.raw_json); } catch (e) {}
        
        return {
          id: item.id,
          latitude: parseFloat(parsed.Location?.Latitude || 0),
          longitude: parseFloat(parsed.Location?.Longitude || 0),
          altitude: parseFloat(parsed.Location?.Altitude || 0),
          accuracy: parseFloat(parsed.Location?.Accuracy || 0),
          timestamp: item.timestamp,
          battery_level: parseInt(parsed.Battery_Level || 0),
          speed: parsed.Location?.Speed || '',
          provider: parsed.Location?.Provider || 'unknown'
        };
      }).filter(loc => 
        loc.latitude !== 0 && 
        loc.longitude !== 0 && 
        loc.accuracy < 200
      ); // Filter out invalid coordinates and low accuracy locations
      
      res.json(locations);
    });
  });
});

// Get alarm settings (admin only)
router.get('/alarm/settings', authenticateAdmin, (req, res) => {
  isAlarmEnabled((err, enabled) => {
    if (err) return res.status(500).json({ error: 'DB error' });
    res.json({ alarm_enabled: enabled });
  });
});

// Update alarm settings (admin only)
router.post('/alarm/settings', authenticateAdmin, (req, res) => {
  const { alarm_enabled } = req.body;
  
  if (typeof alarm_enabled !== 'boolean') {
    return res.status(400).json({ error: 'Invalid alarm_enabled value' });
  }
  
  setAlarmSetting('alarm_enabled', alarm_enabled.toString(), (err) => {
    if (err) return res.status(500).json({ error: 'DB error' });
    res.json({ success: true, alarm_enabled });
  });
});

// Get access logs (admin only)
router.get('/access-logs', authenticateAdmin, (req, res) => {
  const limit = parseInt(req.query.limit) || 100;
  const offset = parseInt(req.query.offset) || 0;
  
  getAccessLogs(limit, offset, (err, logs) => {
    if (err) return res.status(500).json({ error: 'DB error' });
    res.json(logs);
  });
});

// Get access logs by IP address (admin only)
router.get('/access-logs/ip/:ip', authenticateAdmin, (req, res) => {
  const ip = req.params.ip;
  const limit = parseInt(req.query.limit) || 100;
  const offset = parseInt(req.query.offset) || 0;
  
  getAccessLogsByIP(ip, limit, offset, (err, logs) => {
    if (err) return res.status(500).json({ error: 'DB error' });
    res.json(logs);
  });
});

// Get access logs by path (admin only)
router.get('/access-logs/path/:path', authenticateAdmin, (req, res) => {
  const path = req.params.path;
  const limit = parseInt(req.query.limit) || 100;
  const offset = parseInt(req.query.offset) || 0;
  
  getAccessLogsByPath(path, limit, offset, (err, logs) => {
    if (err) return res.status(500).json({ error: 'DB error' });
    res.json(logs);
  });
});

// Get access logs by method (admin only)
router.get('/access-logs/method/:method', authenticateAdmin, (req, res) => {
  const method = req.params.method;
  const limit = parseInt(req.query.limit) || 100;
  const offset = parseInt(req.query.offset) || 0;
  
  getAccessLogsByMethod(method, limit, offset, (err, logs) => {
    if (err) return res.status(500).json({ error: 'DB error' });
    res.json(logs);
  });
});

// Get access logs with combined filters (admin only)
router.get('/access-logs/filter', authenticateAdmin, (req, res) => {
  const { ip, path, method, limit = 100, offset = 0 } = req.query;
  
  getAccessLogsWithFilters({ ip, path, method }, parseInt(limit), parseInt(offset), (err, logs) => {
    if (err) return res.status(500).json({ error: 'DB error' });
    res.json(logs);
  });
});

// Get access log statistics (admin only)
router.get('/access-logs/stats', authenticateAdmin, (req, res) => {
  getAccessLogStats((err, stats) => {
    if (err) return res.status(500).json({ error: 'DB error' });
    res.json(stats);
  });
});

// Get all known devices (admin only)
router.get('/known-devices', authenticateAdmin, (req, res) => {
  getAllKnownIPs((err, devices) => {
    if (err) return res.status(500).json({ error: 'DB error' });
    res.json(devices);
  });
});

// Whitelist a device (admin only)
router.post('/known-devices/:ip/whitelist', authenticateAdmin, (req, res) => {
  const ip = req.params.ip;
  whitelistIP(ip, (err) => {
    if (err) return res.status(500).json({ error: 'DB error' });
    res.json({ success: true, message: `IP ${ip} whitelisted` });
  });
});

// Remove device from whitelist (admin only)
router.delete('/known-devices/:ip/whitelist', authenticateAdmin, (req, res) => {
  const ip = req.params.ip;
  removeWhitelistIP(ip, (err) => {
    if (err) return res.status(500).json({ error: 'DB error' });
    res.json({ success: true, message: `IP ${ip} removed from whitelist` });
  });
});

// Forget a known device (admin only) - removes from known devices but preserves access logs
router.delete('/known-devices/:ip', authenticateAdmin, (req, res) => {
  const ip = req.params.ip;
  forgetKnownIP(ip, (err) => {
    if (err) return res.status(500).json({ error: 'DB error' });
    res.json({ success: true, message: `IP ${ip} forgotten from known devices` });
  });
});

// Get all blacklisted IPs (admin only)
router.get('/blacklist', authenticateAdmin, (req, res) => {
  getAllBlacklistedIPs((err, blacklistedIPs) => {
    if (err) return res.status(500).json({ error: 'DB error' });
    res.json(blacklistedIPs);
  });
});

// Add IP to blacklist (admin only)
router.post('/blacklist', authenticateAdmin, (req, res) => {
  const { ip_address, reason } = req.body;
  if (!ip_address) return res.status(400).json({ error: 'IP address is required' });
  
  addToBlacklist(ip_address, reason || 'No reason provided', req.user.username, (err) => {
    if (err) return res.status(500).json({ error: 'DB error' });
    res.json({ success: true, message: `IP ${ip_address} added to blacklist` });
  });
});

// Remove IP from blacklist (admin only)
router.delete('/blacklist/:ip', authenticateAdmin, (req, res) => {
  const ip = req.params.ip;
  removeFromBlacklist(ip, (err) => {
    if (err) return res.status(500).json({ error: 'DB error' });
    res.json({ success: true, message: `IP ${ip} removed from blacklist` });
  });
});

module.exports = router;
