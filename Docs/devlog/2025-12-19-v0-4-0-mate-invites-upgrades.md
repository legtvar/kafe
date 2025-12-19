# v0.4.0: MATE, Invites, and Upgrades

I haven't been writing this devlog as much as I probably should have. Still, some work got done and here it is...

## Changelog

- Added support for Blender models and their validation through PIGEONS. It's called MATE and was implemented by Niko Mlynarčík as part of his bachelor's thesis.
- Added invites. Permissions can now be given to non-existing accounts and an invitation email will be sent.
- Added batched invitations (by Niko).
- Added the MATE organization (by Niko).
- Added an endpoint to download all of a project's shards in a ZIP file (by Niko).
- Added custom problem details in effort to unify API's error responses.
- Moved to Postgres 17.
- Added GNU Terry Pratchett.
- Moved from `react-app-rewired` to `vite`.
- Fixed missing converted videos for videos with resolutions smaller than 480p.
- Updated dependencies on both front end and back end.
