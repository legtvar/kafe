using Kafe;
using System.Collections.Immutable;
using System.Globalization;

var formatProvider = new AggregateFormatProvider(CultureInfo.InvariantCulture, [new FileLengthFormatter()]);
Console.WriteLine($"{2_000_000.ToString("fs", formatProvider)}");
Console.WriteLine(string.Format(formatProvider, "{0:fsF2} is the same as {0:FSF2}", 2_000));
