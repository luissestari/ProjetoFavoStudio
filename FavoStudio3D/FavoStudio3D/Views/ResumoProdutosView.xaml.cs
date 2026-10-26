using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FavoStudio3D.Views
{
    public class ProdutoItemModel
    {
        public string? CaminhoImagem { get; set; }
        public string? Nome { get; set; }
        public string? CodigoSku { get; set; }
        public string? FilamentoNome { get; set; }
        public string? FilamentoDetalhe { get; set; }
        public Brush? CorFilamentoBrush { get; set; }
        public string? ImpressoraNome { get; set; }
        public string? ImpressoraModelo { get; set; }
        public string? CustoTotalFormatado { get; set; }
        public string? PrecoSugeridoFormatado { get; set; }
    }

    public partial class ResumoProdutosView : UserControl
    {
        public static ObservableCollection<ProdutoItemModel> ProdutosBancoDados = new ObservableCollection<ProdutoItemModel>();

        public ResumoProdutosView()
        {
            InitializeComponent();
            CarregarProdutos();
            AtualizarPaginacao();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (Window.GetWindow(this) is DashboardWindow dashboard)
                {
                    try
                    {
                        if (dashboard.FindName("TextoTituloPagina") is TextBlock txtTitulo)
                            txtTitulo.Text = "Produtos";

                        if (dashboard.FindName("TextoSubtituloPagina") is TextBlock txtSub)
                            txtSub.Text = "Os produtos cadastrados aparecem aqui após serem calculados e salvos na calculadora.";
                    }
                    catch { }
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void CarregarProdutos()
        {
            ListaProdutos.ItemsSource = ProdutosBancoDados;
            AtualizarPaginacao();
        }

        private void AtualizarPaginacao()
        {
            if (PanelPaginacao == null) return;
            PanelPaginacao.Children.Clear();

            int totalItens = ProdutosBancoDados.Count;
            int itensPorPagina = 5;
            int totalPaginas = Math.Max(1, (int)Math.Ceiling((double)totalItens / itensPorPagina));

            if (totalPaginas > 1)
            {
                var btnAnterior = new Button { Width = 32, Height = 32, Background = Brushes.Transparent, BorderThickness = new Thickness(0), Cursor = Cursors.Hand };
                btnAnterior.Content = new TextBlock { Text = "←", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = (Brush)Brushes.SandyBrown };
                PanelPaginacao.Children.Add(btnAnterior);
            }

            for (int i = 1; i <= totalPaginas; i++)
            {
                if (i == 1)
                {
                    var borderPagina = new Border { Width = 28, Height = 28, CornerRadius = new CornerRadius(5), Background = (Brush)new BrushConverter().ConvertFrom("#B87B2E"), Margin = new Thickness(4, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center };
                    borderPagina.Child = new TextBlock { Text = i.ToString(), FontSize = 13, FontWeight = FontWeights.Bold, Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                    PanelPaginacao.Children.Add(borderPagina);
                }
                else
                {
                    var borderPagina = new Border { Width = 28, Height = 28, CornerRadius = new CornerRadius(5), Background = Brushes.Transparent, Margin = new Thickness(4, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center, Cursor = Cursors.Hand };
                    borderPagina.Child = new TextBlock { Text = i.ToString(), FontSize = 13, FontWeight = FontWeights.SemiBold, Foreground = (Brush)new BrushConverter().ConvertFrom("#795548"), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                    PanelPaginacao.Children.Add(borderPagina);
                }
            }

            if (totalPaginas > 1)
            {
                var btnProximo = new Button { Width = 32, Height = 32, Background = Brushes.Transparent, BorderThickness = new Thickness(0), Cursor = Cursors.Hand };
                btnProximo.Content = new TextBlock { Text = "→", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = (Brush)new BrushConverter().ConvertFrom("#795548") };
                PanelPaginacao.Children.Add(btnProximo);
            }
        }

        private void TxtPesquisa_TextChanged(object sender, TextChangedEventArgs e)
        {
            string termo = TxtPesquisa.Text.ToLower();

            if (string.IsNullOrWhiteSpace(termo))
            {
                ListaProdutos.ItemsSource = ProdutosBancoDados;
            }
            else
            {
                // Filtra verificando se o termo existe no Nome do produto OU no Nome do filamento
                var filtrados = new ObservableCollection<ProdutoItemModel>(
                    ProdutosBancoDados.Where(p =>
                        (p.Nome != null && p.Nome.ToLower().Contains(termo)) ||
                        (p.FilamentoNome != null && p.FilamentoNome.ToLower().Contains(termo))
                    )
                );
                ListaProdutos.ItemsSource = filtrados;
            }
        }

        private void BtnNovoCalculo_Click(object sender, RoutedEventArgs e)
        {
            var telaCalculadora = new CalculadoraView();
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            while (parent != null)
            {
                if (parent is ContentControl cc) { cc.Content = telaCalculadora; return; }
                if (parent is Panel panel)
                {
                    int index = panel.Children.IndexOf(this);
                    if (index >= 0) { panel.Children.RemoveAt(index); panel.Children.Insert(index, telaCalculadora); return; }
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
        }

        private void MenuEditar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is ProdutoItemModel produtoSelecionado)
            {
                MessageBox.Show($"Editar produto: {produtoSelecionado.Nome}", "Editar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void MenuExcluir_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.DataContext is ProdutoItemModel produtoSelecionado)
            {
                ProdutosBancoDados.Remove(produtoSelecionado);
                CarregarProdutos();
            }
        }
    }
}