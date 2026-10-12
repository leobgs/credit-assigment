using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PengajuanKredit.Api.Data;
using PengajuanKredit.Api.Dtos;
using PengajuanKredit.Api.Middleware;

namespace PengajuanKredit.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ExceptionHandlingMiddlewareTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_ErrorHandling_" + Guid.NewGuid());
                });
            });
        });
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", "Password123!"));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);
        return client;
    }

    [Fact]
    public async Task GetById_IdTidakAda_HarusMenghasilkan404ProblemDetails()
    {
        var client = await CreateAuthenticatedClientAsync();
        var randomId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/pengajuan-kredit/{randomId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(404);
        problem.Title.Should().Be("Data Tidak Ditemukan");
        problem.Detail.Should().Contain(randomId.ToString());
    }

    [Fact]
    public async Task Create_InputTidakValid_HarusMenghasilkan400ProblemDetails()
    {
        var client = await CreateAuthenticatedClientAsync();
        var invalidRequest = new CreatePengajuanKreditRequest(-100m, 150m, 0);

        var response = await client.PostAsJsonAsync("/api/pengajuan-kredit", invalidRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Plafon");
    }

    [Fact]
    public async Task ExceptionTakTerduga_HarusMenghasilkan500TanpaBocorStackTrace()
    {
        // Setup isolated host dengan endpoint khusus yang melempar Exception tak terduga
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddLogging();
                        services.AddRouting();
                    })
                    .Configure(app =>
                    {
                        app.UseMiddleware<ExceptionHandlingMiddleware>();
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/api/test-error-500", (HttpContext context) =>
                            {
                                throw new InvalidOperationException("Sensitive internal database connection failed: secret_db_pwd at line 42");
                            });
                        });
                    });
            })
            .StartAsync();

        var client = host.GetTestClient();
        var response = await client.GetAsync("/api/test-error-500");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var content = await response.Content.ReadAsStringAsync();
        var problem = JsonSerializer.Deserialize<ProblemDetails>(content, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        problem.Should().NotBeNull();
        problem!.Status.Should().Be(500);
        problem.Title.Should().Be("Terjadi Kesalahan pada Server");
        problem.Detail.Should().Be("Terjadi kesalahan internal pada server. Silakan coba beberapa saat lagi.");

        // Pastikan tidak bocor stack trace atau pesan internal sensitif
        content.Should().NotContain("secret_db_pwd");
        content.Should().NotContain("at line 42");
        content.Should().NotContain("StackTrace");
    }
}
