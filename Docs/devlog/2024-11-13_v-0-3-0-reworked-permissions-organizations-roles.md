# v0.3.0: Reworked Permissions, Organizations, and Roles

> Originally released on 2024-11-13, but this text has been written on 2025-12-19.

## Changelog

- Permissions have been reworked again. No longer are they computed on the go inside a recursive SQL function. They are now materialized using an event projection, which makes read checks a LOT faster.
- Moved from LEMMA's VM onto our own. Also from ÚVT infra to Stratus@FI, which is ran by CVT FI.
- Added a proper staging environment at https://kafe-stage.fi.muni.cz
- Added support for logging in using MU Unified Login.
- Added a static docs website built with [Lume](https://lume.land/) and added this devlog to it.
- Added a system of _corrections_ to fix mistakes that are too severe for mere event upcasters.
- Added organization and roles.
- Reworked the home page to include multiple organizations.
- Added some API tests.
- Moved from npm to pnpm.
