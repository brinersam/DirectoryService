CREATE TABLE departments_positions
(
    position_id UUID NOT NULL REFERENCES positions(id),
    department_id UUID NOT NULL REFERENCES departments(id),
    PRIMARY KEY (position_id, department_id)
);