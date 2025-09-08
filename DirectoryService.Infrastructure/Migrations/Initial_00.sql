CREATE TABLE "Departments"
(
    "Id" UUID NOT NULL PRIMARY KEY,
    "Name" VARCHAR(150) NOT NULL,
    "Identifier" VARCHAR(150) NOT NULL,
    "ParentId" UUID NULL FOREIGN KEY REFERENCES "Departments"("Id"),
    "Path" JSONB NOT NULL,
    "Depth" SMALLINT NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAtUtc" TIMESTAMP NOT NULL DEFAULT now(),
    "UpdatedAtUtc" TIMESTAMP NOT NULL DEFAULT now()
)

CREATE INDEX idx_departments_parentid ON "Departments" ("ParentId");

CREATE TABLE "Position"
(
    "Id" UUID NOT NULL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL UNIQUE,
    "Description" VARCHAR(1000) NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAtUtc" TIMESTAMP NOT NULL DEFAULT now(),
    "UpdatedAtUtc" TIMESTAMP NOT NULL DEFAULT now()
)

CREATE TABLE "Location"
(
    "Id" UUID NOT NULL PRIMARY KEY,
    "Name" JSONB NOT NULL UNIQUE,
    "Address" VARCHAR(120) NOT NULL UNIQUE,
    "Description" VARCHAR(1000) NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAtUtc" TIMESTAMP NOT NULL DEFAULT now(),
    "UpdatedAtUtc" TIMESTAMP NOT NULL DEFAULT now()
)