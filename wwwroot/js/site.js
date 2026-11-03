(function () {
  "use strict";

  /* ---------------- interface text (Arabic when <html lang="ar">) ---------------- */
  var AR = {
    "Copied": "تم النسخ",
    "Save": "حفظ",
    "Saved": "محفوظ",
    "Tender saved": "تم حفظ المناقصة",
    "Removed from saved": "أزيلت من المحفوظات",
    "Organization name": "اسم الجهة",
    "Company name": "اسم الشركة",
    "Email": "البريد الإلكتروني",
    "Work email": "البريد الإلكتروني للعمل",
    "Create your login": "أنشئ بيانات الدخول",
    "Choose your account type": "اختر نوع الحساب",
    "Title": "العنوان",
    "Description": "الوصف",
    "e.g. Warranty period": "مثال: فترة الضمان",
    "e.g. Minimum 3 years onsite warranty": "مثال: ضمان في الموقع لمدة 3 سنوات على الأقل",
    "Remove": "إزالة",
    "Could not update the request": "تعذر تحديث الطلب",
    "Updated": "تم التحديث",
    "ACCEPTED": "مقبول",
    "REJECTED": "مرفوض",
    "Request accepted": "تم قبول الطلب",
    "Request rejected": "تم رفض الطلب"
  };
  function t(text) {
    return (document.documentElement.lang === "ar" && AR[text]) || text;
  }
  window.t = t;

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
      btn.textContent = t("Copied");
      setTimeout(function () { btn.textContent = old; }, 1400);
    };
    try { navigator.clipboard.writeText(text).then(done, done); } catch (e) { done(); }
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

  /* ---------------- mobile sidebar (app shell) ---------------- */
  var menuBtn = document.getElementById("menuBtn");
  var sidebar = document.getElementById("sidebar");
  var sidebarBackdrop = document.getElementById("sidebarBackdrop");
  var sidebarClose = document.getElementById("sidebarClose");

  function openSidebar() {
    if (sidebar) sidebar.classList.add("is-open");
    if (sidebarBackdrop) sidebarBackdrop.classList.add("is-open");
    document.body.style.overflow = "hidden";
  }
  function closeSidebar() {
    if (sidebar) sidebar.classList.remove("is-open");
    if (sidebarBackdrop) sidebarBackdrop.classList.remove("is-open");
    document.body.style.overflow = "";
  }
  if (menuBtn && sidebar) {
    menuBtn.addEventListener("click", function () {
      if (sidebar.classList.contains("is-open")) closeSidebar();
      else openSidebar();
    });
  }
  if (sidebarBackdrop) sidebarBackdrop.addEventListener("click", closeSidebar);
  if (sidebarClose) sidebarClose.addEventListener("click", closeSidebar);

  /* ---------------- mobile menu (marketing header) ---------------- */
  var marketingMenuBtn = document.getElementById("marketingMenuBtn");
  var marketingMobileNav = document.getElementById("marketingMobileNav");
  if (marketingMenuBtn && marketingMobileNav) {
    marketingMenuBtn.addEventListener("click", function (e) {
      e.stopPropagation();
      var open = marketingMobileNav.classList.toggle("is-open");
      marketingMenuBtn.setAttribute("aria-expanded", open ? "true" : "false");
    });
    document.addEventListener("click", function (e) {
      if (!marketingMobileNav.classList.contains("is-open")) return;
      if (marketingMobileNav.contains(e.target) || marketingMenuBtn.contains(e.target)) return;
      marketingMobileNav.classList.remove("is-open");
      marketingMenuBtn.setAttribute("aria-expanded", "false");
    });
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
    btn.querySelector(".save-label").textContent = t(saved ? "Saved" : "Save");
    showToast(t(saved ? "Tender saved" : "Removed from saved"));
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
    if (orgLabel) orgLabel.textContent = t(type === "client" ? "Organization name" : "Company name");

    var emailLabel = document.getElementById("suEmailLabel");
    var emailInput = document.getElementById("suEmail");
    if (emailLabel) emailLabel.textContent = t(isIndividual ? "Email" : "Work email");
    if (emailInput) emailInput.placeholder = isIndividual ? "you@example.com" : "you@company.om";
  };
  window.goSignupStep2 = function () {
    document.getElementById("su-step1").hidden = true;
    document.getElementById("su-step2").hidden = false;
    document.getElementById("su-dot-1").classList.remove("is-active");
    document.getElementById("su-dot-2").classList.add("is-active");
    document.getElementById("su-step-label").textContent = t("Create your login");
  };
  window.goSignupStep1 = function () {
    document.getElementById("su-step1").hidden = false;
    document.getElementById("su-step2").hidden = true;
    document.getElementById("su-dot-1").classList.add("is-active");
    document.getElementById("su-dot-2").classList.remove("is-active");
    document.getElementById("su-step-label").textContent = t("Choose your account type");
  };

  /* ---------------- tender form: repeatable custom fields ---------------- */
  var cfCount = document.querySelectorAll(".cf-row").length;
  window.addCustomFieldRow = function () {
    var wrap = document.getElementById("tf-customfields");
    if (!wrap) return;
    var row = document.createElement("div");
    row.className = "cf-row";
    row.innerHTML =
      '<div class="field"><label class="field-label">' + t("Title") + '</label><input type="text" name="CustomFields[' + cfCount + '].Title" placeholder="' + t("e.g. Warranty period") + '"></div>' +
      '<div class="field"><label class="field-label">' + t("Description") + '</label><input type="text" name="CustomFields[' + cfCount + '].Description" placeholder="' + t("e.g. Minimum 3 years onsite warranty") + '"></div>' +
      '<button type="button" class="icon-btn btn-icon remove-row-btn" aria-label="' + t("Remove") + '" onclick="this.closest(\'.cf-row\').remove()"><svg class="icon"><use href="#i-trash"></use></svg></button>';
    wrap.appendChild(row);
    cfCount++;
  };

  /* ---------------- admin: accept/reject (posts to /Admin?handler=RequestStatus) ---------------- */
  window.adminSetStatus = function (btn, status) {
    var row = btn.closest("tr");
    var requestId = row.dataset.requestId;
    var token = document.querySelector('#adminToken input[name="__RequestVerificationToken"]');
    if (requestId && token) {
      var body = new URLSearchParams({ requestId: requestId, status: status, __RequestVerificationToken: token.value });
      fetch("/Admin?handler=RequestStatus", { method: "POST", body: body })
        .then(function (res) {
          if (!res.ok) throw new Error("HTTP " + res.status);
          markStatus(row, status);
        })
        .catch(function () { showToast(t("Could not update the request")); });
      return;
    }
    markStatus(row, status);
  };

  function markStatus(row, status) {
    var statusCell = row.querySelector(".pill-status");
    var classMap = { ACCEPTED: "status-accepted", REJECTED: "status-rejected" };
    statusCell.className = "pill-status " + (classMap[status] || "status-pending");
    statusCell.textContent = t(status);
    row.querySelector(".row-actions").innerHTML = '<span style="color:var(--ink-faint);font-size:12px">' + t("Updated") + '</span>';
    showToast(t(status === "ACCEPTED" ? "Request accepted" : "Request rejected"));
  }

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
    var input = document.getElementById("setLang");
    if (input && el.dataset.lang) input.value = el.dataset.lang;
  };
})();
