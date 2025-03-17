using CommandLine;

namespace Watson.Extensions;

public static class ParserResultExtensions
{
    public static TResult MapResult<
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
        T17,
        T18,
        TResult
    >(
        this ParserResult<object> result,
        Func<T1, TResult> parsedFunc1,
        Func<T2, TResult> parsedFunc2,
        Func<T3, TResult> parsedFunc3,
        Func<T4, TResult> parsedFunc4,
        Func<T5, TResult> parsedFunc5,
        Func<T6, TResult> parsedFunc6,
        Func<T7, TResult> parsedFunc7,
        Func<T8, TResult> parsedFunc8,
        Func<T9, TResult> parsedFunc9,
        Func<T10, TResult> parsedFunc10,
        Func<T11, TResult> parsedFunc11,
        Func<T12, TResult> parsedFunc12,
        Func<T13, TResult> parsedFunc13,
        Func<T14, TResult> parsedFunc14,
        Func<T15, TResult> parsedFunc15,
        Func<T16, TResult> parsedFunc16,
        Func<T17, TResult> parsedFunc17,
        Func<T18, TResult> parsedFunc18,
        Func<IEnumerable<Error>, TResult> notParsedFunc
    )
    {
        if (result is not Parsed<object> parsed) return notParsedFunc(((NotParsed<object>)result).Errors);

        return parsed.Value switch
        {
            T1 value => parsedFunc1(value),
            T2 parsedValue => parsedFunc2(parsedValue),
            T3 value1 => parsedFunc3(value1),
            T4 parsedValue1 => parsedFunc4(parsedValue1),
            T5 value2 => parsedFunc5(value2),
            T6 parsedValue2 => parsedFunc6(parsedValue2),
            T7 value3 => parsedFunc7(value3),
            T8 parsedValue3 => parsedFunc8(parsedValue3),
            T9 value4 => parsedFunc9(value4),
            T10 parsedValue4 => parsedFunc10(parsedValue4),
            T11 value5 => parsedFunc11(value5),
            T12 parsedValue5 => parsedFunc12(parsedValue5),
            T13 value6 => parsedFunc13(value6),
            T14 parsedValue6 => parsedFunc14(parsedValue6),
            T15 value7 => parsedFunc15(value7),
            T16 parsedValue7 => parsedFunc16(parsedValue7),
            T17 value8 => parsedFunc17(value8),
            T18 parsedValue8 => parsedFunc18(parsedValue8),
            _ => throw new InvalidOperationException()
        };
    }
}