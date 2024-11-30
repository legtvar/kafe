# Make `Hrib` conversion to `string` explicit

Turns out the implicit conversion leads to some nasty, hard-to-discover bugs when used in methods that take an `object` or `params object[]` (i.e. a shit ton of Marten methods).
From now on use `(string)hrib` or `hrib.Value` to access the underlying `string`.
