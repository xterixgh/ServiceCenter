using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenter.Data.Models
{
    public class StatusLog
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("Ticket")]
        [Required]
        public int Ticket_Id { get; set; }
        public Ticket Ticket { get; set; } = null!;

        [ForeignKey("User")]
        [Required]
        public int User_Id { get; set; }
        public User User { get; set; } = null!;

        public int? OldStatus_Id { get; set; }

        [ForeignKey("NewStatus")]
        [Required]
        public int NewStatus_Id { get; set; }
        public TicketStatus NewStatus { get; set; } = null!;
    }
}
