namespace CorlaneCabinetOrderFormV3.Tests;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public void BuildDispatcherExceptionResponse_ReturnsMessageContainingExceptionText()
    {
        var ex = new InvalidOperationException("Something broke");

        var (message, title, handled) = App.BuildDispatcherExceptionResponse(ex);

        Assert.Contains("Something broke", message);
        Assert.Equal("Unexpected Error", title);
        Assert.True(handled);
    }

    [Fact]
    public void BuildDispatcherExceptionResponse_HandlesNestedExceptionMessage()
    {
        var inner = new ArgumentException("bad arg");
        var ex = new InvalidOperationException("outer", inner);

        var (message, _, _) = App.BuildDispatcherExceptionResponse(ex);

        // Top-level message is what gets shown
        Assert.Contains("outer", message);
    }

    [Fact]
    public void BuildFatalExceptionResponse_ReturnsMessage_WhenGivenException()
    {
        var ex = new OutOfMemoryException("no memory");

        var result = App.BuildFatalExceptionResponse(ex);

        Assert.NotNull(result);
        Assert.Contains("no memory", result.Value.Message);
        Assert.Equal("Fatal Error", result.Value.Title);
    }

    [Fact]
    public void BuildFatalExceptionResponse_ReturnsNull_WhenNotAnException()
    {
        // AppDomain.UnhandledException can technically pass a non-Exception object
        var result = App.BuildFatalExceptionResponse("just a string");

        Assert.Null(result);
    }
}