using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PengajuanKredit.Api.Data;
using PengajuanKredit.Api.Dtos;

namespace PengajuanKredit.Tests.Controllers;

public class AuthAndAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _databaseName = "TestDb_Auth_" + Guid.NewGuid();

    public AuthAndAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Ganti AppDbContext dengan InMemory agar test independen dari PostgreSQL
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });
            });
        });
    }

    [Fact]
    public async Task GetAllPengajuanKredit_TanpaToken_HarusMenghasilkan401Unauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/pengajuan-kredit");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task HitungAngsuran_TanpaToken_HarusMenghasilkan401Unauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/angsuran/hitung?plafon=100000000&bunga=12&tenor=60");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_KredensialSalah_HarusMenghasilkan401Unauthorized()
    {
        var client = _factory.CreateClient();
        var request = new LoginRequest("user_salah", "password_salah");

        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_KredensialBenar_HarusMenghasilkan200DanToken()
    {
        var client = _factory.CreateClient();
        var request = new LoginRequest("admin", "Password123!");

        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResult.Should().NotBeNull();
        loginResult!.Token.Should().NotBeNullOrWhiteSpace();
        loginResult.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task EndpointProtected_DenganTokenValid_HarusMenghasilkan200OK()
    {
        var client = _factory.CreateClient();

        // 1. Login untuk dapatkan token
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", "Password123!"));
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);

        // 2. Akses GET list pengajuan kredit
        var getListResponse = await client.GetAsync("/api/pengajuan-kredit");
        getListResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Akses GET hitung angsuran
        var getHitungResponse = await client.GetAsync("/api/angsuran/hitung?plafon=100000000&bunga=12&tenor=60");
        getHitungResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
