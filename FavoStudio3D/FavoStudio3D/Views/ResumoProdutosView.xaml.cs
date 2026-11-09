using FavoStudio3D.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
// --- IMPORTAÇÕES PARA O PDF ---
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

// --- SOLUÇÃO PARA O ERRO DE AMBIGUIDADE (CORES E FONTES) ---
using QColors = QuestPDF.Helpers.Colors;
using QFonts = QuestPDF.Helpers.Fonts;

namespace FavoStudio3D.Views
{
    public class ProdutoItemModel
    {
        public bool Selecionado { get; set; } = false;
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

        public string PesoGramaSalvo { get; set; } = "";
        public string HorasImpSalvo { get; set; } = "";
        public string MinutosImpSalvo { get; set; } = "";
        public string QuantidadeSalva { get; set; } = "1";
        public string FreteSalvo { get; set; } = "";
        public string MargemSalva { get; set; } = "";
        public string ValorHoraMaoSalvo { get; set; } = "";
        public string HorasMaoSalvo { get; set; } = "";
        public string MinutosMaoSalvo { get; set; } = "";
        public object? FilamentoObjetoSalvo { get; set; }
        public object? ImpressoraObjetoSalvo { get; set; }
        public System.Collections.Generic.List<string> CustosMarcadosSalvos { get; set; } = new System.Collections.Generic.List<string>();

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
                        if (dashboard.FindName("TextoTituloPagina") is TextBlock txtTitulo)
                            txtTitulo.Text = "Produtos";

                        if (dashboard.FindName("TextoSubtituloPagina") is TextBlock txtSub)
                            txtSub.Text = "Os produtos cadastrados aparecem aqui após serem calculados e salvos na calculadora.";

                        dashboard.Title = "Favo Studio 3D - Produtos";

                        if (dashboard.FindName("MenuProdutosBorder") is Border borderProd &&
                            dashboard.FindName("MenuProdutosTexto") is TextBlock textoProd)
                        {
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

            if (TxtContadorProdutos != null)
            {
                TxtContadorProdutos.Text = totalItens == 1 ? "1 produto cadastrado" : $"{totalItens} produtos cadastrados";
            }

            AtualizarVisibilidadeBotoesPDF();

            var borderAnterior = new Border
            {
                Width = 28,
                Height = 28,
                CornerRadius = new CornerRadius(6),
                Background = Brushes.White,
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#EADCC9"),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Cursor = Cursors.Hand
            };
            borderAnterior.Child = new TextBlock { Text = "<", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = (Brush)new BrushConverter().ConvertFrom("#795548"), HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = System.Windows.VerticalAlignment.Center };
            PanelPaginacao.Children.Add(borderAnterior);

            var borderPagina = new Border
            {
                Width = 28,
                Height = 28,
                CornerRadius = new CornerRadius(6),
                Background = (Brush)new BrushConverter().ConvertFrom("#EBB45D"),
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };
            borderPagina.Child = new TextBlock { Text = "1", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = (Brush)new BrushConverter().ConvertFrom("#3E2723"), HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = System.Windows.VerticalAlignment.Center };
            PanelPaginacao.Children.Add(borderPagina);

            var borderProximo = new Border
            {
                Width = 28,
                Height = 28,
                CornerRadius = new CornerRadius(6),
                Background = Brushes.White,
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#EADCC9"),
                BorderThickness = new Thickness(1),
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Cursor = Cursors.Hand
            };
            borderProximo.Child = new TextBlock { Text = ">", FontSize = 12, FontWeight = FontWeights.Bold, Foreground = (Brush)new BrushConverter().ConvertFrom("#795548"), HorizontalAlignment = System.Windows.HorizontalAlignment.Center, VerticalAlignment = System.Windows.VerticalAlignment.Center };
            PanelPaginacao.Children.Add(borderProximo);
        }

        private void ChkProduto_Changed(object sender, RoutedEventArgs e)
        {
            AtualizarVisibilidadeBotoesPDF();
        }

        private void AtualizarVisibilidadeBotoesPDF()
        {
            if (PainelBotoesPDF != null)
            {
                bool algumSelecionado = ProdutosCadastrados.Any(p => p.Selecionado);
                PainelBotoesPDF.Visibility = algumSelecionado ? Visibility.Visible : Visibility.Collapsed;
            }
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

        // =================================================================
        // GERAÇÃO DOS PDFs - DESIGN PREMIUM
        // =================================================================
        private void BtnGerarCatalogo_Click(object sender, RoutedEventArgs e)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.UseSystemFonts = true;

            var produtosSelecionados = ProdutosCadastrados.Where(p => p.Selecionado).ToList();
            if (produtosSelecionados.Count == 0) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Ficheiro PDF (*.pdf)|*.pdf",
                FileName = "Catalogo_FavoStudio3D.pdf",
                Title = "Guardar Catálogo"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(40);

                            // =========================================================
                            // NOVA LÓGICA INFALÍVEL PARA A IMAGEM DE FUNDO
                            // Lê a imagem embutida (Recurso) diretamente da memória
                            // =========================================================
                            try
                            {
                                // O URI pack://application:,,, aponta para a raiz do assembly onde estão os Recursos
                                var uri = new Uri("pack://application:,,,/fundo_catalogo.png");
                                var resourceStream = System.Windows.Application.GetResourceStream(uri);

                                if (resourceStream != null)
                                {
                                    using (var stream = resourceStream.Stream)
                                    using (var ms = new MemoryStream())
                                    {
                                        stream.CopyTo(ms);
                                        page.Background().Image(ms.ToArray());
                                    }
                                }
                                else
                                {
                                    page.PageColor("#FAEDDC"); // Cor de segurança se o stream for nulo
                                }
                            }
                            catch (Exception ex)
                            {
                                // Log do erro na consola de debug, aplica cor de fundo bege em caso de falha
                                Debug.WriteLine($"Erro ao carregar a imagem de fundo: {ex.Message}");
                                page.PageColor("#FAEDDC");
                            }

                            page.DefaultTextStyle(x => x.FontSize(12).FontFamily(QFonts.Arial));

                            page.Content().Column(col =>
                            {
                                col.Item().Height(140);

                                col.Item().PaddingBottom(30).AlignRight().Row(dateRow =>
                                {
                                    dateRow.AutoItem()
                                           .Background("#EADCC9")
                                           .PaddingHorizontal(15)
                                           .PaddingVertical(6)
                                           .Text(DateTime.Now.ToString("dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("pt-BR"))).FontSize(10).FontColor("#381300");
                                });

                                col.Item().Column(listaCol =>
                                {
                                    listaCol.Spacing(25);

                                    foreach (var prod in produtosSelecionados)
                                    {
                                        listaCol.Item()
                                            .Background(QColors.White)
                                            .Padding(15)
                                            .Row(cardRow =>
                                            {
                                                cardRow.ConstantItem(100).Height(100).Background("#F9F5F0").AlignCenter().AlignMiddle().Element(imgContainer =>
                                                {
                                                    if (!string.IsNullOrWhiteSpace(prod.CaminhoImagem) && File.Exists(prod.CaminhoImagem))
                                                        imgContainer.Image(prod.CaminhoImagem);
                                                    else
                                                        imgContainer.Text("Sem foto").FontSize(10).FontColor("#A99F96");
                                                });

                                                cardRow.RelativeItem().PaddingLeft(20).AlignMiddle().Column(texto =>
                                                {
                                                    texto.Item().Text(prod.Nome).FontSize(22).Bold().FontColor("#1A1A1A");
                                                    texto.Item().PaddingTop(4).Text($"Material: {prod.FilamentoNome}").FontSize(12).FontColor("#4A4A4A");
                                                    texto.Item().Text($"Cor: {prod.FilamentoDetalhe}").FontSize(12).FontColor("#4A4A4A");
                                                    texto.Item().PaddingTop(10).Text($"Ref: {prod.CodigoSku}").FontSize(10).FontColor("#A99F96");
                                                });

                                                cardRow.ConstantItem(140).AlignMiddle().AlignRight().Element(priceBadge =>
                                                {
                                                    priceBadge.Background("#FFD54F")
                                                              .PaddingHorizontal(15)
                                                              .PaddingVertical(12)
                                                              .Text(prod.PrecoSugeridoFormatado).FontSize(20).Bold().FontColor("#381300");
                                                });
                                            });
                                    }
                                });
                            });
                        });
                    }).GeneratePdf(saveFileDialog.FileName);

                    Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao gerar o PDF: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnGerarRelatorio_Click(object sender, RoutedEventArgs e)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.UseSystemFonts = true;

            var produtosSelecionados = ProdutosCadastrados.Where(p => p.Selecionado).ToList();
            if (produtosSelecionados.Count == 0) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "Ficheiro PDF (*.pdf)|*.pdf", FileName = "Relatorio_FavoStudio3D.pdf", Title = "Guardar Relatório" };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(2, Unit.Centimetre);
                            page.PageColor(QColors.White);
                            page.DefaultTextStyle(x => x.FontSize(11).FontFamily(QFonts.Arial));

                            page.Header().BorderBottom(2).BorderColor("#F0D1A7").PaddingBottom(10).Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("RELATÓRIO DE CUSTOS E MARGENS").FontSize(16).FontColor("#381300").Black();
                                    col.Item().Text("Favo Studio 3D - Gestão Interna").FontSize(10).FontColor("#A99F96");
                                });
                                row.ConstantItem(100).AlignRight().AlignBottom().Text(DateTime.Now.ToString("dd/MM/yyyy")).FontSize(10).FontColor("#795548");
                            });

                            page.Content().PaddingVertical(20).Column(col =>
                            {
                                col.Item().Table(table =>
                                {
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3);
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(1.5f);
                                        columns.RelativeColumn(2);
                                        columns.RelativeColumn(1.5f);
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Background("#F0D1A7").Padding(8).Text("Produto").FontColor("#381300").SemiBold();
                                        header.Cell().Background("#F0D1A7").Padding(8).AlignCenter().Text("Tempo").FontColor("#381300").SemiBold();
                                        header.Cell().Background("#F0D1A7").Padding(8).AlignCenter().Text("Peso").FontColor("#381300").SemiBold();
                                        header.Cell().Background("#F0D1A7").Padding(8).AlignRight().Text("Custo").FontColor("#381300").SemiBold();
                                        header.Cell().Background("#F0D1A7").Padding(8).AlignCenter().Text("Margem").FontColor("#381300").SemiBold();
                                    });

                                    bool isZebra = false;
                                    foreach (var prod in produtosSelecionados)
                                    {
                                        var bgColor = isZebra ? "#FDF8F0" : "#FFFFFF";
                                        var tempo = (prod.HorasImpSalvo == "0" && prod.MinutosImpSalvo == "0") ? "-" : $"{prod.HorasImpSalvo}h {prod.MinutosImpSalvo}m";
                                        var peso = prod.PesoGramaSalvo == "0" ? "-" : $"{prod.PesoGramaSalvo}g";

                                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor("#EADCC9").Padding(8).Text(prod.Nome).SemiBold().FontColor("#3E2723");
                                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor("#EADCC9").Padding(8).AlignCenter().Text(tempo).FontColor("#795548");
                                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor("#EADCC9").Padding(8).AlignCenter().Text(peso).FontColor("#795548");
                                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor("#EADCC9").Padding(8).AlignRight().Text(prod.CustoTotalFormatado).FontColor("#D32F2F");
                                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor("#EADCC9").Padding(8).AlignCenter().Text($"{prod.MargemSalva}%").FontColor("#4CAF50").SemiBold();

                                        isZebra = !isZebra;
                                    }
                                });
                            });
                        });
                    }).GeneratePdf(saveFileDialog.FileName);

                    Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                }
                catch (Exception ex) { MessageBox.Show($"Erro ao gerar o Relatório: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
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
                CalculadoraView.MemoriaRascunho.ProdutoEmEdicaoRef = produto;
                CalculadoraView.MemoriaRascunho.CalculoPendente = false;

                CalculadoraView.MemoriaRascunho.Nome = produto.Nome ?? "";
                CalculadoraView.MemoriaRascunho.CaminhoImagem = produto.CaminhoImagem ?? "";
                CalculadoraView.MemoriaRascunho.Peso = produto.PesoGramaSalvo ?? "";
                CalculadoraView.MemoriaRascunho.Horas = produto.HorasImpSalvo ?? "";
                CalculadoraView.MemoriaRascunho.Minutos = produto.MinutosImpSalvo ?? "";
                CalculadoraView.MemoriaRascunho.Quantidade = string.IsNullOrWhiteSpace(produto.QuantidadeSalva) ? "1" : produto.QuantidadeSalva;
                CalculadoraView.MemoriaRascunho.Frete = produto.FreteSalvo ?? "";
                CalculadoraView.MemoriaRascunho.Margem = produto.MargemSalva ?? "";
                CalculadoraView.MemoriaRascunho.ValorHora = produto.ValorHoraMaoSalvo ?? "";
                CalculadoraView.MemoriaRascunho.HorasMao = produto.HorasMaoSalvo ?? "";
                CalculadoraView.MemoriaRascunho.MinutosMao = produto.MinutosMaoSalvo ?? "";

                CalculadoraView.MemoriaRascunho.FilamentoSelecionado = produto.FilamentoObjetoSalvo;
                CalculadoraView.MemoriaRascunho.ImpressoraSelecionada = produto.ImpressoraObjetoSalvo;
                CalculadoraView.MemoriaRascunho.CustosSelecionadosDescricoes = produto.CustosMarcadosSalvos != null
                    ? new System.Collections.Generic.List<string>(produto.CustosMarcadosSalvos)
                    : new System.Collections.Generic.List<string>();

                if (produto.DadosCompletos != null)
                {
                    CalculadoraView.MemoriaRascunho.Categoria = produto.DadosCompletos.ProdutoCategoria ?? "";
                    CalculadoraView.MemoriaRascunho.Descricao = produto.DadosCompletos.ProdutoDescricao ?? "";
                }

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