﻿using Xunit;

namespace Dax.Formatter.Tests.Readme;

// In this file the name of the client means a client that answers from a fake service: see ReadmeClient.
// The alias must stay inside the namespace: outside it, the real type in the parent namespace would win.
using DaxFormatterClient = ReadmeClient;

// The regions are the code of the readme, written for applications: ConfigureAwait would be noise there.
#pragma warning disable CA2007

/// <summary>
/// The C# code of the package readme. Each region is one code block of <c>dotnet/README.md</c>, character for
/// character: <see cref="ReadmeTests"/> compares them, and the tests here run them.
/// </summary>
public class ReadmeSamples
{
    [Fact]
    public async Task FormatAsync_GettingStartedSample_FormatsTheQuery()
    {
        #region GettingStarted
        var client = new DaxFormatterClient();

        DaxFormatResult result = await client.FormatAsync("EVALUATE FILTER(Customer, Customer[Country] = \"Italy\")");

        Console.WriteLine(result.FormattedText);
        #endregion

        Assert.Equal("EVALUATE\r\nFILTER ( Customer, Customer[Country] = \"Italy\" )", result.FormattedText);
    }

    [Fact]
    public async Task FormatBatchAsync_ModelObjectsSample_UpdatesTheMeasuresThatAreValid()
    {
        var client = new DaxFormatterClient();
        var model = new Model();

        #region ModelObjects
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
        #endregion

        Assert.Equal("CALCULATE ( SUM ( Sales[Amount] ), Sales[Quantity] > 0 )", model.Measures[0].Expression);
        Assert.Equal("SUMX(Sales, Sales[Amount] *", model.Measures[1].Expression);
        Assert.Equal("DIVIDE ( [Sales Amount], [Total Cost] )", model.Measures[2].Expression);
    }

    [Fact]
    public async Task FormatAsync_ModelInfoSample_SendsTheDescriptionOfTheModel()
    {
        var client = new DaxFormatterClient();

        #region ModelInfo
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
        #endregion

        await client.FormatAsync("EVALUATE ModelInfo", options);

        var body = ReadmeService.Handler.Requests.Last(request => request.Dax[0] == "EVALUATE ModelInfo").Json;
        Assert.Equal(
            ("SSAS", "Tabular", "Enterprise64", "OnPremise"),
            (body.GetProperty("serverType").GetString(), body.GetProperty("serverMode").GetString(),
                body.GetProperty("serverEdition").GetString(), body.GetProperty("serverLocation").GetString()));
        Assert.Equal(
            "5e3e900585e7834b034d4f17187e521b3c7b7de0d1637f79de4b63717d4f0783",
            body.GetProperty("serverName").GetString());
    }

    [Fact]
    public async Task FormatAsync_OptionsSample_SendsTheOptions()
    {
        var client = new DaxFormatterClient();

        #region Options
        var options = new DaxFormatOptions
        {
            LineStyle = DaxLineStyle.ShortLine,
            SpacingStyle = DaxSpacingStyle.NoSpaceAfterFunction,
            SeparatorStyle = DaxSeparatorStyle.SemicolonAndComma,
        };
        #endregion

        await client.FormatAsync("EVALUATE Options", options);

        var body = ReadmeService.Handler.Requests.Last(request => request.Dax[0] == "EVALUATE Options").Json;
        Assert.Equal((1, true), (body.GetProperty("maxLineLength").GetInt32(), body.GetProperty("skipSpaceAfterFunctionName").GetBoolean()));
        Assert.Equal((";", ","), (body.GetProperty("listSeparator").GetString(), body.GetProperty("decimalSeparator").GetString()));
    }

    private sealed class Measure
    {
        public Measure(string name, string expression)
        {
            Name = name;
            Expression = expression;
        }

        public string Name { get; }

        public string Expression { get; set; }
    }

    private sealed class Model
    {
        public List<Measure> Measures { get; } =
        [
            new Measure("Sales", "CALCULATE(SUM(Sales[Amount]), Sales[Quantity] > 0)"),
            new Measure("Broken", "SUMX(Sales, Sales[Amount] *"),
            new Measure("Cost Ratio", "DIVIDE([Sales Amount], [Total Cost])"),
        ];
    }
}
