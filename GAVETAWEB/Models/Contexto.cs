using Microsoft.EntityFrameworkCore;

namespace GAVETAWEB.Models
{
    public class Contexto : DbContext
    {

        public Contexto(DbContextOptions<Contexto> options) : base(options)
        {

        }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Grupo> Grupo { get; set; }
        public DbSet<ParticipanteGrupo> ParticipanteGrupo { get; set; }
        public DbSet<Deposito> Deposito { get; set; }
        public DbSet<Sorteio> Sorteio { get; set; }



    }
}