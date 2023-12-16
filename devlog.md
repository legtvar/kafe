```
      █      █                                                                              
     █      █                                                                               
      █      █         █  █▀ ██   ▄████  ▄███▄       ██▄   ▄███▄      ▄   █    ████▄   ▄▀   
                       █▄█   █ █  █▀   ▀ █▀   ▀      █  █  █▀   ▀      █  █    █   █ ▄▀     
  ███████████████      █▀▄   █▄▄█ █▀▀    ██▄▄        █   █ ██▄▄   █     █ █    █   █ █ ▀▄   
  █            ███     █  █  █  █ █      █▄   ▄▀     █  █  █▄   ▄▀ █    █ ███▄ ▀████ █   █  
   █           ███       █      █  █     ▀███▀       ███▀  ▀███▀    █  █      ▀       ███   
    █         █  █      ▀      █    ▀                                █▐                     
     █████████                ▀                                      ▐                      
```

> A very minimalistic [_architecture design record_](https://github.com/joelparkerhenderson/architecture-decision-record).

# Architecture Decisions

# Static `Create`s (2023-12-16)

Made the `Create` methods on all projections `static`.

Turns out this is necessary when using Marten. (Thanks, Oskar.)

## System Hrib (2023-12-15)

Changed the system `Hrib` from `'*'` to `"system"` because `'*'` is weird in URLs.

# History

These decisions were made before we started writing this devlog.
Nevertheless, they are important to remember.

## Hrib

We use string Human-Readable Identifier Ballast (`Hrib`) for Ids on pretty much everything.
These are essentially YouTube's 11-chars-long Ids but without the checks for swear words.


# Other

## `pg_dump`

To dump all:

```bash
sudo -u postgres pg_dumpall > lemma-yyyy-MM-dd-all.sql
```

To dump and tar WMA

```bash
NAME=lemma-yyyy-MM-dd sudo -u postgres pg_dump --schema lemma --format d --file "/tmp/$NAME" && tar -cf "/tmp/$NAME.tar" -C "/tmp/$NAME" .
```
