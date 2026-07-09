using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GAVETAWEB.Models
{
    [Table("Grupo")]
    public class Grupo
    {
        [Column("Id")]
        [Display(Name = "Código")]
        public int Id { get; set; }

        [Column("Nome")]
        [Display(Name = "Nome")]
        public string Nome { get; set; }

        [Column("Descricao")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; }

        [Column("DataCriacao")]
        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; }

        [Column("CriadorId")]
        [Display(Name = "Criador")]
        public int CriadorId { get; set; }
    }
}
