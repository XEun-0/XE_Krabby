using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Krabby.Core.Models;

namespace Krabby.Core.Interfaces
{
    internal interface IAnimeData
    {
        Task<Anime?> GetByIdAsync(int aid);
        Task SaveAsync(Anime anime);
    }
}
