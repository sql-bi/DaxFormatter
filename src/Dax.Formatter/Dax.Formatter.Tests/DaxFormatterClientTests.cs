namespace Dax.Formatter.Tests
{
    using System.Linq;
    using System.Threading.Tasks;
    using Xunit;

    public class DaxFormatterClientTests
    {
        [Theory]
        [InlineData("evaluate('Table')", "EVALUATE\r\n( 'Table' )\r\n")]
        [InlineData("evaluate('Table') order by 'Table'[Column]", "EVALUATE\r\n( 'Table' )\r\nORDER BY 'Table'[Column]\r\n")]
        [InlineData("[X] := CALCULATE(SUM(Sales[Sales Amount]), USERELATIONSHIP(Sales[DueDateKey],'Date'[DateKey]))", "[X] :=\r\nCALCULATE (\r\n    SUM ( Sales[Sales Amount] ),\r\n    USERELATIONSHIP ( Sales[DueDateKey], 'Date'[DateKey] )\r\n)\r\n")]
        public async Task DaxFormatterClient_FormatAsync_SingleExpression(string expression, string expectedExpression)
        {
            var responses = await DaxFormatterClient.FormatAsync(expression);

            Assert.Single(responses);

            var response = responses.Single();
            var actualExpression = response.Formatted;

            Assert.Equal(expectedExpression, actualExpression);
        }

        [Theory]
        [InlineData("evaluate('Table')", "EVALUATE\r\n( 'Table' )\r\n", 10)]
        public async Task DaxFormatterClient_FormatAsync_MultipleParallelCalls(string expression, string expectedExpression, int repeatCount)
        {
            var tasks = Enumerable.Repeat(expression, repeatCount).AsParallel().Select((e) => DaxFormatterClient.FormatAsync(e));
            var responses = await Task.WhenAll(tasks);
            var results = responses.SelectMany((r) => r).ToList();

            Assert.NotEmpty(results);
            Assert.Equal(repeatCount, results.Count);

            var errors = results.SelectMany((r) => r.Errors);
            Assert.Empty(errors);

            var formattedDistinct = results.Select((r) => r.Formatted).Distinct();
            Assert.Single(formattedDistinct);

            var actualExpression = formattedDistinct.Single();
            Assert.Equal(expectedExpression, actualExpression);
        }

        [Theory]
        [InlineData("evaluate('Table')", "EVALUATE\r\n( 'Table' )\r\n", 10)]
        public async Task DaxFormatterClient_FormatAsync_SingleBatchCall(string expression, string expectedExpression, int repeatCount)
        {
            var expressions = Enumerable.Repeat(expression, repeatCount);

            var response = await DaxFormatterClient.FormatAsync(expressions);
            var results = response.Select((r) => r).ToList();

            Assert.NotEmpty(results);
            Assert.Equal(repeatCount, results.Count);

            var errors = results.SelectMany((r) => r.Errors);
            Assert.Empty(errors);

            var formattedDistinct = results.Select((r) => r.Formatted).Distinct();
            Assert.Single(formattedDistinct);

            var actualExpression = formattedDistinct.Single();
            Assert.Equal(expectedExpression, actualExpression);
        }

        [Theory]
        [InlineData("EVALUATE( Ta ble )", 0, 13)]
        [InlineData("EVALUATE( Table ) ORDER Table[Column]", 0, 24)]
        public async Task DaxFormatterClient_FormatAsync_SingleExpressionWithErrors(string expression, int expectedErrorLine, int expectedErrorColumn)
        {
            var responses = await DaxFormatterClient.FormatAsync(expression);

            Assert.Single(responses);

            var response = responses.Single();

            Assert.Equal(string.Empty, response.Formatted);
            Assert.Single(response.Errors);

            var error = response.Errors.Single();

            Assert.Equal(expectedErrorColumn, error.Column);
            Assert.Equal(expectedErrorLine, error.Line);
        }
    }
}
