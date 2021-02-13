namespace Dax.Formatter.Models
{
    using Dax.Formatter.Security;
    using System;

    public abstract class DaxFormatterRequest
    {
        private string _serverName;
        private string _databaseName;

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
        public string ServerEdition { get; set; }

        public ServerType? ServerType { get; set; }

        /// <summary>
        /// Values: null, "SharePoint", "Tabular"
        /// In DISCOVER_XML_METADATA it is in ServerMode item
        /// /// </summary>
        public string ServerMode { get; set; }

        /// <summary>
        /// Values: null, "OnPremise", "Azure"
        /// In DISCOVER_XML_METADATA it is in /ddl400:ServerLocation node
        /// </summary>
        public string ServerLocation { get; set; }

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

        public int? MaxLineLenght { get; set; } = (int)DaxFormatterLineStyle.LongLine;

        public bool? SkipSpaceAfterFunctionName { get; set; } = Convert.ToBoolean((int)DaxFormatterSpacingStyle.BestPractice);

        public char ListSeparator { get; set; }  = ',';

        public char DecimalSeparator { get; set; } = '.';

        // TODO add default value for CallerApp
        public string CallerApp { get; set; }

        // TODO add default value for CallerVersion
        public string CallerVersion { get; set; }
    }
}
