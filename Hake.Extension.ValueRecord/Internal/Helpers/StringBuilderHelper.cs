using System;
using System.Collections.Generic;
using System.Text;

namespace Hake.Extension.ValueRecord.Internal.Helpers
{
    internal static class StringBuilderHelper
    {
        public static void AppendIndent(this StringBuilder builder, int count)
        {
            while (count > 0)
            {
                builder.Append("    ");
                count--;
            }
        }
    }
}
