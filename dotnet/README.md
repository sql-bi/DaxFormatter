# Dax.Formatter [![CI](https://github.com/sql-bi/DaxFormatter/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/sql-bi/DaxFormatter/actions/workflows/dotnet-ci.yml)

`Dax.Formatter` is the .NET client of [DAX Formatter](https://www.daxformatter.com), the service by SQLBI that formats DAX code and reports its syntax errors.

## Install

```bash
dotnet add package Dax.Formatter
```

## Format a query

```csharp
var client = new DaxFormatterClient();

DaxFormatResult result = await client.FormatAsync("EVALUATE FILTER(Customer, Customer[Country] = \"Italy\")");

Console.WriteLine(result.FormattedText);
// EVALUATE
// FILTER ( Customer, Customer[Country] = "Italy" )
```

Create one client and reuse it: it is thread-safe.

## Format the measures of a model

Pass your objects and a function that returns the DAX of each one. Each result carries its object in `Item`.

```csharp
var options = new DaxFormatOptions { Kind = DaxKind.Expression };

IReadOnlyList<DaxFormatResult<Measure>> results =
    await client.FormatBatchAsync(model.Measures, measure => measure.Expression, options);

foreach (var result in results)
{
    if (result.Succeeded)
    {
        result.Item.Expression = result.FormattedText;
    }
    else
    {
        Console.WriteLine($"{result.Item.Name}: {result.Errors[0].Message}");
    }
}
```

- Use `DaxKind.Expression` for the DAX of a model object: a measure, a calculated column or table, a calculation item, a user-defined function. Use the default, `DaxKind.Auto`, for queries.
- One call takes thousands of texts. The results come back in the order of the input.

## Describe your model

Optionally, describe the model your DAX comes from. The data is anonymized, and helps SQLBI understand how DAX Formatter is used and improve it.

```csharp
var options = new DaxFormatOptions
{
    ModelInfo = new DaxModelInfo
    {
        ServerName = "MyServer",
        DatabaseName = "Contoso",
        ServerType = DaxServerType.AnalysisServices,
        ServerMode = DaxServerMode.Tabular,
        ServerEdition = DaxServerEdition.Enterprise64,
        ServerLocation = DaxServerLocation.OnPremise,
        ServerVersion = "16.0.143.21",
        CompatibilityLevel = 1600,
    },
};
```

- `ModelInfo` doesn't change the formatting.
- `ServerName` and `DatabaseName` leave your machine only as SHA-256 hashes, never as names.
- `ServerMode`, `ServerEdition` and `ServerLocation` have the names of the TOM enums: convert them by name, such as `Enum.Parse<DaxServerEdition>(server.Edition.ToString())`.

## Formatting options

```csharp
var options = new DaxFormatOptions
{
    LineStyle = DaxLineStyle.ShortLine,
    SpacingStyle = DaxSpacingStyle.NoSpaceAfterFunction,
    SeparatorStyle = DaxSeparatorStyle.SemicolonAndComma,
};
```

- `LineStyle`: `LongLine` (default) keeps an expression on one line while it fits, `ShortLine` puts each argument on its own line.
- `SpacingStyle`: `SpaceAfterFunction` (default) writes `SUM ( x )`, `NoSpaceAfterFunction` writes `SUM( x )`.
- `SeparatorStyle`: `CommaAndPoint` (default) for `DIVIDE ( 1.5, 2 )`, `SemicolonAndComma` for `DIVIDE ( 1,5; 2 )`.

## Errors

- A syntax error doesn't throw: `Succeeded` is `false`, and `Errors` lists each error with its `Message` and its zero-based `Line` and `Column`.
- When the service or the network fails, after a few retries the call throws `DaxFormatterException`.

## Feedback

Report bugs and request features in [GitHub Issues](https://github.com/sql-bi/DaxFormatter/issues).
