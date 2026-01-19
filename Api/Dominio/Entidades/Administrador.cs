using minimal_api.Dominio.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace minimal_api.Dominio.Entidades
{
    public class Administrador
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public string Email { get; set; } = default!;
        [Required]
        [StringLength(500)]
        public string Senha { get; set; } = default!;
        [Required]
        [StringLength(500)]
        public string SenhaFake { get; set; } = default!;
        [Required]
        [StringLength(10)]
        public string Perfil { get; set; } = default!;
        public bool Caminho { get; set; } = default!;
    }
}
