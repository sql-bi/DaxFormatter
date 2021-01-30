using System.Threading.Tasks;
using Xunit;

namespace Dax.Formatter.Tests
{
    public class DaxFormatterClientTests
    {
        [Theory]
        [InlineData("evaluate('Table')", "EVALUATE\r\n( 'Table' )\r\n")]
        [InlineData("evaluate('Table') order by 'Table'[Column]", "EVALUATE\r\n( 'Table' )\r\nORDER BY 'Table'[Column]\r\n")]
        public async Task DaxFormatterClient_FormatAsync_ComputeCorrectResult(string value, string expected)
        {
            var response = await DaxFormatterClient.FormatAsync(value);
            var actual = response.Formatted;

            Assert.Equal(actual, expected);
        }
    }
}
