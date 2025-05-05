document.addEventListener("DOMContentLoaded", function() {
    const submitButton = document.getElementById("kerdes");

    if (submitButton) {
        submitButton.addEventListener("click", async function(event) {
            event.preventDefault();

            const emailInput = document.getElementById("gyikkere");
            const questionInput = document.getElementById("gyikker");

            const email = emailInput?.value.trim();
            const text = questionInput?.value.trim();

            if (!email || !text) {
                alert("Kérjük, adja meg az e-mail címet és a kérdést is!");
                return;
            }

            try {
                const response = await fetch('https://loter-production.up.railway.app/api/Questions', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ email, text })
                });

                const responseData = await response.json();

                if (!response.ok) {
                    throw new Error(responseData.error || "Hiba történt a beküldés során.");
                }

                alert("Köszönjük kérdését! Hamarosan válaszolunk.");
                emailInput.value = "";
                questionInput.value = "";

            } catch (error) {
                console.error("Error:", error);
                alert(`Hiba történt: ${error.message}`);
            }
        });
    }
});

window.onload = loadQuestions;
