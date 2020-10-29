using System;
using System.Diagnostics;

namespace SaveConverter
{
    public static class DebugHelper
    {
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public static void Throw(Exception e) => throw e;
    }
}
