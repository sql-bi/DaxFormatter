namespace Dax.Formatter.Models
{
    using System.Collections.Generic;

    public class DaxFormatterMultipleRequests : DaxFormatterRequestBase
    {
        public DaxFormatterMultipleRequests()
        {
        }
        public List<string> Dax { get; set; } = new List<string>();
    }
}
