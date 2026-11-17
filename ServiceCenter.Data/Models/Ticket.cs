using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenter.Data.Models
{
    public class Ticket
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Required]
        public string FaultText { get; set; } = "";

        public DateTime CreatedAt { get; set; }

        public DateTime? DueDate { get; set; }

        public string Source { get; set; } = "desk";

        [ForeignKey("Client")]
        [Required]
        public int Client_Id { get; set; }
        public Client Client { get; set; } = null!;

        [ForeignKey("DeviceType")]
        [Required]
        public int DeviceType_Id { get; set; }
        public DeviceType DeviceType { get; set; } = null!;

        [ForeignKey("ServiceItem")]
        [Required]
        public int Service_Id { get; set; }
        public ServiceItem ServiceItem { get; set; } = null!;

        [ForeignKey("TicketStatus")]
        [Required]
        public int Status_Id { get; set; }
        public TicketStatus TicketStatus { get; set; } = null!;

        [ForeignKey("Master")]
        public int? Master_Id { get; set; }
        public User? Master { get; set; }
    }
}
