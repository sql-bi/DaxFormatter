namespace Dax.Formatter.Tests
{
    using Dax.Formatter.Models;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class DaxFormatterClientFixture
    {
        public DaxFormatterClient Client { get; private set; }

        public DaxFormatterClientFixture() => Client = new DaxFormatterClient();
    }

    public class DaxFormatterClientTests : IClassFixture<DaxFormatterClientFixture>
    {
        private readonly DaxFormatterClientFixture _fixture;

        public DaxFormatterClientTests(DaxFormatterClientFixture formatter)
        {
            _fixture = formatter;
        }

        [Theory]
        [InlineData("evaluate('Table')", "EVALUATE\r\n( 'Table' )\r\n")]
        [InlineData("evaluate('Table') order by 'Table'[Column]", "EVALUATE\r\n( 'Table' )\r\nORDER BY 'Table'[Column]\r\n")]
        [InlineData("[X] := CALCULATE(SUM(Sales[Sales Amount]), USERELATIONSHIP(Sales[DueDateKey],'Date'[DateKey]))", "[X] :=\r\nCALCULATE (\r\n    SUM ( Sales[Sales Amount] ),\r\n    USERELATIONSHIP ( Sales[DueDateKey], 'Date'[DateKey] )\r\n)\r\n")]
        public async Task DaxFormatterClient_FormatAsync_SingleExpressionSucceded(string expression, string expectedExpression)
        {
            var response = await _fixture.Client.FormatAsync(expression);

            AssertSucceded(response, expectedExpression);
        }

        [Theory]
        [InlineData("EVALUATE( Ta ble )", 0, 13)]
        [InlineData("EVALUATE( Table ) ORDER Table[Column]", 0, 24)]
        public async Task DaxFormatterClient_FormatAsync_SingleExpressionFailsWithError(string expression, int expectedErrorLine, int expectedErrorColumn)
        {
            var response = await _fixture.Client.FormatAsync(expression);

            AssertFailsWithError(response, expectedErrorLine, expectedErrorColumn);
        }

        [Theory]
        [InlineData("evaluate('Table')", "EVALUATE\r\n( 'Table' )\r\n")]
        [InlineData("evaluate('Table') order by 'Table'[Column]", "EVALUATE\r\n( 'Table' )\r\nORDER BY 'Table'[Column]\r\n")]
        [InlineData("[X] := CALCULATE(SUM(Sales[Sales Amount]), USERELATIONSHIP(Sales[DueDateKey],'Date'[DateKey]))", "[X] :=\r\nCALCULATE (\r\n    SUM ( Sales[Sales Amount] ),\r\n    USERELATIONSHIP ( Sales[DueDateKey], 'Date'[DateKey] )\r\n)\r\n")]
        public async Task DaxFormatterClient_FormatAsync_SingleRequestSucceded(string expression, string expectedExpression)
        {
            var request = new DaxFormatterSingleRequest();
            request.Dax = expression;

            var response = await _fixture.Client.FormatAsync(request);

            AssertSucceded(response, expectedExpression);
        }

        [Theory]
        [InlineData("evaluate('Table')", "EVALUATE\r\n( 'Table' )\r\n", 10)]
        [InlineData("evaluate('Table') order by 'Table'[Column]", "EVALUATE\r\n( 'Table' )\r\nORDER BY 'Table'[Column]\r\n", 10)]
        [InlineData("[X] := CALCULATE(SUM(Sales[Sales Amount]), USERELATIONSHIP(Sales[DueDateKey],'Date'[DateKey]))", "[X] :=\r\nCALCULATE (\r\n    SUM ( Sales[Sales Amount] ),\r\n    USERELATIONSHIP ( Sales[DueDateKey], 'Date'[DateKey] )\r\n)\r\n", 10)]
        public async Task DaxFormatterClient_FormatAsync_MultipleRequestSucceded(string expression, string expectedExpression, int repeat)
        {
            var expressions = Enumerable.Repeat(expression, repeat);
            var request = new DaxFormatterMultipleRequest();
            request.Dax.AddRange(expressions);

            var response = await _fixture.Client.FormatAsync(request);

            AssertSingleBatchSucceded(response, expectedExpression, repeat);
        }
        
        [Theory]
        [InlineData("EVALUATE( Ta ble )", 0, 13)]
        [InlineData("EVALUATE( Table ) ORDER Table[Column]", 0, 24)]
        public async Task DaxFormatterClient_FormatAsync_SingleRequestFailsWithError(string expression, int expectedErrorLine, int expectedErrorColumn)
        {
            var request = new DaxFormatterSingleRequest();
            request.Dax = expression;

            var response = await _fixture.Client.FormatAsync(request);

            AssertFailsWithError(response, expectedErrorLine, expectedErrorColumn);
        }

        [Theory]
        [InlineData("evaluate('Table')", "EVALUATE\r\n( 'Table' )\r\n", 10)]
        [InlineData("evaluate('Table') order by 'Table'[Column]", "EVALUATE\r\n( 'Table' )\r\nORDER BY 'Table'[Column]\r\n", 10)]
        [InlineData("[X] := CALCULATE(SUM(Sales[Sales Amount]), USERELATIONSHIP(Sales[DueDateKey],'Date'[DateKey]))", "[X] :=\r\nCALCULATE (\r\n    SUM ( Sales[Sales Amount] ),\r\n    USERELATIONSHIP ( Sales[DueDateKey], 'Date'[DateKey] )\r\n)\r\n", 10)]
        public async Task DaxFormatterClient_FormatAsync_SingleBatchExpressionSucceded(string expression, string expectedExpression, int repeat)
        {
            var expressions = Enumerable.Repeat(expression, repeat);
            var response = await _fixture.Client.FormatAsync(expressions);

            AssertSingleBatchSucceded(response, expectedExpression, repeat);
        }

        [Theory]
        [InlineData("evaluate('Table')", "EVALUATE\r\n( 'Table' )\r\n", 10)]
        [InlineData("evaluate('Table') order by 'Table'[Column]", "EVALUATE\r\n( 'Table' )\r\nORDER BY 'Table'[Column]\r\n", 10)]
        [InlineData("[X] := CALCULATE(SUM(Sales[Sales Amount]), USERELATIONSHIP(Sales[DueDateKey],'Date'[DateKey]))", "[X] :=\r\nCALCULATE (\r\n    SUM ( Sales[Sales Amount] ),\r\n    USERELATIONSHIP ( Sales[DueDateKey], 'Date'[DateKey] )\r\n)\r\n", 10)]
        public async Task DaxFormatterClient_FormatAsync_SingleBatchRequestSucceded(string expression, string expectedExpression, int repeat)
        {
            var expressions = Enumerable.Repeat(expression, repeat);
            var request = new DaxFormatterMultipleRequest();
            request.Dax.AddRange(expressions);

            var response = await _fixture.Client.FormatAsync(request);

            AssertSingleBatchSucceded(response, expectedExpression, repeat);
        }

        [Theory]
        [InlineData("evaluate('Table')", "EVALUATE\r\n( 'Table' )\r\n", 10)]
        public async Task DaxFormatterClient_FormatAsync_MultipleParallelExpressionSucceded(string expression, string expectedExpression, int repeat)
        {
            var tasks = Enumerable.Repeat(expression, repeat).AsParallel().Select((e) => _fixture.Client.FormatAsync(e));
            var responses = await Task.WhenAll(tasks);

            AssertMultipleParallelSucceded(responses, expectedExpression, repeat);
        }

        [Theory]
        [InlineData("evaluate('Table')", "EVALUATE\r\n( 'Table' )\r\n", 1)]
        public async Task DaxFormatterClient_FormatAsync_MultipleParallelRequestSucceded(string expression, string expectedExpression, int repeat)
        {
            var request = new DaxFormatterSingleRequest();
            request.Dax = expression;

            var tasks = Enumerable.Repeat(request, repeat).AsParallel().Select((r) => _fixture.Client.FormatAsync(r));
            var responses = await Task.WhenAll(tasks);

            AssertMultipleParallelSucceded(responses, expectedExpression, repeat);
        }

        private static void AssertSucceded(DaxFormatterMultipleResponse response, string expectedExpression)
        {
            Assert.NotNull(response);
            Assert.Single(response);

            var result = response.Single();
            Assert.NotEmpty(result.Formatted);
            Assert.Empty(result.Errors);

            var actualExpression = result.Formatted;
            Assert.Equal(expectedExpression, actualExpression);
        }

        private static void AssertSucceded(DaxFormatterSingleResponse response, string expectedExpression)
        {
            Assert.NotNull(response);

            Assert.NotEmpty(response.Formatted);
            Assert.Empty(response.Errors);

            var actualExpression = response.Formatted;
            Assert.Equal(expectedExpression, actualExpression);
        }

        private static void AssertFailsWithError(DaxFormatterSingleResponse response, int expectedErrorLine, int expectedErrorColumn)
        {
            Assert.NotNull(response);
            
            Assert.Empty(response.Formatted);
            Assert.Single(response.Errors);

            var actualError = response.Errors.Single();
            Assert.Equal(expectedErrorLine, actualError.Line);
            Assert.Equal(expectedErrorColumn, actualError.Column);
        }

        private static void AssertFailsWithError(DaxFormatterMultipleResponse response, int expectedErrorLine, int expectedErrorColumn)
        {
            Assert.NotNull(response);
            Assert.Single(response);

            var result = response.Single();
            Assert.Empty(result.Formatted);
            Assert.Single(result.Errors);

            var actualError = result.Errors.Single();
            Assert.Equal(expectedErrorLine, actualError.Line);
            Assert.Equal(expectedErrorColumn, actualError.Column);
        }

        private static void AssertSingleBatchSucceded(DaxFormatterMultipleResponse response, string expectedExpression, int repeat)
        {
            Assert.NotNull(response);

            var results = response.Select((r) => r).ToList();
            Assert.Equal(repeat, results.Count);

            var errors = results.SelectMany((r) => r.Errors);
            Assert.Empty(errors);

            var formattedResults = results.Select((r) => r.Formatted).Distinct();
            Assert.Single(formattedResults);

            var actualExpression = formattedResults.Single();
            Assert.Equal(expectedExpression, actualExpression);
        }

        private static void AssertMultipleParallelSucceded(IEnumerable<DaxFormatterSingleResponse> responses, string expectedExpression, int repeat)
        {
            var results = responses.ToList();
            Assert.NotEmpty(results);
            Assert.Equal(repeat, results.Count);

            var errors = results.SelectMany((r) => r.Errors);
            Assert.Empty(errors);

            var formattedResults = results.Select((r) => r.Formatted).Distinct();
            Assert.Single(formattedResults);

            var actualExpression = formattedResults.Single();
            Assert.Equal(expectedExpression, actualExpression);
        }

        private static void AssertMultipleParallelSucceded(IEnumerable<DaxFormatterMultipleResponse> responses, string expectedExpression, int repeat)
        {
            var results = responses.SelectMany((r) => r).ToList();
            Assert.NotEmpty(results);
            Assert.Equal(repeat, results.Count);

            var errors = results.SelectMany((r) => r.Errors);
            Assert.Empty(errors);

            var formattedResults = results.Select((r) => r.Formatted).Distinct();
            Assert.Single(formattedResults);

            var actualExpression = formattedResults.Single();
            Assert.Equal(expectedExpression, actualExpression);
        }
    }
}
