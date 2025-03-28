﻿using CommandLine;

namespace Watson.Extensions;

public static class ParserExtensions
{
    public static ParserResult<object> ParseArguments<
        T1,
        T2,
        T3,
        T4,
        T5,
        T6,
        T7,
        T8,
        T9,
        T10,
        T11,
        T12,
        T13,
        T14,
        T15,
        T16,
        T17
    >(this Parser parser, IEnumerable<string> args)
    {
        return parser.ParseArguments(
            args,
            typeof(T1),
            typeof(T2),
            typeof(T3),
            typeof(T4),
            typeof(T5),
            typeof(T6),
            typeof(T7),
            typeof(T8),
            typeof(T9),
            typeof(T10),
            typeof(T11),
            typeof(T12),
            typeof(T13),
            typeof(T14),
            typeof(T15),
            typeof(T16),
            typeof(T17)
        );
    }
}