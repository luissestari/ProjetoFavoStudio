using System.Collections.ObjectModel;
using FavoStudio3D.Models;

namespace FavoStudio3D.Data
{
    public static class DadosNaMemoriaData
    {
        // Usamos ObservableCollection para que a tela no WPF se atualize automaticamente quando um item for inserido ou removido
        public static ObservableCollection<Filamento> Filamentos { get; set; } = new ObservableCollection<Filamento>();
        public static ObservableCollection<CustoAdicional> Custos { get; set; } = new ObservableCollection<CustoAdicional>();

        static DadosNaMemoriaData()
        {
            // Carrega alguns dados de teste em memória ao iniciar o aplicativo
            CarregarDadosExemplo();
        }

        private static void CarregarDadosExemplo()
        {
            // Filamentos Iniciais
            Filamentos.Add(new Filamento { Id = 1, Nome = "PLA Branco", Tipo = "PLA", CorHex = "#FFFFFF", PrecoPorKg = 100.00m });
            Filamentos.Add(new Filamento { Id = 2, Nome = "PETG Preto", Tipo = "PETG", CorHex = "#000000", PrecoPorKg = 120.00m });

            // Custos Iniciais
            Custos.Add(new CustoAdicional { Id = 1, Descricao = "Saco a vácuo", Valor = 135.00m, Tipo = "Custo Fixo" });
            Custos.Add(new CustoAdicional { Id = 2, Descricao = "Lixas", Valor = 30.00m, Tipo = "Custo Adicional" });
        }
    }
}