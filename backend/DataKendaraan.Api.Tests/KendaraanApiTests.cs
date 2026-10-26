using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DataKendaraan.Api.Dtos;

namespace DataKendaraan.Api.Tests;

public class KendaraanApiTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly ApiFactory _factory = new();
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _factory.InitializeDatabaseAsync();
        _client = _factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }

    private static KendaraanRequest ContohRequest(string noRegistrasi = "B-1234-NEW") => new()
    {
        NoRegistrasi = noRegistrasi,
        NamaPemilik = "Pemilik Baru",
        Alamat = "Jl. Baru No 1",
        MerkKendaraan = "Honda Beat",
        TahunPembuatan = 2020,
        KapasitasSilinder = 125,
        Warna = "Merah",
        BahanBakar = "Bensin"
    };

    private async Task<T> BacaAsync<T>(HttpResponseMessage response)
    {
        var isi = await response.Content.ReadAsStringAsync();
        var hasil = JsonSerializer.Deserialize<T>(isi, JsonOptions);
        Assert.NotNull(hasil);
        return hasil!;
    }

    [Fact]
    public async Task Post_Valid_Mengembalikan201()
    {
        var request = ContohRequest("B-1111-AAA");

        var response = await _client.PostAsJsonAsync("/api/kendaraan", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var body = await BacaAsync<KendaraanDto>(response);
        Assert.Equal("B-1111-AAA", body.NoRegistrasi);
        Assert.Equal("Pemilik Baru", body.NamaPemilik);
        Assert.Equal(2020, body.TahunPembuatan);
        Assert.Equal(125, body.KapasitasSilinder);
        Assert.Equal("Merah", body.Warna);
    }

    [Fact]
    public async Task Post_Duplikat_Mengembalikan409()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/kendaraan",
            ContohRequest("B-7763-TXY"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var body = await BacaAsync<ApiError>(response);
        Assert.Equal("Nomor registrasi sudah terdaftar.", body.Message);
    }

    [Fact]
    public async Task Post_TanpaNamaPemilik_Mengembalikan400()
    {
        var request = ContohRequest("B-2222-BBB");
        request.NamaPemilik = string.Empty;

        var response = await _client.PostAsJsonAsync("/api/kendaraan", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await BacaAsync<ApiError>(response);
        Assert.NotNull(body.Errors);
        Assert.True(body.Errors!.ContainsKey("namaPemilik"));
        Assert.Contains("wajib diisi", body.Errors["namaPemilik"][0]);
    }

    [Fact]
    public async Task Put_PkBaruSudahDipakai_Mengembalikan409()
    {
        var request = ContohRequest("B-7763-TXY");

        var response = await _client.PutAsJsonAsync(
            "/api/kendaraan/B-1678-BDG",
            request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var body = await BacaAsync<ApiError>(response);
        Assert.Equal("Nomor registrasi sudah terdaftar.", body.Message);
    }

    [Fact]
    public async Task Delete_IdTidakAda_Mengembalikan404()
    {
        var response = await _client.DeleteAsync("/api/kendaraan/B-TIDAK-ADA");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await BacaAsync<ApiError>(response);
        Assert.Equal("Data kendaraan tidak ditemukan.", body.Message);
    }

    [Fact]
    public async Task Get_FilterNamaCaseInsensitive_MengembalikanSatuHasil()
    {
        var response = await _client.GetAsync("/api/kendaraan?namaPemilik=messi");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await BacaAsync<List<KendaraanDto>>(response);
        Assert.Single(body);
        Assert.Equal("Lionel Messi", body[0].NamaPemilik);
        Assert.Equal("B-7763-TXY", body[0].NoRegistrasi);
    }
}
