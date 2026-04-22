# 3D Printer Dashboard – Project Milestones

---

## Milestone 0 — Foundation (DONE)

**Goal:** Working backend with clean structure

### Completed
- .NET API running
- Swagger working
- Git repo cleaned (no secrets, no bin/obj)
- Config system working (user-secrets)
- Endpoints:
  - /api/ping
  - /api/printer/config-status
  - /api/printer/target
- Service layer introduced
- Progress log started

**Status:** Complete  
**Next:** MQTT connectivity

---

## Milestone 1 — MQTT Connection

**Goal:** Establish connection to Bambu printer over MQTT

### Tasks

- Install MQTTnet
- Create BambuMqttService
- Implement TryConnectAsync()
- Add endpoint:
  - /api/printer/test-mqtt
- Configure:
  - TLS
  - credentials (bblp + access code)

### Completed

- Configuration standardized to `BambuPrinter` section
- Printer LAN details confirmed:
  - IP: `192.168.4.29`
  - Access code retrieved
- User-secrets configured and verified:
  - `BambuPrinter:Host`
  - `BambuPrinter:AccessCode`
- Configuration binding implemented via `BambuPrinterOptions`
- Options registered in dependency injection
- Verified configuration binding at runtime
- Removed legacy/misaligned config (`Bambu:*`)
- Repository workflow established:
  - feature branch created (`feature/milestone.1-mqtt-connection`)
  - changes committed and merged via PR

### Status

🟡 **In Progress** — configuration complete, MQTT not yet implemented

### Next up

- Install MQTTnet
- Create `BambuMqttService`
- Implement `TryConnectAsync()`
- Add `/api/printer/test-mqtt` endpoint

### Success Criteria

- Endpoint returns:
  - `success = true` on successful connection  
  OR  
  - a meaningful error message on failure
p
---

## Milestone 2 — Subscribe to Printer Data

**Goal:** Receive real data from printer

### Tasks
- Subscribe to MQTT topics
- Identify topics for:
  - status
  - telemetry
- Log incoming messages
- Deserialize JSON payloads

### Success Criteria
- Real printer data visible

---

## Milestone 3 — State Management Layer

**Goal:** Convert raw MQTT data into usable application state

### Tasks
- Create models:
  - PrinterState
  - JobState
- Map MQTT payloads to objects
- Maintain in-memory state

### Endpoint
- /api/printer/state

### Success Criteria
- API returns structured printer state

---

## Milestone 4 — Command Execution

**Goal:** Send commands to printer

### Tasks
- Publish MQTT messages
- Implement commands:
  - start print
  - pause
  - stop
- Create API endpoints

### Success Criteria
- Printer responds to commands

---

## Milestone 5 — Frontend (Angular)

**Goal:** Visual dashboard

### Tasks
- Create Angular app
- Display:
  - printer status
  - job progress
- Connect to API

### Success Criteria
- Live dashboard updates

---

## Milestone 6 — Persistence

**Goal:** Store historical data

### Tasks
- Add database (SQL Server / SQLite)
- Store:
  - job history
  - printer metrics

### Success Criteria
- Historical queries available

---

## Milestone 7 — Notifications

**Goal:** Alerts for events

### Tasks
- Detect:
  - print complete
  - errors
- Send notifications (email/webhook)

### Success Criteria
- Alerts triggered correctly

---

## Milestone 8 — Production Readiness

**Goal:** Deployable system

### Tasks
- Dockerize API
- Secure secrets
- Configure logging

### Success Criteria
- System runs reliably outside dev environment
