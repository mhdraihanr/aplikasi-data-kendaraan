const kendaraanBody = document.getElementById("kendaraanBody");
const emptyState = document.getElementById("emptyState");
const kendaraanTable = document.getElementById("kendaraanTable");
const filterForm = document.getElementById("filterForm");
const filterNoRegistrasi = document.getElementById("filterNoRegistrasi");
const filterNamaPemilik = document.getElementById("filterNamaPemilik");

let deleteTarget = null;
let deleteModal = null;

function tampilkan(value) {
  return value === null || value === undefined || value === "" ? "-" : value;
}

function buatSel(teks, className) {
  const sel = document.createElement("td");
  sel.textContent = teks;
  if (className) sel.className = className;
  return sel;
}

function buatBaris(item, index) {
  const baris = document.createElement("tr");

  baris.append(
    buatSel(String(index + 1), "numeric"),
    buatSel(item.noRegistrasi, "registrasi"),
    buatSel(tampilkan(item.namaPemilik)),
    buatSel(tampilkan(item.merkKendaraan)),
    buatSel(tampilkan(item.tahunPembuatan), "center"),
    buatSel(
      item.kapasitasSilinder == null ? "-" : `${item.kapasitasSilinder} cc`,
    ),
    buatSel(tampilkan(item.warna)),
    buatSel(tampilkan(item.bahanBakar)),
  );

  const aksi = document.createElement("td");
  const grup = document.createElement("div");
  grup.className = "action-links";

  const detail = document.createElement("a");
  detail.className = "action-detail";
  detail.href = `form.html?mode=detail&id=${encodeURIComponent(item.noRegistrasi)}`;
  detail.textContent = "Detail";
  detail.setAttribute("aria-label", `Detail data ${item.noRegistrasi}`);

  const edit = document.createElement("a");
  edit.className = "action-edit";
  edit.href = `form.html?mode=edit&id=${encodeURIComponent(item.noRegistrasi)}`;
  edit.textContent = "Edit";
  edit.setAttribute("aria-label", `Edit data ${item.noRegistrasi}`);

  const hapus = document.createElement("a");
  hapus.className = "action-delete";
  hapus.href = "#";
  hapus.textContent = "Delete";
  hapus.setAttribute("role", "button");
  hapus.setAttribute("aria-label", `Hapus data ${item.noRegistrasi}`);
  hapus.addEventListener("click", (event) => {
    event.preventDefault();
    bukaModalHapus(item.noRegistrasi);
  });
  hapus.addEventListener("keydown", (event) => {
    if (event.key === "Enter" || event.key === " ") {
      event.preventDefault();
      bukaModalHapus(item.noRegistrasi);
    }
  });

  grup.append(detail, edit, hapus);
  aksi.append(grup);
  baris.append(aksi);

  return baris;
}

function renderTabel(data) {
  kendaraanBody.replaceChildren(...data.map(buatBaris));

  const kosong = data.length === 0;
  kendaraanTable.classList.toggle("d-none", kosong);
  emptyState.classList.toggle("d-none", !kosong);
}

async function muatData() {
  const filters = {
    noRegistrasi: filterNoRegistrasi.value.trim(),
    namaPemilik: filterNamaPemilik.value.trim(),
  };

  try {
    const data = await getList(filters);
    renderTabel(data);
  } catch (error) {
    showToast(error.message ?? PESAN.muatGagal, "error");
  }
}

function bukaModalHapus(noRegistrasi) {
  deleteTarget = noRegistrasi;
  document.getElementById("deleteModalText").replaceChildren(
    document.createTextNode("Anda yakin menghapus data "),
    Object.assign(document.createElement("strong"), {
      textContent: noRegistrasi,
    }),
    document.createTextNode(" ?"),
  );

  deleteModal.show();
}

async function konfirmasiHapus() {
  const tombolOk = document.getElementById("btnDeleteOk");
  tombolOk.disabled = true;
  tombolOk.textContent = "Menghapus…";

  try {
    await remove(deleteTarget);
    deleteModal.hide();
    showToast(PESAN.hapusSukses, "success");
    await muatData();
  } catch (error) {
    showToast(error.message ?? PESAN.hapusGagal, "error");
  } finally {
    tombolOk.disabled = false;
    tombolOk.textContent = "OK";
    deleteTarget = null;
  }
}

function bacaStatus() {
  const status = new URLSearchParams(window.location.search).get("status");
  const pesan = {
    created: PESAN.simpanSukses,
    updated: PESAN.ubahSukses,
    deleted: PESAN.hapusSukses,
  };

  if (!pesan[status]) return;

  showToast(pesan[status], "success");
  history.replaceState(null, "", window.location.pathname);
}

document.addEventListener("DOMContentLoaded", () => {
  deleteModal = new bootstrap.Modal(document.getElementById("deleteModal"));

  const cari = (event) => {
    event.preventDefault();
    muatData();
  };

  filterForm.addEventListener("submit", cari);
  document.getElementById("btnSearch").addEventListener("click", cari);

  [filterNoRegistrasi, filterNamaPemilik].forEach((input) => {
    input.addEventListener("keydown", (event) => {
      if (event.key === "Enter") cari(event);
    });
  });

  document
    .getElementById("btnDeleteOk")
    .addEventListener("click", konfirmasiHapus);

  bacaStatus();
  muatData();
});
