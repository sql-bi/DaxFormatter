namespace Dax.Formatter.Tests.Models
{
    using Dax.Formatter.Models;
    using System.Text.Json;
    using Xunit;

    public class DaxFormatterMultipleRequestTests
    {
        [Fact]
        public void DaxFormatterMultipleRequest_DaxIsNotNull()
        {
            var request = new DaxFormatterMultipleRequest();

            Assert.NotNull(request.Dax);
        }

        [Fact]
        public void DaxFormatterMultipleRequest_SensitiveServerNameIsProtected()
        {
            var expected = "f4139cb312e198bedebf0d71198c27db761f5c4cab73da7dd2faea2672d082af";

            var request = new DaxFormatterMultipleRequest()
            {
                ServerName = "MyServerName1"
            };

            Assert.Equal(expected, request.ServerName);
        }

        [Fact]
        public void DaxFormatterMultipleRequest_SensitiveDatabaseNameIsProtected()
        {
            var expected = "6341a49658aeebdfbcc648aea92b6005b393d3f89205cf56166c5626c80b9863";

            var request = new DaxFormatterMultipleRequest()
            {
                DatabaseName = "MyDatabaseName1"
            };

            Assert.Equal(expected, request.DatabaseName);
        }

        [Fact]
        public void DaxFormatterMultipleRequest_ServerTypeSerialization()
        {
            var expected = "PBI Report Server";

            var request = new DaxFormatterMultipleRequest()
            {
                ServerType = ServerType.PowerBIReportServer
            };

            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                Converters = { new CustomJsonStringEnumConverter(allowIntegerValues:false) },
                IgnoreNullValues = true
            };

            var serialized = JsonSerializer.Serialize(request, options);

            Assert.Contains(expected, serialized);
        }
    }
}
