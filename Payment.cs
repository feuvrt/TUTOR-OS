namespace TutorOS;

public class Payment
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public string StudentName { get; set; } = "";

    public int Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string Status { get; set; } = "";

    public string Comment { get; set; } = "";
}