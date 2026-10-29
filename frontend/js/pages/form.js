const KONFIG_MODE = {
  tambah: {
    judul: "Tambah Data Kendaraan",
    labelTombol: "Simpan",
    aksi: "POST",
  },
  edit: { judul: "Edit Data Kendaraan", labelTombol: "Ubah", aksi: "PUT" },
  detail: { judul: "Detail Data Kendaraan", labelTombol: null, aksi: null },
};

const WARNA_OPTIONS = ["Merah", "Hitam", "Biru", "Abu-Abu"];

const FIELD_WAJIB = new Map([
  ["noRegistrasi", "No. Registrasi Kendaraan wajib diisi."],
  ["namaPemilik", "Nama Pemilik wajib diisi."],
]);

const FIELD_TEKS = [
  "noRegistrasi",
  "namaPemilik",
  "merkKendaraan",
  "warna",
  "bahanBakar",
  "alamat",
];

const FIELD_ANGKA = ["tahunPembuatan", "kapasitasSilinder"];

const FIELD_DEF = [
  { id: "noRegistrasi", label: "No. Registrasi Kendaraan", maxLength: 20 },
  { id: "tahunPembuatan", label: "Tahun Pembuatan", type: "number", min: 1900 },
  { id: "namaPemilik", label: "Nama Pemilik", maxLength: 100 },
  {
    id: "kapasitasSilinder",
    label: "Kapasitas Silinder",
    type: "number",
    min: 1,
  },
  { id: "merkKendaraan", label: "Merk Kendaraan", maxLength: 100 },
  { id: "warna", label: "Warna Kendaraan", kontrol: "warna" },
  { id: "alamat", label: "Alamat Pemilik Kendaraan", kontrol: "alamat" },
  { id: "bahanBakar", label: "Bahan Bakar", maxLength: 50 },
];

const PESAN_TIDAK_ADA = "Data kendaraan tidak ditemukan.";

const SPASI_LEBAR =
  /[\s\u00A0\u1680\u2000-\u200A\u2028\u2029\u202F\u205F\u3000\uFEFF]/g;

function el(tag, props = {}, ...children) {
  const node = document.createElement(tag);
  Object.assign(node, props);
  node.append(...children.filter((c) => c !== null && c !== undefined));
  return node;
}

function fieldWrapper(id, labelText, control) {
  const label = el("label", { className: "form-label", htmlFor: id });
  label.append(labelText);
  if (FIELD_WAJIB.has(id)) {
    label.append(
      " ",
      el("span", { className: "text-danger", textContent: "*" }),
    );
  }

  const error = el("div", {
    className: "form-text text-danger d-none",
    id: `error-${id}`,
  });
  error.dataset.errorFor = id;

  return el("div", { className: "col-md-6 mb-3" }, label, control, error);
}

function buatKontrol(def) {
  if (def.kontrol === "warna") {
    const select = el("select", {
      className: "form-select",
      id: def.id,
      name: def.id,
    });
    select.append(el("option", { value: "", textContent: "Pilih Warna" }));
    WARNA_OPTIONS.forEach((w) =>
      select.append(el("option", { value: w, textContent: w })),
    );
    return select;
  }

  if (def.kontrol === "alamat") {
    return el("textarea", {
      className: "form-control",
      id: def.id,
      name: def.id,
      rows: 4,
      maxLength: 255,
    });
  }

  const props = { className: "form-control", id: def.id, name: def.id };
  if (def.type === "number") {
    Object.assign(props, { type: "number", min: def.min });
  } else {
    Object.assign(props, {
      type: "text",
      maxLength: def.maxLength,
      autoComplete: "off",
    });
  }
  return el("input", props);
}

function buatField(def) {
  return fieldWrapper(def.id, def.label, buatKontrol(def));
}

function buatTombol() {
  const wrapper = el("div", { className: "d-flex gap-2 mt-4" });
  wrapper.append(
    el("button", {
      type: "submit",
      className: "btn btn-primary px-4",
      id: "btnSubmit",
      textContent: "Simpan",
    }),
    el("a", {
      href: "index.html",
      className: "btn btn-secondary px-4",
      textContent: "Kembali",
    }),
  );
  return wrapper;
}

function bangunForm() {
  const form = el("form", { id: "kendaraanForm", noValidate: true });
  form.append(el("div", { className: "row" }, ...FIELD_DEF.map(buatField)));
  form.append(buatTombol());
  return { form, submit: form.querySelector("#btnSubmit") };
}

function bersihkanError() {
  $("[data-error-for]").each(function () {
    $(this).text("").addClass("d-none");
    $("#" + this.dataset.errorFor).removeClass("is-invalid");
  });
}

function tampilkanError(field, pesan) {
  const $node = $(`[data-error-for="${field}"]`);
  if (!$node.length) return;

  $node.text(pesan).removeClass("d-none");
  $("#" + field).addClass("is-invalid");
}

function bersihkanErrorSatu(field) {
  const $node = $(`[data-error-for="${field}"]`);
  if (!$node.length) return;

  $node.text("").addClass("d-none");
  $("#" + field).removeClass("is-invalid");
}

function normalisasiTeks(teks) {
  return teks.replace(SPASI_LEBAR, " ").trim();
}

function validasiClient(nilai) {
  const error = {};

  FIELD_WAJIB.forEach((pesan, id) => {
    if (!nilai[id]) error[id] = pesan;
  });

  const tahunMaks = new Date().getFullYear();
  if (
    nilai.tahunPembuatan !== "" &&
    (nilai.tahunPembuatan < 1900 || nilai.tahunPembuatan > tahunMaks)
  ) {
    error.tahunPembuatan = `Tahun Pembuatan harus antara 1900 dan ${tahunMaks}.`;
  }

  if (nilai.kapasitasSilinder !== "" && nilai.kapasitasSilinder < 1) {
    error.kapasitasSilinder = "Kapasitas Silinder harus lebih dari 0.";
  }

  return error;
}

function bacaForm() {
  const nilai = {};
  FIELD_TEKS.forEach((id) => {
    nilai[id] = normalisasiTeks($("#" + id).val() ?? "");
  });

  FIELD_ANGKA.forEach((id) => {
    const raw = normalisasiTeks($("#" + id).val() ?? "");
    nilai[id] = raw === "" ? "" : Number(raw);
  });

  return nilai;
}

function isiForm(data) {
  [...FIELD_TEKS, ...FIELD_ANGKA].forEach((id) => {
    $("#" + id).val(data[id] ?? "");
  });
}

function pasangNormalisasiLangsung(form) {
  FIELD_TEKS.forEach((id) => {
    const kontrol = form.querySelector(`#${id}`);
    if (!kontrol) return;

    kontrol.addEventListener("blur", () => {
      const bersih = normalisasiTeks(kontrol.value);
      if (bersih === kontrol.value) return;

      kontrol.value = bersih;
      if (bersih === "" && FIELD_WAJIB.has(id)) {
        tampilkanError(id, FIELD_WAJIB.get(id));
        return;
      }
      bersihkanErrorSatu(id);
    });
  });
}

function kunciForm() {
  $("#kendaraanForm input, #kendaraanForm select, #kendaraanForm textarea")
    .prop("disabled", true)
    .css("cursor", "not-allowed");
}

async function simpan(nilai, idLama, aksi) {
  const payload = {
    noRegistrasi: nilai.noRegistrasi,
    namaPemilik: nilai.namaPemilik,
    alamat: nilai.alamat || null,
    merkKendaraan: nilai.merkKendaraan || null,
    tahunPembuatan: nilai.tahunPembuatan === "" ? null : nilai.tahunPembuatan,
    kapasitasSilinder:
      nilai.kapasitasSilinder === "" ? null : nilai.kapasitasSilinder,
    warna: nilai.warna || null,
    bahanBakar: nilai.bahanBakar || null,
  };

  return aksi === "PUT" ? update(idLama, payload) : create(payload);
}

function tanganiError(error) {
  if (error.errors) {
    const tanpaField = [];

    Object.entries(error.errors).forEach(([field, pesan]) => {
      if ($(`[data-error-for="${field}"]`).length) {
        tampilkanError(field, pesan[0]);
      } else {
        tanpaField.push(pesan[0]);
      }
    });

    if (tanpaField.length > 0) {
      showToast(tanpaField[0], "error");
    }
    return;
  }

  if (error.status === 409) {
    tampilkanError("noRegistrasi", error.message);
    return;
  }

  showToast(error.message ?? PESAN.koneksiGagal, "error");
}

$(async function () {
  const params = new URLSearchParams(window.location.search);
  const mode = params.get("mode") ?? "tambah";
  const konfig = KONFIG_MODE[mode] ?? KONFIG_MODE.tambah;
  const idLama = params.get("id");

  $("#formTitle").text(konfig.judul);

  const { form, submit } = bangunForm();
  $("#formContainer").append(form);

  if (konfig.aksi) pasangNormalisasiLangsung(form);

  if (konfig.labelTombol) {
    submit.textContent = konfig.labelTombol;
  } else {
    submit.remove();
  }

  if (konfig.aksi) {
    if (konfig.aksi === "PUT" && !idLama) {
      showToast(PESAN_TIDAK_ADA, "error");
      window.location.replace("index.html");
      return;
    }

    if (konfig.aksi === "PUT") {
      try {
        isiForm(await getOne(idLama));
      } catch {
        showToast(PESAN_TIDAK_ADA, "error");
        window.location.replace("index.html");
        return;
      }
    }

    $(form).on("submit", async (event) => {
      event.preventDefault();
      bersihkanError();

      const nilai = bacaForm();
      const errorClient = validasiClient(nilai);
      if (Object.keys(errorClient).length > 0) {
        Object.entries(errorClient).forEach(([f, p]) => tampilkanError(f, p));
        return;
      }

      submit.disabled = true;
      try {
        await simpan(nilai, idLama, konfig.aksi);
        const status = konfig.aksi === "PUT" ? "updated" : "created";
        window.location.href = `index.html?status=${status}`;
      } catch (error) {
        tanganiError(error);
      } finally {
        submit.disabled = false;
      }
    });
    return;
  }

  if (!idLama) {
    showToast(PESAN_TIDAK_ADA, "error");
    window.location.replace("index.html");
    return;
  }

  try {
    isiForm(await getOne(idLama));
    kunciForm();
  } catch {
    showToast(PESAN_TIDAK_ADA, "error");
    window.location.replace("index.html");
  }
});
