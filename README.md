# 3D Printer Dashboard

A Dockerized full-stack dashboard for tracking 3D printing filament inventory and usage, with real-time printer telemetry via MQTT. Built with Angular and .NET.

---

## Overview

This project is a personal engineering effort focused on building a practical system for managing 3D printing resources and understanding printer activity over time.

The primary goal is to track filament inventory, usage, and print history in a structured and extensible way. Secondary goals include integrating real-time telemetry from MQTT-enabled printers to enrich data and enable future automation.

This repository is also intended to demonstrate consistent development practices, including version control discipline, CI/CD pipelines, and containerized deployment.

---

## Key Features (Planned)

### Filament Management

* Track filament spools (type, material, color, brand, weight)
* Monitor remaining filament per spool
* Log filament usage per print
* Support multiple spools and materials

### Usage Tracking

* Associate filament usage with print jobs
* Historical tracking of consumption
* Basic analytics (total usage, usage over time)

### Printer Telemetry (MQTT)

* Read-only integration with MQTT-enabled printers
* Capture print status and activity
* Correlate telemetry data with filament usage

### Dashboard

* Overview of filament inventory
* Active and recent print jobs
* Usage summaries and trends

---

## Tech Stack

* **Frontend:** Angular 17+
* **Backend:** .NET 8 Web API
* **Containerization:** Docker & Docker Compose
* **Messaging:** MQTT (read-only telemetry)
* **CI/CD:** GitHub Actions

---

## Project Structure

```
3d-printer-dashboard/
├── api/        # .NET backend (REST API, business logic)
├── ui/         # Angular frontend (dashboard UI)
├── data/       # Database files, migrations, seed data
├── docker/     # Dockerfiles and related config
├── config/     # Environment and app configuration
├── secrets/    # Local-only secrets (not committed)
├── docs/       # Project documentation and roadmap
├── .github/    # Workflows, templates, repo configuration
└── docker-compose.yml
```

---

## Getting Started

### Prerequisites

* Docker
* Docker Compose
* Node.js (for local UI development, optional)
* .NET SDK (for local API development, optional)

### Run with Docker

```
docker compose up --build
```

This will build and start all services defined in `docker-compose.yml`.

---

## Configuration

### Secrets

Sensitive files (such as MQTT certificates) are stored locally in the `secrets/` directory and are **not committed to source control**.

Example:

```
secrets/
└── blcert.pem
```

These are mounted into containers at runtime.

---

## Development Workflow

* All changes are made through feature branches
* Pull Requests are used for all merges into `main`
* GitHub Actions run builds and validation on every PR
* `main` is kept in a deployable state

Example branch names:

* `feat/filament-model`
* `feat/mqtt-integration`
* `fix/api-validation`
* `chore/add-ci-workflows`

---

## CI/CD

GitHub Actions are used to automate:

* API build and test
* UI build and lint
* Docker image builds

All checks must pass before merging into `main`.

---

## Roadmap

### Phase 1

* Project setup (repo, CI, Docker)
* Basic API and UI scaffolding

### Phase 2

* Filament data model
* CRUD operations for spools
* Basic UI for inventory management

### Phase 3

* Usage tracking per print
* Historical data and summaries

### Phase 4

* MQTT integration (read-only telemetry)
* Printer status ingestion

### Phase 5

* Dashboard views and analytics
* Data visualization

---

## Design Principles

* Keep the system modular and easy to extend
* Prefer containerized services over local installs
* Separate concerns between API, UI, and data layers
* Treat telemetry as supplemental, not authoritative
* Maintain clean, readable commit history

---

## Future Enhancements

* Multi-printer support
* Filament cost tracking
* Predictive usage and restocking alerts
* Advanced analytics and reporting
* Optional printer control features (if supported)

---

## License

MIT License

---

## Author

Darren Rector

---

## Notes

This project is actively developed as a portfolio piece to demonstrate full-stack development, DevOps practices, and system design in a real-world scenario.
