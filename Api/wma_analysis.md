# Analysis of the old LEMMA DB

| table | description |
|-------|-------------|
| appf  | záznam o požadavku na přihlášku filmu |
| author | autor filmu |
| bans | bany v RS |
| collections | skupiny techniky v RS |
| emaillog | záznamy o odeslaných emailech jak z RS, tak z WMA |
| entry | stalking toho, která IP stahuje (a předpokládám i nahrává) videa |
| issuemasters | lidi zodpovědní za techniku v RS |
| issuemasters__persons | (person id, issuemaster id) |
| opening_hours | časy, kdy jsou issuemasters k dispozici |
| permission_categories | RS: kategorie LEMMA a LEMMAnad100 |
| persons | účty, jména, adresy a jejich status |
| persons__permission_categories | kategorie oprávnění pro `person` s daným id |
| playlist | seznam videí |
| playlistitem | zařazení danéo videa do daného playlistu na daný index |
| project | projekt s popisem, vlastníkem a dalšími metadaty |
| project__person | n-n asociace člověka s projektem |
| project__reservation__source | n-n-n asociace projektu s rezervací a rezervačním zdrojem |
| projectgroup | skupina projektů |
| requests | žádosti o přístup do RS |
| reservations | rezervace v RS |
| reservationsystem | konfigurace rezervačního systému (hlavně hlavičky emailů) |
| role_table | crew daného projektu s danou rolí (režisér, scénárista, apod.) |
| source_types | typy techinky (kamery, zvuksety, atd.) |
| sources | individuální rezervovatelné položky RS |
| sources__reservations | asociace rezervovatelné položky s rezervací a datem vrácení |
| stateholidays | státní svátky |
| text | české texty (hlavně hlavičky emailů) |
| vacations | dovolené výdejářů techniky |
| variables | jen `EmailsEnabled` nastavené na true |
| video | videa ve WMA navázaná na daný projekt |
| videolink | asi externí link na dané video |
| vote | prázdná tabulka hlasování za videa |
| wma_property | konfigurace WMA (převážně odkazy) |

## Otevřené otázky

* Jaký je rozdíl mezi `author` a `persons`? Resp. proč existují obě?
* Potřebujeme `projectgroup`?
* Jak vyřešit GDPR ve spojení s event sourcingem?
