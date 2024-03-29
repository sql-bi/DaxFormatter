﻿namespace Dax.Formatter.Models
{
    using System;

    public class DaxFormatterSingleRequest : DaxFormatterRequest
    {
        internal static DaxFormatterSingleRequest CreateFrom(string expression)
        {
            var request =  new DaxFormatterSingleRequest
            {
                Dax = expression
            };
            return request;
        }

        internal override Uri DaxTextFormatUri { get; } = new Uri("https://www.daxformatter.com/api/daxformatter/daxtextformat");

        public string? Dax { get; set; }
    }
}
