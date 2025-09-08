CREATE TABLE departments
(
    id UUID NOT NULL PRIMARY KEY,
    name VARCHAR(150) NOT NULL,
    identifier VARCHAR(150) NOT NULL,
    parent_id UUID NULL REFERENCES departments(id),
    path JSONB NOT NULL,
    depth SMALLINT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at_utc TIMESTAMP NOT NULL DEFAULT now(),
    updated_at_utc TIMESTAMP NOT NULL DEFAULT now()
);

CREATE INDEX idx_departments_parent_id ON departments (parent_id);

CREATE TABLE position
(
    id UUID NOT NULL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(1000) NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at_utc TIMESTAMP NOT NULL DEFAULT now(),
    updated_at_utc TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE location
(
    id UUID NOT NULL PRIMARY KEY,
    name JSONB NOT NULL UNIQUE,
    address VARCHAR(120) NOT NULL UNIQUE,
    description VARCHAR(1000) NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at_utc TIMESTAMP NOT NULL DEFAULT now(),
    updated_at_utc TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE department_location
(
    department_id UUID NOT NULL REFERENCES departments(id),
    location_id UUID NOT NULL REFERENCES location(id),
    PRIMARY KEY (department_id, location_id)
);
