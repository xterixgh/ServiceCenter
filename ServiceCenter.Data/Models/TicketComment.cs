using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenter.Data.Models
{
    public class TicketComment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Required]
        public string Text { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        [ForeignKey("Ticket")]
        [Required]
        public int Ticket_Id { get; set; }
        public Ticket Ticket { get; set; } = null!;

        [ForeignKey("User")]
        [Required]
        public int User_Id { get; set; }
        public User User { get; set; } = null!;
    }
}
