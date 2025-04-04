document.addEventListener("DOMContentLoaded", () => {
    // API endpoints (use relative paths for production)
    const apiBaseUrl = "http://localhost:5156/api";
    const packagesApiUrl = `${apiBaseUrl}/Packages`;
    const registrationsApiUrl = `${apiBaseUrl}/Instructors`; // Changed from Instructors
    const appointmentsApiUrl = `${apiBaseUrl}/Appointments`;

    // Helper functions
    const padTime = (time) => time.length === 4 ? `0${time}` : time;

    const showAlert = (message, isSuccess = true) => {
        alert(message);
        if (isSuccess) document.getElementById("appointment-form")?.reset();
    };

    // Enhanced fetch with timeout
    const fetchWithTimeout = async (url, options = {}, timeout = 8000) => {
        const controller = new AbortController();
        const id = setTimeout(() => controller.abort(), timeout);
        
        try {
            const response = await fetch(url, {
                ...options,
                signal: controller.signal
            });
            clearTimeout(id);
            
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({}));
                throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
            }
            return await response.json();
        } catch (error) {
            if (error.name === 'AbortError') {
                throw new Error('Request timeout');
            }
            throw error;
        }
    };

    // Fetch registrations (instructors) data
    const fetchRegistrations = async () => {
        try {
            const data = await fetchWithTimeout(registrationsApiUrl);
            const dropdown = document.getElementById("instructorDropdown");
            
            if (!dropdown) {
                console.error("Instructor dropdown not found");
                return;
            }

            dropdown.innerHTML = `<option value="">-- Válassz --</option>`;
            data.forEach(item => {
                const option = document.createElement("option");
                option.value = item.id;
                option.textContent = item.name || `Oktató #${item.id}`;
                dropdown.appendChild(option);
            });
        } catch (error) {
            console.error("Error loading instructors:", error);
            showAlert(`Hiba történt az oktatók betöltésekor: ${error.message}`, false);
        }
    };

    // Fetch packages data
    const fetchPackages = async () => {
        try {
            const data = await fetchWithTimeout(packagesApiUrl);
            const dropdown = document.getElementById("packageDropdown");
            
            if (!dropdown) {
                console.error("Package dropdown not found");
                return;
            }

            dropdown.innerHTML = `<option value="">-- Válassz --</option>`;
            data.forEach(item => {
                const option = document.createElement("option");
                option.value = item.id;
                option.textContent = item.name || `Csomag #${item.id}`;
                dropdown.appendChild(option);
            });
        } catch (error) {
            console.error("Error loading packages:", error);
            showAlert(`Hiba történt a csomagok betöltésekor: ${error.message}`, false);
        }
    };

    // Form submission handler
    const handleFormSubmit = async (event) => {
        event.preventDefault();
        const submitButton = event.target.querySelector('button[type="submit"]');
        
        try {
            submitButton.disabled = true;
            submitButton.textContent = "Küldés...";

            // Get form values
            const formData = {
                Nev: document.getElementById("nevid").value.trim(),
                Email: document.getElementById("emailid").value.trim(),
                Tel: document.getElementById("telid").value.trim(),
                DatumIdo: `${document.getElementById("datumid").value}T${
                    padTime(document.getElementById("ido").value.split("-")[0].trim())
                }:00`,
                ValasztottOktatoId: parseInt(document.getElementById("instructorDropdown").value),
                ValasztottCsomagId: parseInt(document.getElementById("packageDropdown").value)
            };

            // Validation
            if (Object.values(formData).some(v => !v) || 
                isNaN(formData.ValasztottOktatoId) || 
                isNaN(formData.ValasztottCsomagId)) {
                throw new Error("Kérjük, töltsd ki az összes mezőt érvényes adatokkal!");
            }

            // Send data
            const result = await fetchWithTimeout(appointmentsApiUrl, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(formData)
            });

            showAlert(result.message || "Sikeres időpontfoglalás!");
        } catch (error) {
            console.error("Submission error:", error);
            showAlert(error.message || "Hiba történt az időpontfoglalás során!", false);
        } finally {
            submitButton.disabled = false;
            submitButton.textContent = "Időpont foglalása";
        }
    };

    // Initialize
    fetchPackages();
    fetchRegistrations(); // Changed from fetchInstructors

    const form = document.getElementById("appointment-form");
    if (form) {
        form.addEventListener("submit", handleFormSubmit);
    } else {
        console.error("Appointment form not found");
    }
});