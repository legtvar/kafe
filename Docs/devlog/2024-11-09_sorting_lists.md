# Sorting Lists

I added sorting to all list endpoints:

- It is configured using the `sort` query parameter.
- It only works on properties that have the `Sortable` attribute, so that we don't leak other data or create weird SQL queries. A list of these properties per entity type is stored in `SortableMetadata`.
- I also added `EntityMetadataProvider`, which obtains and caches `SortableMetadata`.
- `LocalizedString`s are sorted by the `iv` culture by default.
  This can be changed by adding a dot and a two-letter culture code.
  For example `name.cs` will prefer to sort by the Czech variant of the `Name` property and will only use `iv` as a fallback.
- All endpoints sort by `name.iv` by default, except `AccountListEndpoint` which sorts by `emailAddress`.

Goodbye lists in randomized order every time we deploy the app. :D
