using System;
using System.Collections.Generic;
using System.Text;

namespace IPLD.Git
{
    internal static class StringExtensions
    {
        public static byte[] AsBytes(this string value) => Encoding.UTF8.GetBytes(value);
    }
}
