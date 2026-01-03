# SimulideService (Backend)

## Overview

SimulideService is the authoritative backend for a real-time
collaborative text editor built on **Operational Transformation (OT)**.

It provides:

- Durable document storage
- Deterministic operation sequencing
- Real-time synchronization via SignalR
- Clear correctness guarantees under concurrency

The backend is the **single source of truth** for all document state.

---

## Architectural Goals

1. **Strong Correctness Guarantees**

   - All clients converge on the same document state.

2. **Clear Write Authority**

   - Only the backend mutates authoritative document content.

3. **Scalable Real-Time Collaboration**

   - Designed to scale beyond a single instance.

4. **Explicit Tradeoffs**
   - Correctness is prioritized over throughput under conflict.

---

## High-Level Architecture

- **HTTP APIs**

  - Document creation
  - Document retrieval
  - Metadata access

- **SignalR Hub**

  - Real-time operation submission
  - Operation broadcast
  - Session coordination

- **CQRS**
  - EF Core for write consistency
  - Dapper for efficient reads

---

## Document Authority Model

The backend owns:

- Document content
- Document version
- Operation ordering
- Validation rules

Clients submit _intent_.
The backend decides _truth_.

---

## Operation Processing Flow

1. Receive operation from client.
2. Validate:
   - Document exists
   - Version matches or can be transformed
   - Operation bounds are valid
3. Transform operation as required.
4. Apply operation to authoritative document.
5. Increment document version.
6. Persist changes transactionally.
7. Broadcast resulting operation to session participants.

This flow guarantees convergence across all clients.

---

## Versioning & Concurrency

- Document versions are monotonically increasing.
- Every operation references the version it was based on.
- Conflicts are resolved server-side via OT.

If an operation cannot be safely applied:

- It is rejected explicitly.
- The client must recover.

Silent failure is not acceptable.

---

## CQRS Rationale

CQRS is used intentionally:

- **Write Side**

  - Requires strict consistency
  - Handles OT transforms and versioning
  - Uses EF Core transactions

- **Read Side**
  - Optimized for performance
  - Uses Dapper
  - Supports high-frequency document reads

This separation prevents read optimization from compromising write correctness.

---

## Real-Time Collaboration (SignalR)

SignalR provides:

- Persistent connections
- Session-based grouping (per document)
- Low-latency bidirectional messaging

Current architecture assumes:

- One logical collaboration hub
- Backend remains authoritative even under fanout

---

## Scaling Strategy (Planned)

As the system scales horizontally:

- Multiple backend instances will exist.
- Clients may connect to different instances.

Redis will be introduced to support:

- Cross-instance operation fanout
- Session membership tracking
- Presence and cursor state
- Connection coordination

The primary database will remain reserved for **durable state only**.

---

## Persistence Boundaries

**Database (Durable):**

- Document content
- Document version
- Optional operation history

**Redis (Ephemeral, Planned):**

- Active sessions
- Connected users
- Presence metadata
- Pub/sub coordination

This separation prevents unnecessary database churn.

---

## Testing Strategy

- **Unit Tests**

  - OT transforms
  - Validation logic
  - Version handling

- **Functional Tests**

  - HTTP APIs
  - SignalR workflows
  - Persistence integration

- **Performance Tests**
  - Concurrent collaboration
  - Mixed REST + SignalR load
  - Throughput and latency profiling

Correctness is validated before performance is optimized.

---

## Operational Expectations

- Containerized local development
- Deterministic startup via migrations / init scripts
- Clear separation of environments
- Structured logging for operation flows

---

## Non-Goals

- Peer-to-peer collaboration
- Offline conflict resolution
- Rich text semantics (plain text only)

---

## Summary

SimulideService is a **correctness-first collaboration backend**.

Its design deliberately:

- Centralizes authority
- Makes conflicts explicit
- Scales coordination without sacrificing determinism

This allows the system to grow in users, features, and infrastructure
without compromising convergence guarantees.
