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

        [Fact]
        public void DaxFormatterSingleRequest_ServerTypeSerialization()
        {
            var expected = "PBI Report Server";

            var request = new DaxFormatterSingleRequest()
            {
                ServerType = ServerType.PowerBIReportServer
            };

            var serialized = JsonSerializer.Serialize(request);

            Assert.Contains(expected, serialized);
        }
    }
}
