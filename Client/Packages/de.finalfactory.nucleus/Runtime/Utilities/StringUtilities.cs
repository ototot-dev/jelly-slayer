using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using UnityEngine;

namespace FinalFactory.Utilities
{
    [PublicAPI]
    [DebuggerStepThrough]
    public static class StringUtilities
    {
        [MethodImpl(GlobalConst.ImplOptions)]
        public static string Repeat(this string str, int count)
        {
            var result = string.Empty;
            for (var i = 0; i < count; i++)
            {
                result += str;
            }
            return result;
        }
        
        //extension method for string that checks if the string is null or empty
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);
        
       
        [MethodImpl(GlobalConst.ImplOptions)]
        public static string ToKebabCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var kebab = Regex.Replace(input, @"([a-z0-9])([A-Z])|([A-Z]{2,})", "$1$3-$2");
            return kebab.ToLower();
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static string ToPascalCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var pascal = Regex.Replace(input, @"(?:^|-|_|\s+)([a-z])", match => match.Groups[1].Value.ToUpper());
            return pascal;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static string ToCamelCase(this string input)
        {
            var pascal = ToPascalCase(input);
            return char.ToLower(pascal[0]) + pascal[1..];
        }


        [MethodImpl(GlobalConst.ImplOptions)]
        public static string ToSnakeCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var snake = Regex.Replace(input, @"([a-z0-9])([A-Z])|([A-Z]{2,})", "$1$3_$2");
            return snake.ToLower();
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static string ToSpacePascalCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            var spacePascal = Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1 $2");
            return spacePascal;
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static string SanitizeFileName(this string input)
        {
            // Replace invalid characters with an underscore
            var invalidChars = new string(Path.GetInvalidFileNameChars());
            var invalidCharsPattern = $"[{Regex.Escape(invalidChars)}]";
            return Regex.Replace(input, invalidCharsPattern, "_");
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static string NameOf(GameObject go) => go == null ? "null" : go.name;

        public static string ConvertSecondsToTimeString(int seconds)
        {
            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;
            return $"{minutes:D2}:{remainingSeconds:D2}";
        }
    }
}