using System.Text.Json;
using DataKendaraan.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace DataKendaraan.Api.Tests;

public class ApiExceptionHandlerTests
{
    [Fact]
    public async Task UnhandledException_Mengembalikan500JsonTanpaDetailInternal()
    {
        var handler = new ApiExceptionHandler(NullLogger<ApiExceptionHandler>.Instance);
        var body = new MemoryStream();
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "GET";
        httpContext.Request.Path = "/api/boom";
        httpContext.Response.Body = body;

        var handled = await handler.TryHandleAsync(
            httpContext,
            new InvalidOperationException("rahasia internal"),
            CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);

        body.Position = 0;
        var json = await new StreamReader(body).ReadToEndAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("Terjadi kesalahan pada server.", doc.RootElement.GetProperty("message").GetString());
        Assert.False(doc.RootElement.TryGetProperty("errors", out _));
        Assert.DoesNotContain("rahasia internal", json);
    }
}
