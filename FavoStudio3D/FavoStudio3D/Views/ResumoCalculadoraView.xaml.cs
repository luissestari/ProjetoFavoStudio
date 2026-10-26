using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FavoStudio3D.Models;

namespace FavoStudio3D.Views
{
    public partial class ResumoCalculadoraView : UserControl
    {
        public ResumoCalculadoraView(ResultadoCalculoModel dados)
        {
            InitializeComponent();
            DataContext = dados;
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
                            txtTitulo.Text = "Revisar e salvar";
                        if (dashboard.FindName("TextoSubtituloPagina") is TextBlock txtSub)
                            txtSub.Text = "Revise as informações antes de salvar o produto.";
                    }
                    catch { }
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is DashboardWindow dashboard)
            {
                try
                {
                    if (dashboard.FindName("TextoTituloPagina") is TextBlock txtTitulo)
                        txtTitulo.Text = "Calculadora";
                    if (dashboard.FindName("TextoSubtituloPagina") is TextBlock txtSub)
                        txtSub.Text = "Calcule os custos de produção e defina o preço de venda dos seus produtos 3D.";
                }
                catch { }
            }

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

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            // Exibe o modal customizado de sucesso
            ModalSucesso.Visibility = Visibility.Visible;
        }

        private void BtnFecharModal_Click(object sender, RoutedEventArgs e)
        {
            var resultadoCalculado = this.DataContext as ResultadoCalculoModel;
            int proximoNumero = ResumoProdutosView.ProdutosBancoDados.Count + 1;
            string skuSequencial = $"#PROD-{proximoNumero:D3}";

            var novoProduto = new ProdutoItemModel
            {
                Nome = string.IsNullOrWhiteSpace(CalculadoraView.MemoriaRascunho.Nome) ? "Peça 3D" : CalculadoraView.MemoriaRascunho.Nome,
                CodigoSku = skuSequencial,

                // Puxa exatamente o nome (ex: "PLA Preto")
                FilamentoNome = CalculadoraView.MemoriaRascunho.NomeFilamentoVisual,

                // Puxa o HEX exato gravado no sistema sem conversões extras (ex: "#000000")
                FilamentoDetalhe = CalculadoraView.MemoriaRascunho.CodigoHexVisual,

                // Aplica a cor real na bolinha
                CorFilamentoBrush = CalculadoraView.MemoriaRascunho.CorFilamentoVisualBrush,

                ImpressoraNome = CalculadoraView.MemoriaRascunho.NomeImpressoraVisual,
                ImpressoraModelo = CalculadoraView.MemoriaRascunho.ModeloImpressoraVisual,
                CustoTotalFormatado = resultadoCalculado?.CustoTotalProducao ?? "R$ 0,00",
                PrecoSugeridoFormatado = resultadoCalculado?.PrecoVendaSugerido ?? "R$ 0,00",
                CaminhoImagem = string.IsNullOrWhiteSpace(CalculadoraView.MemoriaRascunho.CaminhoImagem) ? null : CalculadoraView.MemoriaRascunho.CaminhoImagem
            };

            ResumoProdutosView.ProdutosBancoDados.Add(novoProduto);

            DependencyObject parent = VisualTreeHelper.GetParent(this);
            while (parent != null)
            {
                if (parent is ContentControl cc) { cc.Content = new ResumoProdutosView(); return; }
                if (parent is Panel panel)
                {
                    int index = panel.Children.IndexOf(this);
                    if (index >= 0) { panel.Children.RemoveAt(index); panel.Children.Insert(index, new ResumoProdutosView()); return; }
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
        }
    }
}