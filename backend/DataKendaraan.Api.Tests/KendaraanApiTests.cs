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

    [Theory]
    [InlineData("\u00A0")]
    [InlineData("\u2009\u00A0")]
    [InlineData("\u3000")]
    [InlineData(" \t ")]
    public async Task Post_NamaPemilikHanyaWhitespaceUnicode_Mengembalikan400(string nama)
    {
        var request = ContohRequest("B-2222-WSP");
        request.NamaPemilik = nama;

        var response = await _client.PostAsJsonAsync("/api/kendaraan", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await BacaAsync<ApiError>(response);
        Assert.NotNull(body.Errors);
        Assert.True(body.Errors!.ContainsKey("namaPemilik"));
        Assert.Contains("wajib diisi", body.Errors["namaPemilik"][0]);
    }

    [Fact]
    public async Task Post_WhitespaceUnicodeDinormalisasi_SebelumDisimpan()
    {
        var request = ContohRequest("  B-9001-WSP  ");
        request.NamaPemilik = "  Budi\u00A0Santoso  ";
        request.MerkKendaraan = "Honda\u00A0Vario";
        request.Alamat = "\u00A0Jl.\u3000Baru No 1\u00A0";
        request.BahanBakar = "\tBensin  ";

        var response = await _client.PostAsJsonAsync("/api/kendaraan", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await BacaAsync<KendaraanDto>(response);
        Assert.Equal("B-9001-WSP", body.NoRegistrasi);
        Assert.Equal("Budi Santoso", body.NamaPemilik);
        Assert.Equal("Honda Vario", body.MerkKendaraan);
        Assert.Equal("Jl. Baru No 1", body.Alamat);
        Assert.Equal("Bensin", body.BahanBakar);
    }

    [Fact]
    public async Task Get_FilterNamaDenganWhitespaceUnicode_MenemukanData()
    {
        var request = ContohRequest("B-9002-WSP");
        request.NamaPemilik = "Budi\u00A0Santoso";
        await _client.PostAsJsonAsync("/api/kendaraan", request);

        var url = "/api/kendaraan?namaPemilik=" + Uri.EscapeDataString(" Budi\u3000Santoso ");
        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await BacaAsync<List<KendaraanDto>>(response);
        var hasil = Assert.Single(body);
        Assert.Equal("B-9002-WSP", hasil.NoRegistrasi);
        Assert.Equal("Budi Santoso", hasil.NamaPemilik);
    }

    [Fact]
    public async Task Put_NoRegistrasiDenganWhitespacePinggir_TidakMemicuKonflikPalsu()
    {
        var request = ContohRequest("B-1678-BDG");
        request.NoRegistrasi = "  B-1678-BDG  ";
        request.NamaPemilik = "Cristiano\u00A0Ronaldo";

        var response = await _client.PutAsJsonAsync(
            "/api/kendaraan/" + Uri.EscapeDataString(" B-1678-BDG "),
            request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await BacaAsync<KendaraanDto>(response);
        Assert.Equal("B-1678-BDG", body.NoRegistrasi);
        Assert.Equal("Cristiano Ronaldo", body.NamaPemilik);
    }
}
