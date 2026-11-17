namespace Dax.Formatter.Models
{
    public class DaxFormatterError
    {
        public int? Line { get; set; }

        public int? Column { get; set; }

        public string? Message { get; set; }
    }
}
