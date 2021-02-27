using System;

namespace Forseti
{
    public class SyntaxAttribute : Attribute
    {
        public string Syntax;

        public SyntaxAttribute(string syntax) { Syntax = syntax; }
    }
}
