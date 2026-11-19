# Contributing

Thank you for helping with DAX Formatter. This repository holds the client packages; the DAX Formatter service itself is maintained elsewhere, so a problem with a formatting result is reported here as an issue, not fixed with a pull request.

## Issues

- Report a bug or request a feature in [GitHub Issues](https://github.com/sql-bi/DaxFormatter/issues), with the form that fits.
- Report a security vulnerability privately, as the [security policy](SECURITY.md) explains, never in a public issue.
- For a change larger than a fix, open an issue first, so that the approach is agreed before the work starts.

## Build and test

Each language folder builds and tests on its own: the commands and their requirements are in [`dotnet/AGENTS.md`](dotnet/AGENTS.md) and [`typescript/AGENTS.md`](typescript/AGENTS.md).

## Commits and pull requests

- Commit messages follow [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/): `type(scope): summary`, in the imperative, lowercase start, at most 72 characters, for example `docs(mcp): link client setup on SQLBI Docs`. Usual types: `feat`, `fix`, `docs`, `refactor`, `test`, `build` (also dependencies, as Dependabot writes them), `ci`, `chore`.
- One logical change per commit; the body says what changed and why, in `-` bullets wrapped at 72 columns.
- Pull requests are squash-merged into `master`, so the pull request title becomes the commit subject: write it as a Conventional Commits subject. The description is plain text with `-` bullets, without headings or tables.

## License

The repository is licensed under the [MIT license](LICENSE.md).
