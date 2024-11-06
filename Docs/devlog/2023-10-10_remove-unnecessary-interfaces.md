---
title: Remove unnecessary interfaces
---

# Remove unnecessary interfaces

Some services will never have another implementation, so I removed the interfaces and the `Default*` prefix.
If at some point, we actually have integration tests, we should likely run them against a real PostgreSQL anyway (and not use mocking.)
