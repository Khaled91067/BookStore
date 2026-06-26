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
    // Find the toast element by its ID
    var toastElement = document.getElementById('cartToast');

    // If the element exists (meaning TempData wasn't null), show it
    if (toastElement) {
        // Initialize the toast with a 3-second delay before it auto-hides
        var toast = new bootstrap.Toast(toastElement, {
            delay: 3000
        });

        // Show the toast
        toast.show();
    }
});
