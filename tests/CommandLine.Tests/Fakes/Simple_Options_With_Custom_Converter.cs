// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using CSharpx;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CommandLine.Tests.Fakes
{
    class Simple_Options_With_Custom_Converter
    {
        [Option('s', "stringvalue", Env = "StringValue", CustomConverter = typeof(TheCustomConverter))]
        public TheCustomClass StringValue { get; set; }
    }

    public class TheCustomConverter : ICustomConverter
    {
        public Maybe<object> Convert(IEnumerable<string> vals, Type type, bool isScalar)
        {
            var firstVal = vals.First();

            if (firstVal == "invalidvalue")
            {
                return new Just<object>(null);
            }
            else
            {
                var customClass = new TheCustomClass(firstVal);
                var just = new Just<object>(customClass);
                return just;
            }
        }
    }

    public class TheCustomClass
    {
        public string Data { get; }

        public TheCustomClass(string data)
        {
            Data = data;
        }
    }
}
