using Dax.Formatter.Configuration;
using Xunit;

namespace Dax.Formatter.Tests.Configuration;

public class FormatSettingsTests
{
    [Fact]
    public void From_Null_UsesTheDefaults()
    {
        var settings = FormatSettings.From(null);

        Assert.Equal(DaxKind.Auto, settings.Kind);
        Assert.Equal((0, false), (settings.MaxLineLength, settings.SkipSpaceAfterFunctionName));
        Assert.Equal((",", "."), (settings.ListSeparator, settings.DecimalSeparator));
    }

    [Fact]
    public void From_ModelInfo_KeepsOnlyTheHashesOfTheNames()
    {
        var options = new DaxFormatOptions
        {
            ModelInfo = new DaxModelInfo { ServerName = "MyServerName1", DatabaseName = "MyDatabaseName1" },
        };

        var settings = FormatSettings.From(options);

        Assert.Equal("f4139cb312e198bedebf0d71198c27db761f5c4cab73da7dd2faea2672d082af", settings.ServerNameHash);
        Assert.Equal("6341a49658aeebdfbcc648aea92b6005b393d3f89205cf56166c5626c80b9863", settings.DatabaseNameHash);
    }

    [Theory]
    [InlineData(DaxServerType.AnalysisServices, "SSAS")]
    [InlineData(DaxServerType.PowerBIDesktop, "PBI Desktop")]
    [InlineData(DaxServerType.PowerBIReportServer, "PBI Report Server")]
    [InlineData(DaxServerType.PowerPivot, "PowerPivot")]
    [InlineData(DaxServerType.Ssdt, "SSDT")]
    [InlineData(DaxServerType.AzureAnalysisServices, "AzureAS")]
    [InlineData(DaxServerType.PowerBIService, "PBI Service")]
    [InlineData(DaxServerType.Offline, "Offline")]
    public void From_ServerType_TakesTheValueOfTheService(DaxServerType serverType, string expected)
    {
        var options = new DaxFormatOptions { ModelInfo = new DaxModelInfo { ServerType = serverType } };

        var settings = FormatSettings.From(options);

        Assert.Equal(expected, settings.ServerType);
    }

    [Fact]
    public void From_ServerEnumerations_TakeTheNamesOfTheMembers()
    {
        var options = new DaxFormatOptions
        {
            ModelInfo = new DaxModelInfo
            {
                ServerMode = DaxServerMode.SharePoint,
                ServerEdition = DaxServerEdition.StandardDeveloper64,
                ServerLocation = DaxServerLocation.Azure,
            },
        };

        var settings = FormatSettings.From(options);

        Assert.Equal(
            ("SharePoint", "StandardDeveloper64", "Azure"),
            (settings.ServerMode, settings.ServerEdition, settings.ServerLocation));
    }

    [Fact]
    public void From_ModelInfoWithoutServerValues_SendsNone()
    {
        var options = new DaxFormatOptions { ModelInfo = new DaxModelInfo() };

        var settings = FormatSettings.From(options);

        Assert.Equal(
            (null, null, null, null),
            (settings.ServerType, settings.ServerMode, settings.ServerEdition, settings.ServerLocation));
    }

    [Theory]
    [InlineData(8, 0, 0, 0)]
    [InlineData(0, -1, 0, 0)]
    [InlineData(0, 0, 16, 0)]
    [InlineData(0, 0, 0, 2)]
    public void From_ServerValueOutsideItsEnumeration_Throws(int serverType, int serverMode, int serverEdition, int serverLocation)
    {
        var options = new DaxFormatOptions
        {
            ModelInfo = new DaxModelInfo
            {
                ServerType = (DaxServerType)serverType,
                ServerMode = (DaxServerMode)serverMode,
                ServerEdition = (DaxServerEdition)serverEdition,
                ServerLocation = (DaxServerLocation)serverLocation,
            },
        };

        Assert.Throws<ArgumentException>("options", () => FormatSettings.From(options));
    }

    [Fact]
    public void From_Options_CopiesTheValues()
    {
        var options = new DaxFormatOptions { LineStyle = DaxLineStyle.ShortLine };
        var settings = FormatSettings.From(options);

        options.LineStyle = DaxLineStyle.LongLine;

        Assert.Equal(1, settings.MaxLineLength);
    }

    [Theory]
    [InlineData(2, 0, 0, 0)]
    [InlineData(0, 2, 0, 0)]
    [InlineData(0, 0, -1, 0)]
    [InlineData(0, 0, 0, 2)]
    public void From_ValueOutsideItsEnumeration_Throws(int kind, int lineStyle, int spacingStyle, int separatorStyle)
    {
        var options = new DaxFormatOptions
        {
            Kind = (DaxKind)kind,
            LineStyle = (DaxLineStyle)lineStyle,
            SpacingStyle = (DaxSpacingStyle)spacingStyle,
            SeparatorStyle = (DaxSeparatorStyle)separatorStyle,
        };

        Assert.Throws<ArgumentException>("options", () => FormatSettings.From(options));
    }
}
