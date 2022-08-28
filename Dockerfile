# Install the following packages:
FROM node:18 as base

WORKDIR /usr/src/app

# Environment variables
ENV CI=true
ENV LOG_LEVEL=info
ENV FORCE_COLOR=true

# Update packages and install dependencies for the build process
RUN apt-get update && \
    apt-get upgrade -y --no-install-recommends && \
    apt-get install -y --no-install-recommends build-essential python3 dumb-init && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* && \
    apt-get autoremove

# Copy the lock files 
COPY --chown=node:node yarn.lock .
COPY --chown=node:node package.json .
COPY --chown=node:node .yarnrc.yml .
COPY --chown=node:node .yarn/ .yarn/
COPY --chown=node:node prisma/ .

ENTRYPOINT ["dumb-init", "--"]

###############
# Build Stage #
###############
FROM base as build

ENV NODE_ENV="development"

# Copy the source files to the build context
COPY --chown=node:node tsconfig.base.json .
COPY --chown=node:node tsup.config.ts .
COPY --chown=node:node src/ src/
COPY --chown=node:node prisma/ prisma/

# Run the build process
RUN yarn install --immutable
RUN yarn run prisma generate
RUN yarn run build

##############
# Run Stage #
#############
FROM base as runner

# Environment variables
ENV NODE_ENV="production"
ENV NODE_OPTIONS="--enable-source-maps --preserve-symlinks"

WORKDIR /usr/src/app

COPY --chown=node:node --from=build /usr/src/app/dist dist
COPY --chown=node:node --from=build /usr/src/app/node_modules node_modules/.prisma
COPY --chown=node:node --from=build /usr/src/app/node_modules ./node_modules
COPY --chown=node:node start.sh /usr/src/app/start.sh

USER node

CMD ["./start.sh"]