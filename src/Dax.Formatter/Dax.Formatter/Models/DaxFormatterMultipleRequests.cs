namespace Dax.Formatter.Models
{
    using System.Collections.Generic;

    public class DaxFormatterMultipleRequests : DaxFormatterRequest
    {
        public DaxFormatterMultipleRequests()
        {
        }
        public List<string> Dax { get; set; } = new List<string>();
    }
}
