#!/bin/bash

su postgres -c "pg_ctl -D /var/lib/postgres/data -l /var/lib/postgres/$(date +%s).log start"

cd /kafe/Web && yes | pnpm install

cd /kafe/Web/proxy & yes | pnpm install

cd /kafe/Api && dotnet restore --interactive false
