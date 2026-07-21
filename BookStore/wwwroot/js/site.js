// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Example starter JavaScript for disabling form submissions if there are invalid fields
(() => {
    'use strict'

    // Fetch all the forms we want to apply custom Bootstrap validation styles to
    const forms = document.querySelectorAll('.needs-validation')

    // Loop over them and prevent submission
    Array.from(forms).forEach(form => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault()
                event.stopPropagation()
            }

            form.classList.add('was-validated')
        }, false)
    })
})()

document.addEventListener("DOMContentLoaded", function () {
    // Find all toast elements
    var toastElements = document.querySelectorAll('.toast');
    toastElements.forEach(function (toastEl) {
        var toast = new bootstrap.Toast(toastEl, {
            delay: 4000
        });
        toast.show();
    });

    // Intercept "Add to Cart" link clicks (Ajax add)
    $(document).on("click", "a[href*='/Order/AddToCart']", function (e) {
        e.preventDefault();
        const url = $(this).attr("href");
        
        $.ajax({
            url: url,
            type: "GET",
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            },
            success: function (response) {
                if (typeof response === "string" && response.indexOf('id="login-submit"') !== -1) {
                    // Redirected to login page
                    window.location.href = "/Identity/Account/Login?ReturnUrl=" + encodeURIComponent(window.location.pathname + window.location.search);
                    return;
                }
                
                if (response.success) {
                    $(".badge.rounded-pill.bg-success").text(response.cartCount);
                    showToast(response.message, "success");
                } else {
                    showToast(response.message || "Error adding item to cart.", "danger");
                }
            },
            error: function (xhr) {
                if (xhr.status === 401) {
                    window.location.href = "/Identity/Account/Login?ReturnUrl=" + encodeURIComponent(window.location.pathname + window.location.search);
                } else {
                    showToast("Failed to add book to cart. Please try again.", "danger");
                }
            }
        });
    });

    // Intercept details page post form submissions
    $(document).on("submit", "form[action*='/Order/AddToCart']", function (e) {
        e.preventDefault();
        const form = $(this);
        const url = form.attr("action");
        const data = form.serialize();

        $.ajax({
            url: url,
            type: "POST",
            data: data,
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            },
            success: function (response) {
                if (typeof response === "string" && response.indexOf('id="login-submit"') !== -1) {
                    window.location.href = "/Identity/Account/Login?ReturnUrl=" + encodeURIComponent(window.location.pathname + window.location.search);
                    return;
                }
                
                if (response.success) {
                    $(".badge.rounded-pill.bg-success").text(response.cartCount);
                    showToast(response.message, "success");
                } else {
                    showToast(response.message || "Error adding item to cart.", "danger");
                }
            },
            error: function (xhr) {
                if (xhr.status === 401) {
                    window.location.href = "/Identity/Account/Login?ReturnUrl=" + encodeURIComponent(window.location.pathname + window.location.search);
                } else {
                    showToast("Failed to add book to cart. Please try again.", "danger");
                }
            }
        });
    });

    // Dynamic Toast helper
    function showToast(message, type) {
        let toastContainer = $(".toast-container");
        if (toastContainer.length === 0) {
            toastContainer = $('<div class="toast-container position-fixed bottom-0 end-0 p-3" style="z-index: 1055;"></div>');
            $("body").append(toastContainer);
        }

        const toastId = "toast_" + Date.now();
        const bgClass = type === "success" ? "text-bg-success" : "text-bg-danger";
        const iconClass = type === "success" ? "bi-check-circle" : "bi-exclamation-triangle";

        const toastHtml = `
            <div id="${toastId}" class="toast align-items-center ${bgClass} border-0" role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">
                        <i class="bi ${iconClass} me-2"></i>
                        ${message}
                    </div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>
        `;

        toastContainer.append(toastHtml);
        const toastEl = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastEl, { delay: 4000 });
        toast.show();

        toastEl.addEventListener('hidden.bs.toast', function () {
            $(toastEl).remove();
        });
    }
});
