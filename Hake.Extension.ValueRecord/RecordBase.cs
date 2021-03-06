﻿using Hake.Extension.ValueRecord.Internal.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.ValueRecord
{
    public enum RecordType
    {
        Set,
        List,
        Scaler
    }

    public abstract class RecordBase
    {
        public RecordType Type { get; private set; }

        protected RecordBase(RecordType type)
        {
            Type = type;
        }
    }

    public static class RecordBaseExtension
    {
        private static bool IsValidNameChar(this char ch)
        {
            return ch.IsAlpha() || ch.IsNumber() || ch == '_' || ch == '-';
        }

        public static RecordBase RecordFromPath(this RecordBase record, string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (record == null)
                throw new ArgumentNullException(nameof(record));
            if (path.Length <= 0)
                throw new ArgumentException("path cannot be empty");

            RecordBase current = record;
            char ch;
            int index = 0;
            int len = path.Length;
            int state = 1;
            StringBuilder builder = new StringBuilder(path.Length);
            string key;
            int listindex;
            while (true)
            {
                if (index >= path.Length)
                {
                    if (state == 3)
                    {
                        key = builder.ToString();
                        if (current is SetRecord set)
                            current = set[key];
                        else
                            throw new Exception($"record is not a set while trying to read property {key}");
                    }
                    else if (state == 2 || state == 4 || state == 6)
                        throw new Exception("unexcepted end of path");
                    break;
                }

                ch = path[index];
                if (state == 1)
                {
                    if (ch == '[') { state = 2; }
                    else if (ch.IsValidNameChar()) { builder.Append(ch); state = 3; }
                    else throw new Exception($"unexcepted char {ch}");
                }
                else if (state == 2)
                {
                    if (ch.IsNumber()) { builder.Append(ch); state = 4; }
                    else throw new Exception($"unexcepted char {ch}");
                }
                else if (state == 3)
                {
                    if (ch == '[')
                    {
                        key = builder.ToString();
                        if (current is SetRecord set)
                            current = set[key];
                        else
                            throw new Exception($"record is not a set while trying to read property {key}");
                        builder.Clear();
                        state = 2;
                    }
                    else if (ch.IsValidNameChar()) { builder.Append(ch); }
                    else if (ch == '.')
                    {
                        key = builder.ToString();
                        if (current is SetRecord set)
                            current = set[key];
                        else
                            throw new Exception($"record is not a set while trying to read property {key}");
                        builder.Clear();
                        state = 6;
                    }
                    else throw new Exception($"unexcepted char {ch}");
                }
                else if (state == 4)
                {
                    if (ch.IsNumber()) { builder.Append(ch); }
                    else if (ch == ']')
                    {
                        listindex = int.Parse(builder.ToString());
                        if (current is ListRecord list)
                            current = list[listindex];
                        else
                            throw new Exception($"record is not a list while trying to read element at {listindex}");
                        builder.Clear();
                        state = 5;
                    }
                    else throw new Exception($"unexcepted char {ch}");
                }
                else if (state == 5)
                {
                    if (ch == '[') { state = 2; }
                    else if (ch == '.') { state = 6; }
                    else throw new Exception($"unexcepted char {ch}");
                }
                else if (state == 6)
                {
                    if (ch.IsValidNameChar()) { builder.Append(ch); state = 3; }
                    else throw new Exception($"unexcepted char {ch}");
                }
                else
                    throw new Exception($"unknow state of {state}");
                index++;
            }
            return current;
        }
        public static bool TryGetRecordFromPath(this RecordBase record, string path, out RecordBase result)
        {
            if (path == null)
            {
                result = null;
                return false;
            }
            if (record == null)
            {
                result = null;
                return false;
            }
            if (path.Length <= 0)
            {
                result = null;
                return false;
            }

            RecordBase current = record;
            char ch;
            int index = 0;
            int len = path.Length;
            int state = 1;
            StringBuilder builder = new StringBuilder(path.Length);
            string key;
            int listindex;
            while (true)
            {
                if (index >= path.Length)
                {
                    if (state == 3)
                    {
                        key = builder.ToString();
                        if (current is SetRecord set)
                        {
                            if (!set.TryGetValue(key, out current))
                            {
                                result = null;
                                return false;
                            }
                        }
                        else
                        {
                            result = null;
                            return false;
                        }
                    }
                    else if (state == 2 || state == 4 || state == 6)
                    {
                        result = null;
                        return false;
                    }
                    break;
                }

                ch = path[index];
                if (state == 1)
                {
                    if (ch == '[') { state = 2; }
                    else if (ch.IsValidNameChar()) { builder.Append(ch); state = 3; }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                else if (state == 2)
                {
                    if (ch.IsNumber()) { builder.Append(ch); state = 4; }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                else if (state == 3)
                {
                    if (ch == '[')
                    {
                        key = builder.ToString();
                        if (current is SetRecord set)
                        {
                            if (!set.TryGetValue(key, out current))
                            {
                                result = null;
                                return false;
                            }
                        }
                        else
                        {
                            result = null;
                            return false;
                        }
                        builder.Clear();
                        state = 2;
                    }
                    else if (ch.IsValidNameChar()) { builder.Append(ch); }
                    else if (ch == '.')
                    {
                        key = builder.ToString();
                        if (current is SetRecord set)
                        {
                            if (!set.TryGetValue(key, out current))
                            {
                                result = null;
                                return false;
                            }
                        }
                        else
                        {
                            result = null;
                            return false;
                        }
                        builder.Clear();
                        state = 6;
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                else if (state == 4)
                {
                    if (ch.IsNumber()) { builder.Append(ch); }
                    else if (ch == ']')
                    {
                        if (!int.TryParse(builder.ToString(), out listindex))
                        {
                            result = null;
                            return false;
                        }
                        if (current is ListRecord list)
                        {
                            if (list.Count <= listindex)
                            {
                                result = null;
                                return false;
                            }
                            current = list[listindex];
                        }
                        else
                        {
                            result = null;
                            return false;
                        }
                        builder.Clear();
                        state = 5;
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                else if (state == 5)
                {
                    if (ch == '[') { state = 2; }
                    else if (ch == '.') { state = 6; }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                else if (state == 6)
                {
                    if (ch.IsValidNameChar()) { builder.Append(ch); state = 3; }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
                else
                {
                    result = null;
                    return false;
                }

                index++;
            }
            result = current;
            return true;
        }

        public static T ReadAs<T>(this RecordBase record, string path)
        {
            RecordBase rec = RecordFromPath(record, path);
            if (rec is ScalerRecord scaler)
                return scaler.ReadAs<T>();
            else
                throw new Exception("value in specific path is not a scaler");
        }
        public static bool TryReadAs<T>(this RecordBase record, string path, out T value)
        {
            if (!TryGetRecordFromPath(record, path, out RecordBase rec))
            {
                value = default(T);
                return false;
            }

            if (rec is ScalerRecord scaler)
            {
                return scaler.TryReadAs<T>(out value);
            }
            else
            {
                value = default(T);
                return false;
            }
        }
    }

}
