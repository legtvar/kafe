---
title: AccountCreated
---

# `AccountCreated`

`TemporaryAccountCreated` got deprecated in favour of `AccountCreated` because that event can be used for both temporary and external accounts.

I also tried to use `ExternalAccountAssociated` both as `Create` and `Apply` event, but it got called for both thus erasing the projection at that point which led to the removal of existing permissions.
Marten apparently supports this use case in 7.0.0 but we should avoid it anyway as it's kinda smelly.
