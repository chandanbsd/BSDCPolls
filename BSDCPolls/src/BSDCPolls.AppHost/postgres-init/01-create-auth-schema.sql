-- Pre-creates schemas and extensions that GoTrue v2 requires before it can run its own migrations.
-- GoTrue never issues CREATE SCHEMA itself, and its first migration installs pgcrypto into
-- the extensions schema — both must exist before GoTrue starts.
-- This script runs automatically on first PostgreSQL container startup
-- via /docker-entrypoint-initdb.d/ before any application connects.
CREATE SCHEMA IF NOT EXISTS auth;
CREATE SCHEMA IF NOT EXISTS extensions;
CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA extensions;
