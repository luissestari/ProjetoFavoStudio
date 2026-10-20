using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FavoStudio3D.Views
{
    public partial class CadastroCustosView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<CorFilamento> ListaCores { get; set; } = new ObservableCollection<CorFilamento>();

        private CorFilamento _corSelecionada = new CorFilamento();
        public CorFilamento CorSelecionada
        {
            get { return _corSelecionada; }
            set { _corSelecionada = value; OnPropertyChanged(nameof(CorSelecionada)); }
        }

        private Visibility _botaoOutraCorVisivel = Visibility.Visible;
        public Visibility BotaoOutraCorVisivel
        {
            get { return _botaoOutraCorVisivel; }
            set { _botaoOutraCorVisivel = value; OnPropertyChanged(nameof(BotaoOutraCorVisivel)); }
        }

        private Visibility _painelNovaCorVisivel = Visibility.Collapsed;
        public Visibility PainelNovaCorVisivel
        {
            get { return _painelNovaCorVisivel; }
            set { _painelNovaCorVisivel = value; OnPropertyChanged(nameof(PainelNovaCorVisivel)); }
        }

        private string _novaCorNome = string.Empty;
        public string NovaCorNome
        {
            get { return _novaCorNome; }
            set { _novaCorNome = value; OnPropertyChanged(nameof(NovaCorNome)); }
        }

        private string _novaCorHex = string.Empty;
        public string NovaCorHex
        {
            get { return _novaCorHex; }
            set { _novaCorHex = value; OnPropertyChanged(nameof(NovaCorHex)); }
        }

        public ObservableCollection<string> ListaTiposCusto { get; set; } = new ObservableCollection<string>();

        private string _tipoCustoSelecionado = string.Empty;
        public string TipoCustoSelecionado
        {
            get { return _tipoCustoSelecionado; }
            set { _tipoCustoSelecionado = value; OnPropertyChanged(nameof(TipoCustoSelecionado)); }
        }

        private Visibility _visibilidadeFilamento = Visibility.Visible;
        public Visibility VisibilidadeFilamento
        {
            get { return _visibilidadeFilamento; }
            set { _visibilidadeFilamento = value; OnPropertyChanged(nameof(VisibilidadeFilamento)); }
        }

        private Visibility _visibilidadeCusto = Visibility.Collapsed;
        public Visibility VisibilidadeCusto
        {
            get { return _visibilidadeCusto; }
            set { _visibilidadeCusto = value; OnPropertyChanged(nameof(VisibilidadeCusto)); }
        }

        // ==========================================
        // VARIÁVEIS DE CONTROLO DO MODO DE EDIÇÃO
        // ==========================================
        private FilamentoModel? _filamentoEmEdicao = null;
        private CustoModel? _custoEmEdicao = null;

        public CadastroCustosView()
        {
            InitializeComponent();
            DataContext = this;

            CarregarCores();
            CarregarTiposCusto();
            AtivarAbaFilamento();
        }

        // ==========================================
        // MÉTODOS PARA CARREGAR OS DADOS DO RESUMO PARA A EDIÇÃO
        // ==========================================
        public void CarregarFilamentoParaEdicao(FilamentoModel filamento)
        {
            _filamentoEmEdicao = filamento;
            AtivarAbaFilamento();

            TxtNome.Text = filamento.Nome;
            TxtTipo.Text = filamento.Tipo;
            TxtPreco.Text = filamento.PrecoFormatado.Replace("R$", "").Trim();

            var corCorreta = ListaCores.FirstOrDefault(c => c.Hex.Equals(filamento.CorHex, System.StringComparison.OrdinalIgnoreCase));
            if (corCorreta != null)
            {
                CorSelecionada = corCorreta;
            }
            else
            {
                var novaCor = new CorFilamento { Nome = "Cor Personalizada", Hex = filamento.CorHex, Borda = "Transparent", VisibilidadeExcluir = Visibility.Visible };
                ListaCores.Add(novaCor);
                CorSelecionada = novaCor;
            }
        }

        public void CarregarCustoParaEdicao(CustoModel custo)
        {
            _custoEmEdicao = custo;
            AtivarAbaCusto();

            TxtDescricaoCusto.Text = custo.Descricao;
            TxtValorCusto.Text = custo.ValorFormatado.Replace("R$", "").Trim();

            if (ListaTiposCusto.Contains(custo.Tipo))
                TipoCustoSelecionado = custo.Tipo;
            else
                TipoCustoSelecionado = custo.Tipo;
        }

        // ==========================================
        // MÉTODOS QUE ATUALIZAM O SUBTÍTULO DO DASHBOARD
        // ==========================================
        public void AtivarAbaFilamento()
        {
            VisibilidadeFilamento = Visibility.Visible;
            VisibilidadeCusto = Visibility.Collapsed;
            TabFilamento.Background = (Brush)new BrushConverter().ConvertFromString("#EBB45D")!;
            TabCusto.Background = Brushes.Transparent;

            AtualizarSubtituloDashboard("Cadastre seus filamentos para atualizar os cálculos.");
        }

        public void AtivarAbaCusto()
        {
            VisibilidadeFilamento = Visibility.Collapsed;
            VisibilidadeCusto = Visibility.Visible;
            TabCusto.Background = (Brush)new BrushConverter().ConvertFromString("#EBB45D")!;
            TabFilamento.Background = Brushes.Transparent;

            AtualizarSubtituloDashboard("Cadastre custos fixos e adicionais para atualizar os cálculos.");
        }

        private void AtualizarSubtituloDashboard(string novoSubtitulo)
        {
            Dispatcher.BeginInvoke(new System.Action(() =>
            {
                if (Window.GetWindow(this) is DashboardWindow dashboard)
                {
                    dashboard.TextoSubtituloPagina.Text = novoSubtitulo;
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void TabFilamento_Click(object sender, MouseButtonEventArgs e) => AtivarAbaFilamento();
        private void TabCusto_Click(object sender, MouseButtonEventArgs e) => AtivarAbaCusto();

        private void TabResumo_Click(object sender, MouseButtonEventArgs e)
        {
            var telaResumo = new ResumoCustosMateriaisView();
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            while (parent != null)
            {
                if (parent is ContentControl cc) { cc.Content = telaResumo; return; }
                if (parent is Panel panel)
                {
                    int index = panel.Children.IndexOf(this);
                    if (index >= 0) { panel.Children.RemoveAt(index); panel.Children.Insert(index, telaResumo); return; }
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
        }

        private void CarregarCores()
        {
            ListaCores.Add(new CorFilamento { Nome = "Selecione uma cor", Hex = "Transparent", Borda = "Transparent", VisibilidadeExcluir = Visibility.Collapsed, VisibilidadeCirculo = Visibility.Collapsed });

            ListaCores.Add(new CorFilamento { Nome = "Branco", Hex = "#FFFFFF", Borda = "#E0E0E0", VisibilidadeExcluir = Visibility.Collapsed });
            ListaCores.Add(new CorFilamento { Nome = "Preto", Hex = "#212121", Borda = "Transparent", VisibilidadeExcluir = Visibility.Collapsed });
            ListaCores.Add(new CorFilamento { Nome = "Cinza", Hex = "#9E9E9E", Borda = "Transparent", VisibilidadeExcluir = Visibility.Collapsed });
            ListaCores.Add(new CorFilamento { Nome = "Vermelho", Hex = "#F44336", Borda = "Transparent", VisibilidadeExcluir = Visibility.Collapsed });
            ListaCores.Add(new CorFilamento { Nome = "Azul", Hex = "#2196F3", Borda = "Transparent", VisibilidadeExcluir = Visibility.Collapsed });

            CorSelecionada = ListaCores[0];
            ListaCores.RemoveAt(0);
        }

        private void CarregarTiposCusto()
        {
            TipoCustoSelecionado = "Selecione o tipo";
            ListaTiposCusto.Add("Custo fixo");
            ListaTiposCusto.Add("Custo adicional");
        }

        private void TipoCusto_Click(object sender, RoutedEventArgs e)
        {
            var botao = sender as Button;
            if (botao?.DataContext is string tipoClicado)
            {
                TipoCustoSelecionado = tipoClicado;
                BtnAbrirTiposCusto.IsChecked = false;
            }
        }

        private void CorSelecionada_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = sender as FrameworkElement;
            if (item?.DataContext is CorFilamento corClicada)
            {
                CorSelecionada = corClicada;
                BtnAbrirCores.IsChecked = false;
                ResetarPainelExpansao();
            }
        }

        private void BtnExcluirCor_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var botao = sender as Button;
            if (botao?.DataContext is CorFilamento corParaExcluir)
            {
                ListaCores.Remove(corParaExcluir);
                if (CorSelecionada == corParaExcluir && ListaCores.Count > 0)
                {
                    CorSelecionada = ListaCores[0];
                }
            }
        }

        private void BtnCorPersonalizada_Click(object sender, RoutedEventArgs e)
        {
            BotaoOutraCorVisivel = Visibility.Collapsed;
            PainelNovaCorVisivel = Visibility.Visible;
        }

        private void BtnCancelarNovaCor_Click(object sender, RoutedEventArgs e)
        {
            ResetarPainelExpansao();
        }

        private void BtnSalvarNovaCor_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NovaCorNome) || string.IsNullOrWhiteSpace(NovaCorHex)) return;

            string hexDigitado = NovaCorHex.Trim();
            if (!hexDigitado.StartsWith("#")) hexDigitado = "#" + hexDigitado;

            try { var c = new BrushConverter().ConvertFromString(hexDigitado); }
            catch { MessageBox.Show("Código HEX inválido!", "Erro", MessageBoxButton.OK, MessageBoxImage.Error); return; }

            var novaCor = new CorFilamento
            {
                Nome = NovaCorNome.Trim(),
                Hex = hexDigitado,
                Borda = (hexDigitado.ToUpper() == "#FFFFFF" || hexDigitado.ToUpper() == "#FFF") ? "#E0E0E0" : "Transparent",
                VisibilidadeExcluir = Visibility.Visible
            };

            ListaCores.Add(novaCor);
            CorSelecionada = novaCor;
            BtnAbrirCores.IsChecked = false;
            ResetarPainelExpansao();
        }

        private void ResetarPainelExpansao()
        {
            NovaCorNome = string.Empty; NovaCorHex = string.Empty;
            PainelNovaCorVisivel = Visibility.Collapsed;
            BotaoOutraCorVisivel = Visibility.Visible;
        }

        private void BtnLimpar_Click(object sender, RoutedEventArgs e)
        {
            TxtNome.Text = ""; TxtTipo.Text = ""; TxtPreco.Text = ""; TxtQuantidade.Text = "";
            TxtDescricaoCusto.Text = ""; TxtValorCusto.Text = ""; TipoCustoSelecionado = "Selecione o tipo";
            _filamentoEmEdicao = null;
            _custoEmEdicao = null;
        }

        private void BtnSalvarCadastro_Click(object sender, RoutedEventArgs e)
        {
            // ==============================================================
            // VALIDAÇÃO INTELIGENTE (Depende da aba que está aberta)
            // ==============================================================
            if (VisibilidadeFilamento == Visibility.Visible)
            {
                // Se a aba filamento estiver visível, valida os campos de filamento
                if (string.IsNullOrWhiteSpace(TxtNome.Text) ||
                    string.IsNullOrWhiteSpace(TxtTipo.Text) ||
                    string.IsNullOrWhiteSpace(TxtPreco.Text))
                {
                    ModalValidacao.Visibility = Visibility.Visible;
                    return;
                }
            }
            else
            {
                // Se a aba custo estiver visível, valida os campos de custo
                if (string.IsNullOrWhiteSpace(TxtDescricaoCusto.Text) ||
                    string.IsNullOrWhiteSpace(TxtValorCusto.Text))
                {
                    ModalValidacao.Visibility = Visibility.Visible;
                    return;
                }
            }

            if (VisibilidadeFilamento == Visibility.Visible)
            {
                var filamentoAtualizado = new FilamentoModel
                {
                    Nome = TxtNome.Text,
                    Tipo = TxtTipo.Text,
                    CorHex = CorSelecionada.Hex,
                    PrecoFormatado = "R$ " + TxtPreco.Text
                };

                if (_filamentoEmEdicao != null) // Atualizar existente
                {
                    int index = ResumoCustosMateriaisView.FilamentosBancoDados.IndexOf(_filamentoEmEdicao);
                    if (index >= 0) ResumoCustosMateriaisView.FilamentosBancoDados[index] = filamentoAtualizado;
                }
                else // Criar novo
                {
                    ResumoCustosMateriaisView.FilamentosBancoDados.Add(filamentoAtualizado);
                }
            }
            else
            {
                var custoAtualizado = new CustoModel
                {
                    Descricao = TxtDescricaoCusto.Text,
                    ValorFormatado = "R$ " + TxtValorCusto.Text,
                    Tipo = TipoCustoSelecionado == "Selecione o tipo" ? "Custo fixo" : TipoCustoSelecionado
                };

                if (_custoEmEdicao != null) // Atualizar existente
                {
                    int index = ResumoCustosMateriaisView.CustosBancoDados.IndexOf(_custoEmEdicao);
                    if (index >= 0) ResumoCustosMateriaisView.CustosBancoDados[index] = custoAtualizado;
                }
                else // Criar novo
                {
                    ResumoCustosMateriaisView.CustosBancoDados.Add(custoAtualizado);
                }
            }

            var telaResumo = new ResumoCustosMateriaisView();
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            while (parent != null)
            {
                if (parent is ContentControl cc) { cc.Content = telaResumo; return; }
                if (parent is Panel panel)
                {
                    int index = panel.Children.IndexOf(this);
                    if (index >= 0) { panel.Children.RemoveAt(index); panel.Children.Insert(index, telaResumo); return; }
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
        }

        // Função que fecha o modal de aviso quando o utilizador clica em OK (Posicionada na classe correta agora)
        private void BtnFecharValidacao_Click(object sender, RoutedEventArgs e)
        {
            ModalValidacao.Visibility = Visibility.Collapsed;
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class CorFilamento
    {
        public string Nome { get; set; } = string.Empty;
        public string Hex { get; set; } = string.Empty;
        public string Borda { get; set; } = string.Empty;
        public Visibility VisibilidadeExcluir { get; set; } = Visibility.Collapsed;
        public Visibility VisibilidadeCirculo { get; set; } = Visibility.Visible;
    }
}