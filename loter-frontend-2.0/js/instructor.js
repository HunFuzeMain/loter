document.addEventListener("DOMContentLoaded", function() {
    const form = document.getElementById("instructorForm");
    const responseMessage = document.getElementById("responseMessage");

    if (form) {
        form.addEventListener("submit", async function(event) {
            event.preventDefault();
            
            // Show loading state
            const submitButton = form.querySelector("button[type='submit']");
            submitButton.disabled = true;
            submitButton.innerHTML = '<span class="spinner">⏳</span> Küldés...';

            try {
                const formData = new FormData();
                
                // Add text fields (exactly matching InstructorDto)
                formData.append("Name", document.getElementById("nev").value);
                formData.append("Email", document.getElementById("email").value);
                formData.append("Phone", document.getElementById("tel").value);
                formData.append("Address", document.getElementById("lakcim").value);
                
                // Add file fields with validation
                const minositesFile = document.getElementById("minosites").files[0];
                const igazolvanyFile = document.getElementById("igazolvany").files[0];
                const cvFile = document.getElementById("cv").files[0];

                // Client-side file validation
                if (!validateFile(minositesFile, ['.pdf', '.jpg', '.jpeg', '.png'])) {
                    throw new Error("Minősítés: csak PDF, JPG vagy PNG fájlok (max 5MB)");
                }
                if (!validateFile(igazolvanyFile, ['.pdf', '.jpg', '.jpeg', '.png'])) {
                    throw new Error("Igazolvány: csak PDF, JPG vagy PNG fájlok (max 5MB)");
                }
                if (!validateFile(cvFile, ['.pdf', '.doc', '.docx'])) {
                    throw new Error("Önéletrajz: csak PDF, DOC vagy DOCX fájlok (max 5MB)");
                }

                formData.append("QualificationFile", minositesFile);
                formData.append("IdCardFile", igazolvanyFile);
                formData.append("CVFile", cvFile);

                const response = await fetch('http://localhost:5156/api/Instructors', {
                    method: 'POST',
                    body: formData
                });

                if (!response.ok) {
                    const errorData = await response.json();
                    const errorMsg = errorData.errors 
                        ? Object.values(errorData.errors).flat().join('\n')
                        : errorData.Message || "Hiba történt a beküldés során";
                    throw new Error(errorMsg);
                }

                const result = await response.json();
                showSuccessMessage(result.Message || "Sikeres jelentkezés!");
                form.reset();
                
            } catch (error) {
                console.error("Error:", error);
                showErrorMessage(error.message);
            } finally {
                submitButton.disabled = false;
                submitButton.textContent = "Jelentkezés elküldése";
            }
        });
    }

    function validateFile(file, allowedExtensions, maxSize = 5 * 1024 * 1024) {
        if (!file) return false;
        if (file.size > maxSize) return false;
        const extension = file.name.split('.').pop().toLowerCase();
        return allowedExtensions.includes(`.${extension}`);
    }

    function showSuccessMessage(message) {
        responseMessage.textContent = message;
        responseMessage.style.color = "green";
        responseMessage.style.display = "block";
    }

    function showErrorMessage(message) {
        responseMessage.textContent = message;
        responseMessage.style.color = "red";
        responseMessage.style.display = "block";
    }
});