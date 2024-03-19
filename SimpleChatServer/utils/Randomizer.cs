using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleChatServer
{
    public class Randomizer
    {
        private static readonly Random _random = new();

        public static string String(int length, bool lowerCase = false)
        {
            var stringBuilder = new StringBuilder();

            var offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26;

            for (var i = 0; i < length; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                stringBuilder.Append(@char);
            }

            return lowerCase ? stringBuilder.ToString().ToLower() : stringBuilder.ToString();
        }
    }
}
