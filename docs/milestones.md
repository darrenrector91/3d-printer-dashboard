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
- MQTTnet package installed
- Service contract created:
  - `IBambuMqttService`
  - `MqttConnectionTestResult`
- Service implementation completed:
  - `BambuMqttService`
- Service registered in DI container
- MQTT connection probe implemented:
  - validates required configuration
  - creates MQTT client
  - applies TLS configuration
  - uses printer credentials
  - attempts connect/disconnect
  - returns sanitized error messages
- Controller endpoint added:
  - `/api/printer/test-mqtt`
- Controller support enabled in `Program.cs`
- Existing printer target endpoint updated to use `BambuPrinter:Host`
- Endpoint tested and verified:
  - returns success or meaningful failure response

### Status

🟢 **Complete** — MQTT connectivity verified via API endpoint

### Next

- Milestone 2 — Subscribe to Printer Data

### Success Criteria

- Endpoint returns:
  - `success = true` on successful connection  
  OR  
  - a meaningful error message on failure

---

## Milestone 2 — Subscribe to Printer Data

**Goal:** Receive real data from printer

### Branch

feature/milestone.2-subscribe-printer-data

### Commit flow

1. `milestone.2 add persistent MQTT client service contract`
2. `milestone.2 add printer MQTT hosted service skeleton`
3. `milestone.2 register printer MQTT hosted service`
4. `milestone.2 implement persistent MQTT connection startup`
5. `milestone.2 add MQTT reconnect handling`
6. `milestone.2 add printer topic subscriptions`
7. `milestone.2 add raw MQTT message logging`
8. `milestone.2 add endpoint for MQTT connection status`
9. `milestone.2 capture sample printer payloads`
10. `milestone.2 document printer topic findings`

### What each commit should contain

#### 1. `milestone.2 add persistent MQTT client service contract`

Add the long-lived service interface.

Suggested file:
- `Services/Interfaces/IPrinterMqttClientService.cs`

Suggested shape:
- `StartAsync()`
- `StopAsync()`
- `IsConnected`

This separates the persistent connection work from the Milestone 1 probe service.

---

#### 2. `milestone.2 add printer MQTT hosted service skeleton`

Add a background service class, but no real MQTT logic yet.

Suggested file:
- `Services/PrinterMqttHostedService.cs`

Include:
- constructor injection
- logger
- config/options
- empty `ExecuteAsync()`

Keep it buildable and boring.

---

#### 3. `milestone.2 register printer MQTT hosted service`

Wire the hosted service into DI.

In `Program.cs`:
- register the service
- register it as a hosted/background service

Still no connection logic yet.

---

#### 4. `milestone.2 implement persistent MQTT connection startup`

Now add the real startup connection logic.

Include:
- create MQTT client
- apply TLS
- apply credentials
- connect on app start
- log success/failure

This is where the service becomes real.

---

#### 5. `milestone.2 add MQTT reconnect handling`

Handle disconnects and reconnect attempts.

Include:
- disconnected event handler
- delayed retry
- reconnect logging

Keep reconnect simple at first. No fancy backoff yet unless needed.

---

#### 6. `milestone.2 add printer topic subscriptions`

Subscribe once connected.

This commit should only focus on:
- known topic subscription list
- subscribe after successful connect/reconnect

Do not parse payloads yet.

---

#### 7. `milestone.2 add raw MQTT message logging`

Handle received messages and log raw payloads.

Include:
- message received handler
- topic name logging
- raw payload logging

This gives you truth from the printer before you invent models too early.

---

#### 8. `milestone.2 add endpoint for MQTT connection status`

Add a small endpoint to answer:
- connected?
- last connect attempt?
- last error?

Example:
- `/api/printer/mqtt-status`

Keep it operational, not fancy.

---

#### 9. `milestone.2 capture sample printer payloads`

Once messages are flowing, save representative payload samples somewhere in the repo.

Good options:
- `docs/mqtt-samples/`
- `docs/printer-payloads.md`

This makes Milestone 3 much easier.

---

#### 10. `milestone.2 document printer topic findings`

Write down:
- topics seen
- what looks like status
- what looks like telemetry
- any command-related topics discovered

This closes out the milestone cleanly.

### Recommended rule for Milestone 2

Do not jump into deserialization early.

First prove:
- persistent connection works
- reconnect works
- subscriptions work
- messages are arriving

Then move to modeling.

### Immediate next commit

Start with:

`milestone.2 add persistent MQTT client service contract`

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
