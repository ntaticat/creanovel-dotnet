using System;
using System.Collections.Generic;
using WebAPI.Models;

namespace WebAPI.Repositories
{
    public interface IRepo<T> where T: class
    {
        T GetById(Guid novelaId);
        IEnumerable<T> GetAll();
        void Create(T novela);
        void Delete(T novela);
    }
}