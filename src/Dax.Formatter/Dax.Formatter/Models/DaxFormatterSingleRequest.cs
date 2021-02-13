namespace Dax.Formatter.Models
{
    public class DaxFormatterSingleRequest : DaxFormatterRequest
    {
        internal static DaxFormatterSingleRequest GetFrom(string expression)
        {
            return new DaxFormatterSingleRequest
            {
                Dax = expression
            };
        }

        public DaxFormatterSingleRequest()
        {
        }

        public string Dax { get; set; }
    }
}
