using Microsoft.JSInterop;
using System.Text.Json;

namespace Tests.Mocks;

/// <summary>
/// Mock implementation of IJSRuntime for testing JavaScript interoperability
/// </summary>
public class MockJSRuntime : IJSRuntime
{
    private readonly Dictionary<string, object?> _setupMethods = new();
    private readonly List<JSInvocation> _invocations = new();
    private readonly Dictionary<string, Exception> _exceptions = new();
    private bool _strictMode = false;

    /// <summary>
    /// When true, any call to an identifier not in _setupMethods or _exceptions throws JSException
    /// </summary>
    public bool StrictMode => _strictMode;

    /// <summary>
    /// Enable or disable strict mode
    /// </summary>
    public MockJSRuntime SetStrictMode(bool enabled)
    {
        _strictMode = enabled;
        return this;
    }

    public IReadOnlyList<JSInvocation> Invocations => _invocations.AsReadOnly();

    /// <summary>
    /// Setup a JavaScript method to return a specific value
    /// </summary>
    public MockJSRuntime Setup<T>(string identifier, T returnValue)
    {
        _setupMethods[identifier] = returnValue;
        return this;
    }

    /// <summary>
    /// Setup a JavaScript method to throw an exception
    /// </summary>
    public MockJSRuntime SetupException(string identifier, Exception exception)
    {
        _exceptions[identifier] = exception;
        return this;
    }

    /// <summary>
    /// Verify that a JavaScript method was called
    /// </summary>
    public bool WasCalled(string identifier)
    {
        return _invocations.Any(i => i.Identifier == identifier);
    }

    /// <summary>
    /// Verify that a JavaScript method was called with specific arguments
    /// </summary>
    public bool WasCalledWith(string identifier, params object[] expectedArgs)
    {
        return _invocations.Any(i => 
            i.Identifier == identifier && 
            ArgumentsMatch(i.Args, expectedArgs));
    }

    /// <summary>
    /// Get the number of times a method was called
    /// </summary>
    public int GetCallCount(string identifier)
    {
        return _invocations.Count(i => i.Identifier == identifier);
    }

    /// <summary>
    /// Clear all recorded invocations
    /// </summary>
    public void ClearInvocations()
    {
        _invocations.Clear();
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        return InvokeAsync<TValue>(identifier, CancellationToken.None, args);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        // Record the invocation
        _invocations.Add(new JSInvocation(identifier, args ?? Array.Empty<object?>()));

        // Check if an exception should be thrown
        if (_exceptions.ContainsKey(identifier))
        {
            throw _exceptions[identifier];
        }

        // In strict mode, throw if identifier is not explicitly set up
        if (_strictMode && !_setupMethods.ContainsKey(identifier))
        {
            var methodName = identifier.Contains('.') ? identifier.Split('.').Last() : identifier;
            throw new JSException($"Could not find '{identifier}' ('{methodName}' was undefined)");
        }

        // Check if a return value is setup
        if (_setupMethods.ContainsKey(identifier))
        {
            var returnValue = _setupMethods[identifier];
            
            // Handle null return values
            if (returnValue == null)
            {
                return ValueTask.FromResult(default(TValue)!);
            }

            // Handle direct type matches
            if (returnValue is TValue directValue)
            {
                return ValueTask.FromResult(directValue);
            }

            // Handle JSON serialization/deserialization for complex types
            try
            {
                var json = JsonSerializer.Serialize(returnValue);
                var deserializedValue = JsonSerializer.Deserialize<TValue>(json);
                return ValueTask.FromResult(deserializedValue!);
            }
            catch (JsonException)
            {
                // If JSON conversion fails, try direct cast
                return ValueTask.FromResult((TValue)returnValue);
            }
        }

        // Return default value if no setup found
        return ValueTask.FromResult(default(TValue)!);
    }

    private static bool ArgumentsMatch(object?[] actual, object[] expected)
    {
        if (actual.Length != expected.Length)
            return false;

        for (int i = 0; i < actual.Length; i++)
        {
            if (!Equals(actual[i], expected[i]))
                return false;
        }

        return true;
    }
}

/// <summary>
/// Represents a JavaScript method invocation for testing
/// </summary>
public record JSInvocation(string Identifier, object?[] Args)
{
    public T GetArg<T>(int index)
    {
        if (index >= Args.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        var arg = Args[index];
        if (arg is T directValue)
            return directValue;

        if (arg == null)
            return default(T)!;

        // Try JSON conversion for complex types
        try
        {
            var json = JsonSerializer.Serialize(arg);
            return JsonSerializer.Deserialize<T>(json)!;
        }
        catch
        {
            return (T)arg;
        }
    }
}