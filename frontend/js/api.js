const API_BASE = "http://localhost:5000/api";

const kendaraanUrl = (noRegistrasi) =>
  `/kendaraan/${encodeURIComponent(noRegistrasi)}`;

async function request(path, options = {}) {
  const response = await fetch(`${API_BASE}${path}`, {
    headers: { "Content-Type": "application/json" },
    ...options,
  });

  if (response.status === 204) return null;

  const body = await response.json().catch(() => null);

  if (!response.ok) {
    throw {
      status: response.status,
      ...(body ?? { message: "Terjadi kesalahan." }),
    };
  }

  return body;
}

function getList(filters = {}) {
  const params = new URLSearchParams();
  if (filters.noRegistrasi) params.set("noRegistrasi", filters.noRegistrasi);
  if (filters.namaPemilik) params.set("namaPemilik", filters.namaPemilik);

  const query = params.toString();
  return request(`/kendaraan${query ? `?${query}` : ""}`);
}

function getOne(noRegistrasi) {
  return request(kendaraanUrl(noRegistrasi));
}

function create(payload) {
  return request("/kendaraan", {
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
  return request(kendaraanUrl(noRegistrasi), {
    method: "DELETE",
  });
}
