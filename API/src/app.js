const express = require('express');
const cors = require('cors');
const bodyParser = require('body-parser');
const path = require('path');
const https = require('https');
const fs = require('fs');
const { initDB, addAccessLog, getKnownIP, addKnownIP, updateKnownIP, isIPBlacklisted, shouldSendBlockedNotification, updateBlockedNotificationRecord, checkDeviceConnectivity, shouldSendConnectivityNotification, updateConnectivityNotificationRecord, isAlarmEnabled } = require('./db');
const routes = require('./routes');
require('dotenv').config({ path: path.resolve(__dirname, '../.env') });

let fetch;
(async () => {
  fetch = (await import('node-fetch')).default;
})();

const app = express();
const PORT = process.env.PORT || 3000;

initDB();

app.use(cors());
app.use(bodyParser.json());
app.use((req, res, next) => {
  console.log('[DEBUG] After bodyParser, req.body:', req.body);
  next();
});

// Blacklist check middleware (MUST BE FIRST - before static files and everything else)
app.use((req, res, next) => {
  // Get IP address
  const ip = req.headers['x-forwarded-for'] || 
             req.headers['x-real-ip'] || 
             req.connection.remoteAddress || 
             req.socket.remoteAddress ||
             (req.connection.socket ? req.connection.socket.remoteAddress : null) ||
             req.ip;
  
  // Debug logging removed for production

  // Check if IP is blacklisted
  isIPBlacklisted(ip, (err, blacklistedIP) => {
    if (err) {
      console.error('[ERROR] Failed to check blacklist:', err);
      return next(); // Continue if there's an error
    }

    if (blacklistedIP) {
      console.log(`[SECURITY] Blocked blacklisted IP: ${ip} - Reason: ${blacklistedIP.reason}`);
      
      // Check if we should send a notification (prevent spam)
      shouldSendBlockedNotification(ip, (err, shouldSend) => {
        if (err) {
          console.error('[ERROR] Failed to check notification status:', err);
        }
        
        if (shouldSend) {
          // Send push notification about blocked access attempt
          const pushoverToken = process.env.PUSHOVER_APP_TOKEN;
          const pushoverUser = process.env.PUSHOVER_USER_KEY;
          
          if (pushoverToken && pushoverUser) {
            const message = `🚫 BLOCKED ACCESS ATTEMPT!\n\nIP: ${ip}\nPath: ${req.originalUrl}\nUser-Agent: ${req.headers['user-agent'] || 'Unknown'}\nReason: ${blacklistedIP.reason}\nTime: ${new Date().toLocaleString()}`;
            
            fetch('https://api.pushover.net/1/messages.json', {
              method: 'POST',
              headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
              body: new URLSearchParams({
                token: pushoverToken,
                user: pushoverUser,
                message,
                title: 'MotoDataLogger - Blocked Access',
                priority: 1,
                sound: 'siren'
              })
            }).catch(err => {
              console.error('[ERROR] Failed to send blocked access notification:', err);
            });
          }
          
          // Update notification record
          updateBlockedNotificationRecord(ip, (err) => {
            if (err) {
              console.error('[ERROR] Failed to update notification record:', err);
            }
          });
        }
      });

      // BLOCK ALL CONTENT - No exceptions
      const isAPIRequest = req.originalUrl.startsWith('/api/');
      const isAccessDeniedPage = req.originalUrl.startsWith('/access-denied.html');
      
      if (isAPIRequest) {
        // Return JSON for API requests
        return res.status(403).json({
          error: 'Access denied',
          message: 'Your IP address has been blacklisted',
          reason: blacklistedIP.reason
        });
      } else if (isAccessDeniedPage) {
        // For access denied page, serve it directly without redirect (prevents loop)
        return next();
      } else {
        // Redirect to access denied page for ALL other web requests
        const deniedUrl = `/access-denied.html?ip=${encodeURIComponent(ip)}&reason=${encodeURIComponent(blacklistedIP.reason)}`;
        return res.redirect(deniedUrl);
      }
    }

    // IP is not blacklisted, continue
    next();
  });
});

app.use(express.static(path.join(__dirname, '../public')));

// IP logging middleware
app.use((req, res, next) => {
  const startTime = Date.now();
  
  // Capture original send function
  const originalSend = res.send;
  
  // Override send function to capture response status
  res.send = function(data) {
    const endTime = Date.now();
    const processingTime = endTime - startTime;
    
    // Get IP address (handles proxy scenarios)
    const ip = req.headers['x-forwarded-for'] || 
               req.headers['x-real-ip'] || 
               req.connection.remoteAddress || 
               req.socket.remoteAddress ||
               (req.connection.socket ? req.connection.socket.remoteAddress : null) ||
               req.ip;
    
    // Prepare log data
    const logData = {
      ip_address: ip,
      path: req.originalUrl,
      method: req.method,
      user_agent: req.headers['user-agent'] || null,
      referer: req.headers['referer'] || null,
      timestamp: new Date().toISOString(),
      request_body: (req.method === 'POST' || req.method === 'PUT') ? JSON.stringify(req.body) : null,
      response_status: res.statusCode,
      processing_time_ms: processingTime
    };
    
    // Log to database (async, don't wait for completion)
    addAccessLog(logData, (err) => {
      if (err) {
        console.error('[ERROR] Failed to log access:', err);
      }
    });

    // Check for new/unknown device and send notification
    checkForNewDevice(logData.ip_address, logData.user_agent, logData.path);
    
    // Call original send function
    return originalSend.call(this, data);
  };
  
  next();
});

// Function to check for new devices and send notifications
function checkForNewDevice(ip_address, user_agent, path) {
  // Skip localhost and internal IPs
  if (ip_address === '::1' || ip_address === '127.0.0.1' || ip_address.startsWith('192.168.') || ip_address.startsWith('10.') || ip_address.startsWith('172.')) {
    return;
  }

  getKnownIP(ip_address, (err, knownIP) => {
    if (err) {
      console.error('[ERROR] Failed to check known IP:', err);
      return;
    }

    if (!knownIP) {
      // New device detected!
      console.log(`[SECURITY] New device detected: ${ip_address}`);
      
      // Add to known IPs
      addKnownIP(ip_address, user_agent, (err) => {
        if (err) {
          console.error('[ERROR] Failed to add known IP:', err);
        }
      });

      // Send push notification
      const pushoverToken = process.env.PUSHOVER_APP_TOKEN;
      const pushoverUser = process.env.PUSHOVER_USER_KEY;
      
      if (pushoverToken && pushoverUser) {
        const message = `🚨 NEW DEVICE DETECTED!\n\nIP: ${ip_address}\nPath: ${path}\nUser-Agent: ${user_agent || 'Unknown'}\nTime: ${new Date().toLocaleString()}`;
        
        fetch('https://api.pushover.net/1/messages.json', {
          method: 'POST',
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
          body: new URLSearchParams({
            token: pushoverToken,
            user: pushoverUser,
            message,
            title: 'MotoDataLogger - New Device Alert',
            priority: 1, // High priority
            sound: 'siren' // Use siren sound for security alerts
          })
        }).catch(err => {
          console.error('[ERROR] Failed to send new device notification:', err);
        });
      }
    } else {
      // Known device, update last seen
      updateKnownIP(ip_address, user_agent, (err) => {
        if (err) {
          console.error('[ERROR] Failed to update known IP:', err);
        }
      });
    }
  });
}

// Request logging middleware
app.use((req, res, next) => {
  const now = new Date().toISOString();
  const method = req.method;
  const url = req.originalUrl;
  let log = `[${now}] ${method} ${url}`;
  if (method === 'POST' || method === 'PUT') {
    log += ` | body: ${JSON.stringify(req.body)}`;
  }
  console.log(log);
  next();
});

// Error-handling middleware to log request body and return status code on error
app.use((err, req, res, next) => {
  const status = err.statusCode || err.status || 500;
  console.error('Error processing request:', err);
  if (req.body) {
    console.error('Request body:', req.body);
  }
  res.status(status).json({ error: 'Internal server error', code: status });
});

app.use('/api', routes);

app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, '../public/index.html'));
});

app.get('/access-denied.html', (req, res) => {
  res.sendFile(path.join(__dirname, '../public/access-denied.html'));
});

// Function to check device connectivity and send notifications
function checkDeviceConnectivityAndNotify() {
  checkDeviceConnectivity((err, connectivity) => {
    if (err) {
      console.error('[ERROR] Failed to check device connectivity:', err);
      return;
    }

    if (connectivity.shouldNotify) {
      // Check if we should send notification (prevent spam)
      shouldSendConnectivityNotification('device_disconnected', (err, shouldSend) => {
        if (err) {
          console.error('[ERROR] Failed to check notification status:', err);
          return;
        }

        if (shouldSend) {
          console.log(`[CONNECTIVITY] Device disconnected for ${connectivity.timeSinceLastSeen} minutes`);
          
          // Send push notification
          const pushoverToken = process.env.PUSHOVER_APP_TOKEN;
          const pushoverUser = process.env.PUSHOVER_USER_KEY;
          
          if (pushoverToken && pushoverUser) {
            const message = `⚠️ DEVICE DISCONNECTED!\n\nDevice has been offline for ${connectivity.timeSinceLastSeen} minutes\nLast seen: ${connectivity.lastSeen ? new Date(connectivity.lastSeen).toLocaleString() : 'Never'}\nTime: ${new Date().toLocaleString()}`;
            
            fetch('https://api.pushover.net/1/messages.json', {
              method: 'POST',
              headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
              body: new URLSearchParams({
                token: pushoverToken,
                user: pushoverUser,
                message,
                title: 'MotoDataLogger - Device Disconnected',
                priority: 1, // High priority
                sound: 'falling' // Use falling sound for connectivity alerts
              })
            }).then(() => {
              // Update notification record
              updateConnectivityNotificationRecord('device_disconnected', (err) => {
                if (err) {
                  console.error('[ERROR] Failed to update connectivity notification record:', err);
                }
              });
            }).catch(err => {
              console.error('[ERROR] Failed to send connectivity notification:', err);
            });
          }
        }
      });
    }
  });
}

// Start periodic connectivity monitoring (check every 2 minutes)
setInterval(checkDeviceConnectivityAndNotify, 2 * 60 * 1000);

// HTTPS configuration
const httpsOptions = {
  key: fs.readFileSync(path.join(__dirname, '../ssl/key.pem')),
  cert: fs.readFileSync(path.join(__dirname, '../ssl/cert.pem'))
};

https.createServer(httpsOptions, app).listen(PORT, () => {
  console.log(`HTTPS Server running on port ${PORT}`);
  console.log(`Access your app at: https://localhost:${PORT}`);
});
