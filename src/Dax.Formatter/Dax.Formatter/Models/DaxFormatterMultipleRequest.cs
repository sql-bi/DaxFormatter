namespace Dax.Formatter.Models
{
    using System.Collections.Generic;

    public class DaxFormatterMultipleRequest : DaxFormatterRequest
    {
        public DaxFormatterMultipleRequest()
        {
        }

        public List<string> Dax { get; set; } = new List<string>();
    }
}
