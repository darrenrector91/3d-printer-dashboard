# 3D Printer Dashboard – Progress Log

## 2026-04-21

### Completed
- Set up .NET API
- Swagger running
- Cleaned repo (removed bin/obj + secrets)
- Config system working (host + access code)
- Created endpoints:
  - /api/ping
  - /api/printer/config-status
  - /api/printer/target
- Standardized configuration to `BambuPrinter` section
- Implemented secure user-secrets handling
- Installed MQTTnet package
- Created MQTT service layer:
  - `IBambuMqttService`
  - `BambuMqttService`
  - `MqttConnectionTestResult`
- Implemented MQTT connection probe:
  - TLS enabled
  - credentials configured (`bblp` + access code)
  - connect/disconnect validation
  - sanitized error handling
- Added endpoint:
  - `/api/printer/test-mqtt`
- Enabled controller support in API
- Verified MQTT connectivity via API endpoint

### Notes
- MQTT connectivity to printer is working over LAN
- TLS certificate validation is currently permissive (acceptable for dev)
- Service structure is in place for expansion (subscriptions, state, commands)
- API now mixes minimal endpoints and controller-based endpoints intentionally

### Next Steps
- Establish persistent MQTT connection (long-lived client)
- Subscribe to printer topics
- Log incoming MQTT messages
- Begin identifying payload structure for status and telemetry