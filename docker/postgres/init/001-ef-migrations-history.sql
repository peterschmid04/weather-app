-- Keeps the EF Core migrations history table available immediately after the
-- PostgreSQL container initializes. EF Core still owns the real migration
-- entries; this file only prevents first-start relation errors.
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);
