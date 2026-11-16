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
        private bool _modoVisualizacao;

        public ResumoCalculadoraView(ResultadoCalculoModel dados, bool modoVisualizacao = false)
        {
            InitializeComponent();
            DataContext = dados;
            _modoVisualizacao = modoVisualizacao;
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
                            txtTitulo.Text = _modoVisualizacao ? "Detalhes do Produto" : "Revisar e salvar";

                        if (dashboard.FindName("TextoSubtituloPagina") is TextBlock txtSub)
                            txtSub.Text = _modoVisualizacao ? "Visualize as informações e os custos completos desta peça." : "Revise as informações antes de salvar o produto.";
                    }
                    catch { }
                }

                if (_modoVisualizacao)
                {
                    EsconderBotaoSalvar(this);
                }
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void EsconderBotaoSalvar(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Button btn)
                {
                    if (btn.Name == "BtnSalvar" || AcharTexto(btn, "Salvar"))
                    {
                        btn.Visibility = Visibility.Collapsed;
                        continue;
                    }
                }
                EsconderBotaoSalvar(child);
            }
        }

        private bool AcharTexto(DependencyObject parent, string texto)
        {
            if (parent is TextBlock tb && tb.Text.Contains(texto)) return true;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                if (AcharTexto(VisualTreeHelper.GetChild(parent, i), texto)) return true;
            }
            return false;
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

        private void SincronizarMenuLateral(string nomeBotao1, string nomeBotao2)
        {
            if (Window.GetWindow(this) is DashboardWindow dashboard)
            {
                // Tenta marcar o botão correspondente no menu lateral esquerdo
                RadioButton? rb = (dashboard.FindName(nomeBotao1) as RadioButton) ?? (dashboard.FindName(nomeBotao2) as RadioButton);
                if (rb != null)
                {
                    rb.IsChecked = true;
                }
            }
        }

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
            if (_modoVisualizacao)
            {
                if (Window.GetWindow(this) is DashboardWindow dashboard)
                {
                    try
                    {
                        if (dashboard.FindName("TextoTituloPagina") is TextBlock txtTitulo) txtTitulo.Text = "Produtos";
                        if (dashboard.FindName("TextoSubtituloPagina") is TextBlock txtSub) txtSub.Text = "Os produtos cadastrados aparecem aqui após serem calculados e salvos na calculadora.";
                    }
                    catch { }
                }

                SincronizarMenuLateral("MenuProdutos", "BtnProdutos");
                NavegarPara(new ResumoProdutosView());
            }
            else
            {
                CalculadoraView.MemoriaRascunho.CalculoPendente = false;
                if (Window.GetWindow(this) is DashboardWindow dashboard)
                {
                    try
                    {
                        if (dashboard.FindName("TextoTituloPagina") is TextBlock txtTitulo) txtTitulo.Text = "Calculadora";
                        if (dashboard.FindName("TextoSubtituloPagina") is TextBlock txtSub) txtSub.Text = "Calcule os custos de produção e defina o preço de venda dos seus produtos 3D.";
                    }
                    catch { }
                }

                SincronizarMenuLateral("MenuCalculadora", "BtnCalculadora");
                NavegarPara(new CalculadoraView());
            }
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            ModalSucesso.Visibility = Visibility.Visible;
        }

        private void BtnFecharModal_Click(object sender, RoutedEventArgs e)
        {
            // Fecha o modal visualmente primeiro
            ModalSucesso.Visibility = Visibility.Collapsed;

            var resultadoCalculado = this.DataContext as ResultadoCalculoModel;

            // VERIFICA SE ESTÁ A EDITAR UM PRODUTO EXISTENTE
            // VERIFICA SE ESTÁ A EDITAR UM PRODUTO EXISTENTE
            // VERIFICA SE ESTÁ A EDITAR UM PRODUTO EXISTENTE
            if (CalculadoraView.MemoriaRascunho.ProdutoEmEdicaoRef is ProdutoItemModel produtoExistente)
            {
                // Atualiza o produto existente em vez de criar um novo (evita duplicar)
                produtoExistente.Nome = string.IsNullOrWhiteSpace(CalculadoraView.MemoriaRascunho.Nome) ? "Peça 3D" : CalculadoraView.MemoriaRascunho.Nome;
                produtoExistente.FilamentoNome = CalculadoraView.MemoriaRascunho.NomeFilamentoVisual;
                produtoExistente.FilamentoDetalhe = CalculadoraView.MemoriaRascunho.NomeCorVisual;
                produtoExistente.CorFilamentoBrush = CalculadoraView.MemoriaRascunho.CorFilamentoVisualBrush;
                produtoExistente.ImpressoraNome = CalculadoraView.MemoriaRascunho.NomeImpressoraVisual;
                produtoExistente.ImpressoraModelo = CalculadoraView.MemoriaRascunho.ModeloImpressoraVisual;
                produtoExistente.CustoTotalFormatado = resultadoCalculado?.CustoTotalProducao ?? "R$ 0,00";
                produtoExistente.PrecoSugeridoFormatado = resultadoCalculado?.PrecoVendaSugerido ?? "R$ 0,00";
                produtoExistente.CaminhoImagem = string.IsNullOrWhiteSpace(CalculadoraView.MemoriaRascunho.CaminhoImagem) ? string.Empty : CalculadoraView.MemoriaRascunho.CaminhoImagem;
                produtoExistente.DadosCompletos = resultadoCalculado;

                // Grava os dados brutos para edição futura e relatórios PDF
                produtoExistente.PesoGramaSalvo = CalculadoraView.MemoriaRascunho.Peso;
                produtoExistente.HorasImpSalvo = CalculadoraView.MemoriaRascunho.Horas;
                produtoExistente.MinutosImpSalvo = CalculadoraView.MemoriaRascunho.Minutos;
                produtoExistente.QuantidadeSalva = CalculadoraView.MemoriaRascunho.Quantidade;
                produtoExistente.FreteSalvo = CalculadoraView.MemoriaRascunho.Frete;
                produtoExistente.MargemSalva = CalculadoraView.MemoriaRascunho.Margem;
                produtoExistente.ValorHoraMaoSalvo = CalculadoraView.MemoriaRascunho.ValorHora;
                produtoExistente.HorasMaoSalvo = CalculadoraView.MemoriaRascunho.HorasMao;
                produtoExistente.MinutosMaoSalvo = CalculadoraView.MemoriaRascunho.MinutosMao;
                produtoExistente.FilamentoObjetoSalvo = CalculadoraView.MemoriaRascunho.FilamentoSelecionado;
                produtoExistente.ImpressoraObjetoSalvo = CalculadoraView.MemoriaRascunho.ImpressoraSelecionada;
                produtoExistente.CustosMarcadosSalvos = new System.Collections.Generic.List<string>(CalculadoraView.MemoriaRascunho.CustosSelecionadosDescricoes);

                // Limpa a referência de edição após salvar
                CalculadoraView.MemoriaRascunho.ProdutoEmEdicaoRef = null;
            }
            else
            {
                // Se não estiver a editar, cria um novo registo normalmente com SKU sequencial
                int proximoNumero = ResumoProdutosView.ProdutosCadastrados.Count + 1;
                string skuSequencial = $"#PROD-{proximoNumero:D3}";

                var novoProduto = new ProdutoItemModel
                {
                    Nome = string.IsNullOrWhiteSpace(CalculadoraView.MemoriaRascunho.Nome) ? "Peça 3D" : CalculadoraView.MemoriaRascunho.Nome,
                    CodigoSku = skuSequencial,
                    FilamentoNome = CalculadoraView.MemoriaRascunho.NomeFilamentoVisual,
                    FilamentoDetalhe = CalculadoraView.MemoriaRascunho.NomeCorVisual,
                    CorFilamentoBrush = CalculadoraView.MemoriaRascunho.CorFilamentoVisualBrush,
                    ImpressoraNome = CalculadoraView.MemoriaRascunho.NomeImpressoraVisual,
                    ImpressoraModelo = CalculadoraView.MemoriaRascunho.ModeloImpressoraVisual,
                    CustoTotalFormatado = resultadoCalculado?.CustoTotalProducao ?? "R$ 0,00",
                    PrecoSugeridoFormatado = resultadoCalculado?.PrecoVendaSugerido ?? "R$ 0,00",
                    CaminhoImagem = string.IsNullOrWhiteSpace(CalculadoraView.MemoriaRascunho.CaminhoImagem) ? string.Empty : CalculadoraView.MemoriaRascunho.CaminhoImagem,
                    DadosCompletos = resultadoCalculado,

                    // Grava os dados brutos para edição futura e relatórios PDF
                    PesoGramaSalvo = CalculadoraView.MemoriaRascunho.Peso,
                    HorasImpSalvo = CalculadoraView.MemoriaRascunho.Horas,
                    MinutosImpSalvo = CalculadoraView.MemoriaRascunho.Minutos,
                    QuantidadeSalva = CalculadoraView.MemoriaRascunho.Quantidade,
                    FreteSalvo = CalculadoraView.MemoriaRascunho.Frete,
                    MargemSalva = CalculadoraView.MemoriaRascunho.Margem,
                    ValorHoraMaoSalvo = CalculadoraView.MemoriaRascunho.ValorHora,
                    HorasMaoSalvo = CalculadoraView.MemoriaRascunho.HorasMao,
                    MinutosMaoSalvo = CalculadoraView.MemoriaRascunho.MinutosMao,
                    FilamentoObjetoSalvo = CalculadoraView.MemoriaRascunho.FilamentoSelecionado,
                    ImpressoraObjetoSalvo = CalculadoraView.MemoriaRascunho.ImpressoraSelecionada,
                    CustosMarcadosSalvos = new System.Collections.Generic.List<string>(CalculadoraView.MemoriaRascunho.CustosSelecionadosDescricoes)
                };

                ResumoProdutosView.ProdutosCadastrados.Add(novoProduto);
            }     // Atualiza o produto existente em vez de criar um novo (evita duplicar)


            CalculadoraView.MemoriaRascunho.CalculoPendente = false;

            if (Window.GetWindow(this) is DashboardWindow dashboard)
            {
                try
                {
                    if (dashboard.FindName("TextoTituloPagina") is TextBlock txtTitulo) txtTitulo.Text = "Produtos";
                    if (dashboard.FindName("TextoSubtituloPagina") is TextBlock txtSub) txtSub.Text = "Os produtos cadastrados aparecem aqui após serem calculados e salvos na calculadora.";
                }
                catch { }
            }

            // --- CORREÇÃO APLICADA AQUI ---
            // Limpa todos os dados da memória rascunho (campos digitados, imagem, cálculos)
            CalculadoraView.MemoriaRascunho.LimparTudo();

            // Sincroniza o botão dourado do menu lateral para a aba "Produtos" de forma imediata
            SincronizarMenuLateral("MenuProdutos", "BtnProdutos");

            // Navega para a tela de resumo de produtos
            NavegarPara(new ResumoProdutosView());
        }
    }
}