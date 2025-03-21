document.addEventListener("DOMContentLoaded", () => {
    // URLs of your Swagger API endpoints
    const packagesApiUrl = "http://localhost:5156/api/Packages"; // Endpoint for Packages
    const instructorsApiUrl = "http://localhost:5156/api/Instructors"; // Endpoint for Instructors
    const submitApiUrl = "http://localhost:5156/api/Submit"; // Endpoint for submitting data

    // Function to fetch data from the Packages API
    async function fetchPackages() {
        try {
            const response = await fetch(packagesApiUrl);
            if (!response.ok) throw new Error("Network response was not ok");
            const packages = await response.json();
            populateDropdown("packageDropdown", packages, "name");
        } catch (error) {
            console.error("Error fetching packages:", error);
        }
    }

    // Function to fetch data from the Instructors API
    async function fetchInstructors() {
        try {
            const response = await fetch(instructorsApiUrl);
            if (!response.ok) throw new Error("Network response was not ok");
            const instructors = await response.json();
            populateDropdown("instructorDropdown", instructors, "name");
        } catch (error) {
            console.error("Error fetching instructors:", error);
        }
    }

    // Function to populate a dropdown
    function populateDropdown(dropdownId, data, displayProperty) {
        const dropdown = document.getElementById(dropdownId);
        if (!dropdown) {
            console.error(`Dropdown element with ID '${dropdownId}' not found.`);
            return;
        }
        dropdown.innerHTML = `<option value="">-- Válassz --</option>`;
        data.forEach(item => {
            const option = document.createElement("option");
            option.value = item.id;
            option.textContent = item[displayProperty];
            dropdown.appendChild(option);
        });
    }

    // Function to send data to the server
    async function sendDataToServer(data) {
        try {
            const response = await fetch(submitApiUrl, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(data),
            });
            if (!response.ok) throw new Error("Network response was not ok");
            const result = await response.json();
            console.log("Server response:", result);
            alert("Sikeres időpontfoglalás!");
        } catch (error) {
            console.error("Error sending data to server:", error);
            alert("Hiba történt az időpontfoglalás során. Kérjük, próbáld újra.");
        }
    }

    // Function to handle form submission
    function handleFormSubmit(event) {
        event.preventDefault(); // Prevent the default form submission

        // Get the form values
        const nev = document.getElementById("nevid").value;
        const email = document.getElementById("emailid").value;
        const tel = document.getElementById("telid").value;
        const datum = document.getElementById("datumid").value;
        const ido = document.getElementById("ido").value;
        const valasztottOktatoId = document.getElementById("instructorDropdown").value;
        const valasztottCsomagId = document.getElementById("packageDropdown").value;

        // Validate the inputs
        if (!nev || !email || !tel || !datum || !ido || !valasztottOktatoId || !valasztottCsomagId) {
            alert("Kérjük, töltsd ki az összes mezőt!");
            return;
        }

        // Prepare the data to send
        const data = {
            Nev: nev,
            Email: email,
            Datum: datum,
            Ido: ido,
            Tel: tel,
            ValasztottOktatoId: parseInt(valasztottOktatoId),
            ValasztottCsomagId: parseInt(valasztottCsomagId),
        };

        // Send the data to the server
        sendDataToServer(data);
    }

    // Fetch data when the page loads
    fetchPackages();
    fetchInstructors();

    // Add event listener to the form submission
    const form = document.getElementById("appointment-form");
    form.addEventListener("submit", handleFormSubmit);
});