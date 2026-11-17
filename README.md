# DAX Formatter

[DAX Formatter](https://www.daxformatter.com) is a service by SQLBI that formats DAX expressions following the [rules for DAX code formatting](https://www.sqlbi.com/articles/rules-for-dax-code-formatting/) and reports their syntax errors. This repository holds the packages that call it from your own code and from AI clients.

## Packages

| Package | Published as | What it does |
|---|---|---|
| [.NET client library](dotnet/README.md) | [`Dax.Formatter`](https://www.nuget.org/packages/Dax.Formatter) on NuGet | Formats DAX from .NET applications |
| [MCP server](typescript/packages/daxformatter-mcp/README.md) | [`@sqlbi/daxformatter-mcp`](https://www.npmjs.com/package/@sqlbi/daxformatter-mcp) on npm | Formats DAX from AI clients that support the Model Context Protocol, running locally over stdio |

The service also hosts a remote MCP server, which needs nothing installed.

## Documentation

- User documentation: [SQLBI Docs](https://docs.sqlbi.com/dax-formatter/).
- Connecting an AI client: to the [remote MCP server](https://docs.sqlbi.com/dax-formatter/mcp-http) or to the [stdio MCP server](https://docs.sqlbi.com/dax-formatter/mcp-stdio).
- For contributors and maintainers: [`docs/`](docs/README.md).

## Support

Report bugs and request features in [GitHub Issues](https://github.com/sql-bi/DaxFormatter/issues). Report security vulnerabilities privately, as the [security policy](SECURITY.md) explains.

## Contributing

See [`CONTRIBUTING.md`](CONTRIBUTING.md).

## License

[MIT](LICENSE.md)
