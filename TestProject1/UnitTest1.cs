using ConsoleApp1.Entidades;
using ConsoleApp1.Interfaces;
using ConsoleApp1.Servicos;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TestProject1
{
    [TextFixture]

    public class TesteServicoCliente
    {
        private Mock<IRepositorio<Cliente>> _repositorio;
        private ServicoCliente _servico;
        private List<string> _mensagens; 

        [SetUp]
        public void Inicializar()
        {
            _repositorio = new Mock<IRepositorio<Cliente>>(MockBehavior.Strict);
            _servico = new ServicoCliente(_repositorio.Object);


            _mensagens = new List<string>();
            _repositorio.Setup(x => x.Mensagens).Returns(_mensagens);
        }
        [TearDown]
        public void Finalizar()
        {
            _repositorio.VerifyAll();
        }
        //Incluir 
        //Sucesso 
        [Test]
        public void Incluir_DadosValidos_Sucesso()
        {
            //arrange
            var cliente = new Cliente
            {
                Nome = "Teste"
            };
            _repositorio.Setup( x => x.Incluir(It.IsAny<Cliente>() ) ).Returns(true ).Callback<Cliente>(cliente => cliente.Id++ );
            _repositorio.Setup(x => x.Incluir(It.IsAny<Cliente>( ))).Returns(true).Callback<Cliente>(cliente => cliente.Id++);
            //act 
            var retorno = _servico.Incluir(cliente);
            Assert.IsTrue(retorno);
            Assert.IsEmpty(_servico.Mensagens );
            Assert.Greater(cliente.Id, 0);

            //assert 
        }
        [Test]
        public void Incluir_DadosInvalidos_Erro()
        {
            //arrange 
            var cliente = new Cliente();
            _repositorio.Setup(x => x.Incluir( It.IsAny<Cliente>())).Returns(false ).Callback(() => _mensagens.Add("Erro ao incluir no banco"));
            //Action
            var retorno = _servico.Incluir(cliente);
            //assert
            Assert.IsFalse(retorno);
            Assert.IsNotEmpty(_servico.Mensagens);


        }
       
        //Erro
#region retonar
        //Retornar 
        //Sucesso
        [TestCase(1)]
        public void Retornar_ClienteExiste_RetornarCliente(long id )
        {
            //arrange
            _repositorio.Setup(x => x.Retornar(It.IsAny<long>())).Returns<long>(id => new Cliente { Id = id });


            //act
            var retorno = _servico.Retornar(id);

            //assert 
            Assert.IsNotNull(retorno);
            Assert.IsEmpty(_servico.Mensagens);
            Assert.AreEqual(1, retorno.Id);

        }

        //erro
        [Test]
        public void Retornar_IdInvalido_NaoRetornarNada()
        {
            //arrange
            _repositorio.Setup(x => x.Retornar(It.Is<long>(x => x < 0))).Returns((Cliente)null).Callback( () => _mensagens.Add("Id Inválido") );




            //act
            var retorno = _servico.Retornar( -1 );
            //assert
            Assert.IsNull(retorno);
            Assert.IsNotEmpty(_servico.Mensagens);
        }
        [Test]
        public void Merge_ClienteNovoCorreto_Inserir()
        {
            //arrange 
            var cliente = new Cliente ();

            //act
            _repositorio.Setup(x => x.Merge(It.IsAny<Cliente>())).Returns(true).Callback<Cliente>(cliente => cliente.Id++);
            //assert
            var retorno = _servico.Merge(cliente);
            Assert.IsTrue(retorno);
            Assert.IsEmpty(_servico.Mensagens);

        }
        #endregion
        [Test]
        public void Merge_ClienteExistente_Atualizar()
        {
            //arrange 
            var cliente = new Cliente
            { Id = 1 };

            _repositorio.Setup( x=> x.Retornar(It.IsAny<long>() ) ).Returns(cliente);
            _repositorio.Setup(x => x.Merge(It.IsAny<Cliente>() )).Returns(true);


            //act
            var retorno = _servico.Merge(cliente);

            //assert
            Assert.IsTrue(retorno);
            Assert.IsEmpty(_servico.Mensagens);

            _repositorio.Verify(x => x.Retornar(It.IsAny<long>()), Times.Once ,"" );
            _repositorio.Verify(x => x.Merge(It.IsAny<Cliente>()), Times.Once, "");
            _repositorio.Verify(x => x.Incluir(It.IsAny<Cliente>()), Times.Never, "");
        }
        [Test]
        public void Merge_ClienteExistente_Erro()
        {
            //arrange 
            var cliente = new Cliente
            { Id = 1 };

            _repositorio.Setup(x => x.Retornar(It.IsAny<long>())).Returns(cliente);
            _repositorio.Setup(x => x.Merge(It.IsAny<Cliente>())).Returns(false).Callback(() => _mensagens.Add("O banco caiu"));


            //act
            var retorno = _servico.Merge(cliente);

            //assert
            Assert.IsFalse(retorno);
            Assert.IsNotEmpty(_servico.Mensagens);

            _repositorio.Verify(x => x.Retornar(It.IsAny<long>()), Times.Once, "");
            _repositorio.Verify(x => x.Merge(It.IsAny<Cliente>()), Times.Once, "");
            _repositorio.Verify(x => x.Incluir(It.IsAny<Cliente>()), Times.Never, "");
        }
        [Test]
        public void Merge_ClienteComIdInvalido_Erro()
        {
            //arrange
            var cliente = new Cliente
            {
                Id = 1
            };

            //act
            _repositorio.Setup(x => x.Retornar(It.IsAny<long>())).Returns((Cliente)null).Callback(() => _mensagens.Add("Clientes não encontrado na base") );
            //assert
            var retorno = _servico.Merge(cliente);
            Assert.IsFalse(retorno);
            Assert.IsNotEmpty(_servico.Mensagens);

            _repositorio.Verify(x => x.Retornar(It.IsAny<long>()), Times.Once, "Deve Consultar Cliente");
            _repositorio.Verify(x => x.Merge(It.IsAny<Cliente>()) , Times.Never, "Não Deve Atualizar o Cliente");
            _repositorio.Verify(x => x.Incluir(It.IsAny<Cliente>()), Times.Never, "Não Deve Incluir o Cliente");

        }

        #region Consultar
        [TestCase("a" , 2)]
        [TestCase("e", 1)]
        [TestCase("z", 0)]
        // filtrar sucesso 
        public void Consultar_WhereValido_RetornarClientes(string filtro , int count)
        {
            var listaCLientes = new List<Cliente>
            {
                new Cliente{Nome = "Ernesto"},
                new Cliente {Nome = "Augusto"} ,
                new Cliente{Nome = "Ana"}
            };
            _repositorio.Setup(x => x.Consultar(It.IsAny<Expression<Func<Cliente, bool>>>())).
                Returns<Expression<Func<Cliente, bool>>>(e => listaCLientes.Where(e.Compile()).ToList());
            var retorno = _servico.Consultar(c => c.Nome.Contains(filtro, StringComparison.InvariantCultureIgnoreCase));

        }

        //filtrar erro 
        

        [Test]
        public void Consultar_WhereInvalido_NaoRetornarNada()
        {
            //arrange
            _repositorio.Setup(x => x.Consultar(null)).Returns((IList<Cliente>)null);

            //act
            var retorno = _servico.Consultar(null);

            //assert
            Assert.IsNull(retorno);
        }

        [Test]
        public void MarcarAlteracao_ClientesComDeterminadaLetra_ChamarMerge()
        {
            var listaClientes = new List<Cliente>
            {
                new Cliente{ Id = 1 , Nome = "Ernesto"},
                new Cliente{ Id = 2 , Nome = "Augusto"},
                new Cliente{ Id = 3 , Nome = "Ana"}
            };

            _repositorio.Setup(x => x.Consultar(It.IsAny<Expression<Func<Cliente,bool>>>()))
                .Returns<Expression<Func<Cliente, bool>>>(e=> listaClientes.Where(e.Compile()).ToList() );

            _repositorio.Setup(x => x.Retornar(It.IsAny<long>())).Returns<long>(id => listaClientes.Find(x => x.Id == id));

            _repositorio.Setup(x => x.Merge(It.IsAny<Cliente>())).Returns(true);

            var retorno = _servico.MarcarAlteracao(c => c.Nome.Contains("a", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(retorno);

            _repositorio.Verify(x => x.Retornar(It.IsAny<long>()), Times.Exactly(2), "DeveConsultar");
            _repositorio.Verify(x => x.Merge(It.IsAny<Cliente>()), Times.Exactly(2), "DeveConsultar");

        }
        #endregion Consultar
        [Test]
        public void MarcarAlteracao_ClientesComDeterminadaLetra_ChamarMergeInstabilidade()
        {
            var listaClientes = new List<Cliente>
            {
                new Cliente{ Id = 1 , Nome = "Ernesto"},
                new Cliente{ Id = 2 , Nome = "Augusto"},
                new Cliente{ Id = 3 , Nome = "Ana"}
            };

            _repositorio.Setup(x => x.Consultar(It.IsAny<Expression<Func<Cliente, bool>>>()))
                .Returns<Expression<Func<Cliente, bool>>>(e => listaClientes.Where(e.Compile()).ToList());

            _repositorio.Setup(x => x.Retornar(It.IsAny<long>())).Returns<long>(id => listaClientes.Find(x => x.Id == id));

            _repositorio.SetupSequence(x => x.Merge(It.IsAny<Cliente>())).Returns(true).Returns(false);

            var retorno = _servico.MarcarAlteracao(c => c.Nome.Contains("a", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsFalse(retorno);

            _repositorio.Verify(x => x.Retornar(It.IsAny<long>()), Times.Exactly(2), "DeveConsultar");
            _repositorio.Verify(x => x.Merge(It.IsAny<Cliente>()), Times.Exactly(2), "DeveConsultar");

        }
    }
}