CREATE EXTENSION ltree;

ALTER TABLE departments
ALTER COLUMN path SET DATA TYPE ltree
USING (path->>'Value')::ltree;

CREATE INDEX ix_path_gist ON departments USING gist(path);