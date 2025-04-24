document.addEventListener("DOMContentLoaded", () => {
    const API_BASE = "http://localhost:5156/api";
    const MAX_PARTICIPANTS = 7;

    const elements = {
        form: document.getElementById("appointment-form"),
        submitBtn: document.getElementById("submit-btn"),
        availabilityDisplay: document.getElementById("availability-display"),
        inputs: {
            instructor: document.getElementById("instructorDropdown"),
            package: document.getElementById("packageDropdown"),
            date: document.getElementById("datumid"),
            time: document.getElementById("ido"),
            name: document.getElementById("nevid"),
            email: document.getElementById("emailid"),
            phone: document.getElementById("telid")
        }
    };

    if (!elements.form || !elements.submitBtn) return;

    const init = async () => {
        try {
            await loadDropdowns();
            setupEventListeners();
        } catch (error) {
            console.error("Initialization failed:", error);
            showError("Az oldal betöltése sikertelen. Kérjük, frissítsd az oldalt.");
        }
    };

    const loadDropdowns = async () => {
        await Promise.all([
            fetchHiredInstructors(),
            fetchDropdownOptions("Packages", "packageDropdown")
        ]);
    };

    const fetchHiredInstructors = async () => {
        const dropdown = document.getElementById("instructorDropdown");
        if (!dropdown) return;

        try {
            const response = await fetch(`${API_BASE}/Instructors`);
            if (!response.ok) throw new Error("Oktatók lekérése sikertelen");

            const instructors = await response.json();
            const hired = instructors.filter(i => i.status === "Hired");

            dropdown.innerHTML = '<option value="">-- Válassz egy oktatót --</option>' + 
                hired.map(ins => `<option value="${ins.id}">${ins.name}</option>`).join('');
        } catch (error) {
            console.error("Hiba az oktatók betöltésekor:", error);
            throw error;
        }
    };

    const fetchDropdownOptions = async (endpoint, elementId) => {
        const dropdown = document.getElementById(elementId);
        if (!dropdown) return;

        try {
            const response = await fetch(`${API_BASE}/${endpoint}`);
            if (!response.ok) throw new Error("Hálózati hiba");

            const options = await response.json();
            dropdown.innerHTML = '<option value="">-- Válassz --</option>' + 
                options.map(opt => `<option value="${opt.id}">${opt.name}</option>`).join('');
        } catch (error) {
            console.error(`Error loading ${endpoint}:`, error);
            throw error;
        }
    };

    const setupEventListeners = () => {
        elements.inputs.time?.addEventListener("change", checkAvailability);
        elements.inputs.date?.addEventListener("change", checkAvailability);
        elements.inputs.instructor?.addEventListener("change", checkAvailability);
        elements.form.addEventListener("submit", handleFormSubmit);
    };

    const checkAvailability = async () => {
        if (!elements.availabilityDisplay) return;

        const { instructor, date, time } = elements.inputs;
        if (!instructor.value || !date.value || !time.value) return;

        try {
            const params = new URLSearchParams({
                instructorId: instructor.value,
                date: date.value,
                timeRange: time.value
            });

            const response = await fetch(`${API_BASE}/Appointments/availability?${params}`);
            if (!response.ok) throw new Error("Elérhetőség lekérdezése sikertelen");

            const { isAvailable, currentParticipants } = await response.json();

            elements.availabilityDisplay.innerHTML = isAvailable
                ? `<span class="available">${MAX_PARTICIPANTS - currentParticipants} hely maradt</span>`
                : `<span class="full">Foglalt (${MAX_PARTICIPANTS}/${MAX_PARTICIPANTS})</span>`;
            elements.availabilityDisplay.style.display = "block";
        } catch (error) {
            console.error("Availability error:", error);
            elements.availabilityDisplay.style.display = "none";
        }
    };

    const handleFormSubmit = async (e) => {
        e.preventDefault();

        elements.submitBtn.disabled = true;
        const spinner = document.createElement("span");
        spinner.className = "spinner";
        elements.submitBtn.insertAdjacentElement("afterbegin", spinner);
        elements.submitBtn.querySelector(".button-text").textContent = "Küldés...";

        try {
            const data = getFormData();
            await submitAppointment(data);
            showSuccess("Sikeres időpontfoglalás!");
            elements.form.reset();
            elements.availabilityDisplay.style.display = "none";
        } catch (err) {
            showError(err.message || "Ismeretlen hiba történt.");
        } finally {
            spinner.remove();
            elements.submitBtn.disabled = false;
            elements.submitBtn.querySelector(".button-text").textContent = "Mentés";
        }
    };

    const getFormData = () => {
        const data = {
            instructorId: elements.inputs.instructor.value,
            packageId: elements.inputs.package.value,
            date: elements.inputs.date.value,
            timeRange: elements.inputs.time.value,
            clientName: elements.inputs.name.value,
            email: elements.inputs.email.value,
            phone: elements.inputs.phone.value,
            notes: document.getElementById("textarea").value.trim()
        };

        if (Object.values(data).some(v => !v)) {
            throw new Error("Kérjük, töltsd ki az összes mezőt!");
        }

        return data;
    };

    const submitAppointment = async (data) => {
        const response = await fetch(`${API_BASE}/Appointments`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            const error = await response.json().catch(() => ({}));
            throw new Error(error.message || "A foglalás sikertelen");
        }
    };

    const showError = (msg) => alert("Hiba: " + msg);
    const showSuccess = (msg) => alert(msg);

    init();
});
