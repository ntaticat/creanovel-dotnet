using System;
using System.Collections.Generic;
using CreaNovelNETCore.Models;

namespace CreaNovelNETCore.Repositories
{
    public interface IRepo<T> where T: class
    {
        T GetById(Guid novelaId);
        IEnumerable<T> GetAll();
        void Create(T novela);
        void Delete(T novela);
    }
}