const $kendaraanBody = $("#kendaraanBody");
const $emptyState = $("#emptyState");
const $kendaraanTable = $("#kendaraanTable");
const $filterForm = $("#filterForm");
const $filterNoRegistrasi = $("#filterNoRegistrasi");
const $filterNamaPemilik = $("#filterNamaPemilik");

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

function buatTautan({ className, href, teks, ariaLabel }) {
  const link = document.createElement("a");
  link.className = className;
  link.href = href;
  link.textContent = teks;
  link.setAttribute("aria-label", ariaLabel);
  return link;
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

  const id = encodeURIComponent(item.noRegistrasi);
  const detail = buatTautan({
    className: "action-detail",
    href: `form.html?mode=detail&id=${id}`,
    teks: "Detail",
    ariaLabel: `Detail data ${item.noRegistrasi}`,
  });
  const edit = buatTautan({
    className: "action-edit",
    href: `form.html?mode=edit&id=${id}`,
    teks: "Edit",
    ariaLabel: `Edit data ${item.noRegistrasi}`,
  });
  const hapus = buatTautan({
    className: "action-delete",
    href: "#",
    teks: "Delete",
    ariaLabel: `Hapus data ${item.noRegistrasi}`,
  });
  hapus.setAttribute("role", "button");
  hapus.dataset.id = item.noRegistrasi;

  const grup = document.createElement("div");
  grup.className = "action-links";
  grup.append(detail, edit, hapus);

  const aksi = document.createElement("td");
  aksi.append(grup);
  baris.append(aksi);

  return baris;
}

function renderTabel(data) {
  $kendaraanBody.empty();
  data.forEach((item, index) => $kendaraanBody.append(buatBaris(item, index)));

  const kosong = data.length === 0;
  $kendaraanTable.toggleClass("d-none", kosong);
  $emptyState.toggleClass("d-none", !kosong);
}

async function muatData() {
  const filters = {
    noRegistrasi: $filterNoRegistrasi.val().trim(),
    namaPemilik: $filterNamaPemilik.val().trim(),
  };

  try {
    renderTabel(await getList(filters));
  } catch (error) {
    showToast(error.message ?? PESAN.muatGagal, "error");
  }
}

function bukaModalHapus(noRegistrasi) {
  deleteTarget = noRegistrasi;
  $("#deleteModalText")
    .empty()
    .append(
      document.createTextNode("Anda yakin menghapus data "),
      document.createElement("strong"),
      document.createTextNode(" ?"),
    );
  $("#deleteModalText strong").text(noRegistrasi);

  deleteModal.show();
}

async function konfirmasiHapus() {
  const $tombolOk = $("#btnDeleteOk");
  $tombolOk.prop("disabled", true).text("Menghapus…");

  try {
    await remove(deleteTarget);
    deleteModal.hide();
    showToast(PESAN.hapusSukses, "success");
    await muatData();
  } catch (error) {
    showToast(error.message ?? PESAN.hapusGagal, "error");
  } finally {
    $tombolOk.prop("disabled", false).text("OK");
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

$(function () {
  deleteModal = new bootstrap.Modal(document.getElementById("deleteModal"));

  $filterForm.on("submit", (event) => {
    event.preventDefault();
    muatData();
  });
  $("#btnSearch").on("click", () => muatData());

  $filterNoRegistrasi.add($filterNamaPemilik).on("keydown", (event) => {
    if (event.key === "Enter") {
      event.preventDefault();
      muatData();
    }
  });

  $kendaraanBody.on("click", ".action-delete", (event) => {
    event.preventDefault();
    bukaModalHapus(event.currentTarget.dataset.id);
  });
  $kendaraanBody.on("keydown", ".action-delete", (event) => {
    if (event.key === "Enter" || event.key === " ") {
      event.preventDefault();
      bukaModalHapus(event.currentTarget.dataset.id);
    }
  });

  $("#btnDeleteOk").on("click", konfirmasiHapus);

  bacaStatus();
  muatData();
});
