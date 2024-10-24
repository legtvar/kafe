---
title: Use ProblemDetails
tags: devlog
---

# Use `ProblemDetails`

With KAFE finally safely resting in its new home, `mlejnek` on Stratus.FI, I finally have time to work directly on KAFE.

I bumped into a bug with the `TemporaryAccountClosed` event.
Marten had no knowledge of it, yet it was in the database.
This happened because, I removed the event type from the C# codebase back in December.
The fix was simple enough: I added an upcaster from `TemporaryAccountClosed` to `TemporaryAccountRefreshed`.

However, there was also an issue with the app not logging the error, so I had to take a look at
[ASP.NET Core's error handling](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling).
Based on that, I realized that the JSON that ASP.NET Core returns is actually standardized as
[RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807).
So now we use `Microsoft.AspNetCore.Mvc.ProblemDetails` and log the error as well.
