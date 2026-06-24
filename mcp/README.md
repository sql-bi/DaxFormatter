# @sqlbi/daxformatter-mcp

A [Model Context Protocol](https://modelcontextprotocol.io) (MCP) server that formats and validates DAX from any MCP-enabled AI client — Claude Code, Claude Desktop, Cursor, VS Code, and others — using the SQLBI DAX Formatter service.

## Installation guide

The server runs **locally on your machine** — no install step, no account, no API key, no login. It exposes a single tool, `format_dax`. Find your app below and follow the short steps; afterwards, just ask your assistant to *"format this DAX"*.

### Visual Studio Code

[![Install in VS Code](https://img.shields.io/badge/VS_Code-Install_DAX_Formatter-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect/mcp/install?name=DaxFormatter&config=%7B%22type%22%3A%22stdio%22%2C%22command%22%3A%22npx%22%2C%22args%22%3A%5B%22-y%22%2C%22%40sqlbi%2Fdaxformatter-mcp%22%5D%7D)
[![Install in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_DAX_Formatter-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=DaxFormatter&config=%7B%22type%22%3A%22stdio%22%2C%22command%22%3A%22npx%22%2C%22args%22%3A%5B%22-y%22%2C%22%40sqlbi%2Fdaxformatter-mcp%22%5D%7D&quality=insiders)

Click a badge above — VS Code opens and asks you to confirm. That's it.

<details>
<summary>Prefer the terminal?</summary>

```bash
code --add-mcp "{\"name\":\"DaxFormatter\",\"type\":\"stdio\",\"command\":\"npx\",\"args\":[\"-y\",\"@sqlbi/daxformatter-mcp\"]}"
```
</details>

### Claude

**Claude Desktop** — add it through the config file:

1. Open **Settings → Developer → Edit Config** to reveal `claude_desktop_config.json`.
2. Inside `mcpServers`, add the standard `DaxFormatter` entry shown in [Any MCP client](#any-mcp-client) below.
3. Restart Claude Desktop, then confirm the server appears under **Settings → Developer**.

The `format_dax` tool is now available in chat. The same connector is automatically shared with Claude Code, so you set it up once for both.

<details>
<summary>Advanced: add it from the Claude Code terminal</summary>

```bash
# -s user enables it in every project; omit -s for the current project only
claude mcp add -s user DaxFormatter -- npx -y @sqlbi/daxformatter-mcp

# check it was added
claude mcp get DaxFormatter
```
</details>

### Codex

In the **Codex app**:

1. Open **Settings → Integrations → MCP servers → Add**.
2. Choose the **command (STDIO)** type and give it a name (e.g. `DaxFormatter`).
3. Set the command to `npx`, then use **Add argument** to add the two arguments separately: first `-y`, then `@sqlbi/daxformatter-mcp`.
4. Click **Save**.

The `format_dax` tool is now available in your threads. The app and the CLI share the same settings, so this also covers the **Codex CLI** and IDE extension.

<details>
<summary>Advanced: add it from the Codex terminal, or edit the config file by hand</summary>

Add it from the terminal:

```bash
# add it to the user-level Codex configuration (available in every project)
codex mcp add DaxFormatter -- npx -y @sqlbi/daxformatter-mcp

# check it was added
codex mcp get DaxFormatter
codex mcp list
```

Both the app and the CLI write the same entry to `~/.codex/config.toml` (Windows: `%USERPROFILE%\.codex\config.toml`), which you can also edit by hand:

```toml
[mcp_servers.DaxFormatter]
command = "npx"
args = ["-y", "@sqlbi/daxformatter-mcp"]
```

To scope the server to one repository, put the same table in a project-scoped `.codex/config.toml` at the repo root instead. Codex loads it only for **trusted** projects and prompts for trust on first use.
</details>

### Any MCP client

Don't see your app above? Most MCP-aware tools read a standard JSON config. Add this entry to your client's configuration file — or to a `.mcp.json` at the root of a project to share it with everyone working on that repo:

```json
{
  "mcpServers": {
    "DaxFormatter": {
      "command": "npx",
      "args": ["-y", "@sqlbi/daxformatter-mcp"]
    }
  }
}
```

## What it does

The server exposes a single tool, **`format_dax`** — just ask your assistant to format or check some DAX and it will use it:

- Formats one or more DAX expressions in a single call.
- Reports syntax errors with their line and column, so the same call both **formats** and **validates** (an expression that can't be parsed comes back unformatted, with the errors).
- Optional formatting controls: line style, spacing, and the list/decimal separators.

## Privacy

Your DAX is sent to the DAX Formatter web service (`daxformatter.com`), where the formatting happens. If you provide server or database names, they are **SHA-256 hashed** before being sent — they never leave your machine in clear text.

## License

[MIT](https://github.com/sql-bi/DaxFormatter/blob/master/LICENSE.md)
