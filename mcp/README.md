# @sqlbi/daxformatter-mcp

A [Model Context Protocol](https://modelcontextprotocol.io) (MCP) server that formats and validates DAX from any MCP-enabled AI client — Claude Code, Claude Desktop, Cursor, VS Code, and others — using the SQLBI DAX Formatter service.

Local **stdio** server, run on demand via `npx`. Requires [Node.js](https://nodejs.org) 18+.

## Installation guide

The server is **local, stdio, anonymous** — no install step, no API key, no login. It exposes one tool, `format_dax`. After connecting, just ask the agent to format DAX.

### Visual Studio Code

[![Install in VS Code](https://img.shields.io/badge/VS_Code-Install_DAX_Formatter-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect/mcp/install?name=DaxFormatter&config=%7B%22type%22%3A%22stdio%22%2C%22command%22%3A%22npx%22%2C%22args%22%3A%5B%22-y%22%2C%22%40sqlbi%2Fdaxformatter-mcp%22%5D%7D)
[![Install in VS Code Insiders](https://img.shields.io/badge/VS_Code_Insiders-Install_DAX_Formatter-24bfa5?style=flat-square&logo=visualstudiocode&logoColor=white)](https://insiders.vscode.dev/redirect/mcp/install?name=DaxFormatter&config=%7B%22type%22%3A%22stdio%22%2C%22command%22%3A%22npx%22%2C%22args%22%3A%5B%22-y%22%2C%22%40sqlbi%2Fdaxformatter-mcp%22%5D%7D&quality=insiders)

Click a badge to install (VS Code asks for confirmation), or add it from the command line:

```bash
code --add-mcp '{"name":"DaxFormatter","type":"stdio","command":"npx","args":["-y","@sqlbi/daxformatter-mcp"]}'
```

### Claude Code (CLI)

```bash
# add the server (-s user enables it in every project; omit -s for the current project only)
claude mcp add -s user DaxFormatter -- npx -y @sqlbi/daxformatter-mcp

# verify it was added and check the connection status
claude mcp get DaxFormatter
```

### Claude Desktop / other clients (config file)

Add this to the client's MCP configuration file (e.g. `claude_desktop_config.json`, or a `.mcp.json` at the repo root):

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

No separate install step: `npx` downloads and runs the server on demand.

## What it does

The server exposes a single tool, **`format_dax`** — just ask your assistant to format or check some DAX and it will use it:

- Formats one or more DAX expressions in a single call.
- Reports syntax errors with their line and column, so the same call both **formats** and **validates** (an expression that can't be parsed comes back unformatted, with the errors).
- Optional formatting controls: line style, spacing, and the list/decimal separators.

## Privacy

Your DAX is sent to the DAX Formatter web service (`daxformatter.com`), where the formatting happens. If you provide server or database names, they are **SHA-256 hashed** before being sent — they never leave your machine in clear text.

## License

[MIT](https://github.com/sql-bi/DaxFormatter/blob/master/LICENSE.md)
