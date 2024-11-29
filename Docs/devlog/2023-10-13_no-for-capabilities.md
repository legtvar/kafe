# Stop putting capabilities into the session cookie

That was a dumb idea.
If one were to add a capability to a user, the change had seemingly no effect because capabilities were extracted from the session cookie, to avoid a trip to the DB.
I removed the `ApiUser` class and instead get the `AccountInfo` from the DB on every authorized request.
It's not too slow right now but eventually we might want to add caching.
We mustn't forget to invalidate the cache, when new capabilites are given to a user though.
