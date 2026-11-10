export const workspaceSchema=`
CREATE TABLE IF NOT EXISTS gamecli_ui.projects(id text PRIMARY KEY,name text NOT NULL,created_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS gamecli_ui.project_members(project_id text REFERENCES gamecli_ui.projects(id),actor text,role text NOT NULL CHECK(role IN ('owner','editor','viewer')),PRIMARY KEY(project_id,actor));
CREATE TABLE IF NOT EXISTS gamecli_ui.template_admins(actor text PRIMARY KEY);
CREATE TABLE IF NOT EXISTS gamecli_ui.project_ui_revisions(project_id text NOT NULL,version integer NOT NULL,payload jsonb NOT NULL,created_at timestamptz NOT NULL DEFAULT now(),PRIMARY KEY(project_id,version));
CREATE TABLE IF NOT EXISTS gamecli_ui.template_versions(template_id text NOT NULL,version integer NOT NULL,name text NOT NULL,category_id text NOT NULL,payload jsonb NOT NULL,scripts jsonb,created_at timestamptz NOT NULL DEFAULT now(),PRIMARY KEY(template_id,version));
CREATE TABLE IF NOT EXISTS gamecli_ui.template_candidates(id uuid PRIMARY KEY,template_id text NOT NULL REFERENCES gamecli_ui.templates(id),base_version integer NOT NULL,project_id text REFERENCES gamecli_ui.projects(id),actor text NOT NULL,payload jsonb NOT NULL,scripts jsonb,status text NOT NULL DEFAULT 'pending' CHECK(status IN ('pending','published','rejected')),created_at timestamptz NOT NULL DEFAULT now());
CREATE OR REPLACE FUNCTION gamecli_ui.immutable_history() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION '历史版本不可修改或删除'; END $$;
DROP TRIGGER IF EXISTS immutable_history ON gamecli_ui.template_versions;
CREATE TRIGGER immutable_history BEFORE UPDATE OR DELETE ON gamecli_ui.template_versions FOR EACH ROW EXECUTE FUNCTION gamecli_ui.immutable_history();
DROP TRIGGER IF EXISTS immutable_history ON gamecli_ui.project_ui_revisions;
CREATE TRIGGER immutable_history BEFORE UPDATE OR DELETE ON gamecli_ui.project_ui_revisions FOR EACH ROW EXECUTE FUNCTION gamecli_ui.immutable_history();
CREATE OR REPLACE FUNCTION gamecli_ui.archive_project() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN
 INSERT INTO gamecli_ui.project_ui_revisions(project_id,version,payload) VALUES(NEW.project_id,NEW.version,NEW.payload); RETURN NEW; END $$;
DROP TRIGGER IF EXISTS archive_project ON gamecli_ui.project_ui;
CREATE TRIGGER archive_project AFTER INSERT OR UPDATE ON gamecli_ui.project_ui FOR EACH ROW EXECUTE FUNCTION gamecli_ui.archive_project();
CREATE OR REPLACE FUNCTION gamecli_ui.guard_template() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN
 IF TG_OP='DELETE' THEN RAISE EXCEPTION '已发布模板不可删除'; END IF;
 IF current_setting('gamecli.publish',true) IS DISTINCT FROM 'approved' THEN RAISE EXCEPTION '请通过模板候选发布新版本，不能直接覆盖标准模板'; END IF;
 IF TG_OP='UPDATE' AND (NEW.id<>OLD.id OR NEW.version<>OLD.version+1) THEN RAISE EXCEPTION '发布必须递增模板版本'; END IF;
 RETURN NEW; END $$;
DROP TRIGGER IF EXISTS guard_template ON gamecli_ui.templates;
CREATE TRIGGER guard_template BEFORE INSERT OR UPDATE OR DELETE ON gamecli_ui.templates FOR EACH ROW EXECUTE FUNCTION gamecli_ui.guard_template();
CREATE OR REPLACE FUNCTION gamecli_ui.archive_template() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN
 INSERT INTO gamecli_ui.template_versions(template_id,version,name,category_id,payload,scripts) VALUES(NEW.id,NEW.version,NEW.name,NEW.category_id,NEW.payload,NEW.scripts); RETURN NEW; END $$;
DROP TRIGGER IF EXISTS archive_template ON gamecli_ui.templates;
CREATE TRIGGER archive_template AFTER INSERT OR UPDATE ON gamecli_ui.templates FOR EACH ROW EXECUTE FUNCTION gamecli_ui.archive_template();
`;
