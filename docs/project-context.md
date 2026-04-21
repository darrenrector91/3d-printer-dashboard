# 3D Printer Dashboard — Project Context

## Purpose
Track and control a Bambu Lab 3D printer from a custom dashboard.

---

## Architecture

### Backend
- .NET 8 Web API
- Runs locally (dev) and later on server

### Frontend (planned)
- Angular

### Printer Communication
- MQTT over TLS
- Username: `bblp`
- Auth: Access Code
- Broker runs on printer (LAN)

### Hardware
- Bambu Lab printer (MQTT capable)
- Raspberry Pi (future)
  - Will act as always-on bridge / local service host

---

## Current Capabilities
- API running
- Swagger working
- Config system (appsettings + user-secrets)
- Health endpoints:
  - `/api/ping`
  - `/api/printer/config-status`
  - `/api/printer/target`

---

## Not Yet Implemented
- MQTT connection
- Real-time printer data
- Commands (start/pause/etc)
- Frontend UI

---

## Development Rules
- No secrets in repo
- `bin/` and `obj/` ignored
- Strong typing over dynamic JSON
- Keep services separated (no logic in controllers)

---

## Current Milestone
Milestone 1 — MQTT Connection
