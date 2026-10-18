using Krabby.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace Krabby.Persistence;

public class KrabbyDbContext : DbContext
{
    public KrabbyDbContext(DbContextOptions<KrabbyDbContext> options)
        : base(options) { }

    public DbSet<Models.AnimeData> Anime => base.Set<Models.AnimeData>();
    public DbSet<EpisodeData> Episodes => Set<EpisodeData>();
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.AnimeData>()
            .HasKey(x => x.Aid);

        modelBuilder.Entity<EpisodeData>()
            .HasKey(x => x.Eid);

        modelBuilder.Entity<EpisodeData>()
            .HasIndex(x => x.Aid);
    }
}