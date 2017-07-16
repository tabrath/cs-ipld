using System;
using IPLD.ContentIdentifier;

namespace IPLD.BlockFormat
{
    public class InvalidHashException : Exception
    {
        public Cid Expected { get; }
        public Cid Actual { get; }

        public InvalidHashException(Cid expected, Cid actual)
            : base($"Hash mismatch, expected {expected}, got {actual}")
        {
            Expected = expected;
            Actual = actual;
        }
    }
}
