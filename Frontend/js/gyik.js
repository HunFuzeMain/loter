document.addEventListener("DOMContentLoaded", function() {
    const submitButton = document.getElementById("kerdes");
    
    if (submitButton) {
        submitButton.addEventListener("click", async function(event) {
            event.preventDefault();
            
            const questionInput = document.getElementById("gyikker");
            const questionText = questionInput?.value.trim();
            
            if (!questionText) {
                alert("Kérjük, írjon be egy kérdést!");
                return;
            }

            try {
                const response = await fetch('https://loter-production.up.railway.app/api/Questions', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        Text: questionText
                    })
                });

                const responseData = await response.json();
                
                if (!response.ok) {
                    throw new Error(responseData.error || "Network response was not ok");
                }

                alert("Köszönjük kérdését! Hamarosan válaszolunk.");
                questionInput.value = "";
            } catch (error) {
                console.error("Error:", error);
                alert(`Hiba történt: ${error.message}`);
            }
        });
    }
});
