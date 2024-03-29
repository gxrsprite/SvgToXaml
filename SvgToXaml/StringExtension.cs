﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SvgToXaml
{
    public static class StringExtension
    {
        public static string Wrap(this string src, string span)
        {
            if (!src.StartsWith(span))
            {
                src = span + src;
            }
            if (!src.EndsWith(span))
            {
                src = src + span;
            }
            return src;
        }
        public static string Maohao(this string src)
        {
            if (string.IsNullOrEmpty(src))
            {
                return string.Empty;
            }
            if (!src.StartsWith("\"") && !src.EndsWith("\""))
            {
                return src.Wrap("\"");
            }
            else
            {
                return src;
            }
        }
    }
}
