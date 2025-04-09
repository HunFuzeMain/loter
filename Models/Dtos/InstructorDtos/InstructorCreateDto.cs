using System.ComponentModel.DataAnnotations;

public class InstructorCreateDto
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    public string Phone { get; set; }

    [Required(ErrorMessage = "Address is required")]
    public string Address { get; set; }

    [Required(ErrorMessage = "Qualification file is required")]
    public IFormFile QualificationFile { get; set; }

    [Required(ErrorMessage = "ID card file is required")]
    public IFormFile IdCardFile { get; set; }

    [Required(ErrorMessage = "CV file is required")]
    public IFormFile CVFile { get; set; }
}