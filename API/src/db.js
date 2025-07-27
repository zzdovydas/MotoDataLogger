const sqlite3 = require('sqlite3').verbose();
const path = require('path');
const dbPath = path.resolve(__dirname, '../database.sqlite');

const db = new sqlite3.Database(dbPath);

function initDB() {
  db.serialize(() => {
    db.run(`CREATE TABLE IF NOT EXISTS users (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      username TEXT UNIQUE NOT NULL,
      password TEXT NOT NULL,
      token TEXT NOT NULL
    )`);
    db.run(`CREATE TABLE IF NOT EXISTS tracker_data (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      user_id INTEGER,
      location_latitude TEXT,
      location_longitude TEXT,
      location_altitude TEXT,
      angle_azimuth TEXT,
      angle_pitch TEXT,
      angle_roll TEXT,
      battery_level TEXT,
      battery_charging_time_left TEXT,
      timestamp TEXT,
      raw_json TEXT,
      FOREIGN KEY(user_id) REFERENCES users(id)
    )`);
    db.run(`CREATE TABLE IF NOT EXISTS access_logs (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      ip_address TEXT NOT NULL,
      path TEXT NOT NULL,
      method TEXT NOT NULL,
      user_agent TEXT,
      referer TEXT,
      timestamp TEXT NOT NULL,
      request_body TEXT,
      response_status INTEGER,
      processing_time_ms INTEGER
    )`);
    db.run(`CREATE TABLE IF NOT EXISTS known_ips (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      ip_address TEXT UNIQUE NOT NULL,
      first_seen TEXT NOT NULL,
      last_seen TEXT NOT NULL,
      request_count INTEGER DEFAULT 1,
      user_agent TEXT,
      is_whitelisted INTEGER DEFAULT 0
    )`);
    db.run(`CREATE TABLE IF NOT EXISTS blacklisted_ips (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      ip_address TEXT UNIQUE NOT NULL,
      reason TEXT,
      blacklisted_at TEXT NOT NULL,
      blacklisted_by TEXT NOT NULL
    )`);
    db.run(`CREATE TABLE IF NOT EXISTS blocked_access_notifications (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      ip_address TEXT UNIQUE NOT NULL,
      last_notification TEXT NOT NULL,
      notification_count INTEGER DEFAULT 1
    )`, (err) => {
      if (err) {
        console.error('[ERROR] Failed to create blocked_access_notifications table:', err);
      } else {
        console.log('[INFO] blocked_access_notifications table ready');
      }
    });
    
    db.run(`CREATE TABLE IF NOT EXISTS connectivity_notifications (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      notification_type TEXT UNIQUE NOT NULL,
      last_notification TEXT NOT NULL,
      notification_count INTEGER DEFAULT 1
    )`, (err) => {
      if (err) {
        console.error('[ERROR] Failed to create connectivity_notifications table:', err);
      } else {
        console.log('[INFO] connectivity_notifications table ready');
      }
    });
    
    db.run(`CREATE TABLE IF NOT EXISTS alarm_settings (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      setting_name TEXT UNIQUE NOT NULL,
      setting_value TEXT NOT NULL,
      updated_at TEXT NOT NULL
    )`, (err) => {
      if (err) {
        console.error('[ERROR] Failed to create alarm_settings table:', err);
      } else {
        console.log('[INFO] alarm_settings table ready');
      }
    });
    
    // Initialize alarm settings with default values
    db.run(`INSERT OR IGNORE INTO alarm_settings (setting_name, setting_value, updated_at) VALUES ('alarm_enabled', 'true', ?)`, 
      [new Date().toISOString()], (err) => {
        if (err) {
          console.error('[ERROR] Failed to initialize alarm settings:', err);
        } else {
          console.log('[INFO] Alarm settings initialized');
        }
      });
  });
}

function addUser(username, password, token, cb) {
  db.run('INSERT INTO users (username, password, token) VALUES (?, ?, ?)', [username, password, token], cb);
}

function deleteUser(username, cb) {
  db.run('DELETE FROM users WHERE username = ?', [username], cb);
}

function getUserByUsername(username, cb) {
  db.get('SELECT * FROM users WHERE username = ?', [username], cb);
}

function getAllUsers(cb) {
  db.all('SELECT * FROM users', cb);
}

function addTrackerData(user_id, data, cb) {
  db.run(`INSERT INTO tracker_data (user_id, location_latitude, location_longitude, location_altitude, angle_azimuth, angle_pitch, angle_roll, battery_level, battery_charging_time_left, timestamp, raw_json) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)`,
    [user_id, data.Location.Latitude, data.Location.Longitude, data.Location.Altitude, data.Angle_Data.Azimuth, data.Angle_Data.Pitch, data.Angle_Data.Roll, data.Battery_Level, data.Battery_Charging_Time_Left, data.Timestamp, JSON.stringify(data)], cb);
}

function getLastTrackerData(user_id, cb) {
  db.get('SELECT * FROM tracker_data WHERE user_id = ? ORDER BY id DESC LIMIT 1', [user_id], cb);
}

function getAllTrackerData(user_id, cb) {
  db.all('SELECT * FROM tracker_data WHERE user_id = ? ORDER BY timestamp DESC', [user_id], cb);
}

function addAccessLog(logData, cb) {
  const { ip_address, path, method, user_agent, referer, timestamp, request_body, response_status, processing_time_ms } = logData;
  db.run(`INSERT INTO access_logs (ip_address, path, method, user_agent, referer, timestamp, request_body, response_status, processing_time_ms) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)`,
    [ip_address, path, method, user_agent, referer, timestamp, request_body, response_status, processing_time_ms], cb);
}

function getAccessLogs(limit = 100, offset = 0, cb) {
  db.all('SELECT * FROM access_logs ORDER BY timestamp DESC LIMIT ? OFFSET ?', [limit, offset], cb);
}

function getAccessLogsByIP(ip_address, limit = 100, offset = 0, cb) {
  db.all('SELECT * FROM access_logs WHERE ip_address = ? ORDER BY timestamp DESC LIMIT ? OFFSET ?', [ip_address, limit, offset], cb);
}

function getAccessLogsByPath(path, limit = 100, offset = 0, cb) {
  db.all('SELECT * FROM access_logs WHERE path LIKE ? ORDER BY timestamp DESC LIMIT ? OFFSET ?', [`%${path}%`, limit, offset], cb);
}

function getAccessLogsByMethod(method, limit = 100, offset = 0, cb) {
  db.all('SELECT * FROM access_logs WHERE method = ? ORDER BY timestamp DESC LIMIT ? OFFSET ?', [method, limit, offset], cb);
}

function getAccessLogsWithFilters(filters, limit = 100, offset = 0, cb) {
  let conditions = [];
  let params = [];
  
  if (filters.ip) {
    conditions.push('ip_address = ?');
    params.push(filters.ip);
  }
  
  if (filters.path) {
    conditions.push('path LIKE ?');
    params.push(`%${filters.path}%`);
  }
  
  if (filters.method) {
    conditions.push('method = ?');
    params.push(filters.method);
  }
  
  const whereClause = conditions.length > 0 ? 'WHERE ' + conditions.join(' AND ') : '';
  const query = `SELECT * FROM access_logs ${whereClause} ORDER BY timestamp DESC LIMIT ? OFFSET ?`;
  
  params.push(limit, offset);
  db.all(query, params, cb);
}

function getAccessLogStats(cb) {
  db.all(`
    SELECT 
      ip_address,
      COUNT(*) as request_count,
      MIN(timestamp) as first_seen,
      MAX(timestamp) as last_seen,
      AVG(processing_time_ms) as avg_processing_time
    FROM access_logs 
    GROUP BY ip_address 
    ORDER BY request_count DESC
  `, cb);
}

function getKnownIP(ip_address, cb) {
  db.get('SELECT * FROM known_ips WHERE ip_address = ?', [ip_address], cb);
}

function addKnownIP(ip_address, user_agent, cb) {
  const now = new Date().toISOString();
  db.run('INSERT INTO known_ips (ip_address, first_seen, last_seen, user_agent) VALUES (?, ?, ?, ?)', 
    [ip_address, now, now, user_agent], cb);
}

function updateKnownIP(ip_address, user_agent, cb) {
  const now = new Date().toISOString();
  db.run('UPDATE known_ips SET last_seen = ?, request_count = request_count + 1, user_agent = ? WHERE ip_address = ?', 
    [now, user_agent, ip_address], cb);
}

function getAllKnownIPs(cb) {
  db.all(`
    SELECT 
      ki.*,
      CASE WHEN bi.ip_address IS NOT NULL THEN 1 ELSE 0 END as is_blacklisted,
      bi.reason as blacklist_reason,
      bi.blacklisted_at,
      bi.blacklisted_by
    FROM known_ips ki
    LEFT JOIN blacklisted_ips bi ON ki.ip_address = bi.ip_address
    ORDER BY ki.last_seen DESC
  `, cb);
}

function whitelistIP(ip_address, cb) {
  db.run('UPDATE known_ips SET is_whitelisted = 1 WHERE ip_address = ?', [ip_address], cb);
}

function removeWhitelistIP(ip_address, cb) {
  db.run('UPDATE known_ips SET is_whitelisted = 0 WHERE ip_address = ?', [ip_address], cb);
}

function forgetKnownIP(ip_address, cb) {
  db.run('DELETE FROM known_ips WHERE ip_address = ?', [ip_address], cb);
}

function isIPBlacklisted(ip_address, cb) {
  db.get('SELECT * FROM blacklisted_ips WHERE ip_address = ?', [ip_address], cb);
}

function addToBlacklist(ip_address, reason, blacklisted_by, cb) {
  const now = new Date().toISOString();
  db.run('INSERT INTO blacklisted_ips (ip_address, reason, blacklisted_at, blacklisted_by) VALUES (?, ?, ?, ?)', 
    [ip_address, reason, now, blacklisted_by], cb);
}

function removeFromBlacklist(ip_address, cb) {
  db.run('DELETE FROM blacklisted_ips WHERE ip_address = ?', [ip_address], cb);
}

function getAllBlacklistedIPs(cb) {
  db.all('SELECT * FROM blacklisted_ips ORDER BY blacklisted_at DESC', cb);
}

function getBlockedNotificationRecord(ip_address, cb) {
  db.get('SELECT * FROM blocked_access_notifications WHERE ip_address = ?', [ip_address], cb);
}

function updateBlockedNotificationRecord(ip_address, cb) {
  const now = new Date().toISOString();
  db.run(`
    INSERT INTO blocked_access_notifications (ip_address, last_notification, notification_count) 
    VALUES (?, ?, 1)
    ON CONFLICT(ip_address) DO UPDATE SET 
      last_notification = ?, 
      notification_count = notification_count + 1
  `, [ip_address, now, now], (err) => {
    if (err) {
      console.error('[ERROR] Failed to update notification record:', err);
    }
    if (cb) cb(err);
  });
}

function shouldSendBlockedNotification(ip_address, cb) {
  getBlockedNotificationRecord(ip_address, (err, record) => {
    if (err) {
      console.error('[ERROR] Failed to get notification record:', err);
      cb(err, true); // Send notification if error
      return;
    }
    
    if (!record) {
      cb(null, true); // First time, send notification
      return;
    }
    
    // Check if enough time has passed (e.g., 1 hour)
    const lastNotification = new Date(record.last_notification);
    const now = new Date();
    const hoursDiff = (now - lastNotification) / (1000 * 60 * 60);
    
    // Send notification if more than 1 hour has passed
    const shouldSend = hoursDiff >= 1;
    cb(null, shouldSend);
  });
}

function checkDeviceConnectivity(cb) {
  getAllUsers((err, users) => {
    if (err || !users || users.length === 0) {
      return cb(err || new Error('No users found'));
    }
    
    const user = users[0]; // Get first user
    getLastTrackerData(user.id, (err, data) => {
      if (err) {
        return cb(err);
      }
      
      if (!data) {
        return cb(null, {
          isConnected: false,
          lastSeen: null,
          timeSinceLastSeen: null,
          shouldNotify: true
        });
      }
      
      const lastSeen = new Date(data.timestamp);
      const now = new Date();
      const timeSinceLastSeen = (now - lastSeen) / (1000 * 60); // minutes
      const isConnected = timeSinceLastSeen <= 5; // Connected if last seen within 5 minutes
      
      cb(null, {
        isConnected,
        lastSeen: data.timestamp,
        timeSinceLastSeen: Math.round(timeSinceLastSeen),
        shouldNotify: !isConnected && timeSinceLastSeen > 5
      });
    });
  });
}

function getConnectivityNotificationRecord(notificationType, cb) {
  db.get('SELECT * FROM connectivity_notifications WHERE notification_type = ?', [notificationType], cb);
}

function updateConnectivityNotificationRecord(notificationType, cb) {
  const now = new Date().toISOString();
  db.run(`
    INSERT INTO connectivity_notifications (notification_type, last_notification, notification_count) 
    VALUES (?, ?, 1)
    ON CONFLICT(notification_type) DO UPDATE SET 
      last_notification = ?, 
      notification_count = notification_count + 1
  `, [notificationType, now, now], (err) => {
    if (err) {
      console.error('[ERROR] Failed to update connectivity notification record:', err);
    }
    if (cb) cb(err);
  });
}

function shouldSendConnectivityNotification(notificationType, cb) {
  getConnectivityNotificationRecord(notificationType, (err, record) => {
    if (err) {
      console.error('[ERROR] Failed to get connectivity notification record:', err);
      cb(err, true); // Send notification if error
      return;
    }
    
    if (!record) {
      cb(null, true); // First time, send notification
      return;
    }
    
    // Check if enough time has passed (e.g., 30 minutes)
    const lastNotification = new Date(record.last_notification);
    const now = new Date();
    const minutesDiff = (now - lastNotification) / (1000 * 60);
    
    // Send notification if more than 30 minutes have passed
    const shouldSend = minutesDiff >= 30;
    cb(null, shouldSend);
  });
}

function getAlarmSetting(settingName, cb) {
  db.get('SELECT setting_value FROM alarm_settings WHERE setting_name = ?', [settingName], (err, row) => {
    if (err) {
      return cb(err);
    }
    cb(null, row ? row.setting_value : null);
  });
}

function setAlarmSetting(settingName, settingValue, cb) {
  const now = new Date().toISOString();
  db.run(`
    INSERT INTO alarm_settings (setting_name, setting_value, updated_at) 
    VALUES (?, ?, ?)
    ON CONFLICT(setting_name) DO UPDATE SET 
      setting_value = ?, 
      updated_at = ?
  `, [settingName, settingValue, now, settingValue, now], cb);
}

function isAlarmEnabled(cb) {
  getAlarmSetting('alarm_enabled', (err, value) => {
    if (err) {
      return cb(err);
    }
    cb(null, value === 'true');
  });
}

module.exports = {
  db,
  initDB,
  addUser,
  deleteUser,
  getUserByUsername,
  getAllUsers,
  addTrackerData,
  getLastTrackerData,
  getAllTrackerData,
  addAccessLog,
  getAccessLogs,
  getAccessLogsByIP,
  getAccessLogsByPath,
  getAccessLogsByMethod,
  getAccessLogsWithFilters,
  getAccessLogStats,
  getKnownIP,
  addKnownIP,
  updateKnownIP,
  getAllKnownIPs,
  whitelistIP,
  removeWhitelistIP,
  forgetKnownIP,
  isIPBlacklisted,
  addToBlacklist,
  removeFromBlacklist,
  getAllBlacklistedIPs,
  shouldSendBlockedNotification,
  updateBlockedNotificationRecord,
  getBlockedNotificationRecord,
  checkDeviceConnectivity,
  getConnectivityNotificationRecord,
  updateConnectivityNotificationRecord,
  shouldSendConnectivityNotification,
  getAlarmSetting,
  setAlarmSetting,
  isAlarmEnabled
};
