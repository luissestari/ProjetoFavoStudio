using System.Collections.Generic;
using FavoStudio3D.Models;

namespace FavoStudio3D.Data
{
    public static class DadosNaMemoriaData
    {
        public static List<Filamento> Filamentos { get; set; } = new List<Filamento>
        {
            new Filamento { Nome = "PLA Branco Premium", Tipo = "PLA", PrecoPorKg = 110.00m },
            new Filamento { Nome = "PETG Preto Resistente", Tipo = "PETG", PrecoPorKg = 130.00m }
        };

        public static List<CustoAdicional> Custos { get; set; } = new List<CustoAdicional>
        {
            new CustoAdicional { Descricao = "Embalagem / Caixas", Tipo = "Adicional", Valor = 5.00m },
            new CustoAdicional { Descricao = "Energia Elétrica Base", Tipo = "Fixo", Valor = 150.00m }
        };

        // Nova lista de impressoras em memória
        public static List<Impressora> Impressoras { get; set; } = new List<Impressora>
        {
            new Impressora { Nome = "Ender 3 V2", MarcaModelo = "Creality", ConsumoWatts = 350, CustoKwh = 0.85m, Status = "Ativa" },
            new Impressora { Nome = "Bambu Lab X1-Carbon", MarcaModelo = "Bambu Lab", ConsumoWatts = 1000, CustoKwh = 0.85m, Status = "Ativa" }
        };
    }
}