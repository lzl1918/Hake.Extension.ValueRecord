using Hake.Extension.ValueRecord.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.ValueRecord.Json.Internal.Scanners
{
    internal static class StringScanner
    {
        private static char GetEscapedChar(char ch)
        {
            if (ch == '0') return '\0';
            else if (ch == 't') return '\t';
            else if (ch == 'r') return '\r';
            else if (ch == 'v') return '\v';
            else if (ch == 'n') return '\n';
            else return ch;
        }

        public static bool TryScanString(InternalTextReader reader, out string result)
        {
            char ch;
            int state = 1;
            int read;
            StringBuilder stb = new StringBuilder();
            while (true)
            {
                read = reader.Peek();
                if (read == -1)
                {
                    result = "";
                    return false;
                }

                ch = (char)read;
                if (state == 1)
                {
                    if (ch == '"') { state = 2; reader.Read(); }
                    else
                    {
                        result = "";
                        return false;
                    }
                }
                else if (state == 2)
                {
                    if (ch == '\\') { state = 3; reader.Read(); }
                    else if (ch == '"')
                    {
                        result = stb.ToString();
                        reader.Read();
                        return true;
                    }
                    else { stb.Append(ch); reader.Read(); }
                }
                else if (state == 3)
                {
                    stb.Append(GetEscapedChar(ch));
                    state = 2;
                    reader.Read();
                }
                else
                    throw new Exception($"unknow state of {state}");
            }
        }
    }
}
