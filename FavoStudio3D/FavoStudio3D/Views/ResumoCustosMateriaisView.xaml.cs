using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FavoStudio3D.Views
{
    public partial class ResumoCustosMateriaisView : UserControl
    {
        // Listas que o seu colega vai preencher com dados da Base de Dados
        public ObservableCollection<FilamentoModel> ListaFilamentos { get; set; } = new ObservableCollection<FilamentoModel>();
        public ObservableCollection<CustoModel> ListaOutrosCustos { get; set; } = new ObservableCollection<CustoModel>();

        public ResumoCustosMateriaisView()
        {
            InitializeComponent();
            DataContext = this;

            // Dados Falsos (Mock) apenas para você ver o design funcionar
            CarregarDadosDeTeste();
        }

        private void CarregarDadosDeTeste()
        {
            // Teste: Filamentos
            ListaFilamentos.Add(new FilamentoModel { Nome = "PLA Branco", Tipo = "PLA", CorHex = "#FFFFFF", PrecoFormatado = "R$ 100,00" });
            ListaFilamentos.Add(new FilamentoModel { Nome = "PLA Preto", Tipo = "PLA", CorHex = "#212121", PrecoFormatado = "R$ 100,00" });
            ListaFilamentos.Add(new FilamentoModel { Nome = "PETG Vermelho", Tipo = "PETG", CorHex = "#F44336", PrecoFormatado = "R$ 120,00" });
            ListaFilamentos.Add(new FilamentoModel { Nome = "ABS Azul", Tipo = "ABS", CorHex = "#2196F3", PrecoFormatado = "R$ 90,00" });
            ListaFilamentos.Add(new FilamentoModel { Nome = "Silk Dourado", Tipo = "Silk", CorHex = "#FFD700", PrecoFormatado = "R$ 150,00" });

            // Teste: Outros Custos
            ListaOutrosCustos.Add(new CustoModel { Descricao = "Saco a vácuo", ValorFormatado = "R$ 135,00", Tipo = "Custo fixo" });
            ListaOutrosCustos.Add(new CustoModel { Descricao = "Lixas", ValorFormatado = "R$ 30,00", Tipo = "Custo adicional" });
            ListaOutrosCustos.Add(new CustoModel { Descricao = "Canetas acrílicas", ValorFormatado = "R$ 44,00", Tipo = "Custo adicional" });
            ListaOutrosCustos.Add(new CustoModel { Descricao = "Primer", ValorFormatado = "R$ 29,00", Tipo = "Custo adicional" });
            ListaOutrosCustos.Add(new CustoModel { Descricao = "Base de corte", ValorFormatado = "R$ 24,00", Tipo = "Custo fixo" });
        }

        // --- EVENTOS DE NAVEGAÇÃO (O colega vai ligar isto às telas de cadastro) ---
        private void BtnNovoFilamento_Click(object sender, RoutedEventArgs e)
        {
            // Exemplo: Navegar para CadastroCustosView (Aba Filamento ativa)
            MessageBox.Show("Navegar para a tela de Cadastro de Filamento", "Ação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnNovoCusto_Click(object sender, RoutedEventArgs e)
        {
            // Exemplo: Navegar para CadastroCustosView (Aba Custo ativa)
            MessageBox.Show("Navegar para a tela de Cadastro de Custo", "Ação", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // --- EVENTOS DE AÇÃO (Menu Editar/Excluir) ---
        private void MenuAcoesFilamento_Click(object sender, MouseButtonEventArgs e)
        {
            // O colega vai usar o sender para descobrir qual filamento foi clicado e abrir um ContextMenu (Editar/Excluir)
            var img = sender as FrameworkElement;
            if (img?.DataContext is FilamentoModel filamentoClicado)
            {
                MessageBox.Show($"Abrir menu de opções para o filamento: {filamentoClicado.Nome}", "Ações", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuAcoesCusto_Click(object sender, MouseButtonEventArgs e)
        {
            var img = sender as FrameworkElement;
            if (img?.DataContext is CustoModel custoClicado)
            {
                MessageBox.Show($"Abrir menu de opções para o custo: {custoClicado.Descricao}", "Ações", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    // --- MODELOS DE DADOS PARA O BACKEND ---

    public class FilamentoModel
    {
        public int Id { get; set; } // O colega vai precisar do ID da Base de Dados
        public string Nome { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string CorHex { get; set; } = string.Empty; // Código da cor para pintar a bolinha
        public string PrecoFormatado { get; set; } = string.Empty;

        // Propriedade para colorir o fundo do "Tipo" (PLA = azulzinho, PETG = rosinha, etc)
        public string CorFundoTipo
        {
            get
            {
                if (Tipo.ToUpper().Contains("PLA")) return "#D4E6F1"; // Azul claro
                if (Tipo.ToUpper().Contains("PETG")) return "#FADBD8"; // Rosa claro
                return "#EAEDED"; // Cinza claro padrão
            }
        }
    }

    public class CustoModel
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string ValorFormatado { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; // "Custo fixo" ou "Custo adicional"

        // Propriedade para colorir o fundo do Tipo de Custo (Fixo = Roxo, Adicional = Verde)
        public string CorFundoTipo
        {
            get
            {
                if (Tipo.ToLower().Contains("fixo")) return "#E8DAEF"; // Roxo claro
                if (Tipo.ToLower().Contains("adicional")) return "#D5F5E3"; // Verde claro
                return "#EAEDED";
            }
        }
    }
}