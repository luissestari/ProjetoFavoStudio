using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace FavoStudio3D.Views
{
    public partial class ResumoCustosMateriaisView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public static ObservableCollection<FilamentoModel> FilamentosBancoDados { get; set; } = new ObservableCollection<FilamentoModel>();
        public static ObservableCollection<CustoModel> CustosBancoDados { get; set; } = new ObservableCollection<CustoModel>();

        public ObservableCollection<FilamentoModel> ListaFilamentos { get; set; } = new ObservableCollection<FilamentoModel>();
        public ObservableCollection<CustoModel> ListaOutrosCustos { get; set; } = new ObservableCollection<CustoModel>();

        public ObservableCollection<string> FiltrosFilamentos { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> FiltrosCustos { get; set; } = new ObservableCollection<string>();

        private string _filtroFilamentoSelecionado = "Todos os tipos";
        public string FiltroFilamentoSelecionado
        {
            get => _filtroFilamentoSelecionado;
            set
            {
                _filtroFilamentoSelecionado = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FiltroFilamentoSelecionado)));
                AplicarFiltroFilamentos();
            }
        }

        private string _filtroCustoSelecionado = "Todos os tipos";
        public string FiltroCustoSelecionado
        {
            get => _filtroCustoSelecionado;
            set
            {
                _filtroCustoSelecionado = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FiltroCustoSelecionado)));
                AplicarFiltroCustos();
            }
        }

        public string TextoTotalFilamentos => ListaFilamentos.Count == 0 ? "Nenhum filamento encontrado" :
                                              ListaFilamentos.Count == 1 ? "1 filamento cadastrado" :
                                              $"{ListaFilamentos.Count} filamentos cadastrados";

        public string TextoTotalCustos => ListaOutrosCustos.Count == 0 ? "Nenhum custo encontrado" :
                                          ListaOutrosCustos.Count == 1 ? "1 custo cadastrado" :
                                          $"{ListaOutrosCustos.Count} custos cadastrados";

        public Visibility VisibilidadePaginacaoFilamentos => ListaFilamentos.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VisibilidadePaginacaoCustos => ListaOutrosCustos.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        // ==========================================
        // VARIÁVEIS PARA O MODAL DE EXCLUSÃO
        // ==========================================
        private Visibility _visibilidadeModalExclusao = Visibility.Collapsed;
        public Visibility VisibilidadeModalExclusao
        {
            get => _visibilidadeModalExclusao;
            set { _visibilidadeModalExclusao = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibilidadeModalExclusao))); }
        }

        private string _mensagemExclusao = string.Empty;
        public string MensagemExclusao
        {
            get => _mensagemExclusao;
            set { _mensagemExclusao = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MensagemExclusao))); }
        }

        private FilamentoModel? _filamentoParaExcluir = null;
        private CustoModel? _custoParaExcluir = null;

        public ResumoCustosMateriaisView()
        {
            InitializeComponent();
            DataContext = this;

            FilamentosBancoDados.CollectionChanged += (s, e) => AtualizarPropriedadesFilamentos();
            CustosBancoDados.CollectionChanged += (s, e) => AtualizarPropriedadesCustos();

            AtualizarFiltrosFilamentos();
            AtualizarFiltrosCustos();
            AplicarFiltroFilamentos();
            AplicarFiltroCustos();
        }

        private void AtualizarPropriedadesFilamentos()
        {
            AtualizarFiltrosFilamentos();
            AplicarFiltroFilamentos();
        }

        private void AtualizarPropriedadesCustos()
        {
            AtualizarFiltrosCustos();
            AplicarFiltroCustos();
        }

        private void AtualizarFiltrosFilamentos()
        {
            FiltrosFilamentos.Clear();
            FiltrosFilamentos.Add("Todos os tipos");
            var tiposExistentes = FilamentosBancoDados.Select(f => f.Tipo).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct();
            foreach (var tipo in tiposExistentes) FiltrosFilamentos.Add(tipo);

            if (!FiltrosFilamentos.Contains(FiltroFilamentoSelecionado))
                FiltroFilamentoSelecionado = "Todos os tipos";
        }

        private void AtualizarFiltrosCustos()
        {
            FiltrosCustos.Clear();
            FiltrosCustos.Add("Todos os tipos");
            var tiposExistentes = CustosBancoDados.Select(c => c.Tipo).Where(t => !string.IsNullOrWhiteSpace(t)).Distinct();
            foreach (var tipo in tiposExistentes) FiltrosCustos.Add(tipo);

            if (!FiltrosCustos.Contains(FiltroCustoSelecionado))
                FiltroCustoSelecionado = "Todos os tipos";
        }

        private void AplicarFiltroFilamentos()
        {
            ListaFilamentos.Clear();
            foreach (var item in FilamentosBancoDados)
            {
                if (FiltroFilamentoSelecionado == "Todos os tipos" || item.Tipo == FiltroFilamentoSelecionado)
                {
                    ListaFilamentos.Add(item);
                }
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextoTotalFilamentos)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibilidadePaginacaoFilamentos)));
        }

        private void AplicarFiltroCustos()
        {
            ListaOutrosCustos.Clear();
            foreach (var item in CustosBancoDados)
            {
                if (FiltroCustoSelecionado == "Todos os tipos" || item.Tipo == FiltroCustoSelecionado)
                {
                    ListaOutrosCustos.Add(item);
                }
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextoTotalCustos)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibilidadePaginacaoCustos)));
        }

        private void BtnFiltroFilamento_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is string filtroSelecionado)
            {
                FiltroFilamentoSelecionado = filtroSelecionado;
                if (VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(btn))) is Popup popup) popup.IsOpen = false;
            }
        }

        private void BtnFiltroCusto_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is string filtroSelecionado)
            {
                FiltroCustoSelecionado = filtroSelecionado;
                if (VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(btn))) is Popup popup) popup.IsOpen = false;
            }
        }

        // ==========================================
        // FUNÇÃO ROBUSTA PARA FECHAR TODOS OS POPUPS
        // ==========================================
        private void FecharTodosOsPopups(DependencyObject parent)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is Popup popup && popup.IsOpen)
                {
                    popup.IsOpen = false;
                    // Se o popup tiver sido aberto por um ToggleButton, temos de o desligar também
                    if (popup.PlacementTarget is ToggleButton toggleBtn)
                    {
                        toggleBtn.IsChecked = false;
                    }
                }
                // Continua a procurar recursivamente
                FecharTodosOsPopups(child);
            }
        }

        private void AcaoEditarFilamento_Click(object sender, RoutedEventArgs e)
        {
            FecharTodosOsPopups(this);
            if (sender is Button btn && btn.DataContext is FilamentoModel filamento)
            {
                var telaCadastro = new CadastroCustosView();
                telaCadastro.CarregarFilamentoParaEdicao(filamento);
                MudarDeTela(telaCadastro);
            }
        }

        private void AcaoEditarCusto_Click(object sender, RoutedEventArgs e)
        {
            FecharTodosOsPopups(this);
            if (sender is Button btn && btn.DataContext is CustoModel custo)
            {
                var telaCadastro = new CadastroCustosView();
                telaCadastro.CarregarCustoParaEdicao(custo);
                MudarDeTela(telaCadastro);
            }
        }

        private void AcaoExcluirFilamento_Click(object sender, RoutedEventArgs e)
        {
            FecharTodosOsPopups(this);
            if (sender is Button btn && btn.DataContext is FilamentoModel filamento)
            {
                _filamentoParaExcluir = filamento;
                _custoParaExcluir = null;
                MensagemExclusao = $"Tem certeza que deseja excluir o filamento '{filamento.Nome}'?";
                VisibilidadeModalExclusao = Visibility.Visible;
            }
        }

        private void AcaoExcluirCusto_Click(object sender, RoutedEventArgs e)
        {
            FecharTodosOsPopups(this);
            if (sender is Button btn && btn.DataContext is CustoModel custo)
            {
                _custoParaExcluir = custo;
                _filamentoParaExcluir = null;
                MensagemExclusao = $"Tem certeza que deseja excluir o custo '{custo.Descricao}'?";
                VisibilidadeModalExclusao = Visibility.Visible;
            }
        }

        private void BtnCancelarExclusao_Click(object sender, RoutedEventArgs e)
        {
            VisibilidadeModalExclusao = Visibility.Collapsed;
            _filamentoParaExcluir = null;
            _custoParaExcluir = null;
        }

        private void BtnConfirmarExclusao_Click(object sender, RoutedEventArgs e)
        {
            if (_filamentoParaExcluir != null)
            {
                FilamentosBancoDados.Remove(_filamentoParaExcluir);
            }
            else if (_custoParaExcluir != null)
            {
                CustosBancoDados.Remove(_custoParaExcluir);
            }

            VisibilidadeModalExclusao = Visibility.Collapsed;
            _filamentoParaExcluir = null;
            _custoParaExcluir = null;
        }

        private void BtnNovoFilamento_Click(object sender, RoutedEventArgs e)
        {
            var telaCadastro = new CadastroCustosView();
            telaCadastro.AtivarAbaFilamento();
            MudarDeTela(telaCadastro);
        }

        private void BtnNovoCusto_Click(object sender, RoutedEventArgs e)
        {
            var telaCadastro = new CadastroCustosView();
            telaCadastro.AtivarAbaCusto();
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