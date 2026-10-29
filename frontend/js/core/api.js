const API_BASE = "http://localhost:5000/api";

const KENDARAAN_PATH = "/kendaraan";

const PESAN_KESALAHAN_UMUM = "Terjadi kesalahan.";

const kendaraanUrl = (noRegistrasi) =>
  `${KENDARAAN_PATH}/${encodeURIComponent(noRegistrasi)}`;

function request(path, settings = {}) {
  return $.ajax({
    url: `${API_BASE}${path}`,
    method: settings.method ?? "GET",
    contentType: "application/json",
    data: settings.body,
    dataType: "text",
  })
    .then((text, _status, xhr) => {
      if (xhr.status === 204 || !text) return null;
      return JSON.parse(text);
    })
    .catch((xhr) => {
      let body = null;
      try {
        body = xhr.responseText ? JSON.parse(xhr.responseText) : null;
      } catch {
        body = null;
      }

      return Promise.reject({
        status: xhr.status,
        ...(body ?? { message: PESAN_KESALAHAN_UMUM }),
      });
    });
}

function bangunFilter(filters) {
  const params = new URLSearchParams();
  if (filters.noRegistrasi) params.set("noRegistrasi", filters.noRegistrasi);
  if (filters.namaPemilik) params.set("namaPemilik", filters.namaPemilik);

  const query = params.toString();
  return `${KENDARAAN_PATH}${query ? `?${query}` : ""}`;
}

function getList(filters = {}) {
  return request(bangunFilter(filters));
}

function getOne(noRegistrasi) {
  return request(kendaraanUrl(noRegistrasi));
}

function create(payload) {
  return request(KENDARAAN_PATH, {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

function update(noRegistrasi, payload) {
  return request(kendaraanUrl(noRegistrasi), {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

function remove(noRegistrasi) {
  return request(kendaraanUrl(noRegistrasi), { method: "DELETE" });
}
