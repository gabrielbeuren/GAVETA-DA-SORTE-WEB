using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAVETAWEB.Models
{
    [Table("Sorteio")]
    public class Sorteio
    {
        [Column("Id")]
        [Display(Name = "Código")]
        public int Id { get; set; }


        [Column("GrupoId")]
        [Display(Name = "Grupo")]
        public int GrupoId { get; set; }

        [ForeignKey("GrupoId")]
        public Grupo Grupo { get; set; }


        [Column("UsuarioId")]
        [Display(Name = "Vencedor")]
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }


        [Column("ValorPremio")]
        [Display(Name = "Prêmio")]
        [Precision(10, 2)]
        public decimal ValorPremio { get; set; }


        [Column("DataSorteio")]
        [Display(Name = "Data do Sorteio")]
        public DateTime DataSorteio { get; set; }

    }
}
