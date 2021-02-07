namespace Dax.Formatter.Tests.Models
{
    using Dax.Formatter.Models;
    using Xunit;

    public class DaxFormatterRequestTests
    {
        [Fact]
        public void DaxFormatterRequest_DaxIsNotNull()
        {
            var request = new DaxFormatterMultipleRequests();

            Assert.NotNull(request.Dax);
        }

        [Fact]
        public void DaxFormatterRequest_SensitiveServerNameIsProtected()
        {
            var expected = "f4139cb312e198bedebf0d71198c27db761f5c4cab73da7dd2faea2672d082af";

            var request = new DaxFormatterSingleRequest()
            {
                ServerName = "MyServerName1"
            };

            Assert.Equal(expected, request.ServerName);
        }

        [Fact]
        public void DaxFormatterRequest_SensitiveDatabaseNameIsProtected()
        {
            var expected = "6341a49658aeebdfbcc648aea92b6005b393d3f89205cf56166c5626c80b9863";

            var request = new DaxFormatterSingleRequest()
            {
                DatabaseName = "MyDatabaseName1"
            };

            Assert.Equal(expected, request.DatabaseName);
        }
    }
}
