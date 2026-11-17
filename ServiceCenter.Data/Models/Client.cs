using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenter.Data.Models
{
    public class Client
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = "";

        [Required]
        public string Phone { get; set; } = "";

        public string Note { get; set; } = "";

        public override string ToString() => FullName;
    }
}
