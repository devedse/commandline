// Copyright 2005-2015 Giacomo Stelluti Scala & Contributors. All rights reserved. See License.md in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using CSharpx;
using RailwaySharp.ErrorHandling;

namespace CommandLine.Core
{
    static class OptionMapper
    {
        public static Result<
                IEnumerable<SpecificationProperty>, Error>
            MapValues(
                IEnumerable<SpecificationProperty> propertyTuples,
                IEnumerable<KeyValuePair<string, IEnumerable<string>>> options,
                Func<IEnumerable<string>, Type, bool, Maybe<object>> defaultConverter,
                StringComparer comparer)
        {
            var sequencesAndErrors = propertyTuples
                .Select(
                    pt =>
                    {
                        var matched = options.FirstOrDefault(s =>
                            s.Key.MatchName(((OptionSpecification)pt.Specification).ShortName, ((OptionSpecification)pt.Specification).LongName, comparer)).ToMaybe();
                        var env = ((OptionSpecification)pt.Specification).Env;
                        string envValue;

                        var converter = defaultConverter;

                        var customConverterType = ((OptionSpecification)pt.Specification).CustomConverter;
                        if (customConverterType != null)
                        {
                            ICustomConverter c = Activator.CreateInstance(customConverterType) as ICustomConverter;
                            converter = c.Convert;
                        }

                        if (matched.IsJust())
                        {
                            var retval =
                                (from sequence in matched
                                 from converted in
                                 converter(
                                     sequence.Value,
                                     pt.Property.PropertyType,
                                     pt.Specification.TargetType != TargetType.Sequence)
                                 select Tuple.Create(
                                 pt.WithValue(Maybe.Just(converted)), Maybe.Nothing<Error>())
                                                       )
                                                        .GetValueOrDefault(
                                                            Tuple.Create<SpecificationProperty, Maybe<Error>>(
                                                                pt,
                                                                Maybe.Just<Error>(
                                                                    new BadFormatConversionError(
                                                                        ((OptionSpecification)pt.Specification).FromOptionSpecification()))));
                            return retval;
                        }
                        else if (!string.IsNullOrWhiteSpace(env) && !string.IsNullOrWhiteSpace(envValue = Environment.GetEnvironmentVariable(env)))
                        {
                            var converted =
                                converter(new List<string>() { envValue },
                                    pt.Property.PropertyType,
                                    pt.Specification.TargetType != TargetType.Sequence);

                            var convertedValue = converted.GetValueOrDefault(Tuple.Create<SpecificationProperty, Maybe<Error>>(
                                                                pt,
                                                                Maybe.Just<Error>(
                                                                    new BadFormatConversionError(
                                                                        ((OptionSpecification)pt.Specification).FromOptionSpecification()))));
                            var ptWithValue = pt.WithValue(Maybe.Just(convertedValue));
                            var retval = Tuple.Create(ptWithValue, Maybe.Nothing<Error>());

                            return retval;
                        }
                        else
                        {
                            return Tuple.Create(pt, Maybe.Nothing<Error>());
                        }
                    }
                ).Memoize();
            return Result.Succeed(
                sequencesAndErrors.Select(se => se.Item1),
                sequencesAndErrors.Select(se => se.Item2).OfType<Just<Error>>().Select(se => se.Value));
        }
    }
}
