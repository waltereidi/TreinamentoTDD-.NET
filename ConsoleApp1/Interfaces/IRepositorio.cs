using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ConsoleApp1.Interfaces
{
    public interface IRepositorio<T> where T : IEntidadeBase
    {
        T Retornar(long id);
        bool Incluir(T entidade);
        bool Merge(T entidade);
        List<string> Mensagens { get;  }
        IList<T> Consultar(Expression<Func<T, bool>> where);

    }
}
