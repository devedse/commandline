// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System.Collections.Generic;

namespace CommandLine.Tests.Fakes
{
    class Simple_Options_With_Required_Env
    {
        [Option('s', "stringvalue", Required = true, Env = "StringValue")]
        public string StringValue { get; set; }
    }
}
