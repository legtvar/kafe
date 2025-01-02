# Autodiscover generated video variants

Since I managed to drop the whole production database on October's Friday 13th, we lost all records about the month the conversion daemon spent converting the twenty years of videos into webm.
Since the videos were just sitting there, I added told the daemon to first check if a uncorrupted video already sits there.
If it does, it assumes its the correct one and skips converting it anew and just saves the metadata of the existing one.
