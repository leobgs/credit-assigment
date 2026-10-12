using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PengajuanKredit.Api.Data;
using PengajuanKredit.Api.Dtos;

namespace PengajuanKredit.Tests.Controllers;

public class PengajuanKreditControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly string _databaseName = "TestDb_Crud_" + Guid.NewGuid();

    public PengajuanKreditControllerTests(WebApplicationFactory<Program> factory)
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
                    options.UseInMemoryDatabase(_databaseName);
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
    public async Task Create_TanpaToken_Menghasilkan401Unauthorized()
    {
        var unauthenticatedClient = _factory.CreateClient();
        var request = new CreatePengajuanKreditRequest(50_000_000m, 12m, 24);

        var response = await unauthenticatedClient.PostAsJsonAsync("/api/pengajuan-kredit", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_PlafonNegatif_Menghasilkan400BadRequest()
    {
        var client = await CreateAuthenticatedClientAsync();
        var invalidRequest = new CreatePengajuanKreditRequest(-50_000_000m, 12m, 24);

        var response = await client.PostAsJsonAsync("/api/pengajuan-kredit", invalidRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_DataValid_Menghasilkan201CreatedDanAngsuranTerisi()
    {
        var client = await CreateAuthenticatedClientAsync();
        var validRequest = new CreatePengajuanKreditRequest(100_000_000m, 12m, 60);

        var response = await client.PostAsJsonAsync("/api/pengajuan-kredit", validRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<PengajuanKreditResponse>();
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Angsuran.Should().BeApproximately(2224444.47m, 1m);
    }

    [Fact]
    public async Task GetById_IdTidakAda_Menghasilkan404NotFound()
    {
        var client = await CreateAuthenticatedClientAsync();
        var randomId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/pengajuan-kredit/{randomId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_DataAda_Menghasilkan204NoContentDanGetBerikutnya404()
    {
        var client = await CreateAuthenticatedClientAsync();

        // 1. Buat data baru
        var createResponse = await client.PostAsJsonAsync("/api/pengajuan-kredit", new CreatePengajuanKreditRequest(30_000_000m, 10m, 12));
        var created = await createResponse.Content.ReadFromJsonAsync<PengajuanKreditResponse>();

        // 2. Hapus data
        var deleteResponse = await client.DeleteAsync($"/api/pengajuan-kredit/{created!.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // 3. Pastikan sudah tidak ditemukan
        var getResponse = await client.GetAsync($"/api/pengajuan-kredit/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
