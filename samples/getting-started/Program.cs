using Dax.Formatter;
using Dax.Formatter.Models;
using System;
using System.Threading.Tasks;

namespace BasicFormatter
{
    public class Program
    {
        private static readonly string DaxSingleExpression = "[X] := CALCULATE(SUM(Sales[Amount]), USERELATIONSHIP(Sales[DueDateKey],'Date'[DateKey]))";
 
        private static readonly string[] DaxMultipleExpressions = new[]
        {
            "evaluate('Table') order by 'Table'[Column]",
            "[X] := CALCULATE(SUM(Sales[Amount]), USERELATIONSHIP(Sales[DueDateKey],'Date'[DateKey]))",
            "EVALUATE FILTER(Customer, Customer[name] = \"TestUser\")",
        };

        public static async Task Main()
        {
            await FormatSingleExpression();

            await FormatSingleRequest();

            await FormatMultipleExpressions();

            await FormatMultipleRequests();

            await FormatInvalidExpression();
        }

        static async Task FormatSingleExpression()
        {
            var formatter = new DaxFormatterClient("", "");

            var response = await formatter.FormatAsync(DaxSingleExpression);

            Console.WriteLine(response.Formatted);
        }

        static async Task FormatSingleRequest()
        {
            var formatter = new DaxFormatterClient("", "");

            var request = new DaxFormatterSingleRequest
            {
                Dax = DaxSingleExpression,

                DatabaseName = "MyDatabaseName",
                ServerName = "MyServerName",

                ListSeparator = ',',
                DecimalSeparator = '.',
                MaxLineLenght = DaxFormatterLineStyle.LongLine,
                SkipSpaceAfterFunctionName = DaxFormatterSpacingStyle.BestPractice,

                ServerMode = Dax.Formatter.AnalysisServices.ServerMode.Tabular,
                ServerType = Dax.Formatter.AnalysisServices.ServerType.AnalysisServices,
                ServerEdition = Dax.Formatter.AnalysisServices.ServerEdition.Enterprise,
                ServerLocation = Dax.Formatter.AnalysisServices.ServerLocation.OnPremise,
            };

            var response = await formatter.FormatAsync(request);

            Console.WriteLine(response.Formatted);
        }

        static async Task FormatMultipleExpressions()
        {
            var formatter = new DaxFormatterClient("", "");

            var responses = await formatter.FormatAsync(DaxMultipleExpressions);

            foreach (var response in responses)
            {
                Console.WriteLine(response.Formatted);
            }
        }

        static async Task FormatMultipleRequests()
        {
            var formatter = new DaxFormatterClient("", "");

            var request = new DaxFormatterMultipleRequest
            {
                DatabaseName = "MyDatabaseName",
                ServerName = "MyServerName",

                ListSeparator = ',',
                DecimalSeparator = '.',
                MaxLineLenght = DaxFormatterLineStyle.LongLine,
                SkipSpaceAfterFunctionName = DaxFormatterSpacingStyle.BestPractice,

                ServerMode = Dax.Formatter.AnalysisServices.ServerMode.Tabular,
                ServerType = Dax.Formatter.AnalysisServices.ServerType.AnalysisServices,
                ServerEdition = Dax.Formatter.AnalysisServices.ServerEdition.Enterprise,
                ServerLocation = Dax.Formatter.AnalysisServices.ServerLocation.OnPremise,
            };

            request.Dax.AddRange(DaxMultipleExpressions);

            var responses = await formatter.FormatAsync(request);

            foreach (var response in responses)
            {
                Console.WriteLine(response.Formatted);
            }
        }

        static async Task FormatInvalidExpression()
        {
            var formatter = new DaxFormatterClient("", "");

            var invalidExpression = "[X] := SUMX ( Sales[Amount] ), )";

            var response = await formatter.FormatAsync(invalidExpression);

            Console.WriteLine($"Formatted '{ response.Formatted }' ");

            foreach (var error in response.Errors)
            {
                Console.WriteLine($"Line { error.Line }, Column { error.Column }, Message { error.Message }");
            }
        }
    }
}
