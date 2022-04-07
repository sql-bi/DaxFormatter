namespace Dax.Formatter.Tests.Models
{
    using Dax.Formatter.AnalysisServices;
    using Dax.Formatter.Models;
    using System.Text.Json;
    using Xunit;

    public class DaxFormatterSingleRequestTests
    {
        [Fact]
        public void DaxFormatterSingleRequest_SensitiveServerNameIsProtected()
        {
            var expected = "f4139cb312e198bedebf0d71198c27db761f5c4cab73da7dd2faea2672d082af";

            var request = new DaxFormatterSingleRequest()
            {
                ServerName = "MyServerName1"
            };

            Assert.Equal(expected, request.ServerName);
        }

        [Fact]
        public void DaxFormatterSingleRequest_SensitiveDatabaseNameIsProtected()
        {
            var expected = "6341a49658aeebdfbcc648aea92b6005b393d3f89205cf56166c5626c80b9863";

            var request = new DaxFormatterSingleRequest()
            {
                DatabaseName = "MyDatabaseName1"
            };

            Assert.Equal(expected, request.DatabaseName);
        }

        [Theory]
        [InlineData(ServerType.AnalysisServices, "SSAS")]
        [InlineData(ServerType.PowerBIDesktop, "PBI Desktop")]
        [InlineData(ServerType.PowerBIReportServer, "PBI Report Server")]
        [InlineData(ServerType.PowerPivot, "PowerPivot")]
        [InlineData(ServerType.SSDT, "SSDT")]
        public void DaxFormatterSingleRequest_ServerTypeSerialization(ServerType serverType, string expectedServerType)
        {
            var request = new DaxFormatterSingleRequest
            {
                ServerType = serverType
            };

            var property = GetJsonProperty(request, nameof(DaxFormatterSingleRequest.ServerType));
            var currentServerType = property.GetString();

            Assert.Equal(expectedServerType, currentServerType);
        }

        [Theory]
        [InlineData(DaxFormatterLineStyle.LongLine, 0)]
        [InlineData(DaxFormatterLineStyle.ShortLine, 1)]
        public void DaxFormatterSingleRequest_MaxLineLengthSerialization(DaxFormatterLineStyle lineStyle, int expectedLineStyle)
        {
            var request = new DaxFormatterSingleRequest
            {
                MaxLineLength = lineStyle
            };

            var property = GetJsonProperty(request, nameof(DaxFormatterSingleRequest.MaxLineLength));
            var currentLineStyle = property.GetInt32();

            Assert.Equal(expectedLineStyle, currentLineStyle);
        }


        [Theory]
        [InlineData(DaxFormatterSpacingStyle.NoSpaceAfterFunction, true)]
        [InlineData(DaxFormatterSpacingStyle.SpaceAfterFunction, false)]
        public void DaxFormatterSingleRequest_SkipSpaceAfterFunctionNameSerialization(DaxFormatterSpacingStyle spacingStyle, bool expectedSpacingStyle)
        {
            var request = new DaxFormatterSingleRequest
            {
                SkipSpaceAfterFunctionName = spacingStyle
            };

            var property = GetJsonProperty(request, nameof(DaxFormatterSingleRequest.SkipSpaceAfterFunctionName));
            var currentSpacingStyle = property.GetBoolean();

            Assert.Equal(expectedSpacingStyle, currentSpacingStyle);
        }

        private static JsonElement GetJsonProperty(DaxFormatterSingleRequest request, string propertyName)
        {
            var json = JsonSerializer.Serialize(request);
            var document = JsonDocument.Parse(json);
            var property = document.RootElement.GetProperty(propertyName);

            return property;
        }
    }
}
