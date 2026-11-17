using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServiceCenter.Data.Models
{
    public class User
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Required]
        public string Login { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        [Required]
        public string FullName { get; set; } = "";

        [Required]
        public string Role { get; set; } = "";

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }

        public override string ToString() => FullName;
    }
}
