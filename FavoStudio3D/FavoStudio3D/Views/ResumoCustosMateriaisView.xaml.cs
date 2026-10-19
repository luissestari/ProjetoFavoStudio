using System;
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

        private const int ItensPorPagina = 6;

        private int _totalPaginasFilamentos = 1;
        private int _paginaAtualFilamentos = 1;
        public int PaginaAtualFilamentos
        {
            get => _paginaAtualFilamentos;
            set { _paginaAtualFilamentos = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PaginaAtualFilamentos))); }
        }

        private int _totalPaginasCustos = 1;
        private int _paginaAtualCustos = 1;
        public int PaginaAtualCustos
        {
            get => _paginaAtualCustos;
            set { _paginaAtualCustos = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PaginaAtualCustos))); }
        }

        private string _textoPesquisaFilamento = string.Empty;
        public string TextoPesquisaFilamento
        {
            get => _textoPesquisaFilamento;
            set { _textoPesquisaFilamento = value; PaginaAtualFilamentos = 1; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextoPesquisaFilamento))); AplicarFiltroFilamentos(); }
        }

        private string _textoPesquisaCusto = string.Empty;
        public string TextoPesquisaCusto
        {
            get => _textoPesquisaCusto;
            set { _textoPesquisaCusto = value; PaginaAtualCustos = 1; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextoPesquisaCusto))); AplicarFiltroCustos(); }
        }

        private string _filtroFilamentoSelecionado = "Todos os tipos";
        public string FiltroFilamentoSelecionado
        {
            get => _filtroFilamentoSelecionado;
            set
            {
                _filtroFilamentoSelecionado = value;
                PaginaAtualFilamentos = 1;
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
                PaginaAtualCustos = 1;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FiltroCustoSelecionado)));
                AplicarFiltroCustos();
            }
        }

        public string TextoTotalFilamentos => FilamentosBancoDados.Count == 0 ? "Nenhum filamento encontrado" :
                                              FilamentosBancoDados.Count == 1 ? "1 filamento cadastrado" :
                                              $"{FilamentosBancoDados.Count} filamentos cadastrados";

        public string TextoTotalCustos => CustosBancoDados.Count == 0 ? "Nenhum custo encontrado" :
                                          CustosBancoDados.Count == 1 ? "1 custo cadastrado" :
                                          $"{CustosBancoDados.Count} custos cadastrados";

        public Visibility VisibilidadePaginacaoFilamentos => FilamentosBancoDados.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VisibilidadePaginacaoCustos => CustosBancoDados.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility VisibilidadeEstadoVazio => (FilamentosBancoDados.Count == 0 && CustosBancoDados.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility VisibilidadeConteudo => (FilamentosBancoDados.Count > 0 || CustosBancoDados.Count > 0) ? Visibility.Visible : Visibility.Collapsed;

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

            // Alimenta a memória compartilhada inicial se estiver vazia
            if (FilamentosBancoDados.Count == 0)
            {
                FilamentosBancoDados.Add(new FilamentoModel { Nome = "PLA Branco", Tipo = "PLA", CorHex = "#FFFFFF", PrecoFormatado = "R$ 100,00" });
                FilamentosBancoDados.Add(new FilamentoModel { Nome = "PETG Preto", Tipo = "PETG", CorHex = "#000000", PrecoFormatado = "R$ 120,00" });
            }

            if (CustosBancoDados.Count == 0)
            {
                CustosBancoDados.Add(new CustoModel { Descricao = "Saco a vácuo", ValorFormatado = "R$ 135,00", Tipo = "Custo Fixo" });
                CustosBancoDados.Add(new CustoModel { Descricao = "Lixas", ValorFormatado = "R$ 30,00", Tipo = "Custo Adicional" });
            }

            FilamentosBancoDados.CollectionChanged += (s, e) => AtualizarPropriedadesFilamentos();
            CustosBancoDados.CollectionChanged += (s, e) => AtualizarPropriedadesCustos();

            AtualizarFiltrosFilamentos();
            AtualizarFiltrosCustos();
            AplicarFiltroFilamentos();
            AplicarFiltroCustos();

            // Atualiza o subtítulo sempre que esta tela for carregada na interface
            this.Loaded += (s, e) => AtualizarSubtituloDashboard();
        }

        // =========================================================
        // MÉTODOS DE ATUALIZAÇÃO E COMUNICAÇÃO COM O DASHBOARD
        // =========================================================

        private void AtualizarSubtituloDashboard()
        {
            // Procura a janela mãe (DashboardWindow) para atualizar o texto do cabeçalho
            if (Window.GetWindow(this) is DashboardWindow dashboard)
            {
                if (FilamentosBancoDados.Count > 0 || CustosBancoDados.Count > 0)
                {
                    dashboard.TextoSubtituloPagina.Text = "Gerencie os valores dos seus custos e filamentos para manter os cálculos sempre atualizados.";
                }
                else
                {
                    dashboard.TextoSubtituloPagina.Text = "Cadastre seus filamentos e custos para manter os valores sempre atualizados.";
                }
            }
        }

        private void AtualizarPropriedadesFilamentos()
        {
            AtualizarFiltrosFilamentos();
            AplicarFiltroFilamentos();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibilidadeEstadoVazio)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibilidadeConteudo)));
            AtualizarSubtituloDashboard(); // Mantém o texto atualizado se deletar tudo
        }

        private void AtualizarPropriedadesCustos()
        {
            AtualizarFiltrosCustos();
            AplicarFiltroCustos();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibilidadeEstadoVazio)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibilidadeConteudo)));
            AtualizarSubtituloDashboard(); // Mantém o texto atualizado se deletar tudo
        }

        // =========================================================
        // RESTANTE DO CÓDIGO
        // =========================================================

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
            string termoPesquisa = TextoPesquisaFilamento?.ToLower() ?? "";

            var filtrados = FilamentosBancoDados.Where(item =>
                (FiltroFilamentoSelecionado == "Todos os tipos" || item.Tipo == FiltroFilamentoSelecionado) &&
                (string.IsNullOrWhiteSpace(termoPesquisa) || item.Nome.ToLower().Contains(termoPesquisa))
            ).ToList();

            _totalPaginasFilamentos = (int)Math.Ceiling(filtrados.Count / (double)ItensPorPagina);
            if (_totalPaginasFilamentos == 0) _totalPaginasFilamentos = 1;
            if (PaginaAtualFilamentos > _totalPaginasFilamentos) PaginaAtualFilamentos = _totalPaginasFilamentos;

            var paginados = filtrados.Skip((PaginaAtualFilamentos - 1) * ItensPorPagina).Take(ItensPorPagina);

            foreach (var item in paginados) ListaFilamentos.Add(item);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextoTotalFilamentos)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibilidadePaginacaoFilamentos)));
        }

        private void AplicarFiltroCustos()
        {
            ListaOutrosCustos.Clear();
            string termoPesquisa = TextoPesquisaCusto?.ToLower() ?? "";

            var filtrados = CustosBancoDados.Where(item =>
                (FiltroCustoSelecionado == "Todos os tipos" || item.Tipo == FiltroCustoSelecionado) &&
                (string.IsNullOrWhiteSpace(termoPesquisa) || item.Descricao.ToLower().Contains(termoPesquisa))
            ).ToList();

            _totalPaginasCustos = (int)Math.Ceiling(filtrados.Count / (double)ItensPorPagina);
            if (_totalPaginasCustos == 0) _totalPaginasCustos = 1;
            if (PaginaAtualCustos > _totalPaginasCustos) PaginaAtualCustos = _totalPaginasCustos;

            var paginados = filtrados.Skip((PaginaAtualCustos - 1) * ItensPorPagina).Take(ItensPorPagina);

            foreach (var item in paginados) ListaOutrosCustos.Add(item);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextoTotalCustos)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(VisibilidadePaginacaoCustos)));
        }

        private void PaginaAnteriorFilamento_Click(object sender, MouseButtonEventArgs e)
        {
            if (PaginaAtualFilamentos > 1) { PaginaAtualFilamentos--; AplicarFiltroFilamentos(); }
        }

        private void PaginaProximaFilamento_Click(object sender, MouseButtonEventArgs e)
        {
            if (PaginaAtualFilamentos < _totalPaginasFilamentos) { PaginaAtualFilamentos++; AplicarFiltroFilamentos(); }
        }

        private void PaginaAnteriorCusto_Click(object sender, MouseButtonEventArgs e)
        {
            if (PaginaAtualCustos > 1) { PaginaAtualCustos--; AplicarFiltroCustos(); }
        }

        private void PaginaProximaCusto_Click(object sender, MouseButtonEventArgs e)
        {
            if (PaginaAtualCustos < _totalPaginasCustos) { PaginaAtualCustos++; AplicarFiltroCustos(); }
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

        private void FecharTodosOsPopups(DependencyObject parent)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is Popup popup && popup.IsOpen)
                {
                    popup.IsOpen = false;
                    if (popup.PlacementTarget is ToggleButton toggleBtn) toggleBtn.IsChecked = false;
                }
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
            if (_filamentoParaExcluir != null) FilamentosBancoDados.Remove(_filamentoParaExcluir);
            else if (_custoParaExcluir != null) CustosBancoDados.Remove(_custoParaExcluir);

            VisibilidadeModalExclusao = Visibility.Collapsed;
            _filamentoParaExcluir = null;
            _custoParaExcluir = null;
        }

        private void BtnPrimeiroCadastro_Click(object sender, RoutedEventArgs e) => BtnNovoFilamento_Click(sender, e);

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