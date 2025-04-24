// instructor.js
// Handles the instructor application form submission, file validation, and user feedback

document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("instructorForm");
    const responseMessage = document.getElementById("responseMessage");
  
    if (!form) return; // If form not found, do nothing
  
    form.addEventListener("submit", async (event) => {
      event.preventDefault();
  
      // Show loading state on submit button
      const submitButton = form.querySelector("button[type='submit']");
      submitButton.disabled = true;
      submitButton.innerHTML = '<span class="spinner">⏳</span> Küldés...';
  
      try {
        // Initialize FormData
        const formData = new FormData();
  
        // Append text fields
        formData.append("Name", document.getElementById("name").value);
        formData.append("Email", document.getElementById("email").value);
        formData.append("Phone", document.getElementById("phone").value);
        formData.append("Address", document.getElementById("address").value);
  
        // Fetch selected files
        const qualificationFile = document.getElementById("qualification").files[0];
        const idCardFile = document.getElementById("idcard").files[0];
        const cvFile = document.getElementById("cv").files[0];
  
        // Validate files before appending
        validateFileOrThrow(qualificationFile, ['.pdf', '.jpg', '.jpeg', '.png'], "Minősítés");
        validateFileOrThrow(idCardFile, ['.pdf', '.jpg', '.jpeg', '.png'], "Igazolvány");
        validateFileOrThrow(cvFile, ['.pdf', '.doc', '.docx'], "Önéletrajz");
  
        // Append files to FormData under the correct names
        formData.append("QualificationFileName", qualificationFile);
        formData.append("IdCardFileName", idCardFile);
        formData.append("CVFileName", cvFile);
  
        // Send to backend
        const response = await fetch('http://localhost:5156/api/Instructors', {
          method: 'POST',
          body: formData
        });
  
        // Handle non-OK responses
        if (!response.ok) {
          const text = await response.text();
          console.error("Raw response from server:", text);
          let errorMsg = "Hiba történt a beküldés során";
          try {
            const data = JSON.parse(text);
            errorMsg = data.errors
              ? Object.values(data.errors).flat().join('\n')
              : data.Message || errorMsg;
          } catch {
            errorMsg = text || errorMsg;
          }
          throw new Error(errorMsg);
        }
  
        // Parse success JSON and show message
        const result = await response.json();
        showSuccessMessage(result.Message || "Sikeres jelentkezés!");
        form.reset();
  
      } catch (err) {
        console.error("Error:", err);
        showErrorMessage(err.message || "Ismeretlen hiba történt.");
      } finally {
        // Restore button state
        submitButton.disabled = false;
        submitButton.textContent = "Jelentkezés elküldése";
      }
    });
  
    /**
     * Validates a file against allowed extensions and size.
     * Throws an Error with a contextual message if invalid.
     */
    function validateFileOrThrow(file, allowedExts, fieldName, maxSize = 5 * 1024 * 1024) {
      if (!file) throw new Error(`${fieldName}: nincs kiválasztott fájl`);
      const ext = '.' + file.name.split('.').pop().toLowerCase();
      if (!allowedExts.includes(ext))
        throw new Error(`${fieldName}: csak ${allowedExts.join(', ')} típusok engedélyezettek (max ${maxSize / 1024 / 1024}MB)`);
      if (file.size > maxSize)
        throw new Error(`${fieldName}: a fájl mérete túl nagy (max ${maxSize / 1024 / 1024}MB)`);
    }
  
    /** Displays a success message in green */
    function showSuccessMessage(msg) {
      responseMessage.textContent = msg;
      responseMessage.style.color = "#28a745";
      responseMessage.style.display = "block";
    }
  
    /** Displays an error message in red */
    function showErrorMessage(msg) {
      responseMessage.textContent = msg;
      responseMessage.style.color = "#dc3545";
      responseMessage.style.display = "block";
    }
  });
  