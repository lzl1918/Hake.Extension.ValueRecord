using Hake.Extension.ValueRecord.Internal;
using Hake.Extension.ValueRecord.Internal.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hake.Extension.ValueRecord.Json.Internal.Scanners
{
    internal static class NumberScanner
    {
        private static int GetCharType(char ch)
        {
            if (ch == '0')
                return 0;
            else if (ch.IsNumber())
                return 1;
            else if (ch == 'E' || ch == 'e')
                return 2;
            else if (ch == 'X' || ch == 'x')
                return 3;
            else if (ch == '.')
                return 4;
            else if (ch.IsHex())
                return 5;
            else if (ch == '-' || ch == '+')
                return 6;
            else if (ch.IsBorder() || ch.IsWhiteSpace())
                return 7;
            else
                return 8;
        }
        private static int[,] CONV = new int[11, 9]
        {
            //       0   !0    E    X    .  Hex    +-   B  else
            /*er*/ { 0,   0,   0,   0,   0,   0,   0,   0,   0},
            /*01*/ { 2,   3,   0,   0,   0,   0,   0,   0,   0},
            /*02*/ { 3,   3,   4,   7,   6,   0,   0,  10,   0},
            /*03*/ { 3,   3,   4,   0,   6,   0,   0,  10,   0},
            /*04*/ { 5,   5,   0,   0,   0,   0,   9,   0,   0},
            /*05*/ { 5,   5,   0,   0,   0,   0,   0,  10,   0},
            /*06*/ { 5,   5,   0,   0,   0,   0,   0,   0,   0},
            /*07*/ { 7,   7,   0,   0,   0,   7,   0,   0,   0},
            /*08*/ { 5,   5,   0,   0,   0,   0,   0,  10,   0},
            /*09*/ { 5,   5,   0,   0,   0,   0,   0,   0,   0},
          /*10ac*/ {10,  10,  10,  10,  10,  10,  10,  10,  10}
        };

        public static bool TryScanNumber(InternalTextReader reader, out object result)
        {
            char ch;
            int state = 1, nstate;
            int type;
            int read;
            StringBuilder stb = new StringBuilder();
            string elpPrefix = "";
            while (true)
            {
                read = reader.Peek();
                if (read == -1)
                {
                    if (CONV[state, 7] == 10)
                    {
                        if (state == 5)
                        {
                            if (elpPrefix.Length <= 0)
                            {
                                result = double.Parse(stb.ToString());
                                return true;
                            }
                            else
                            {
                                int intval = int.Parse(stb.ToString());
                                int elpVal = int.Parse(elpPrefix);
                                double pow = Math.Pow(10.0, intval);
                                result = elpVal * pow;
                                return true;
                            }
                        }
                        else if (state == 8)
                        {
                            result = int.Parse(stb.ToString(), System.Globalization.NumberStyles.HexNumber);
                            return true;
                        }
                        else if (state == 3)
                        {
                            string str = stb.ToString();
                            if (str.All(c => c <= '7') && str[0] == '0')
                            {
                                // TODO: oct number
                                result = int.Parse(str);
                                return true;
                            }
                            else
                            {
                                result = int.Parse(str);
                                return true;
                            }
                        }
                        else
                        {
                            result = int.Parse(stb.ToString());
                            return true;
                        }
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }

                ch = (char)read;
                type = GetCharType(ch);
                nstate = CONV[state, GetCharType(ch)];

                if (state == 4 && nstate == 9)
                {
                    stb.Remove(stb.Length - 1, 1);
                    elpPrefix = stb.ToString();
                    stb.Clear();
                    if (ch == '-')
                        stb.Append('-');
                }
                else if (nstate == 10)
                {
                    if (state == 5)
                    {
                        if (elpPrefix.Length <= 0)
                        {
                            result = double.Parse(stb.ToString());
                            return true;
                        }
                        else
                        {
                            int intval = int.Parse(stb.ToString());
                            int elpVal = int.Parse(elpPrefix);
                            double pow = Math.Pow(10.0, intval);
                            result = elpVal * pow;
                            return true;
                        }
                    }
                    else if (state == 8)
                    {
                        result = int.Parse(stb.ToString(), System.Globalization.NumberStyles.HexNumber);
                        return true;
                    }
                    else if (state == 3)
                    {
                        string str = stb.ToString();
                        if (str.All(c => c <= '7') && str[0] == '0')
                        {
                            // TODO: oct number
                            result = int.Parse(str);
                            return true;
                        }
                        else
                        {
                            result = int.Parse(str);
                            return true;
                        }
                    }
                    else
                    {
                        result = int.Parse(stb.ToString());
                        return true;
                    }
                }
                else if (nstate == 0)
                {
                    result = 0;
                    return false;
                }
                else
                    stb.Append(ch);
                state = nstate;
                reader.Read();
            }
        }
    }
}
