using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FavoStudio3D.Views
{
    public partial class ResumoCustosMateriaisView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public static ObservableCollection<FilamentoModel> FilamentosBancoDados { get; set; } = new ObservableCollection<FilamentoModel>();
        public static ObservableCollection<CustoModel> CustosBancoDados { get; set; } = new ObservableCollection<CustoModel>();

        public ObservableCollection<FilamentoModel> ListaFilamentos => FilamentosBancoDados;
        public ObservableCollection<CustoModel> ListaOutrosCustos => CustosBancoDados;

        public string TextoTotalFilamentos => FilamentosBancoDados.Count == 0 ? "Nenhum filamento cadastrado" :
                                              FilamentosBancoDados.Count == 1 ? "1 filamento cadastrado" :
                                              $"{FilamentosBancoDados.Count} filamentos cadastrados";

        public string TextoTotalCustos => CustosBancoDados.Count == 0 ? "Nenhum custo cadastrado" :
                                          CustosBancoDados.Count == 1 ? "1 custo cadastrado" :
                                          $"{CustosBancoDados.Count} custos cadastrados";

        public Visibility VisibilidadePaginacaoFilamentos => FilamentosBancoDados.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VisibilidadePaginacaoCustos => CustosBancoDados.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public ResumoCustosMateriaisView()
        {
            InitializeComponent();
            DataContext = this;

            FilamentosBancoDados.CollectionChanged += (s, e) => AtualizarPropriedadesFilamentos();
            CustosBancoDados.CollectionChanged += (s, e) => AtualizarPropriedadesCustos();
        }

        private void AtualizarPropriedadesFilamentos()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextoTotalFilamentos)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibilidadePaginacaoFilamentos)));
        }

        private void AtualizarPropriedadesCustos()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextoTotalCustos)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibilidadePaginacaoCustos)));
        }

        // ==========================================
        // CORREÇÃO: NAVEGAR ATIVANDO AS ABAS E CORES
        // ==========================================
        private void BtnNovoFilamento_Click(object sender, RoutedEventArgs e)
        {
            var telaCadastro = new CadastroCustosView();
            telaCadastro.AtivarAbaFilamento(); // Garante aba Filamento ativa com botão dourado
            MudarDeTela(telaCadastro);
        }

        private void BtnNovoCusto_Click(object sender, RoutedEventArgs e)
        {
            var telaCadastro = new CadastroCustosView();
            telaCadastro.AtivarAbaCusto(); // Garante aba Custo ativa com botão dourado
            MudarDeTela(telaCadastro);
        }

        private void MudarDeTela(UserControl novaTela)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            while (parent != null)
            {
                if (parent is ContentControl cc) { cc.Content = novaTela; return; }
                if (parent is Panel panel)
                {
                    int index = panel.Children.IndexOf(this);
                    if (index >= 0) { panel.Children.RemoveAt(index); panel.Children.Insert(index, novaTela); return; }
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
        }

        private void MenuAcoesFilamento_Click(object sender, MouseButtonEventArgs e)
        {
            var img = sender as FrameworkElement;
            if (img?.DataContext is FilamentoModel filamento)
            {
                if (MessageBox.Show($"Deseja excluir '{filamento.Nome}'?", "Excluir", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    FilamentosBancoDados.Remove(filamento);
            }
        }

        private void MenuAcoesCusto_Click(object sender, MouseButtonEventArgs e)
        {
            var img = sender as FrameworkElement;
            if (img?.DataContext is CustoModel custo)
            {
                if (MessageBox.Show($"Deseja excluir '{custo.Descricao}'?", "Excluir", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    CustosBancoDados.Remove(custo);
            }
        }
    }

    public class FilamentoModel
    {
        public string Nome { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string CorHex { get; set; } = string.Empty;
        public string PrecoFormatado { get; set; } = string.Empty;

        public string CorFundoTipo => Tipo.ToUpper().Contains("PLA") ? "#D4E6F1" :
                                      Tipo.ToUpper().Contains("PETG") ? "#FADBD8" : "#EAEDED";
    }

    public class CustoModel
    {
        public string Descricao { get; set; } = string.Empty;
        public string ValorFormatado { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;

        public string CorFundoTipo => Tipo.ToLower().Contains("fixo") ? "#E8DAEF" :
                                      Tipo.ToLower().Contains("adicional") ? "#D5F5E3" : "#EAEDED";
    }
}