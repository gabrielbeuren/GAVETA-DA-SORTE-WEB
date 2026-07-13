using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAVETAWEB.Models
{
    [Table("ParticipanteGrupo")]
    public class ParticipanteGrupo
    {
        [Column("Id")]
        [Display(Name = "Código")]
        public int Id { get; set; }

        [Column("UsuarioId")]
        [Display(Name = "Usuário")]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        [Column("GrupoId")]
        [Display(Name = "Grupo")]
        public int GrupoId { get; set; }

        [ForeignKey("GrupoId")]
        public Grupo Grupo { get; set; }



    }
}
