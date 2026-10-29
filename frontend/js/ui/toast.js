function showToast(message, type = "success") {
  const styles = {
    success: { bg: "var(--success)", text: "#fff" },
    error: { bg: "var(--error)", text: "#fff" },
    warning: { bg: "var(--warning)", text: "#fff" },
  };
  const style = styles[type] ?? styles.success;

  const $container = $("#toastContainer");
  const $target = $container.length
    ? $container
    : $("<div>", { id: "toastContainer", class: "toast-container" }).appendTo(
        "body",
      );

  const $toast = $("<div>", {
    class: "toast align-items-center border-0 show mb-2",
    role: "alert",
    "aria-live": type === "error" ? "assertive" : "polite",
    "aria-atomic": "true",
  }).css({ background: style.bg, color: style.text });

  const $body = $("<div>", { class: "toast-body" }).text(message);

  const $close = $("<button>", {
    type: "button",
    class: "btn-close btn-close-white me-2 m-auto",
    "aria-label": "Tutup notifikasi",
  }).on("click", () => $toast.remove());

  $toast.append($("<div>", { class: "d-flex" }).append($body, $close));
  $target.append($toast);

  setTimeout(() => $toast.remove(), 4000);
}
