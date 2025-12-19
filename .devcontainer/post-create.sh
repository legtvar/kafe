#!/bin/bash

su postgres -c "initdb --locale=C.UTF-8 --encoding=UTF8 -D /var/lib/postgres/data --data-checksums" \
    && echo "listen_addresses = '*'" >> /var/lib/postgres/data/postgresql.conf \
    && echo "host    all    all    0.0.0.0/0    trust" >> /var/lib/postgres/data/pg_hba.conf \
    && echo "host    all    all    ::/0         trust" >> /var/lib/postgres/data/pg_hba.conf

mkdir /run/postgresql && chown postgres:postgres /run/postgresql
su postgres -c "pg_ctl -D /var/lib/postgres/data -l /var/lib/postgres/$(date +%s).log start"

cd /kafe/Web/proxy & pnpm install --force

cd /kafe/Web && pnpm install --force

cd /kafe/Api && dotnet restore --force
