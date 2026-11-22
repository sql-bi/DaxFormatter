using System.Net;
using System.Net.Http;
using Dax.Formatter.Tests.Fakes;
using Xunit;

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
        // EVALUATE
        // FILTER ( Customer, Customer[Country] = "Italy" )
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
    public async Task FormatBatchAsync_MixedObjectsSample_UpdatesTheObjectsOfEveryType()
    {
        var client = new DaxFormatterClient();
        var model = new Model();

        #region MixedObjects
        var items = new List<ModelExpression>();

        foreach (var measure in model.Measures)
        {
            items.Add(new(measure.Name, measure.Expression, text => measure.Expression = text));
        }

        foreach (var function in model.Functions)
        {
            items.Add(new(function.Name, function.Expression, text => function.Expression = text));
        }

        var options = new DaxFormatOptions { Kind = DaxKind.Expression };

        var results = await client.FormatBatchAsync(items, item => item.Expression, options);

        foreach (var result in results)
        {
            if (result.Succeeded)
            {
                result.Item.Update(result.FormattedText);
            }
            else
            {
                Console.WriteLine($"{result.Item.Name}: {result.Errors[0].Message}");
            }
        }
        #endregion

        Assert.Equal("CALCULATE ( SUM ( Sales[Amount] ), Sales[Quantity] > 0 )", model.Measures[0].Expression);
        Assert.Equal(
            "(\r\n    amount : NUMERIC,\r\n    rate : NUMERIC\r\n) =>\r\namount * ( 1 + rate )", model.Functions[0].Expression);
        Assert.Equal("Broken", results.Single(result => !result.Succeeded).Item.Name);
    }

    #region MixedObjectsType
    sealed record ModelExpression(string Name, string Expression, Action<string> Update);
    #endregion

    [Fact]
    public async Task FormatAsync_OptionsSample_SendsTheOptions()
    {
        var client = new DaxFormatterClient();

        #region Options
        var options = new DaxFormatOptions
        {
            Kind = DaxKind.Expression,
            LineStyle = DaxLineStyle.ShortLine,
            SpacingStyle = DaxSpacingStyle.NoSpaceAfterFunction,
            SeparatorStyle = DaxSeparatorStyle.SemicolonAndComma,
            ModelInfo = new DaxModelInfo
            {
                ServerType = DaxServerType.PowerBIDesktop,
                DatabaseName = "Contoso",
            },
        };

        DaxFormatResult result = await client.FormatAsync("DIVIDE([Sales Amount]; 1,5)", options);
        #endregion

        var body = ReadmeService.Handler.Requests.Last(request => request.Dax[0].EndsWith("1,5)", StringComparison.Ordinal)).Json;
        Assert.True(result.Succeeded);
        Assert.Equal((1, true), (body.GetProperty("maxLineLength").GetInt32(), body.GetProperty("skipSpaceAfterFunctionName").GetBoolean()));
        Assert.Equal((";", ","), (body.GetProperty("listSeparator").GetString(), body.GetProperty("decimalSeparator").GetString()));
    }

    [Fact]
    public async Task FormatAsync_SyntaxErrorsSample_ReadsThePositionOfTheError()
    {
        var client = new DaxFormatterClient();
        var options = new DaxFormatOptions { Kind = DaxKind.Expression };

        #region SyntaxErrors
        DaxFormatResult result = await client.FormatAsync("SUMX(Sales, Sales[Amount] *", options);

        if (!result.Succeeded)
        {
            foreach (DaxSyntaxError error in result.Errors)
            {
                Console.WriteLine($"Line {error.Line + 1}, column {error.Column + 1}: {error.Message}");
            }
        }
        #endregion

        Assert.Equal((0, 27), (result.Errors[0].Line, result.Errors[0].Column));
    }

    [Fact]
    public async Task FormatAsync_FunctionsSample_FormatsTheBodyOfTheFunction()
    {
        var client = new DaxFormatterClient();

        #region Functions
        var options = new DaxFormatOptions { Kind = DaxKind.Expression };

        DaxFormatResult result = await client.FormatAsync(
            "(amount : NUMERIC, rate : NUMERIC) => amount * (1 + rate)", options);

        Console.WriteLine(result.FormattedText);
        // (
        //     amount : NUMERIC,
        //     rate : NUMERIC
        // ) =>
        // amount * ( 1 + rate )
        #endregion

        Assert.Equal(
            "(\r\n    amount : NUMERIC,\r\n    rate : NUMERIC\r\n) =>\r\namount * ( 1 + rate )", result.FormattedText);
    }

    [Fact]
    public async Task FormatBatchAsync_ProgressSample_ReturnsEveryResult()
    {
        var client = new DaxFormatterClient();
        var options = new DaxFormatOptions { Kind = DaxKind.Expression };
        var expressions = Enumerable.Range(1, 600).Select(number => "[Sales] * " + number).ToList();
        using var cancellation = new CancellationTokenSource();
        var cancellationToken = cancellation.Token;

        #region Progress
        var progress = new Progress<DaxFormatProgressInfo>(
            info => Console.WriteLine($"{info.Completed} of {info.Total} expressions formatted"));

        IReadOnlyList<DaxFormatResult> results =
            await client.FormatBatchAsync(expressions, options, progress, cancellationToken);
        #endregion

        Assert.Equal(expressions, results.Select(result => result.FormattedText));
    }

    [Fact]
    public async Task FormatBatchAsync_FailuresSample_CatchesTheFailureOfTheService()
    {
        using var handler = new FakeFormatterHandler(Respond.Status(HttpStatusCode.BadRequest));
        using var httpClient = new HttpClient(handler);
        var client = new DaxFormatterClient(httpClient);
        string[] expressions = ["[Sales] * 2"];

        #region Failures
        try
        {
            IReadOnlyList<DaxFormatResult> results = await client.FormatBatchAsync(expressions);
            Console.WriteLine($"{results.Count} expressions formatted");
        }
        catch (DaxFormatterException exception)
        {
            Console.WriteLine($"{exception.Reason}: {exception.Message}");
            Console.WriteLine($"Request {exception.RequestId}, status {exception.StatusCode}");
        }
        #endregion

        Assert.Single(handler.Requests);
    }

    [Fact]
    public async Task Constructor_ClientOptionsSample_IdentifiesTheApplication()
    {
        #region ClientOptions
        var client = new DaxFormatterClient(new DaxFormatterClientOptions
        {
            ApplicationName = "Contoso Model Tools",
            ApplicationVersion = "1.4.0",
            RequestTimeout = TimeSpan.FromSeconds(120),
            MaxRetries = 3,
            MaxConcurrentRequests = 4,
        });
        #endregion

        await client.FormatAsync("EVALUATE Tools");

        var body = ReadmeService.Handler.Requests.Last(request => request.Dax[0] == "EVALUATE Tools").Json;
        Assert.Equal("Contoso Model Tools", body.GetProperty("callerApp").GetString());
    }

    [Fact]
    public async Task Constructor_HttpClientSample_SendsTheRequestsWithIt()
    {
        using var handler = new FakeFormatterHandler(Respond.Echo);
        using var httpClient = new HttpClient(handler);

        #region HttpClient
        var client = new DaxFormatterClient(httpClient);
        #endregion

        await client.FormatAsync("EVALUATE Sales");

        Assert.Single(handler.Requests);
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

        public List<Function> Functions { get; } =
        [
            new Function("AddRate", "(amount : NUMERIC, rate : NUMERIC) => amount * (1 + rate)"),
        ];
    }

    private sealed class Function
    {
        public Function(string name, string expression)
        {
            Name = name;
            Expression = expression;
        }

        public string Name { get; }

        public string Expression { get; set; }
    }
}
