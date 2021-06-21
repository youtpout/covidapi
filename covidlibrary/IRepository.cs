using System;
using System.Collections.Generic;
using System.Text;

namespace covidlibrary
{
    public interface IRepository<T>
    {
        void Add(T item);
        void Remove(int id);
        void Update(T item);
        T FindByID(int id);
        IEnumerable<T> FindAll();
        IEnumerable<T> Take(int nb, int skip = 0);
        int Count();
    }
}
