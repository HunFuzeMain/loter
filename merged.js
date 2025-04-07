document.addEventListener("DOMContentLoaded", () => {
    // Configuration
    const API_BASE = "http://localhost:5156/api";
    const MAX_PARTICIPANTS = 7;

    // DOM Elements
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

    // Validate critical elements exist
    if (!elements.form || !elements.submitBtn) {
        console.error("Form or submit button not found!");
        return;
    }

    // Initialize the application
    const init = async () => {
        try {
            await loadDropdowns();
            setupEventListeners();
        } catch (error) {
            console.error("Initialization failed:", error);
            showError("Az oldal betöltése sikertelen. Kérjük, frissítsd az oldalt.");
        }
    };

    // Load dropdown options
    const loadDropdowns = async () => {
        try {
            await Promise.all([
                fetchDropdownOptions("Instructors", "instructorDropdown"),
                fetchDropdownOptions("Packages", "packageDropdown")
            ]);
        } catch (error) {
            console.error("Error loading dropdowns:", error);
            throw error;
        }
    };

    // Fetch and populate a dropdown
    const fetchDropdownOptions = async (endpoint, elementId) => {
        const dropdown = document.getElementById(elementId);
        if (!dropdown) return;

        try {
            const response = await fetch(`${API_BASE}/${endpoint}`);
            if (!response.ok) throw new Error("Network error");
            
            const options = await response.json();
            dropdown.innerHTML = options.map(opt => 
                `<option value="${opt.id}">${opt.name}</option>`
            ).join('');
        } catch (error) {
            console.error(`Error loading ${endpoint}:`, error);
            throw error;
        }
    };

    // Set up event listeners
    const setupEventListeners = () => {
        // Real-time availability check
        elements.inputs.time?.addEventListener("change", checkAvailability);
        elements.inputs.date?.addEventListener("change", checkAvailability);
        elements.inputs.instructor?.addEventListener("change", checkAvailability);

        // Form submission
        elements.form.addEventListener("submit", handleFormSubmit);
    };

    // Check time slot availability
    const checkAvailability = async () => {
        if (!elements.availabilityDisplay) return;

        const instructorId = elements.inputs.instructor?.value;
        const date = elements.inputs.date?.value;
        const timeRange = elements.inputs.time?.value;

        if (!instructorId || !date || !timeRange) return;

        try {
            const params = new URLSearchParams({
                instructorId,
                date,
                timeRange
            });

            const response = await fetch(`${API_BASE}/Appointments/availability?${params}`);
            if (!response.ok) throw new Error("Availability check failed");
            
            const { isAvailable, currentParticipants } = await response.json();
            
            elements.availabilityDisplay.innerHTML = isAvailable
                ? `<span class="available">${MAX_PARTICIPANTS - currentParticipants} hely maradt</span>`
                : `<span class="full">Foglalt (${MAX_PARTICIPANTS}/${MAX_PARTICIPANTS})</span>`;
            
            elements.availabilityDisplay.style.display = "block";
        } catch (error) {
            console.error("Availability check error:", error);
            elements.availabilityDisplay.style.display = "none";
        }
    };

    // Handle form submission
    const handleFormSubmit = async (e) => {
        e.preventDefault();
        
        // Set loading state
        elements.submitBtn.disabled = true;
        if (elements.submitBtn.querySelector(".button-text")) {
            elements.submitBtn.querySelector(".button-text").textContent = "Küldés...";
        }
        elements.submitBtn.insertAdjacentHTML("afterbegin", '<span class="spinner"></span>');

        try {
            const formData = getFormData();
            await submitAppointment(formData);
            
            // Success
            showSuccess("Sikeres időpontfoglalás!");
            elements.form.reset();
            if (elements.availabilityDisplay) {
                elements.availabilityDisplay.style.display = "none";
            }
        } catch (error) {
            console.error("Submission error:", error);
            showError(error.message || "Hiba történt a foglalás során");
        } finally {
            // Reset button state
            elements.submitBtn.disabled = false;
            const spinner = elements.submitBtn.querySelector(".spinner");
            if (spinner) spinner.remove();
            if (elements.submitBtn.querySelector(".button-text")) {
                elements.submitBtn.querySelector(".button-text").textContent = "Mentés";
            }
        }
    };

    // Get form data with validation
    const getFormData = () => {
        const data = {
            instructorId: elements.inputs.instructor?.value,
            packageId: elements.inputs.package?.value,
            date: elements.inputs.date?.value,
            timeRange: elements.inputs.time?.value,
            clientName: elements.inputs.name?.value,
            email: elements.inputs.email?.value,
            phone: elements.inputs.phone?.value
        };

        // Validate required fields
        if (Object.values(data).some(val => !val)) {
            throw new Error("Kérjük, töltsd ki az összes mezőt!");
        }

        return data;
    };

    // Submit appointment to backend
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

    // Show error message
    const showError = (message) => {
        alert(`Hiba: ${message}`);
    };

    // Show success message
    const showSuccess = (message) => {
        alert(message);
    };

    // Start the application
    init();
});