const jwt = require('jsonwebtoken');
const bcrypt = require('bcryptjs');
require('dotenv').config();
const { getUserByUsername } = require('./db');

const ADMIN_USERNAME = process.env.ADMIN_USERNAME;
const ADMIN_PASSWORD = process.env.ADMIN_PASSWORD;
const JWT_SECRET = process.env.JWT_SECRET;

function generateToken(payload) {
  return jwt.sign(payload, JWT_SECRET, { expiresIn: '12h' });
}

function verifyToken(token) {
  try {
    return jwt.verify(token, JWT_SECRET);
  } catch (err) {
    return null;
  }
}

function adminLogin(username, password) {
  return username === ADMIN_USERNAME && password === ADMIN_PASSWORD;
}

function authenticateAdmin(req, res, next) {
  const token = req.headers['authorization']?.split(' ')[1];
  if (!token) return res.status(401).json({ error: 'No token provided' });
  const decoded = verifyToken(token);
  if (!decoded || decoded.role !== 'admin') return res.status(401).json({ error: 'Invalid token' });
  req.user = decoded;
  next();
}

function authenticateTracker(req, res, next) {
  console.log('[DEBUG] authenticateTracker called with:', req.body);
  const { Username, Password, Token } = req.body;
  getUserByUsername(Username, (err, user) => {
    if (err || !user) return res.status(401).json({ error: 'Invalid credentials' });
    if (user.password !== Password || user.token !== Token) return res.status(401).json({ error: 'Invalid credentials' });
    req.trackerUser = user;
    next();
  });
}

module.exports = {
  generateToken,
  verifyToken,
  adminLogin,
  authenticateAdmin,
  authenticateTracker
};
