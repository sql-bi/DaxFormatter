
# Dax Formatter [![CI](https://github.com/sql-bi/DaxFormatter/actions/workflows/ci.yml/badge.svg)](https://github.com/sql-bi/DaxFormatter/actions/workflows/ci.yml)

DAX Formatter is a service available at https://www.daxformatter.com.
The service receives DAX expressions and format them according to rules for [DAX code formatting](https://www.sqlbi.com/articles/rules-for-dax-code-formatting/).
The NuGet package contains a client library to invoke the serivce from your .NET application.
All the requests are designed to be asynchronous.
You should minimize the requests made to the service: to format multiple expressions, use a single API call providing the `DaxFormatterMultipleRequest` structure.

# Quick guide
Create an instance of `DaxFormatterClient`.
Invoke the `FormatAsync` method for each request.
Possibly, use the `DaxFormatterMultipleRequest` structure to provide more statistical details about the database and the server (all the names are anonymized and used only to count the number of unique servers/databases serviced).

# DaxFormatterClient
The default constructor creates an instance of the client using the current assembly name and version to identify the client. You can specify an application name and version in the constructor arguments.
The client instance resolve the server endpoint just once and reuse it in following requests.
It is suggested to reuse the same instance in multiple requests to minimize the latency.
```csharp
var formatter = new DaxFormatterClient();
var response = await formatter.FormatAsync( "evaluate('Table') order by 'Table'[Column]" );
```
# DaxFormatterMultipleRequest
Use the `DaxFormatterMultipleRequest` class to send multiple DAX expressions in a single API call.
```csharp
var formatter = new DaxFormatterClient();
var request = new DaxFormatterMultipleRequest
{
    DatabaseName = "MyDatabaseName", // The name will be anonymized by the client library
    ServerName = "MyServerName",     // The name will be anonymized by the client library

    // Format arguments (you can skip all of them to keep the default values)
    ListSeparator = ',',
    DecimalSeparator = '.',
    MaxLineLength = DaxFormatterLineStyle.LongLine,
    SkipSpaceAfterFunctionName = DaxFormatterSpacingStyle.BestPractice,

    // Identify the server type using the corresponding enum values
    ServerMode = Dax.Formatter.AnalysisServices.ServerMode.Tabular,
    ServerType = Dax.Formatter.AnalysisServices.ServerType.AnalysisServices,
    ServerEdition = Dax.Formatter.AnalysisServices.ServerEdition.Enterprise,
    ServerLocation = Dax.Formatter.AnalysisServices.ServerLocation.OnPremise,
};

// Add the code to format    
request.Dax.Add( "evaluate('Table') order by 'Table'[Column]" );
request.Dax.Add( "[X]:=CALCULATE([Amt],USERELATIONSHIP(Sales[DueDateKey],'Date'[DateKey]))" );

// Send the request and wait the result
var responses = await formatter.FormatAsync(request);

// Process the result
foreach (var response in responses)
{
    Console.WriteLine(response.Formatted);
}
```
# DaxFormatterSingleRequest
Use the `DaxFormatterSingleRequest` class to send one DAX expressions in a single API call.
```csharp
var formatter = new DaxFormatterClient();
var request = new DaxFormatterSingleRequest
{
    Dax = "evaluate('Table') order by 'Table'[Column]",

    DatabaseName = "MyDatabaseName", // The name will be anonymized by the client library
    ServerName = "MyServerName",     // The name will be anonymized by the client library

    // Format arguments (you can skip all of them to keep the default values)
    ListSeparator = ',',
    DecimalSeparator = '.',
    MaxLineLength = DaxFormatterLineStyle.LongLine,
    SkipSpaceAfterFunctionName = DaxFormatterSpacingStyle.BestPractice,

    // Identify the server type using the corresponding enum values
    ServerMode = Dax.Formatter.AnalysisServices.ServerMode.Tabular,
    ServerType = Dax.Formatter.AnalysisServices.ServerType.AnalysisServices,
    ServerEdition = Dax.Formatter.AnalysisServices.ServerEdition.Enterprise,
    ServerLocation = Dax.Formatter.AnalysisServices.ServerLocation.OnPremise,
};
 
// Send the request and wait the result
var response = await formatter.FormatAsync(request);

// Display the result
Console.WriteLine(response.Formatted);
```
