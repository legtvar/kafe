# Hrib's `Value` vs `ToString` (2024-01-03)

- `Hrib.Value` is for raw access to the underlying string. Use it, for example, in JSON converters.
- `Hrib.ToString()` is for integration with other libraries, most significantly Marten. It throws an exception when used on an invalid HRIB thus preventing `invalid` from reaching the DB.
