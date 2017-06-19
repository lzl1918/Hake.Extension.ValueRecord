using System;
using System.IO;

namespace Hake.Extension.ValueRecord.Json
{
    internal class InternalTextReader
    {
        private TextReader reader;
        private int lineIndex;
        private int lineCount = 0;
        private string lineCache;
        private bool hasLineCache = false;
        public InternalTextReader(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));
            this.reader = reader;
        }

        public int CurrentPosition { get { Peek(); return lineIndex; } }
        public int CurrentLine { get { Peek(); return lineCount; } }
        public string CurrentLineContent { get { Peek(); return lineCache; } }
        public bool HasChar
        {
            get { return Peek() > -1; }
        }
        public int Peek()
        {
            if (hasLineCache == false)
            {
                lineCache = reader.ReadLine();
                if (lineCache == null)
                {
                    hasLineCache = false;
                    return -1;
                }
                lineCache += '\n';
                lineCount++;
                hasLineCache = true;
                lineIndex = 0;
            }
            while (lineIndex >= lineCache.Length)
            {
                lineCache = reader.ReadLine();
                if (lineCache == null)
                {
                    hasLineCache = false;
                    return -1;
                }
                lineCache += '\n';
                lineIndex = 0;
                lineCount++;
            }
            return lineCache[lineIndex];
        }
        public int Read()
        {
            if (hasLineCache == false)
            {
                lineCache = reader.ReadLine();
                if (lineCache == null)
                {
                    hasLineCache = false;
                    return -1;
                }
                lineCache += '\n';
                lineCount++;
                hasLineCache = true;
                lineIndex = 0;
            }
            while (lineIndex >= lineCache.Length)
            {
                lineCache = reader.ReadLine();
                if (lineCache == null)
                {
                    hasLineCache = false;
                    return -1;
                }
                lineCache += '\n';
                lineIndex = 0;
                lineCount++;
            }
            char ch = lineCache[lineIndex];
            lineIndex++;
            return ch;
        }
    }
}
