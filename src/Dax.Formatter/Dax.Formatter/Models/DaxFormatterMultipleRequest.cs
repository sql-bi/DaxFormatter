namespace Dax.Formatter.Models
{
    using System.Collections.Generic;

    public class DaxFormatterMultipleRequest : DaxFormatterRequest
    {
        internal static DaxFormatterMultipleRequest GetFrom(IEnumerable<string> expressions)
        {
            var request = new DaxFormatterMultipleRequest();
            request.Dax.AddRange(expressions);

            return request;
        }

        public DaxFormatterMultipleRequest()
        {
        }

        public List<string> Dax { get; set; } = new List<string>();
    }
}
