using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Aeris
{
    public static class DatedKeys
    {
        private static Random _rand;

        private static readonly StringBuilder Sb;

        static DatedKeys()
        {
            Sb = new StringBuilder(30);
        }

        /// <summary>
        /// Gets the expiry date of a key if possible.
        /// </summary>
        /// <param name="key">Key to get the expiry date out.</param>
        /// <param name="expiryDate">Expiry date, will return <code>DateTime.MinValue</code></param>
        /// <returns></returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public static bool TryParseExpiryDate(string key, out DateTime expiryDate)
        {
            try
            {
                var ts = new TimeSpan(GetTotalDays(key), 0, 0, 0);
                expiryDate = new DateTime(ts.Ticks);
                return true;
            }
            catch
            {
                expiryDate = DateTime.MinValue;
                return false;
            }
        }

        /// <summary>
        ///     Check whether a ket is expired or not.
        /// </summary>
        /// <param name="key">Key to verify</param>
        /// <returns></returns>
        public static bool IsActiveKey(string key)
        {
            if (TryParseExpiryDate(key, out var expiryDate)) return expiryDate > DateTime.Now;

            return false;
        }

        /// <summary>
        ///     Generates a key with a expiry date.
        ///     Precision up to the day.
        /// </summary>
        /// <param name="expiryDate">Expiry date of the key.</param>
        /// <param name="seed">Seed for the Random object. Leaving empty will use a random number</param>
        /// <returns></returns>
        public static string GenerateKey(DateTime expiryDate, int seed = 0)
        {
            // Since you want to allow specification of custom seeds...we can't really reuse instance of Random

            _rand = new Random(seed == 0 ? DateTime.UtcNow.Millisecond : seed);

            // We don't have to clear the string-builder...since we are pretty much guaranteed
            // to replace all chars in it!
            // Since I'm too dumb to figure out the impl of this KeyGen...I've decided to just
            // clear and append, ignoring my statement above

            Sb.Append(_rand.Next(1, 5));

            Sb.Append(_rand.Next(1, 5));

            Sb.Append(_rand.Next(1, 5));

            Sb.Append(_rand.Next(1, 6));

            Sb.Append(_rand.Next(1, 6));

            Sb.Append(_rand.Next(1, 3));

            var ts = new TimeSpan(expiryDate.Ticks);

            var totalDaysToString = ((int) ts.TotalDays).ToString(CultureInfo.InvariantCulture);

            totalDaysToString = totalDaysToString.PadLeft(7, '0');

            const char dash = '-';

            Sb.Append(totalDaysToString[0]);

            Sb.Append(dash);

            Sb.Append(GenerateSegment(5, CharToInt(totalDaysToString[1]),
                CharToInt(Sb[0])));

            Sb.Append(dash);

            Sb.Append(GenerateSegment(5, CharToInt(totalDaysToString[2]),
                CharToInt(Sb[1])));

            Sb.Append(dash);

            Sb.Append(GenerateSegment(5, CharToInt(totalDaysToString[2]),
                CharToInt(Sb[2])));

            Sb.Append(dash);

            Sb.Append(GenerateSegment(5, CharToInt(totalDaysToString[4]),
                CharToInt(Sb[3])));

            Sb.Append(dash);

            Sb.Append(GenerateSegment(5, CharToInt(totalDaysToString[5]),
                CharToInt(Sb[4])));

            Sb.Append(dash);

            Sb.Append(GenerateSegment(5, CharToInt(totalDaysToString[6]),
                CharToInt(Sb[5])));

            Sb.Append(dash);

            return Sb.ToString();
        }

        /// <summary>
        ///     Gets the total amount of days that the key contains.
        /// </summary>
        /// <param name="key">Key to process</param>
        /// <returns></returns>
        private static int GetTotalDays(string key)
        {
            Sb.Clear();

            Sb.Append(key[6]);

            Sb.Append(key[8 + CharToInt(key[0])]);

            Sb.Append(key[14 + CharToInt(key[1])]);

            Sb.Append(key[20 + CharToInt(key[2])]);

            Sb.Append(key[26 + CharToInt(key[3])]);

            Sb.Append(key[33 + CharToInt(key[4])]);

            Sb.Append(key[40 + CharToInt(key[5])]);

            return int.TryParse(Sb.ToString().Replace(",", ""), out var value) ? value : 0;
        }

        /// <summary>
        ///     Generates a segment of a given length and which contains a give number at a given index.
        /// </summary>
        /// <param name="length">Length of the segment</param>
        /// <param name="numberToStore">Number to store</param>
        /// <param name="index">Index of the number to store</param>
        /// <returns></returns>
        private static unsafe string GenerateSegment(int length, int numberToStore, int index)
        {
            var segment = stackalloc char[length];

            segment[index] = IntToChar(numberToStore);

            for (var I = 0; I < index; I++) segment[I] = IntToChar(_rand.Next(0, 9));

            for (var I = index + 1; I < length; I++) segment[I] = IntToChar(_rand.Next(0, 9));

            return new string(segment, 0, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static int CharToInt(char @char)
        {
            return unchecked(@char - 48);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private static char IntToChar(int @int)
        {
            return (char)(@int + 48);
        }
    }
}