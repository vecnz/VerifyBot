#!/bin/sh

pnpm exec prisma migrate deploy
pnpm run start