using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.ValueRecord.Internal.Helpers
{
    internal static class CharTypeHelper
    {
        private static readonly string BORDERS = "{}[],";
        private static readonly string OCTORS = "01234567";
        private static readonly string HEXS = "0123456789ABCDEFabcdef";
        private static readonly string WHITESPACES = " \t\n\r\v";

        public static bool IsWhiteSpace(this char ch)
        {
            return WHITESPACES.IndexOf(ch) >= 0;
        }
        public static bool IsBorder(this char ch)
        {
            return BORDERS.IndexOf(ch) >= 0;
        }
        public static bool IsOctor(this char ch)
        {
            return OCTORS.IndexOf(ch) >= 0;
        }
        public static bool IsHex(this char ch)
        {
            return HEXS.IndexOf(ch) >= 0;
        }
        public static bool IsNumber(this char ch)
        {
            return ch <= '9' && ch >= '0';
        }
        public static bool IsAlpha(this char ch)
        {
            return (ch <= 'z' && ch >= 'a') || (ch <= 'Z' && ch >= 'A');
        }
    }
}
