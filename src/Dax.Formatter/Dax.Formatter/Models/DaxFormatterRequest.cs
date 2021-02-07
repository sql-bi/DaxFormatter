namespace Dax.Formatter.Models
{
    using Dax.Formatter.Security;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class DaxFormatterRequestBase
    {
        private string _serverName;
        private string _databaseName;

        internal static DaxFormatterSingleRequest GetFrom(string expression)
        {
            return new DaxFormatterSingleRequest
            {
                Dax = expression
            };
        }

        internal static DaxFormatterMultipleRequests GetFrom(IEnumerable<string> expressions)
        {
            var request = new DaxFormatterMultipleRequests
            {
                Dax = expressions.ToList()
            };

            return request;
        }

        public DaxFormatterRequestBase()
        {
        }
        
        public string ServerName
        {
            get => _serverName;
            set => _serverName = value.ToHashSHA256();
        }

        public string ServerEdition { get; set; }

        public string ServerType { get; set; }

        public string ServerMode { get; set; }

        public string ServerLocation { get; set; }

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

    public class DaxFormatterSingleRequest : DaxFormatterRequestBase
    {
        public DaxFormatterSingleRequest()
        {
        }
        public string Dax { get; set; }
    }

    public class DaxFormatterMultipleRequests : DaxFormatterRequestBase
    {
        public DaxFormatterMultipleRequests()
        {
        }
        public List<string> Dax { get; set; } = new List<string>();
    }
}
