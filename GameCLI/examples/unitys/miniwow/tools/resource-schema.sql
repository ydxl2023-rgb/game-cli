CREATE SCHEMA miniwow_resources;
CREATE TABLE miniwow_resources.catalog_meta (id integer PRIMARY KEY CHECK(id=1), schema_version integer NOT NULL, owner_tag text NOT NULL);
INSERT INTO miniwow_resources.catalog_meta VALUES (1,1,'miniwow-resource-catalog');
CREATE TABLE miniwow_resources.sources (id text PRIMARY KEY,payload jsonb NOT NULL);
CREATE TABLE miniwow_resources.assets (id text PRIMARY KEY,source_id text NOT NULL REFERENCES miniwow_resources.sources(id),payload jsonb NOT NULL);
CREATE TABLE miniwow_resources.exports (id text PRIMARY KEY,asset_id text NOT NULL REFERENCES miniwow_resources.assets(id),payload jsonb NOT NULL);
CREATE TABLE miniwow_resources.annotations (asset_id text PRIMARY KEY REFERENCES miniwow_resources.assets(id),system_name text,purpose text,status text NOT NULL DEFAULT 'pending',evidence jsonb NOT NULL DEFAULT '[]',updated_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE miniwow_resources.runs (id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY,created_at timestamptz NOT NULL DEFAULT now(),catalog_sha256 text NOT NULL,summary jsonb NOT NULL);
CREATE INDEX assets_source_idx ON miniwow_resources.assets(source_id);
CREATE INDEX exports_asset_idx ON miniwow_resources.exports(asset_id);
CREATE INDEX assets_name_idx ON miniwow_resources.assets((payload->>'name'));
CREATE INDEX assets_system_idx ON miniwow_resources.assets((payload->>'system_guess'));
CREATE INDEX assets_search_idx ON miniwow_resources.assets USING gin(to_tsvector('simple',coalesce(payload->>'name','') || ' ' || coalesce(payload->>'purpose_guess','')));
CREATE VIEW miniwow_resources.resource_search AS
SELECT a.id,a.source_id,a.payload->>'type' AS type,a.payload->>'name' AS name,
coalesce(n.system_name,a.payload->>'system_guess') AS system_name,
coalesce(n.purpose,a.payload->>'purpose_guess') AS purpose,
a.payload->>'confidence' AS confidence,a.payload->>'status' AS extraction_status,
coalesce(n.status,'unreviewed') AS review_status,n.evidence AS review_evidence,
s.payload AS source,a.payload AS asset,
coalesce((SELECT jsonb_agg(e.payload ORDER BY e.id) FROM miniwow_resources.exports e WHERE e.asset_id=a.id),'[]'::jsonb) AS exported_files
FROM miniwow_resources.assets a JOIN miniwow_resources.sources s ON s.id=a.source_id
LEFT JOIN miniwow_resources.annotations n ON n.asset_id=a.id;
