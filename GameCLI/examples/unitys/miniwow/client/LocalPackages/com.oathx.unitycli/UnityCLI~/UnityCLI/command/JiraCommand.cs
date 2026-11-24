using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace UnityCLI
{
    /// <summary>
    /// Implements the standalone CLI `jira` command for Jira REST API access.
    /// </summary>
    internal sealed class JiraCommand : ICommand
    {
        static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        public string Name => "jira";

        public async Task<int> ExecuteAsync(CliOptions options)
        {
            var action = (options.Action ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(action))
            {
                Console.Error.WriteLine(
                    "jira requires an action: config | auth | get | search | mine | projects | issue-types | comment | transitions | transition | create | update.");
                return 1;
            }

            try
            {
                JsonObject data;
                var message = "jira command completed";
                switch (action)
                {
                    case "config":
                        (data, message) = DoConfig(options);
                        break;
                    case "auth":
                    case "auth-test":
                    case "test":
                        (data, message) = await DoAuthTest(options);
                        break;
                    case "get":
                        (data, message) = await DoGet(options);
                        break;
                    case "search":
                        (data, message) = await DoSearch(options, options.JiraJql);
                        break;
                    case "mine":
                        (data, message) = await DoSearch(options, BuildMineJql());
                        break;
                    case "projects":
                        (data, message) = await DoProjects(options);
                        break;
                    case "issue-types":
                    case "issuetypes":
                        (data, message) = await DoIssueTypes(options);
                        break;
                    case "comment":
                        (data, message) = await DoComment(options);
                        break;
                    case "transitions":
                        (data, message) = await DoTransitions(options);
                        break;
                    case "transition":
                        (data, message) = await DoTransition(options);
                        break;
                    case "create":
                        (data, message) = await DoCreate(options);
                        break;
                    case "update":
                        (data, message) = await DoUpdate(options);
                        break;
                    default:
                        Console.Error.WriteLine("Unknown jira action: " + action);
                        return 1;
                }

                data["action"] = action;
                data["respondedAtUtc"] = DateTime.UtcNow.ToString("O");
                var response = Success(message, data, options.DryRun);
                return ResponseFormatter.WriteJsonOrHuman(response, options.JsonOutput, WriteHuman);
            }
            catch (Exception exception)
            {
                var error = Fail("jira_failed", exception.Message);
                return ResponseFormatter.WriteJsonOrHuman(error, options.JsonOutput, ResponseFormatter.WriteError);
            }
        }

        static (JsonObject data, string message) DoConfig(CliOptions options)
        {
            var operation = (options.JiraOperation ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(operation)) operation = "get";

            if (operation == "get")
            {
                var config = JiraConfigStore.Load();
                return (ConfigData(config, includeTokenState: true), "jira config");
            }

            if (operation == "set")
            {
                var config = JiraConfigStore.Load();
                if (!string.IsNullOrWhiteSpace(options.JiraSite)) config.Site = NormalizeSite(options.JiraSite);
                if (!string.IsNullOrWhiteSpace(options.JiraToken))
                {
                    config.ApiToken = options.JiraToken;
                }

                if (string.IsNullOrWhiteSpace(config.Site))
                {
                    throw new InvalidOperationException("jira config set requires --site <jira base url>.");
                }

                JiraConfigStore.Save(config);
                var data = ConfigData(config, includeTokenState: true);
                data["configPath"] = JiraConfigStore.ConfigPath;

                return (data, "jira config saved");
            }

            throw new InvalidOperationException("jira config requires get or set.");
        }

        static async Task<(JsonObject data, string message)> DoAuthTest(CliOptions options)
        {
            var client = CreateClient(options);
            var json = await client.GetJson(client.Api("myself"));
            var data = new JsonObject
            {
                ["site"] = client.Site,
                ["authMode"] = client.Mode,
                ["apiVersion"] = client.ApiVersion,
                ["accountId"] = json["accountId"]?.GetValue<string>() ?? string.Empty,
                ["displayName"] = json["displayName"]?.GetValue<string>() ?? string.Empty,
                ["emailAddress"] = json["emailAddress"]?.GetValue<string>() ?? string.Empty
            };
            return (data, "jira auth ok");
        }

        static async Task<(JsonObject data, string message)> DoGet(CliOptions options)
        {
            RequireIssueKey(options);
            var client = CreateClient(options);
            var path = client.Api("issue/" + Uri.EscapeDataString(options.JiraIssueKey))
                       + "?fields=summary,status,assignee,reporter,issuetype,project,priority,created,updated";
            var issue = await client.GetJson(path);
            var data = new JsonObject
            {
                ["site"] = client.Site,
                ["issue"] = SimplifyIssue(issue)
            };
            return (data, "jira issue: " + options.JiraIssueKey);
        }

        static async Task<(JsonObject data, string message)> DoSearch(CliOptions options, string jql)
        {
            if (string.IsNullOrWhiteSpace(jql))
            {
                throw new InvalidOperationException("jira search requires --jql <expression>.");
            }

            var client = CreateClient(options);
            var count = Math.Clamp(options.Count <= 0 ? 20 : options.Count, 1, 100);
            var path = client.Api("search") + "?maxResults=" + count
                       + "&fields=summary,status,assignee,issuetype,project,priority,updated"
                       + "&jql=" + Uri.EscapeDataString(jql);
            var json = await client.GetJson(path);
            var issues = new JsonArray();
            foreach (var issue in json["issues"]?.AsArray() ?? new JsonArray())
            {
                if (issue is JsonObject obj)
                {
                    issues.Add(SimplifyIssue(obj));
                }
            }

            var data = new JsonObject
            {
                ["site"] = client.Site,
                ["jql"] = jql,
                ["total"] = json["total"]?.GetValue<int>() ?? issues.Count,
                ["returned"] = issues.Count,
                ["issues"] = issues
            };
            return (data, "jira search: " + issues.Count + " issue(s)");
        }

        static async Task<(JsonObject data, string message)> DoProjects(CliOptions options)
        {
            var client = CreateClient(options);
            var count = Math.Clamp(options.Count <= 0 ? 50 : options.Count, 1, 100);
            var json = await client.GetJsonNode(client.IsCloud ? client.Api("project/search") + "?maxResults=" + count : client.Api("project"));
            var projects = new JsonArray();
            var projectNodes = client.IsCloud ? json["values"]?.AsArray() ?? new JsonArray() : json.AsArray();
            foreach (var project in projectNodes.Take(count))
            {
                if (project is not JsonObject obj) continue;
                projects.Add(new JsonObject
                {
                    ["key"] = obj["key"]?.GetValue<string>() ?? string.Empty,
                    ["name"] = obj["name"]?.GetValue<string>() ?? string.Empty,
                    ["id"] = obj["id"]?.GetValue<string>() ?? string.Empty,
                    ["projectTypeKey"] = obj["projectTypeKey"]?.GetValue<string>() ?? string.Empty
                });
            }

            var data = new JsonObject
            {
                ["site"] = client.Site,
                ["total"] = client.IsCloud ? json["total"]?.GetValue<int>() ?? projects.Count : projects.Count,
                ["returned"] = projects.Count,
                ["projects"] = projects
            };
            return (data, "jira projects: " + projects.Count + " project(s)");
        }

        static async Task<(JsonObject data, string message)> DoIssueTypes(CliOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.JiraProjectKey))
            {
                throw new InvalidOperationException("jira issue-types requires --project <PROJECTKEY>.");
            }

            var client = CreateClient(options);
            var json = await client.GetJson(client.Api("issue/createmeta") + "?projectKeys="
                                            + Uri.EscapeDataString(options.JiraProjectKey)
                                            + "&expand=projects.issuetypes");
            var types = new JsonArray();
            var projects = json["projects"]?.AsArray();
            var issueTypes = projects?.FirstOrDefault()?["issuetypes"]?.AsArray() ?? new JsonArray();
            foreach (var issueType in issueTypes)
            {
                if (issueType is not JsonObject obj) continue;
                types.Add(new JsonObject
                {
                    ["id"] = obj["id"]?.GetValue<string>() ?? string.Empty,
                    ["name"] = obj["name"]?.GetValue<string>() ?? string.Empty,
                    ["description"] = obj["description"]?.GetValue<string>() ?? string.Empty
                });
            }

            var data = new JsonObject
            {
                ["site"] = client.Site,
                ["project"] = options.JiraProjectKey,
                ["issueTypes"] = types
            };
            return (data, "jira issue types: " + types.Count + " type(s)");
        }

        static async Task<(JsonObject data, string message)> DoComment(CliOptions options)
        {
            RequireIssueKey(options);
            if (string.IsNullOrWhiteSpace(options.JiraBody))
            {
                throw new InvalidOperationException("jira comment requires --body <text>.");
            }

            var client = CreateClient(options);
            var payload = new JsonObject { ["body"] = CreateTextFieldValue(options.JiraBody, client.SupportsAdf) };
            if (options.DryRun)
            {
                return (new JsonObject
                {
                    ["issueKey"] = options.JiraIssueKey,
                    ["payload"] = payload
                }, "jira comment: dry-run");
            }

            var json = await client.PostJson(client.Api("issue/" + Uri.EscapeDataString(options.JiraIssueKey) + "/comment"), payload);
            var data = new JsonObject
            {
                ["site"] = client.Site,
                ["issueKey"] = options.JiraIssueKey,
                ["commentId"] = json["id"]?.GetValue<string>() ?? string.Empty
            };
            return (data, "jira comment added: " + options.JiraIssueKey);
        }

        static async Task<(JsonObject data, string message)> DoTransitions(CliOptions options)
        {
            RequireIssueKey(options);
            var client = CreateClient(options);
            var json = await client.GetJson(client.Api("issue/" + Uri.EscapeDataString(options.JiraIssueKey) + "/transitions"));
            var transitions = ExtractTransitions(json);
            var data = new JsonObject
            {
                ["site"] = client.Site,
                ["issueKey"] = options.JiraIssueKey,
                ["transitions"] = transitions
            };
            return (data, "jira transitions: " + transitions.Count + " transition(s)");
        }

        static async Task<(JsonObject data, string message)> DoTransition(CliOptions options)
        {
            RequireIssueKey(options);
            if (string.IsNullOrWhiteSpace(options.JiraTransitionTo))
            {
                throw new InvalidOperationException("jira transition requires --to <transition name or id>.");
            }

            var client = CreateClient(options);
            var transitionsJson = await client.GetJson(client.Api("issue/" + Uri.EscapeDataString(options.JiraIssueKey) + "/transitions"));
            var transition = FindTransition(transitionsJson, options.JiraTransitionTo);
            if (transition == null)
            {
                throw new InvalidOperationException("Transition not found: " + options.JiraTransitionTo);
            }

            var payload = new JsonObject
            {
                ["transition"] = new JsonObject { ["id"] = transition["id"]?.GetValue<string>() ?? string.Empty }
            };
            if (options.DryRun)
            {
                return (new JsonObject
                {
                    ["issueKey"] = options.JiraIssueKey,
                    ["transition"] = transition.DeepClone()
                }, "jira transition: dry-run");
            }

            await client.PostJson(client.Api("issue/" + Uri.EscapeDataString(options.JiraIssueKey) + "/transitions"), payload, expectNoContent: true);
            var data = new JsonObject
            {
                ["site"] = client.Site,
                ["issueKey"] = options.JiraIssueKey,
                ["transition"] = transition.DeepClone()
            };
            return (data, "jira transition applied: " + options.JiraIssueKey);
        }

        static async Task<(JsonObject data, string message)> DoCreate(CliOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.JiraProjectKey))
            {
                throw new InvalidOperationException("jira create requires --project <PROJECTKEY>.");
            }

            if (string.IsNullOrWhiteSpace(options.JiraIssueType))
            {
                throw new InvalidOperationException("jira create requires --type <IssueType>.");
            }

            if (string.IsNullOrWhiteSpace(options.JiraSummary))
            {
                throw new InvalidOperationException("jira create requires --summary <text>.");
            }

            var client = CreateClient(options);
            var fields = new JsonObject
            {
                ["project"] = new JsonObject { ["key"] = options.JiraProjectKey },
                ["issuetype"] = new JsonObject { ["name"] = options.JiraIssueType },
                ["summary"] = options.JiraSummary
            };
            if (!string.IsNullOrWhiteSpace(options.JiraDescription))
            {
                fields["description"] = CreateTextFieldValue(options.JiraDescription, client.SupportsAdf);
            }

            var payload = new JsonObject { ["fields"] = fields };
            if (options.DryRun)
            {
                return (new JsonObject { ["payload"] = payload }, "jira create: dry-run");
            }

            var json = await client.PostJson(client.Api("issue"), payload);
            var data = new JsonObject
            {
                ["site"] = client.Site,
                ["key"] = json["key"]?.GetValue<string>() ?? string.Empty,
                ["id"] = json["id"]?.GetValue<string>() ?? string.Empty,
                ["self"] = json["self"]?.GetValue<string>() ?? string.Empty
            };
            return (data, "jira issue created: " + (data["key"]?.GetValue<string>() ?? string.Empty));
        }

        static async Task<(JsonObject data, string message)> DoUpdate(CliOptions options)
        {
            RequireIssueKey(options);
            var client = CreateClient(options);
            var fields = new JsonObject();
            if (!string.IsNullOrWhiteSpace(options.JiraSummary))
            {
                fields["summary"] = options.JiraSummary;
            }

            if (!string.IsNullOrWhiteSpace(options.JiraDescription))
            {
                fields["description"] = CreateTextFieldValue(options.JiraDescription, client.SupportsAdf);
            }

            if (fields.Count == 0)
            {
                throw new InvalidOperationException("jira update requires --summary and/or --description.");
            }

            var payload = new JsonObject { ["fields"] = fields };
            if (options.DryRun)
            {
                return (new JsonObject
                {
                    ["issueKey"] = options.JiraIssueKey,
                    ["payload"] = payload
                }, "jira update: dry-run");
            }

            await client.PutJson(client.Api("issue/" + Uri.EscapeDataString(options.JiraIssueKey)), payload);
            var data = new JsonObject
            {
                ["site"] = client.Site,
                ["issueKey"] = options.JiraIssueKey,
                ["updated"] = true
            };
            return (data, "jira issue updated: " + options.JiraIssueKey);
        }

        static JiraClient CreateClient(CliOptions options)
        {
            var config = JiraConfigStore.Load();
            var site = FirstNonEmpty(options.JiraSite, Environment.GetEnvironmentVariable("JIRA_SITE"), config.Site);
            var token = FirstNonEmpty(
                options.JiraToken,
                Environment.GetEnvironmentVariable("JIRA_TOKEN"),
                Environment.GetEnvironmentVariable("JIRA_API_TOKEN"),
                Environment.GetEnvironmentVariable("JIRA_PAT"),
                config.ApiToken);

            if (string.IsNullOrWhiteSpace(site))
            {
                throw new InvalidOperationException("Missing Jira site. Use --site, JIRA_SITE, or jira config set --site <url>.");
            }

            var normalizedSite = NormalizeSite(site);

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("Missing Jira token. Use --token, JIRA_TOKEN, JIRA_API_TOKEN, JIRA_PAT, or jira config set --token <token>.");
            }

            return new JiraClient(normalizedSite, token);
        }

        static JsonObject SimplifyIssue(JsonObject issue)
        {
            var fields = issue["fields"]?.AsObject();
            var status = fields?["status"]?.AsObject();
            var assignee = fields?["assignee"]?.AsObject();
            var reporter = fields?["reporter"]?.AsObject();
            var issueType = fields?["issuetype"]?.AsObject();
            var project = fields?["project"]?.AsObject();
            var priority = fields?["priority"]?.AsObject();
            return new JsonObject
            {
                ["key"] = issue["key"]?.GetValue<string>() ?? string.Empty,
                ["id"] = issue["id"]?.GetValue<string>() ?? string.Empty,
                ["summary"] = fields?["summary"]?.GetValue<string>() ?? string.Empty,
                ["status"] = status?["name"]?.GetValue<string>() ?? string.Empty,
                ["assignee"] = assignee?["displayName"]?.GetValue<string>() ?? string.Empty,
                ["reporter"] = reporter?["displayName"]?.GetValue<string>() ?? string.Empty,
                ["issueType"] = issueType?["name"]?.GetValue<string>() ?? string.Empty,
                ["project"] = project?["key"]?.GetValue<string>() ?? string.Empty,
                ["priority"] = priority?["name"]?.GetValue<string>() ?? string.Empty,
                ["updated"] = fields?["updated"]?.GetValue<string>() ?? string.Empty,
                ["created"] = fields?["created"]?.GetValue<string>() ?? string.Empty
            };
        }

        static JsonArray ExtractTransitions(JsonObject json)
        {
            var result = new JsonArray();
            foreach (var transition in json["transitions"]?.AsArray() ?? new JsonArray())
            {
                if (transition is not JsonObject obj) continue;
                var to = obj["to"]?.AsObject();
                result.Add(new JsonObject
                {
                    ["id"] = obj["id"]?.GetValue<string>() ?? string.Empty,
                    ["name"] = obj["name"]?.GetValue<string>() ?? string.Empty,
                    ["to"] = to?["name"]?.GetValue<string>() ?? string.Empty
                });
            }

            return result;
        }

        static JsonObject? FindTransition(JsonObject json, string nameOrId)
        {
            foreach (var transition in ExtractTransitions(json))
            {
                if (transition is not JsonObject obj) continue;
                var id = obj["id"]?.GetValue<string>() ?? string.Empty;
                var name = obj["name"]?.GetValue<string>() ?? string.Empty;
                if (string.Equals(id, nameOrId, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(name, nameOrId, StringComparison.OrdinalIgnoreCase))
                {
                    return obj;
                }
            }

            return null;
        }

        static JsonObject ToAdf(string text)
        {
            var content = new JsonArray();
            var normalized = (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
            foreach (var line in normalized.Split('\n'))
            {
                var paragraphContent = new JsonArray();
                if (line.Length > 0)
                {
                    paragraphContent.Add(new JsonObject
                    {
                        ["type"] = "text",
                        ["text"] = line
                    });
                }

                content.Add(new JsonObject
                {
                    ["type"] = "paragraph",
                    ["content"] = paragraphContent
                });
            }

            if (content.Count == 0)
            {
                content.Add(new JsonObject { ["type"] = "paragraph", ["content"] = new JsonArray() });
            }

            return new JsonObject
            {
                ["type"] = "doc",
                ["version"] = 1,
                ["content"] = content
            };
        }

        static JsonObject ConfigData(JiraConfig config, bool includeTokenState)
        {
            var data = new JsonObject
            {
                ["site"] = config.Site ?? string.Empty,
                ["configPath"] = JiraConfigStore.ConfigPath
            };
            if (includeTokenState)
            {
                data["hasSavedToken"] = !string.IsNullOrWhiteSpace(config.ApiToken);
                data["hasEnvToken"] = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JIRA_TOKEN"))
                                      || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JIRA_API_TOKEN"))
                                      || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("JIRA_PAT"));
            }

            return data;
        }

        static string BuildMineJql()
        {
            return "assignee = currentUser() AND resolution = Unresolved ORDER BY updated DESC";
        }

        static void RequireIssueKey(CliOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.JiraIssueKey))
            {
                throw new InvalidOperationException("jira action requires an issue key, such as PROJ-123.");
            }
        }

        static string NormalizeSite(string site)
        {
            return site.Trim().TrimEnd('/');
        }

        static JsonNode CreateTextFieldValue(string text, bool supportsAdf)
        {
            return supportsAdf ? ToAdf(text) : JsonValue.Create(text ?? string.Empty)!;
        }

        static string FirstNonEmpty(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
            }

            return string.Empty;
        }

        static string Success(string message, JsonObject data, bool dryRun)
        {
            var root = new JsonObject
            {
                ["ok"] = true,
                ["status"] = dryRun ? "validated" : "completed",
                ["message"] = message,
                ["data"] = data,
                ["error"] = new JsonObject()
            };
            return root.ToJsonString(JsonOptions);
        }

        static string Fail(string code, string message)
        {
            var root = new JsonObject
            {
                ["ok"] = false,
                ["status"] = "error",
                ["message"] = message,
                ["error"] = new JsonObject
                {
                    ["code"] = code,
                    ["message"] = message
                }
            };
            return root.ToJsonString(JsonOptions);
        }

        static void WriteHuman(JsonElement root)
        {
            var ok = root.TryGetProperty("ok", out var okElement) && okElement.GetBoolean();
            if (!ok)
            {
                ResponseFormatter.WriteError(root);
                return;
            }

            var message = root.TryGetProperty("message", out var messageElement)
                ? messageElement.GetString()
                : "jira command completed";
            Console.WriteLine(message);

            if (!root.TryGetProperty("data", out var data))
            {
                return;
            }

            WriteString(data, "site", "  site: ");
            WriteString(data, "authMode", "  auth: ");
            WriteString(data, "apiVersion", "  api: ");
            WriteString(data, "jql", "  jql: ");
            WriteInt(data, "total", "  total: ");
            WriteInt(data, "returned", "  returned: ");
            WriteBool(data, "hasSavedToken", "  saved token: ");
            WriteBool(data, "hasEnvToken", "  env token: ");
            WriteString(data, "configPath", "  config: ");
            WriteString(data, "tokenNote", "  note: ");

            if (data.TryGetProperty("issue", out var issue) && issue.ValueKind == JsonValueKind.Object)
            {
                WriteIssue(issue, "  ");
            }

            if (data.TryGetProperty("issues", out var issues) && issues.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in issues.EnumerateArray())
                {
                    WriteIssue(item, "  ");
                }
            }

            if (data.TryGetProperty("projects", out var projects) && projects.ValueKind == JsonValueKind.Array)
            {
                foreach (var project in projects.EnumerateArray())
                {
                    Console.WriteLine("  " + GetString(project, "key") + "  " + GetString(project, "name"));
                }
            }

            if (data.TryGetProperty("issueTypes", out var issueTypes) && issueTypes.ValueKind == JsonValueKind.Array)
            {
                foreach (var issueType in issueTypes.EnumerateArray())
                {
                    Console.WriteLine("  " + GetString(issueType, "id") + "  " + GetString(issueType, "name"));
                }
            }

            if (data.TryGetProperty("transitions", out var transitions) && transitions.ValueKind == JsonValueKind.Array)
            {
                foreach (var transition in transitions.EnumerateArray())
                {
                    Console.WriteLine("  " + GetString(transition, "id") + "  " + GetString(transition, "name")
                                      + " -> " + GetString(transition, "to"));
                }
            }

            if (data.TryGetProperty("payload", out var payload))
            {
                Console.WriteLine("  payload: " + payload.GetRawText());
            }
        }

        static void WriteIssue(JsonElement issue, string indent)
        {
            var key = GetString(issue, "key");
            var summary = GetString(issue, "summary");
            var status = GetString(issue, "status");
            var assignee = GetString(issue, "assignee");
            var type = GetString(issue, "issueType");
            Console.WriteLine(indent + key + " [" + status + "] " + summary
                              + (type.Length > 0 ? " (" + type + ")" : string.Empty)
                              + (assignee.Length > 0 ? " assignee=" + assignee : string.Empty));
        }

        static void WriteString(JsonElement element, string name, string prefix)
        {
            var value = GetString(element, name);
            if (value.Length > 0) Console.WriteLine(prefix + value);
        }

        static void WriteInt(JsonElement element, string name, string prefix)
        {
            if (element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number)
            {
                Console.WriteLine(prefix + value.GetInt32());
            }
        }

        static void WriteBool(JsonElement element, string name, string prefix)
        {
            if (element.TryGetProperty(name, out var value)
                && (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False))
            {
                Console.WriteLine(prefix + value.GetBoolean());
            }
        }

        static string GetString(JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString() ?? string.Empty
                : string.Empty;
        }

        sealed class JiraClient
        {
            readonly HttpClient httpClient;

            public JiraClient(string site, string token)
            {
                Site = site;
                IsCloud = false;
                ApiVersion = "2";
                Mode = "bearer";
                SupportsAdf = false;
                httpClient = new HttpClient { BaseAddress = new Uri(site), Timeout = TimeSpan.FromSeconds(30) };
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            public string Site { get; }
            public bool IsCloud { get; }
            public string Mode { get; }
            public string ApiVersion { get; }
            public bool SupportsAdf { get; }

            public string Api(string relativePath)
            {
                return "/rest/api/" + ApiVersion + "/" + relativePath.TrimStart('/');
            }

            public async Task<JsonObject> GetJson(string path)
            {
                using var response = await httpClient.GetAsync(path);
                return (await ReadJsonResponse(response)).AsObject();
            }

            public async Task<JsonNode> GetJsonNode(string path)
            {
                using var response = await httpClient.GetAsync(path);
                return await ReadJsonResponse(response);
            }

            public async Task<JsonObject> PostJson(string path, JsonObject payload, bool expectNoContent = false)
            {
                using var content = new StringContent(payload.ToJsonString(JsonOptions), Encoding.UTF8, "application/json");
                using var response = await httpClient.PostAsync(path, content);
                if (expectNoContent && response.IsSuccessStatusCode)
                {
                    return new JsonObject();
                }

                return (await ReadJsonResponse(response)).AsObject();
            }

            public async Task PutJson(string path, JsonObject payload)
            {
                using var content = new StringContent(payload.ToJsonString(JsonOptions), Encoding.UTF8, "application/json");
                using var response = await httpClient.PutAsync(path, content);
                if (!response.IsSuccessStatusCode)
                {
                    await ThrowHttpError(response);
                }
            }

            static async Task<JsonNode> ReadJsonResponse(HttpResponseMessage response)
            {
                var body = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    await ThrowHttpError(response, body);
                }

                if (string.IsNullOrWhiteSpace(body))
                {
                    return new JsonObject();
                }

                return JsonNode.Parse(body) ?? new JsonObject();
            }

            static async Task ThrowHttpError(HttpResponseMessage response, string? knownBody = null)
            {
                var body = knownBody ?? await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException("Jira HTTP " + (int)response.StatusCode + " " + response.ReasonPhrase
                                                    + (string.IsNullOrWhiteSpace(body) ? string.Empty : ": " + body));
            }
        }

        sealed class JiraConfig
        {
            public string Site { get; set; } = string.Empty;
            public string ApiToken { get; set; } = string.Empty;
        }

        static class JiraConfigStore
        {
            public static string ConfigPath
            {
                get
                {
                    var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    return Path.Combine(home, ".unitycli", "jira.json");
                }
            }

            public static JiraConfig Load()
            {
                try
                {
                    if (!File.Exists(ConfigPath)) return new JiraConfig();
                    var json = File.ReadAllText(ConfigPath);
                    return JsonSerializer.Deserialize<JiraConfig>(json) ?? new JiraConfig();
                }
                catch
                {
                    return new JiraConfig();
                }
            }

            public static void Save(JiraConfig config)
            {
                var dir = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
            }
        }
    }
}

