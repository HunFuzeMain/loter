document.addEventListener("DOMContentLoaded", function() {
    const form = document.getElementById("instructorForm");
    const responseMessage = document.getElementById("responseMessage");

    if (form) {
        form.addEventListener("submit", async function(event) {
            event.preventDefault();
            
            const formData = {
                Nev: document.getElementById("nev").value,
                Email: document.getElementById("email").value,
                Tel: document.getElementById("tel").value,
                Lakcim: document.getElementById("lakcim").value,
                Minosites: document.getElementById("minosites").value,
                MinositesIdeje: document.getElementById("minositesideje").value
            };

            try {
                const response = await fetch('http://localhost:5156/api/Registrations/InstructorApplication', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(formData)
                });

                const result = await response.json();

                if (!response.ok) {
                    throw new Error(result.Message || "Hiba történt a jelentkezés során");
                }

                responseMessage.textContent = "Sikeres jelentkezés! Hamarosan felvesszük Önnel a kapcsolatot.";
                responseMessage.style.color = "green";
                form.reset();
            } catch (error) {
                console.error("Error:", error);
                responseMessage.textContent = error.message;
                responseMessage.style.color = "red";
            }
        });
    }
});