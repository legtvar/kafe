#!/bin/bash

source $NVM_DIR/nvm.sh

cd /kafe/Web && yes | pnpm install

cd /kafe/Web/proxy & yes | pnpm install

cd /kafe/Api && dotnet restore --interactive false
