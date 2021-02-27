namespace Dax.Formatter.Models
{
    using Dax.Formatter.AnalysisServices;
    using Dax.Formatter.Security;
    using Dax.Formatter.Serialization;
    using System;
    using System.Text.Json.Serialization;

    public abstract class DaxFormatterRequest
    {
        private string _serverName;
        private string _databaseName;

        internal abstract Uri DaxTextFormatUri { get; }

        public DaxFormatterRequest()
        {
        }
        
        public string ServerName
        {
            get => _serverName;
            set => _serverName = value.ToHashSHA256();
        }

        /// <summary>
        /// Values: null, "Enterprise64", "Developer64", "Standard64"
        /// In DISCOVER_XML_METADATA it is in /d:Edition node
        /// </summary>
        public ServerEdition? ServerEdition { get; set; }

        [JsonConverter(typeof(JsonEnumMemberConverter))]
        public ServerType? ServerType { get; set; }

        /// <summary>
        /// Values: null, "SharePoint", "Tabular"
        /// In DISCOVER_XML_METADATA it is in ServerMode item
        /// </summary>
        public ServerMode? ServerMode { get; set; }

        /// <summary>
        /// Values: null, "OnPremise", "Azure"
        /// In DISCOVER_XML_METADATA it is in /ddl400:ServerLocation node
        /// </summary>
        public ServerLocation? ServerLocation { get; set; }

        /// <summary>
        /// Example: "14.0.800.192"
        /// </summary>
        public string ServerVersion { get; set; }
        
        public string DatabaseName
        {
            get => _databaseName;
            set => _databaseName = value.ToHashSHA256();
        }

        public string DatabaseCompatibilityLevel { get; set; }

        public DaxFormatterLineStyle? MaxLineLenght { get; set; } = DaxFormatterLineStyle.LongLine;

        public bool? SkipSpaceAfterFunctionName { get; set; } = Convert.ToBoolean((int)DaxFormatterSpacingStyle.BestPractice);

        public char ListSeparator { get; set; }  = ',';

        public char DecimalSeparator { get; set; } = '.';

        // TODO add default value for CallerApp
        public string CallerApp { get; set; }

        // TODO add default value for CallerVersion
        public string CallerVersion { get; set; }
    }
}
