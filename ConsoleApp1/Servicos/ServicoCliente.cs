using ConsoleApp1.Entidades;
using ConsoleApp1.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ConsoleApp1.Servicos
{
    public class ServicoCliente 
    {
        private readonly IRepositorio<Cliente> _repositorio;
        public List<string> Mensagens { get; set; }

        public ServicoCliente(IRepositorio<Cliente> repositorio)
        {
            _repositorio = repositorio;
            Mensagens = new List<string>();
        }
        public Cliente Retornar(long id)
        {
            var retorno = _repositorio.Retornar(id);
            Mensagens.AddRange(_repositorio.Mensagens);

            return retorno;
        }

        public bool Incluir(Cliente entidade)
        {
            var retorno = _repositorio.Incluir(entidade);
            Mensagens.AddRange(_repositorio.Mensagens);

            return retorno;
        }

        public bool Merge(Cliente entidade)
        {
            if (entidade.Id > 0)
            {
                var anterior = Retornar(entidade.Id);
                if (anterior == null)
                {
                    return false;
                }
            }
            var retorno = _repositorio.Merge(entidade);
            Mensagens.AddRange(_repositorio.Mensagens);

            return retorno;
        }

        public  List<Cliente> Consultar(Expression<Func<Cliente,bool>> where)
        {
            var retorno = _repositorio.Consultar(where);
            Mensagens.AddRange(_repositorio.Mensagens);

            return retorno?.ToList();

        }

        public bool MarcarAlteracao(Expression<Func<Cliente, bool>> where )
        {
            var clientes = _repositorio.Consultar(where);
            var sucesso = true; 

            foreach(var cliente in clientes)
            {
                cliente.IsAlterado = true;
                sucesso = sucesso && Merge(cliente);
            }
            return sucesso;
        }
    }
}
