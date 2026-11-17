# @sqlbi/daxformatter-mcp

A [Model Context Protocol](https://modelcontextprotocol.io) (MCP) server that formats and validates DAX from any MCP-enabled AI client — Claude Code, Claude Desktop, Cursor, VS Code, and others — using the SQLBI DAX Formatter service.

## Get started

The server runs **locally on your machine**, over stdio, and needs Node.js 18 or later — no install step, no account, no API key, no login. Your AI client starts it with this command:

```bash
npx -y @sqlbi/daxformatter-mcp
```

To add it to VS Code, Claude, Codex or any other MCP client, follow the steps on [SQLBI Docs](https://docs.sqlbi.com/dax-formatter/mcp-stdio).

To run nothing locally, connect your client to the [remote DAX Formatter MCP server](https://docs.sqlbi.com/dax-formatter/mcp-http) instead.

## What it does

The server exposes a single tool, **`format_dax`** — just ask your assistant to format or check some DAX and it will use it:

- Formats one or more DAX expressions in a single call.
- Reports syntax errors with their line and column, so the same call both **formats** and **validates** (an expression that can't be parsed comes back with an empty `formatted` and its errors).
- Optional formatting controls: line style, spacing, and the list/decimal separators.

## Privacy

Your DAX is sent to the DAX Formatter web service (`daxformatter.com`), where the formatting happens. If you provide server or database names, they are **SHA-256 hashed** before being sent — they never leave your machine in clear text.

## License

[MIT](https://github.com/sql-bi/DaxFormatter/blob/master/LICENSE.md)
