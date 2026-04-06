// ===== BACK TO TOP =====
const backToTop = document.getElementById("backToTop");

if (backToTop) {

    window.addEventListener("scroll", function () {

        if (window.scrollY > 200) {
            backToTop.style.display = "flex";
        } else {
            backToTop.style.display = "none";
        }

    });

    backToTop.addEventListener("click", function () {

        window.scrollTo({
            top: 0,
            behavior: "smooth"
        });

    });
}

// ===== HEADER SHADOW =====
const header = document.getElementById("siteHeader");

if (header) {
    window.addEventListener("scroll", function () {
        if (window.scrollY > 10) {
            header.style.boxShadow = "0 3px 10px rgba(0,0,0,0.1)";
        } else {
            header.style.boxShadow = "none";
        }
    });
}
