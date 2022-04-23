using CSharpx;
using System;
using System.Collections.Generic;

namespace CommandLine
{
    public interface ICustomConverter
    {
        Maybe<object> Convert(IEnumerable<string> vals, Type type, bool isScalar);
    }
}
