﻿using Hake.Extension.ValueRecord.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hake.Extension.ValueRecord.Json
{

    internal static class StringJsonConverter
    {
        private static bool IsWhiteSpace(this char value) => " \t\v\r\n".IndexOf(value) >= 0;
        private static bool IsNumber(this char value) => value >= '0' && value <= '9';

        public static RecordBase ReadJson(TextReader reader, bool ignoreKeyCase = false)
        {
            InternalTextReader internalReader = new InternalTextReader(reader);
            return ReadJson(internalReader, false, ignoreKeyCase);
        }
        private static RecordBase ReadJson(InternalTextReader reader, bool isCalledByList, bool ignoreKeyCase)
        {
            char peek;
            int state = 1;
            while (true)
            {
                if (!reader.HasChar)
                    throw new Exception("unexcepted end of stream");

                peek = (char)reader.Peek();
                if (state == 1)
                {
                    if (peek.IsWhiteSpace()) { reader.Read(); }
                    else if (peek == '[') { state = 2; }
                    else if (peek == '{') { state = 3; }
                    else if (peek == 't') { state = 4; }
                    else if (peek == 'f') { state = 5; }
                    else if (peek == 'n') { state = 6; }
                    else if (peek == '"') { state = 7; }
                    else if (peek.IsNumber()) { state = 8; }
                    else if (peek == '-') { state = 9; reader.Read(); }
                    else if (peek == '/') { state = 10; reader.Read(); }
                    else if (peek == ']' && isCalledByList) { reader.Read(); return null; }
                    else throw BuildException($"unexcepted char '{peek}'", reader);
                }
                else if (state == 2)
                {
                    return ReadList(reader, ignoreKeyCase);
                }
                else if (state == 3)
                {
                    return ReadSet(reader, ignoreKeyCase);
                }
                else if (state == 4)
                {
                    ScanStringOrThrow(reader, "true");
                    ScalerRecord record = new ScalerRecord(true);
                    return record;
                }
                else if (state == 5)
                {
                    ScanStringOrThrow(reader, "false");
                    ScalerRecord record = new ScalerRecord(false);
                    return record;
                }
                else if (state == 6)
                {
                    ScanStringOrThrow(reader, "null");
                    ScalerRecord record = new ScalerRecord(null);
                    return record;
                }
                else if (state == 7)
                {
                    ScalerRecord record = new ScalerRecord(ReadStringOrThrow(reader));
                    return record;
                }
                else if (state == 8)
                {
                    ScalerRecord record = new ScalerRecord(ReadNumberOrThrow(reader));
                    return record;
                }
                else if (state == 9)
                {
                    if (peek.IsNumber())
                    {
                        object val = ReadNumberOrThrow(reader);
                        if (val is int intval)
                            val = -intval;
                        else if (val is double doubleval)
                            val = -doubleval;
                        ScalerRecord record = new ScalerRecord(val);
                        return record;
                    }
                    else throw BuildException($"unexcepted char '{peek}', number expected", reader);
                }
                else if (state == 10)
                {
                    if (peek == '/') { state = 11; reader.Read(); }
                    else throw BuildException($"unexcepted char '{peek}'", reader);
                }
                else if (state == 11)
                {
                    if (peek == '\n') { state = 1; reader.Read(); }
                    else reader.Read();
                }
                else
                    throw new Exception($"unknow state of {state}");
            }
        }

        private static object ReadNumberOrThrow(InternalTextReader reader)
        {
            if (Internal.Scanners.NumberScanner.TryScanNumber(reader, out object result) == false)
                throw BuildException("unexcepted char occurred while scanning number", reader);
            return result;
        }
        private static string ReadStringOrThrow(InternalTextReader reader)
        {
            if (Internal.Scanners.StringScanner.TryScanString(reader, out string result) == false)
                throw BuildException("unexcepted char occurred while scanning string", reader);
            return result;
        }
        private static void ScanStringOrThrow(InternalTextReader reader, string expect)
        {
            int read;
            foreach (char ch in expect)
            {
                read = reader.Read();
                if (read == -1)
                    throw new Exception($"invalid char, {ch} expected but end of stream reached");

                if (read != ch)
                    throw BuildException($"Invalid char, {ch} expected but {(char)read} scanned", reader);
            }
        }
        private static ListRecord ReadList(InternalTextReader reader, bool ignoreKeyCase)
        {
            // resolve '['
            reader.Read();

            ListRecord list = new ListRecord();
            char peek;
            int result;
            while (true)
            {
                RecordBase record = ReadJson(reader, true, ignoreKeyCase);
                if (record == null)
                    break;
                list.Add(record);
                while (true)
                {
                    result = reader.Peek();
                    if (result == -1)
                        throw new Exception("']' excepted but end of stream reached");
                    peek = (char)result;
                    if (peek.IsWhiteSpace()) { reader.Read(); }
                    else if (peek == '/')
                    {
                        reader.Read();
                        result = reader.Peek();
                        if (result == -1)
                            throw new Exception("'/' excepted but end of stream reached");
                        peek = (char)result;
                        if (peek != '/')
                            throw BuildException($"unexcepted char '{peek}' while scanning comment", reader);
                        reader.Read();
                        while (true)
                        {
                            result = reader.Peek();
                            if (result == -1)
                                throw new Exception("']' excepted but end of stream reached");
                            peek = (char)result;
                            if (peek == '\n') { reader.Read(); break; }
                            reader.Read();
                        }
                    }
                    else { break; }
                }
                if (peek == ',') { reader.Read(); }
                else if (peek == ']') { reader.Read(); break; }
                else throw BuildException($"unexcepted char '{peek}' while scanning list", reader);
            }
            return list;
        }
        private static SetRecord ReadSet(InternalTextReader reader, bool ignoreKeyCase)
        {
            // resolve '{'
            reader.Read();

            SetRecord set = new SetRecord(ignoreKeyCase);
            char peek;
            int result;
            string key = "";
            int state = 0;
            int oldstate = 0;
            while (true)
            {
                result = reader.Peek();
                if (result == -1)
                {
                    if (state == 0)
                        throw new Exception("'\"' excepted but end of stream reached");
                    else if (state == 1)
                        throw new Exception("':' excepted but end of stream reached");
                    else if (state == 2)
                        throw new Exception("',' or '}' excepted but end of stream reached");
                    else throw new Exception($"unknown state of {state}");
                }

                peek = (char)result;
                if (state == 0)
                {
                    if (peek.IsWhiteSpace()) { reader.Read(); }
                    else if (peek == '"')
                    {
                        key = ReadStringOrThrow(reader);
                        state = 1;
                    }
                    else if (peek == '/') { oldstate = state; reader.Read(); state = 3; }
                    else if (peek == '}') { reader.Read(); break; }
                    else throw BuildException($"'\"' excepted but '{peek}' scanned", reader);
                }
                else if (state == 1)
                {
                    if (peek.IsWhiteSpace()) { reader.Read(); }
                    else if (peek == ':')
                    {
                        reader.Read();
                        RecordBase record = ReadJson(reader, false, ignoreKeyCase);
                        set.Add(key, record);
                        state = 2;
                    }
                    else if (peek == '/') { oldstate = state; reader.Read(); state = 3; }
                    else throw BuildException($"':' excepted but '{peek}' scanned", reader);
                }
                else if (state == 2)
                {
                    if (peek.IsWhiteSpace()) { reader.Read(); }
                    else if (peek == '/') { oldstate = state; reader.Read(); state = 3; }
                    else if (peek == ',') { reader.Read(); state = 0; }
                    else if (peek == '}') { reader.Read(); break; }
                    else throw BuildException($"',' or '}}' excepted but '{peek}' scanned", reader);
                }
                else if (state == 3)
                {
                    if (peek == '/') { reader.Read(); state = 4; }
                    else throw BuildException($"'/' excepted but '{peek}' scanned", reader);
                }
                else if (state == 4)
                {
                    if (peek == '\n') { reader.Read(); state = oldstate; }
                    else reader.Read();
                }
                else throw new Exception($"unknown state of {state}");
            }
            return set;
        }

        private static Exception BuildException(string message, InternalTextReader reader)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(message);
            builder.AppendLine($"at {reader.CurrentLine}, {reader.CurrentPosition}:");
            builder.Append(reader.CurrentLineContent);
            return new Exception(builder.ToString());
        }
    }
}
