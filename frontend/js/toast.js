function showToast(message, type = "success") {
  const styles = {
    success: { bg: "var(--success)", text: "#fff" },
    error: { bg: "var(--error)", text: "#fff" },
    warning: { bg: "var(--warning)", text: "#fff" },
  };
  const style = styles[type] ?? styles.success;

  let container = document.getElementById("toastContainer");
  if (!container) {
    container = document.createElement("div");
    container.id = "toastContainer";
    container.className = "toast-container";
    document.body.append(container);
  }

  const toast = document.createElement("div");
  toast.className = "toast align-items-center border-0 show mb-2";
  toast.setAttribute("role", "alert");
  toast.setAttribute("aria-live", "assertive");
  toast.setAttribute("aria-atomic", "true");
  toast.style.background = style.bg;
  toast.style.color = style.text;

  const wrapper = document.createElement("div");
  wrapper.className = "d-flex";

  const body = document.createElement("div");
  body.className = "toast-body";
  body.textContent = message;

  const close = document.createElement("button");
  close.type = "button";
  close.className = "btn-close btn-close-white me-2 m-auto";
  close.setAttribute("aria-label", "Tutup notifikasi");
  close.addEventListener("click", () => toast.remove());

  wrapper.append(body, close);
  toast.append(wrapper);
  container.append(toast);

  setTimeout(() => toast.remove(), 4000);
}
