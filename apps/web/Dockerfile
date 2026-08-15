# Dev-oriented image: runs the Next.js dev server from source. Not production-hardened
# (no standalone build/multi-stage runtime image) - that's Phase 4 scope.
FROM node:22-slim

WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci

COPY . .

EXPOSE 3000

CMD ["npm", "run", "dev", "--", "--port", "3000"]
