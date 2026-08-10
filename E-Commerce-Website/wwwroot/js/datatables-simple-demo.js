/*!
 * DataTables-style demo helper for the e-commerce template.
 * This lightweight file provides simple table sorting/searching behavior
 * without requiring an external library.
 */
(function () {
  "use strict";

  window.initSimpleDataTable = function (tableSelector, searchSelector) {
    const table = document.querySelector(tableSelector);
    const search = searchSelector ? document.querySelector(searchSelector) : null;
    if (!table) return;

    const tbody = table.querySelector("tbody") || table;
    const rows = Array.from(tbody.querySelectorAll("tr"));

    if (search) {
      search.addEventListener("input", function () {
        const query = this.value.toLowerCase().trim();

        rows.forEach(function (row) {
          row.style.display =
            row.textContent.toLowerCase().includes(query) ? "" : "none";
        });
      });
    }

    table.querySelectorAll("th").forEach(function (header, columnIndex) {
      header.style.cursor = "pointer";

      header.addEventListener("click", function () {
        const visibleRows = Array.from(tbody.querySelectorAll("tr"));

        visibleRows.sort(function (a, b) {
          const aText = (a.children[columnIndex]?.textContent || "").trim();
          const bText = (b.children[columnIndex]?.textContent || "").trim();

          return aText.localeCompare(bText, undefined, {
            numeric: true,
            sensitivity: "base"
          });
        });

        visibleRows.forEach(function (row) {
          tbody.appendChild(row);
        });
      });
    });
  };
})();
