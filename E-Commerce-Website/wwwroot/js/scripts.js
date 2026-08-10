document.addEventListener("DOMContentLoaded", function () {

    /* =====================================================
       SIDEBAR TOGGLE
    ===================================================== */

    const menuButton =
        document.getElementById("menuButton");

    const app =
        document.querySelector(".app");


    if (menuButton && app) {

        menuButton.addEventListener("click", function () {

            app.classList.toggle("sidebar-collapsed");

        });

    }


    /* =====================================================
       ACTIVE SIDEBAR ITEM
    ===================================================== */

    const navItems =
        document.querySelectorAll(".nav-item");


    navItems.forEach(function (item) {

        item.addEventListener("click", function (event) {

            /*
             * If the link is "#", don't reload the page.
             */

            if (item.getAttribute("href") === "#") {

                event.preventDefault();

            }


            /*
             * Remove active from all items.
             */

            navItems.forEach(function (nav) {

                nav.classList.remove("active");

            });


            /*
             * Add active to clicked item.
             */

            item.classList.add("active");

        });

    });


    /* =====================================================
       ACCOUNT BUTTON
    ===================================================== */

    const accountButton =
        document.getElementById("accountButton");


    if (accountButton) {

        accountButton.addEventListener("click", function () {

            alert("Admin Account");

        });

    }

});