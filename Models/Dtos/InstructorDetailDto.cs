using Org.BouncyCastle.Security;

public class InstructorDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
    public InstructorStatus Status { get; set; }
    public DateTime? HireDate { get; set; }
    public string QualificationFileUrl { get; set; }
    public string IdCardFileUrl { get; set; }
    public string CVFileUrl { get; set; }
    public string Password { get; set; }

}
