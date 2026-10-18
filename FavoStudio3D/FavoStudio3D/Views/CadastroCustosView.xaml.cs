using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public CadastroCustosView()
        {
            InitializeComponent();
            DataContext = this;

            CarregarCores();
            CarregarTiposCusto();
            AtivarAbaFilamento(); // Garante o estado inicial correto
        }

        // ==========================================
        // FUNÇÕES DE CONTROLO DAS ABAS (CORES E TELAS)
        // ==========================================
        public void AtivarAbaFilamento()
        {
            VisibilidadeFilamento = Visibility.Visible;
            VisibilidadeCusto = Visibility.Collapsed;
            TabFilamento.Background = (Brush)new BrushConverter().ConvertFromString("#EBB45D")!;
            TabCusto.Background = Brushes.Transparent;
        }

        public void AtivarAbaCusto()
        {
            VisibilidadeFilamento = Visibility.Collapsed;
            VisibilidadeCusto = Visibility.Visible;
            TabCusto.Background = (Brush)new BrushConverter().ConvertFromString("#EBB45D")!;
            TabFilamento.Background = Brushes.Transparent;
        }

        private void TabFilamento_Click(object sender, MouseButtonEventArgs e)
        {
            AtivarAbaFilamento();
        }

        private void TabCusto_Click(object sender, MouseButtonEventArgs e)
        {
            AtivarAbaCusto();
        }

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

        // ==========================================
        // LÓGICA INTERNA DE DADOS
        // ==========================================
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
        }

        private void BtnSalvarCadastro_Click(object sender, RoutedEventArgs e)
        {
            if (VisibilidadeFilamento == Visibility.Visible)
            {
                ResumoCustosMateriaisView.FilamentosBancoDados.Add(new FilamentoModel
                {
                    Nome = string.IsNullOrWhiteSpace(TxtNome.Text) ? "Sem Nome" : TxtNome.Text,
                    Tipo = string.IsNullOrWhiteSpace(TxtTipo.Text) ? "PLA" : TxtTipo.Text,
                    CorHex = CorSelecionada.Hex,
                    PrecoFormatado = "R$ " + (string.IsNullOrWhiteSpace(TxtPreco.Text) ? "0,00" : TxtPreco.Text)
                });
            }
            else
            {
                ResumoCustosMateriaisView.CustosBancoDados.Add(new CustoModel
                {
                    Descricao = string.IsNullOrWhiteSpace(TxtDescricaoCusto.Text) ? "Sem Descrição" : TxtDescricaoCusto.Text,
                    ValorFormatado = "R$ " + (string.IsNullOrWhiteSpace(TxtValorCusto.Text) ? "0,00" : TxtValorCusto.Text),
                    Tipo = TipoCustoSelecionado == "Selecione o tipo" ? "Custo fixo" : TipoCustoSelecionado
                });
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