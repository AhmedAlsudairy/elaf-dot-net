(function () {
  "use strict";

  /* ---------------- toast ---------------- */
  function showToast(msg) {
    var t = document.getElementById("toast");
    if (!t) return;
    document.getElementById("toastText").textContent = msg;
    t.classList.add("is-shown");
    clearTimeout(window.__toastTimer);
    window.__toastTimer = setTimeout(function () { t.classList.remove("is-shown"); }, 2200);
  }
  window.showToast = showToast;

  // Show a toast for any TempData-driven message rendered into #toast-message
  var flash = document.getElementById("toast-message");
  if (flash && flash.textContent.trim()) {
    showToast(flash.textContent.trim());
  }

  /* ---------------- copy to clipboard ---------------- */
  window.copyText = function (text, btn) {
    var done = function () {
      var old = btn.textContent;
      btn.textContent = "Copied";
      setTimeout(function () { btn.textContent = old; }, 1400);
    };
    try { navigator.clipboard.writeText(text).then(done, done); } catch (e) { done(); }
  };

  /* ---------------- language toggle (marketing nav, cosmetic) ---------------- */
  window.toggleLang = function () {
    var label = document.getElementById("langLabel");
    if (label) label.textContent = label.textContent === "EN" ? "AR" : "EN";
  };

  /* ---------------- generic tab groups ----------------
     Markup contract:
       <div class="tab" data-tab-group="g" data-tab-value="overview">...</div>
       <div data-tab-panel="g" data-tab-value="overview" hidden>...</div>
  */
  function switchTab(group, value) {
    document.querySelectorAll('[data-tab-group="' + group + '"]').forEach(function (el) {
      el.classList.toggle("is-active", el.getAttribute("data-tab-value") === value);
    });
    document.querySelectorAll('[data-tab-panel="' + group + '"]').forEach(function (el) {
      el.hidden = el.getAttribute("data-tab-value") !== value;
    });
  }
  window.switchTab = switchTab;

  document.querySelectorAll("[data-tab-group]").forEach(function (el) {
    el.addEventListener("click", function () {
      switchTab(el.getAttribute("data-tab-group"), el.getAttribute("data-tab-value"));
    });
  });

  /* ---------------- mobile sidebar ---------------- */
  var menuBtn = document.getElementById("menuBtn");
  var sidebar = document.getElementById("sidebar");
  if (menuBtn && sidebar) {
    menuBtn.addEventListener("click", function () { sidebar.classList.toggle("is-open"); });
  }

  /* ---------------- role switcher (persists via cookie, reloads for server-rendered nav) ---------------- */
  var roleSwitch = document.getElementById("roleSwitch");
  if (roleSwitch) {
    roleSwitch.addEventListener("change", function () {
      document.cookie = "munaqasat_role=" + encodeURIComponent(this.value) + ";path=/;max-age=" + (60 * 60 * 24 * 30);
      window.location.reload();
    });
  }

  /* ---------------- save tender toggle (cosmetic, per-viewer only) ---------------- */
  window.toggleSave = function (e, btn) {
    e.preventDefault();
    e.stopPropagation();
    var saved = btn.getAttribute("data-saved") === "true";
    saved = !saved;
    btn.setAttribute("data-saved", saved ? "true" : "false");
    btn.querySelector("use").setAttribute("href", saved ? "#i-bookmark-filled" : "#i-bookmark");
    btn.querySelector(".save-label").textContent = saved ? "Saved" : "Save";
    showToast(saved ? "Tender saved" : "Removed from saved");
  };

  /* ---------------- signup: account type + progressive step ---------------- */
  window.selectAccountType = function (type) {
    var clientPick = document.getElementById("pick-client");
    var contractorPick = document.getElementById("pick-contractor");
    var individualPick = document.getElementById("pick-individual");
    if (clientPick) clientPick.classList.toggle("is-active", type === "client");
    if (contractorPick) contractorPick.classList.toggle("is-active", type === "contractor");
    if (individualPick) individualPick.classList.toggle("is-active", type === "individual");
    var hidden = document.getElementById("accountType");
    if (hidden) hidden.value = type;
    var cont = document.getElementById("su-continue");
    if (cont) cont.disabled = false;

    var isIndividual = type === "individual";
    var orgField = document.getElementById("suOrgField");
    var orgInput = document.getElementById("suOrg");
    var orgLabel = document.getElementById("suOrgLabel");
    if (orgField) orgField.hidden = isIndividual;
    if (orgInput) {
      if (isIndividual) orgInput.removeAttribute("required");
      else orgInput.setAttribute("required", "");
    }
    if (orgLabel) orgLabel.textContent = type === "client" ? "Organization name" : "Company name";

    var emailLabel = document.getElementById("suEmailLabel");
    var emailInput = document.getElementById("suEmail");
    if (emailLabel) emailLabel.textContent = isIndividual ? "Email" : "Work email";
    if (emailInput) emailInput.placeholder = isIndividual ? "you@example.com" : "you@company.om";
  };
  window.goSignupStep2 = function () {
    document.getElementById("su-step1").hidden = true;
    document.getElementById("su-step2").hidden = false;
    document.getElementById("su-dot-1").classList.remove("is-active");
    document.getElementById("su-dot-2").classList.add("is-active");
    document.getElementById("su-step-label").textContent = "Create your login";
  };
  window.goSignupStep1 = function () {
    document.getElementById("su-step1").hidden = false;
    document.getElementById("su-step2").hidden = true;
    document.getElementById("su-dot-1").classList.add("is-active");
    document.getElementById("su-dot-2").classList.remove("is-active");
    document.getElementById("su-step-label").textContent = "Choose your account type";
  };

  /* ---------------- tender form: repeatable custom fields ---------------- */
  var cfCount = document.querySelectorAll(".cf-row").length;
  window.addCustomFieldRow = function () {
    var wrap = document.getElementById("tf-customfields");
    if (!wrap) return;
    var row = document.createElement("div");
    row.className = "cf-row";
    row.innerHTML =
      '<div class="field"><label class="field-label">Title</label><input type="text" name="CustomFields[' + cfCount + '].Title" placeholder="e.g. Warranty period"></div>' +
      '<div class="field"><label class="field-label">Description</label><input type="text" name="CustomFields[' + cfCount + '].Description" placeholder="e.g. Minimum 3 years onsite warranty"></div>' +
      '<button type="button" class="icon-btn btn-icon remove-row-btn" onclick="this.closest(\'.cf-row\').remove()"><svg class="icon"><use href="#i-trash"></use></svg></button>';
    wrap.appendChild(row);
    cfCount++;
  };

  /* ---------------- admin: cosmetic accept/reject ---------------- */
  window.adminSetStatus = function (btn, status) {
    var row = btn.closest("tr");
    var statusCell = row.querySelector(".pill-status");
    var classMap = { ACCEPTED: "status-accepted", REJECTED: "status-rejected" };
    statusCell.className = "pill-status " + (classMap[status] || "status-pending");
    statusCell.textContent = status;
    row.querySelector(".row-actions").innerHTML = '<span style="color:var(--ink-faint);font-size:12px">Updated</span>';
    showToast("Request " + status.toLowerCase());
  };

  /* ---------------- lang tabs on tender form ---------------- */
  window.setFormLang = function (lang) {
    document.getElementById("tf-lang-en").classList.toggle("is-active", lang === "en");
    document.getElementById("tf-lang-ar").classList.toggle("is-active", lang === "ar");
    var dir = lang === "ar" ? "rtl" : "ltr";
    ["tfEntity", "tfTitle"].forEach(function (id) {
      var el = document.getElementById(id);
      if (el) el.dir = dir;
    });
  };

  /* ---------------- settings: language cards ---------------- */
  window.pickLangCard = function (el) {
    document.querySelectorAll(".lang-card").forEach(function (c) { c.classList.remove("is-active"); });
    el.classList.add("is-active");
  };
})();
