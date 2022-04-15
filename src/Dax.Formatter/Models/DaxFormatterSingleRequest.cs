namespace Dax.Formatter.Models
{
    using System;

    public class DaxFormatterSingleRequest : DaxFormatterRequest
    {
        internal static DaxFormatterSingleRequest GetFrom(string? application, string? version, string expression)
        {
            return new DaxFormatterSingleRequest
            {
                CallerApp = application,
                CallerVersion = version,
                Dax = expression
            };
        }

        internal override Uri DaxTextFormatUri { get; } = new Uri("https://www.daxformatter.com/api/daxformatter/daxtextformat");

        public string? Dax { get; set; }
    }
}
