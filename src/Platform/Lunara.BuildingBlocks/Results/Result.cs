namespace Lunara.BuildingBlocks.Results;

/// <summary>
/// Represents the outcome of an operation that does not return a value.
/// Use <see cref="Ok()"/> or <see cref="Fail"/> to create instances,
/// and the generic overloads for value-carrying results.
/// </summary>
public sealed class Result
{
    private static readonly Result _ok = new(isSuccess: true, error: null);

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the error message; <c>null</c> when <see cref="IsSuccess"/> is <c>true</c>.</summary>
    public string? Error { get; }

    /// <summary>Returns the shared successful result singleton.</summary>
    public static Result Ok()
    {
        return _ok;
    }

    /// <summary>Creates a failed result with the supplied <paramref name="error"/> message.</summary>
    /// <param name="error">Human-readable description of the failure.</param>
    public static Result Fail(string error)
    {
        return new Result(isSuccess: false, error: error);
    }

    /// <summary>Creates a successful result carrying <paramref name="value"/>.</summary>
    /// <typeparam name="TValue">Type of the success value.</typeparam>
    /// <param name="value">The success value.</param>
    public static Result<TValue> Ok<TValue>(TValue value)
    {
        return Result<TValue>.Create(isSuccess: true, value: value, error: null);
    }

    /// <summary>Creates a failed result of type <typeparamref name="TValue"/>.</summary>
    /// <typeparam name="TValue">Type of the (absent) success value.</typeparam>
    /// <param name="error">Human-readable description of the failure.</param>
    public static Result<TValue> Fail<TValue>(string error)
    {
        return Result<TValue>.Create(isSuccess: false, value: default, error: error);
    }
}

/// <summary>
/// Represents the outcome of an operation that returns a <typeparamref name="TValue"/> on success.
/// Create instances via <see cref="Result.Ok{TValue}"/> or <see cref="Result.Fail{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">Type of the success value.</typeparam>
public sealed class Result<TValue>
{
    private Result(bool isSuccess, TValue? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the success value.
    /// Only meaningful when <see cref="IsSuccess"/> is <c>true</c>.
    /// </summary>
    public TValue? Value { get; }

    /// <summary>Gets the error message; <c>null</c> when <see cref="IsSuccess"/> is <c>true</c>.</summary>
    public string? Error { get; }

    // Internal factory keeps CA1000 satisfied by keeping all public statics on the non-generic Result.
    internal static Result<TValue> Create(bool isSuccess, TValue? value, string? error)
    {
        return new Result<TValue>(isSuccess, value, error);
    }
}
