-- Pre-creates the auth schema so GoTrue can run its own migrations.
-- GoTrue v2 assumes auth.* tables can be created, but never issues CREATE SCHEMA itself.
-- This script runs automatically on first PostgreSQL container startup
-- via /docker-entrypoint-initdb.d/ before any application connects.
CREATE SCHEMA IF NOT EXISTS auth;
