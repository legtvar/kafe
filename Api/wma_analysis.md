# Analysis of the old LEMMA DB

| table                          | description                                                      |
| ------------------------------ | ---------------------------------------------------------------- |
| appf                           | záznam o požadavku na přihlášku filmu                            |
| author                         | autor filmu                                                      |
| bans                           | bany v RS                                                        |
| collections                    | skupiny techniky v RS                                            |
| emaillog                       | záznamy o odeslaných emailech jak z RS, tak z WMA                |
| entry                          | stalking toho, která IP stahuje (a předpokládám i nahrává) videa |
| issuemasters                   | lidi zodpovědní za techniku v RS                                 |
| issuemasters__persons          | (person id, issuemaster id)                                      |
| opening_hours                  | časy, kdy jsou issuemasters k dispozici                          |
| permission_categories          | RS: kategorie LEMMA a LEMMAnad100                                |
| persons                        | účty, jména, adresy a jejich status                              |
| persons__permission_categories | kategorie oprávnění pro `person` s daným id (RS)                 |
| playlist                       | seznam videí                                                     |
| playlistitem                   | zařazení daného videa do daného playlistu na daný index          |
| project                        | projekt s popisem, vlastníkem a dalšími metadaty                 |
| project__person                | n-n asociace člověka s projektem                                 |
| project__reservation__source   | n-n-n asociace projektu s rezervací a rezervačním zdrojem        |
| projectgroup                   | skupina projektů                                                 |
| requests                       | žádosti o přístup do RS                                          |
| reservations                   | rezervace v RS                                                   |
| reservationsystem              | konfigurace rezervačního systému (hlavně hlavičky emailů)        |
| role_table                     | crew daného projektu s danou rolí (režisér, scénárista, apod.)   |
| source_types                   | typy techinky (kamery, zvuksety, atd.)                           |
| sources                        | individuální rezervovatelné položky RS                           |
| sources__reservations          | asociace rezervovatelné položky s rezervací a datem vrácení      |
| stateholidays                  | státní svátky                                                    |
| text                           | české texty (hlavně hlavičky emailů)                             |
| vacations                      | dovolené výdejářů techniky                                       |
| variables                      | jen `EmailsEnabled` nastavené na true                            |
| video                          | videa ve WMA navázaná na daný projekt                            |
| videolink                      | asi externí link na dané video                                   |
| vote                           | prázdná tabulka hlasování za videa                               |
| wma_property                   | konfigurace WMA (převážně odkazy)                                |

**Pro WMA jsou důležité `appf` (?), `author`, `entry` (?), `persons` (?),
`playlist`, `playlistitem`, `project`, `project__person`, `projectgroup`,
`role_table`, `video`, `videolink` (?), `wma_property` (?).**

## Mapping

> Always take the id and table name.

### Organization

Make a single _LEMMA_ Organization.

### Account

- persons.id
- persons.name
- persons.email
- persons.address

> Id is Učo.
> Only migrate people that have an ACTIVE status and agreement=true.
> Give all of these people a `Survivor of WMA` role in the LEMMA Organization.

### Author

**From `author`**

- author.name

> We probably shouldn't take all of the authors, since the names are copies to the role_table anyway.
> Name collision is thus inevitable.

**From `project`**

- project.externalauthorname
- project.externalauthoruco
- project.externalauthoremail
- project.externalauthorphone

**From `role_table`**

- role_table.name -> crew/cast role
- role_table.authorname
- role_table.project
- role_table.authoruco

> role_table is populated by values from the author table but is not connected to it by a key.

### ProjectGroup

- projectgroup.name

> Lock all project groups.

### Project

- project__persons -- accounts should have read perm to the project
- project.owner -- account should have read & write perms to the project
- project.name
- project.desc -- `iv` version of Description
- project.web
- project.releasedate
- project.group
- project.publicpseudosecret -- all WMA Survivors can read all of the videos

> Don't forget to lock all migrated projects.

> I'm not sure releasedate is actually useful to us.
> It's essentially the date of the festival or the project's completion.
> Maybe we should add something as `AdditionalProjectMetadata` that would contain things like `releasedate` and `web`.

### Artifacts & VideoShards

- video.name
- video.adddate
- video.public -- whether or not the owning project is public within the Organization (effectively the same as publicpseudosecret)
- video.project
- video.responsiblePerson -- we should treat as project owner since not even them can make changes to the migrated projects
- video.affirmation
- video.license
- video.licenseurl

> Turn affirmation, license & licenseurl into metadata.
> Make CC videos public globally.

> Videolink videos are in `/var/www/videos`. If we have no other copy, we should take these as well.
> Videos in videolink should also still redirect to KAFE.

### Playlist

**From `playlist`**

- playlist.name
- playlist.desc

**From `playlistitem`**

- playlistitem.playlist
- playlistitem.position
- playlistitem.video

> All playlists are automatically visible to WMA Survivors.

## Otevřené otázky

- Jaký je rozdíl mezi `author` a `persons`? Resp. proč existují obě?
- Potřebujeme `projectgroup`?
- Jak vyřešit GDPR ve spojení s event sourcingem?
