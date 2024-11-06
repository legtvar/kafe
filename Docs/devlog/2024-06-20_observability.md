---
title: Observability
---

# Observability

This year's film festival has shown me how difficult it is for me to replicate bugs users report to me.
I can't realistically expect KAFE's users to write me detailed reports with screenshots and replication steps (as amazing as that would be).
I'm glad any reports reach me at all.
I therefore decided to invest some time in learning about _instrumentation_, _observability_, and other fancy words one hears about applications running in production.

The end result is six more Docker containers running on mlejnek:

**`blackbox` -- Prometheus's [Blackbox exporter](https://github.com/prometheus/blackbox_exporter)**

It checks that https://kafe.muni.cz and https://games.muni.cz are up and running, as well as their latency, and exports these metrics into Prometheus.

**`prometheus` -- [Metrics monitor](https://prometheus.io/)**

It stores, filters, and displays metrics (variables over time) you throw at it.

**`otel` -- [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/) ([Contrib](https://github.com/open-telemetry/opentelemetry-collector-contrib))**

OpenTelemetry is a collection of APIs, SDKs, formats, etc. for getting, collecting, and exporting telemetry data.
From `Kafe.Api` I export traces and metrics using the `OpenTelemetry` NuGet packages, and logs through Serilog.
All of these reach the `otel` container, from which they are sent further to `prometheus`, `loki`, and `tempo`.

**`loki` -- [Grafana's log aggregator](https://grafana.com/oss/loki/)**

Gets and stores _logs_ from `otel` and makes them available in Grafana.

**`tempo` -- [Grafana's trace aggregator](https://grafana.com/oss/tempo/)**

Gets and stores _traces_ from `otel` and makes them available in Grafana.

**`grafana` -- [Observability front-end and visualizer](https://grafana.com/oss/grafana/)**

Reaches all of the data collected by `blackbox` and `otel`, and stored in `prometheus`, `loki`, and `tempo`.
Allows us to filter it and visualize it.

---

These tools are all free and open-source.
Together they allow us to more easily see errors and exception and hopefully will make hunting for reported bugs easier.
