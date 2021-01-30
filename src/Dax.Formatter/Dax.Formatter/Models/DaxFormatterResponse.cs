namespace Dax.Formatter.Models
{
    using System.Collections.Generic;

    public class DaxFormatterResponse
    {
        public string Formatted { get; set; }

        public List<DaxFormatterError> Errors { get; set; }
    }
}
