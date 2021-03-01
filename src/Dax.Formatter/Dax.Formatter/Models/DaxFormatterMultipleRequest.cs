namespace Dax.Formatter.Models
{
    using System;
    using System.Collections.Generic;

    public class DaxFormatterMultipleRequest : DaxFormatterRequest
    {
        internal static DaxFormatterMultipleRequest GetFrom(string application, string version, IEnumerable<string> expressions)
        {
            var request = new DaxFormatterMultipleRequest();

            request.CallerApp = application;
            request.CallerVersion = version;
            request.Dax.AddRange(expressions);

            return request;
        }

        internal override Uri DaxTextFormatUri { get; } = new Uri("https://www.daxformatter.com/api/daxformatter/daxtextformatmulti"); 
 
        public List<string> Dax { get; set; } = new List<string>();
    }
}
