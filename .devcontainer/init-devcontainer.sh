#!/bin/bash

cd /kafe/Web && pnpm install --force

cd /kafe/Api && dotnet restore --force
