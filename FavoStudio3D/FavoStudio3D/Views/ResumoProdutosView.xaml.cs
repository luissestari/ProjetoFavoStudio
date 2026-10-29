using FavoStudio3D.Models;
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
        public string CaminhoImagem { get; set; } = "";
        public string Nome { get; set; } = "";
        public string CodigoSku { get; set; } = "";
        public string FilamentoNome { get; set; } = "";
        public string FilamentoDetalhe { get; set; } = "";
        public Brush CorFilamentoBrush { get; set; } = Brushes.Black;
        public string ImpressoraNome { get; set; } = "";
        public string ImpressoraModelo { get; set; } = "";
        public string CustoTotalFormatado { get; set; } = "";
        public string PrecoSugeridoFormatado { get; set; } = "";

        public ResultadoCalculoModel? DadosCompletos { get; set; }
    }

    public partial class ResumoProdutosView : UserControl
    {
        public static ObservableCollection<ProdutoItemModel> ProdutosCadastrados = new ObservableCollection<ProdutoItemModel>();
        private ProdutoItemModel? produtoParaExcluir = null;

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
                        // 1. Atualiza os textos do cabeçalho
                        if (dashboard.FindName("TextoTituloPagina") is TextBlock txtTitulo)
                            txtTitulo.Text = "Produtos";

                        if (dashboard.FindName("TextoSubtituloPagina") is TextBlock txtSub)
                            txtSub.Text = "Os produtos cadastrados aparecem aqui após serem calculados e salvos na calculadora.";

                        // 2. Atualiza o título da janela principal
                        dashboard.Title = "Favo Studio 3D - Produtos";

                        // 3. Destaca corretamente o menu de Produtos na barra lateral usando o método da DashboardWindow
                        if (dashboard.FindName("MenuProdutosBorder") is Border borderProd &&
                            dashboard.FindName("MenuProdutosTexto") is TextBlock textoProd)
                        {
                            // Usa reflexão ou chama diretamente se o método for acessível (ou podemos invocar o método privado/público)
                            var metodoAtivar = dashboard.GetType().GetMethod("AtivarBotaoMenu", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            metodoAtivar?.Invoke(dashboard, new object[] { borderProd, textoProd });
                        }
                    }
                    catch { }
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void CarregarProdutos()
        {
            ListaProdutos.ItemsSource = ProdutosCadastrados;
            AtualizarPaginacao();
        }

        private void AtualizarPaginacao()
        {
            if (PanelPaginacao == null) return;
            PanelPaginacao.Children.Clear();

            int totalItens = ProdutosCadastrados.Count;
            int itensPorPagina = 5;
            int totalPaginas = Math.Max(1, (int)Math.Ceiling((double)totalItens / itensPorPagina));

            // Atualiza o texto do contador à esquerda (Ex: "1 produto cadastrado" ou "3 produtos cadastrados")
            if (TxtContadorProdutos != null)
            {
                TxtContadorProdutos.Text = totalItens == 1 ? "1 produto cadastrado" : $"{totalItens} produtos cadastrados";
            }

            // Botão Anterior (<)
            var borderAnterior = new Border
            {
                Width = 28,
                Height = 28,
                CornerRadius = new CornerRadius(6),
                Background = Brushes.White,
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#EADCC9"),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand
            };
            borderAnterior.Child = new TextBlock
            {
                Text = "<",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)new BrushConverter().ConvertFrom("#795548"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            PanelPaginacao.Children.Add(borderAnterior);

            // Número da Página (Ex: 1)
            var borderPagina = new Border
            {
                Width = 28,
                Height = 28,
                CornerRadius = new CornerRadius(6),
                Background = (Brush)new BrushConverter().ConvertFrom("#EBB45D"),
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            borderPagina.Child = new TextBlock
            {
                Text = "1",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)new BrushConverter().ConvertFrom("#3E2723"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            PanelPaginacao.Children.Add(borderPagina);

            // Botão Próximo (>)
            var borderProximo = new Border
            {
                Width = 28,
                Height = 28,
                CornerRadius = new CornerRadius(6),
                Background = Brushes.White,
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#EADCC9"),
                BorderThickness = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand
            };
            borderProximo.Child = new TextBlock
            {
                Text = ">",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)new BrushConverter().ConvertFrom("#795548"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            PanelPaginacao.Children.Add(borderProximo);
        }

        private void TxtPesquisa_TextChanged(object sender, TextChangedEventArgs e)
        {
            string termo = TxtPesquisa.Text?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(termo))
            {
                ListaProdutos.ItemsSource = ProdutosCadastrados;
            }
            else
            {
                var filtrados = new ObservableCollection<ProdutoItemModel>(
                    ProdutosCadastrados.Where(p =>
                        (p.Nome != null && p.Nome.ToLower().Contains(termo)) ||
                        (p.FilamentoNome != null && p.FilamentoNome.ToLower().Contains(termo))
                    )
                );
                ListaProdutos.ItemsSource = filtrados;
            }
        }

        private void NavegarPara(UserControl novaTela)
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

        private void AcionarBotaoLateralCalculadora()
        {
            if (Window.GetWindow(this) is DashboardWindow dashboard)
            {
                object? obj1 = dashboard.FindName("MenuCalculadora");
                object? obj2 = dashboard.FindName("BtnCalculadora");

                RadioButton? rb = (obj1 as RadioButton) ?? (obj2 as RadioButton);
                if (rb != null) rb.IsChecked = true;
                else NavegarPara(new CalculadoraView());
            }
        }

        private void BtnNovoCalculo_Click(object sender, RoutedEventArgs e)
        {
            CalculadoraView.MemoriaRascunho.LimparTudo();
            AcionarBotaoLateralCalculadora();
        }

        private void BtnAcoes_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.DataContext = btn.DataContext;
                btn.ContextMenu.IsOpen = true;
                e.Handled = true;
            }
        }

        private ProdutoItemModel? ObterProdutoDoMenu(object sender)
        {
            if (sender is MenuItem menuItem)
            {
                if (menuItem.DataContext is ProdutoItemModel produtoDireto) return produtoDireto;
                if (menuItem.Parent is ContextMenu menu && menu.PlacementTarget is Button btn && btn.DataContext is ProdutoItemModel produtoBotao) return produtoBotao;
            }
            return null;
        }

        private void MenuVisualizar_Click(object sender, RoutedEventArgs e)
        {
            var produto = ObterProdutoDoMenu(sender);
            if (produto != null)
            {
                if (produto.DadosCompletos != null)
                {
                    NavegarPara(new ResumoCalculadoraView(produto.DadosCompletos, modoVisualizacao: true));
                }
                else
                {
                    var resultado = new ResultadoCalculoModel
                    {
                        ProdutoNome = produto.Nome ?? "Peça 3D",
                        CustoTotalProducao = produto.CustoTotalFormatado ?? "R$ 0,00",
                        PrecoVendaSugerido = produto.PrecoSugeridoFormatado ?? "R$ 0,00",
                        CaminhoImagemProduto = produto.CaminhoImagem ?? ""
                    };
                    resultado.Parametros.Add(new ParametroInfo { Rotulo = "Filamento", Valor = produto.FilamentoNome ?? "" });
                    resultado.Parametros.Add(new ParametroInfo { Rotulo = "Cor", Valor = produto.FilamentoDetalhe ?? "" });
                    resultado.Parametros.Add(new ParametroInfo { Rotulo = "Impressora", Valor = produto.ImpressoraNome ?? "" });

                    NavegarPara(new ResumoCalculadoraView(resultado, modoVisualizacao: true));
                }
            }
        }

        private void MenuEditar_Click(object sender, RoutedEventArgs e)
        {
            var produto = ObterProdutoDoMenu(sender);
            if (produto != null)
            {
                // 1. Define a referência para o sistema saber que estamos a atualizar e não a criar um novo
                CalculadoraView.MemoriaRascunho.ProdutoEmEdicaoRef = produto;
                CalculadoraView.MemoriaRascunho.CalculoPendente = false;

                // 2. Restaura os dados básicos
                CalculadoraView.MemoriaRascunho.Nome = produto.Nome ?? "";
                CalculadoraView.MemoriaRascunho.CaminhoImagem = produto.CaminhoImagem ?? "";

                // 3. Se houver os dados detalhados do cálculo anterior, preenche o rascunho para reaparecer no formulário
                if (produto.DadosCompletos != null)
                {
                    var res = produto.DadosCompletos;
                    CalculadoraView.MemoriaRascunho.Categoria = res.ProdutoCategoria ?? "";
                    CalculadoraView.MemoriaRascunho.Descricao = res.ProdutoDescricao ?? "";

                    // Se você guardar mais propriedades detalhadas no ResultadoCalculoModel ou parâmetros, pode resgatar aqui.
                }

                // 4. Redireciona para a tela da calculadora com os dados carregados
                AcionarBotaoLateralCalculadora();
            }
        }

        private void MenuExcluir_Solicitar_Click(object sender, RoutedEventArgs e)
        {
            var produto = ObterProdutoDoMenu(sender);
            if (produto != null)
            {
                produtoParaExcluir = produto;
                TxtMensagemExclusao.Text = $"Tem certeza que deseja excluir o produto '{produto.Nome}'?";
                ModalExclusao.Visibility = Visibility.Visible;
            }
        }

        private void BtnCancelarExclusao_Click(object sender, RoutedEventArgs e)
        {
            ModalExclusao.Visibility = Visibility.Collapsed;
            produtoParaExcluir = null;
        }

        private void BtnConfirmarExclusao_Click(object sender, RoutedEventArgs e)
        {
            if (produtoParaExcluir != null)
            {
                ProdutosCadastrados.Remove(produtoParaExcluir);
                CarregarProdutos();
            }
            ModalExclusao.Visibility = Visibility.Collapsed;
            produtoParaExcluir = null;
        }
    }
}