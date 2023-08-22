using ConsoleApp1.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1.Entidades
{
    public class Cliente : IEntidadeBase
    {
        public long Id { get; set; }

        public string Nome { get; set; }
        public bool IsAlterado { get;  set; }
    }
}
