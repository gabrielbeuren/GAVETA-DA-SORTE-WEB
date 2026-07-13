using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAVETAWEB.Models
{
    [Table("Deposito")]
    public class Deposito
    {
            
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id")]
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


            [Column("Valor")]
            [Display(Name = "Valor")]
            [Precision(10, 2)]
            public decimal Valor { get; set; }


            [Column("DataDeposito")]
            [Display(Name = "Data do Depósito")]
            public DateTime DataDeposito { get; set; }
        }
    }

