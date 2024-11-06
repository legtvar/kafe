---
title: Organizations and Roles
---

# Organizations and Roles

To migrate accounts from WMA we need a concept of _organizations_.
The reason is the fact that WMA had no true _public_ visibility.
Public actually meant _internal to LEMMA_.
Thus we need the concept of an organization so that we can have something _internal_ to it.
Organizations will also be helpful if we ever invite other labs to add their stuff as well.
Or if we decide to derive permissions based on groups we can get from MUNI login.
Also, from now on, organizations own project groups and playlists.

I also added _roles_ as a convenient way of assigning permissions inside an organization.
They are, however, not finished yet.
In particular, they are not taken into consideration when trying to determine if an entity should be accessible or not.
To fix the artifact list query performance issue, I might implement this by persisting them inside an aggregate instead of computing them in pgsql every time.
