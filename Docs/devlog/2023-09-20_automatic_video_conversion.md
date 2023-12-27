# Automatic Video Conversion (2023-09-20)

Added a daemon that periodically checks if any videos were added that need a web-friendly smaller-resolution variant.
If there's a backlog of videos to convert, forms a queue.
However, to avoid spending months on converting old videos brought in by a migration, it increases the time period it checks in its query.
First, it checks today.
Then, it checks this year.
Last, it checks all videos in the database.

The daemon also first adds the smallest, SD, variant and only then adds the HD ones to shorten the time during which no video might be playable in the browser.
That can actually happen because while we do accept, for instance, MKVs, not all browsers do.

The converted result uses the webm format with the OPUS and VP9 codecs, as that is those [should work pretty much everywhere](https://developer.mozilla.org/en-US/docs/Web/Media/Formats/Video_codecs#recommendations_for_everyday_videos) and are pretty space-efficient.
