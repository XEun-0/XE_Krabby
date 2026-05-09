using Krabby.Persistence.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Krabby.Persistence
{
    internal class AnimeData
    {
        private readonly KrabbyDbContext _db;

        public AnimeData(KrabbyDbContext db)
        {
            _db = db;
        }

        public async Task<Models.AnimeData?> GetByIdAsync(int aid)
        {
            return await _db.Anime.FindAsync(aid);
        }

        public async Task SaveAsync(Models.AnimeData anime)
        {
            _db.Anime.Update(anime);
            await _db.SaveChangesAsync();
        }
    }
}
