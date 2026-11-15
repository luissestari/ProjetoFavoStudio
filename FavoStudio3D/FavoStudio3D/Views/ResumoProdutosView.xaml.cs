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
        // GERAÇÃO DOS PDFs - DESIGN PREMIUM (VERTICAL)
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
                            page.Margin(35);

                            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                            // =========================================================
                            // FUNDO DO CATÁLOGO
                            // =========================================================
                            try
                            {
                                string? nomeExatoDoRecurso = assembly.GetManifestResourceNames()
                                    .FirstOrDefault(n => n.EndsWith("fundo_catalogo.png", StringComparison.OrdinalIgnoreCase));

                                if (!string.IsNullOrEmpty(nomeExatoDoRecurso))
                                {
                                    using (var stream = assembly.GetManifestResourceStream(nomeExatoDoRecurso))
                                    {
                                        if (stream != null)
                                        {
                                            using (var ms = new MemoryStream())
                                            {
                                                stream.CopyTo(ms);
                                                page.Background().Image(ms.ToArray());
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    page.PageColor("#FAEDDC");
                                }
                            }
                            catch (Exception ex)
                            {
                                page.PageColor("#FAEDDC");
                                Debug.WriteLine($"Erro interno ao ler a imagem: {ex.Message}");
                            }

                            page.DefaultTextStyle(x => x.FontSize(11).FontFamily(QFonts.Arial));

                            page.Content().Column(col =>
                            {
                                // =========================================================
                                // CABEÇALHO DO CATÁLOGO
                                // =========================================================
                                col.Item().PaddingBottom(15).Row(headerRow =>
                                {
                                    // LOGO FAVO STUDIO
                                    headerRow.ConstantItem(55).Height(55).Element(logoContainer =>
                                    {
                                        try
                                        {
                                            string? nomeLogo = assembly.GetManifestResourceNames()
                                                .FirstOrDefault(n => n.EndsWith("LogoCat.png", StringComparison.OrdinalIgnoreCase));

                                            if (!string.IsNullOrEmpty(nomeLogo))
                                            {
                                                using (var stream = assembly.GetManifestResourceStream(nomeLogo))
                                                {
                                                    if (stream != null)
                                                    {
                                                        using (var ms = new MemoryStream())
                                                        {
                                                            stream.CopyTo(ms);
                                                            logoContainer.Image(ms.ToArray());
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception) { }
                                    });

                                    // LINHA VERTICAL E TÍTULOS
                                    headerRow.RelativeItem().PaddingLeft(10).AlignMiddle().Row(titleRow =>
                                    {
                                        titleRow.AutoItem().Width(1).Height(36).Background("#381300");

                                        titleRow.RelativeItem().PaddingLeft(10).Column(titleCol =>
                                        {
                                            titleCol.Item().Row(r =>
                                            {
                                                r.AutoItem().Text("Favo Studio ").FontSize(22).Bold().FontColor("#291506");
                                                r.AutoItem().Text("  3D").FontSize(22).Bold().FontColor("#E09E19");
                                            });
                                            titleCol.Item().PaddingTop(1).Text("Inovação em cada detalhe").FontSize(9).FontColor("#6B5A4B");
                                        });
                                    });

                                    // DATA
                                    headerRow.AutoItem().AlignRight().AlignMiddle().Element(dateBadge =>
                                    {
                                        dateBadge.CornerRadius(8)
                                                 .Background("#FDE5BC")
                                                 .Border(1).BorderColor("#E3C17D")
                                                 .PaddingHorizontal(12)
                                                 .PaddingVertical(5)
                                                 .Row(r =>
                                                 {
                                                     r.AutoItem().Width(12).PaddingRight(5).AlignMiddle().Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#381300\" d=\"M19 4h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V10h14v10zm0-12H5V6h14v2zm-7 5h5v5h-5z\"/></svg>");
                                                     r.AutoItem().AlignMiddle().Text(DateTime.Now.ToString("dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("pt-BR"))).FontSize(9).FontColor("#381300");
                                                 });
                                    });
                                });

                                col.Item().PaddingBottom(15).Text("Catálogo de Produtos Exclusivos").FontSize(22).Bold().FontColor("#291506");

                                // =========================================================
                                // LISTA DE PRODUTOS (MAIS COMPACTA)
                                // =========================================================
                                col.Item().Column(listaCol =>
                                {
                                    listaCol.Spacing(12);

                                    foreach (var prod in produtosSelecionados)
                                    {
                                        string pesoExibicao = prod.PesoGramaSalvo;
                                        if (string.IsNullOrWhiteSpace(pesoExibicao) || pesoExibicao == "0")
                                        {
                                            var paramPeso = prod.DadosCompletos?.Parametros?.FirstOrDefault(p => p.Rotulo != null && p.Rotulo.ToLower().Contains("peso"));
                                            pesoExibicao = paramPeso?.Valor?.Replace("g", "").Trim() ?? "0";
                                        }

                                        listaCol.Item()
                                            .CornerRadius(10)
                                            .Background("#D9FFFFFF")
                                            .Border(1).BorderColor("#E3C17D")
                                            .PaddingVertical(10)
                                            .PaddingHorizontal(12)
                                            .Row(cardRow =>
                                            {
                                                cardRow.ConstantItem(75).Height(75)
                                                    .CornerRadius(8)
                                                    .Background("#FFFFFF")
                                                    .Border(1).BorderColor("#EADCC9")
                                                    .Padding(5)
                                                    .AlignCenter().AlignMiddle().Element(imgContainer =>
                                                    {
                                                        if (!string.IsNullOrWhiteSpace(prod.CaminhoImagem) && File.Exists(prod.CaminhoImagem))
                                                            imgContainer.Image(prod.CaminhoImagem);
                                                        else
                                                            imgContainer.Text("Sem foto").FontSize(8).FontColor("#A99F96");
                                                    });

                                                cardRow.RelativeItem().PaddingLeft(15).AlignMiddle().Column(texto =>
                                                {
                                                    texto.Item().Text(prod.Nome).FontSize(17).Bold().FontColor("#291506");

                                                    if (prod.DadosCompletos != null && !string.IsNullOrWhiteSpace(prod.DadosCompletos.ProdutoDescricao))
                                                    {
                                                        texto.Item().PaddingTop(1).Text(prod.DadosCompletos.ProdutoDescricao).FontSize(9).FontColor("#6B5A4B");
                                                    }

                                                    texto.Item().PaddingTop(4).Row(r =>
                                                    {
                                                        r.AutoItem().Width(10).AlignMiddle().Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#5A3B1C\" d=\"M12 2L2 7l10 5 10-5-10-5zm0 14.5l-10-5v2.5l10 5 10-5V11.5l-10 5z\"/></svg>");
                                                        r.AutoItem().PaddingLeft(5).AlignMiddle().Text($"Material: {prod.FilamentoNome}").FontSize(9.5f).FontColor("#381300");
                                                    });

                                                    texto.Item().PaddingTop(2).Row(r =>
                                                    {
                                                        r.AutoItem().Width(10).AlignMiddle().Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#5A3B1C\" d=\"M12 21c-4.418 0-8-3.582-8-8 0-4.667 8-11 8-11s8 6.333 8 11c0 4.418-3.582 8-8 8z\"/></svg>");
                                                        r.AutoItem().PaddingLeft(5).AlignMiddle().Text($"Cor: {prod.FilamentoDetalhe}").FontSize(9.5f).FontColor("#381300");
                                                    });

                                                    texto.Item().PaddingTop(2).Row(r =>
                                                    {
                                                        r.AutoItem().Width(10).AlignMiddle().Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#5A3B1C\" d=\"M19 13v6a2 2 0 01-2 2H7a2 2 0 01-2-2v-6c-1.1 0-2-.9-2-2 0-1 .7-1.8 1.6-2L7 3h10l2.4 6c.9.2 1.6 1 1.6 2 0 1.1-.9 2-2 2zm-7 4h4v-2h-4v2z\"/></svg>");
                                                        r.AutoItem().PaddingLeft(5).AlignMiddle().Text($"Peso: {pesoExibicao}g").FontSize(9.5f).FontColor("#381300");
                                                    });
                                                });

                                                cardRow.ConstantItem(120).AlignMiddle().AlignRight().Element(priceBadge =>
                                                {
                                                    priceBadge.Width(120).Height(36).Layers(layers =>
                                                    {
                                                        layers.Layer().Svg("<svg viewBox=\"0 0 120 36\"><path fill=\"#FFC857\" d=\"M14 0 L120 0 L120 36 L14 36 L0 18 Z\"/></svg>");

                                                        layers.PrimaryLayer().PaddingLeft(14).AlignCenter().AlignMiddle()
                                                              .Text(prod.PrecoSugeridoFormatado).FontSize(15).Bold().FontColor("#381300");
                                                    });
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

        // Redireciona qualquer referência antiga ao nome Relatório direto para o Orçamento
        private void BtnGerarRelatorio_Click(object sender, RoutedEventArgs e)
        {
            BtnGerarOrcamento_Click(sender, e);
        }

        private void BtnGerarOrcamento_Click(object sender, RoutedEventArgs e)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            QuestPDF.Settings.UseSystemFonts = true;

            var produtosSelecionados = ProdutosCadastrados.Where(p => p.Selecionado).ToList();
            if (produtosSelecionados.Count == 0) return;

            var dadosOrcamento = SolicitarDadosOrcamento();
            if (dadosOrcamento == null) return;

            string nomeCliente = string.IsNullOrWhiteSpace(dadosOrcamento.Value.Cliente) ? "Cliente" : dadosOrcamento.Value.Cliente.Trim();
            string prazoProducao = string.IsNullOrWhiteSpace(dadosOrcamento.Value.Prazo) ? "até 2 dias úteis" : dadosOrcamento.Value.Prazo.Trim();

            if (int.TryParse(prazoProducao, out int dias))
                prazoProducao = dias == 1 ? "até 1 dia útil" : $"até {dias} dias úteis";
            else
            {
                prazoProducao = System.Text.RegularExpressions.Regex.Replace(
                    prazoProducao, @"\b1\s+dias\s+[uú]teis\b", "1 dia útil", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                prazoProducao = System.Text.RegularExpressions.Regex.Replace(
                    prazoProducao, @"\b1\s+dias\b", "1 dia", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Ficheiro PDF (*.pdf)|*.pdf",
                FileName = $"Orcamento_{nomeCliente.Replace(" ", "_")}.pdf",
                Title = "Guardar Orçamento"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var culturaBR = new System.Globalization.CultureInfo("pt-BR");
                    var assembly = System.Reflection.Assembly.GetExecutingAssembly();

                    // Carrega a imagem de fundo uma única vez
                    byte[]? fundoBytes = null;
                    try
                    {
                        string? nomeFundo = assembly.GetManifestResourceNames()
                            .FirstOrDefault(n => n.EndsWith("fundo_orcamento.png", StringComparison.OrdinalIgnoreCase));

                        if (!string.IsNullOrEmpty(nomeFundo))
                        {
                            using (var stream = assembly.GetManifestResourceStream(nomeFundo))
                            {
                                if (stream != null)
                                {
                                    using (var ms = new MemoryStream())
                                    {
                                        stream.CopyTo(ms);
                                        fundoBytes = ms.ToArray();
                                    }
                                }
                            }
                        }
                    }
                    catch { }

                    // Carrega a logo uma única vez
                    byte[]? logoBytes = null;
                    try
                    {
                        string? nomeLogo = assembly.GetManifestResourceNames()
                            .FirstOrDefault(n => n.EndsWith("LogoCat.png", StringComparison.OrdinalIgnoreCase));

                        if (!string.IsNullOrEmpty(nomeLogo))
                        {
                            using (var stream = assembly.GetManifestResourceStream(nomeLogo))
                            {
                                if (stream != null)
                                {
                                    using (var ms = new MemoryStream())
                                    {
                                        stream.CopyTo(ms);
                                        logoBytes = ms.ToArray();
                                    }
                                }
                            }
                        }
                    }
                    catch { }

                    Document.Create(container =>
                    {
                        // =============================================================
                        // GERA UMA FOLHA SEPARADA PARA CADA PRODUTO SELECIONADO
                        // =============================================================
                        foreach (var prod in produtosSelecionados)
                        {
                            // Extrai os custos específicos DESTE produto
                            var listaCustos = ExtrairCustosParaOrcamento(
                                new System.Collections.Generic.List<ProdutoItemModel> { prod }, culturaBR);

                            string valorFinalFormatado = string.IsNullOrWhiteSpace(prod.PrecoSugeridoFormatado)
                                ? "R$ 0,00"
                                : prod.PrecoSugeridoFormatado;

                            string qtdTexto = string.IsNullOrWhiteSpace(prod.QuantidadeSalva) ? "1" : prod.QuantidadeSalva.Trim();
                            string sufixoUnidade = qtdTexto == "1" ? "unidade" : "unidades";

                            container.Page(page =>
                            {
                                page.Size(PageSizes.A4);
                                page.MarginHorizontal(35);
                                page.MarginTop(35);
                                page.MarginBottom(25);

                                // 1. IMAGEM DE FUNDO
                                if (fundoBytes != null)
                                    page.Background().Image(fundoBytes);
                                else
                                    page.PageColor("#FAEDDC");

                                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(QFonts.Arial));

                                page.Content().Column(col =>
                                {
                                    // =========================================================
                                    // 2. CABEÇALHO
                                    // =========================================================
                                    col.Item().PaddingBottom(14).Row(topRow =>
                                    {
                                        topRow.RelativeItem().Column(leftHeader =>
                                        {
                                            leftHeader.Item().Row(brandRow =>
                                            {
                                                if (logoBytes != null)
                                                {
                                                    brandRow.ConstantItem(46).Height(46).Image(logoBytes);
                                                }

                                                brandRow.RelativeItem().PaddingLeft(10).AlignMiddle().Column(titleCol =>
                                                {
                                                    titleCol.Item().Row(r =>
                                                    {
                                                        r.AutoItem().Text("Favo Studio ").FontSize(19).Bold().FontColor("#291506");
                                                        r.AutoItem().Text(" 3D").FontSize(19).Bold().FontColor("#E09E19");
                                                    });
                                                    titleCol.Item().PaddingTop(1).Text("Inovação em cada detalhe").FontSize(8.5f).FontColor("#5A3B1C");
                                                });
                                            });

                                            leftHeader.Item().PaddingTop(14).Text("Orçamento").FontSize(30).Bold().FontColor("#291506");

                                            leftHeader.Item().PaddingTop(4).Row(thanksRow =>
                                            {
                                                thanksRow.AutoItem().AlignMiddle().Text("Aqui está o seu orçamento, feito com cuidado e precisão. ").FontSize(10).FontColor("#381300");
                                                thanksRow.AutoItem().PaddingLeft(5).Width(12).Height(12).AlignMiddle()
                                                    .Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"none\" stroke=\"#D99B26\" stroke-width=\"2\" d=\"M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z\"/></svg>");
                                            });
                                        });

                                        topRow.ConstantItem(175).Height(115);
                                    });

                                    // =========================================================
                                    // 3. CARTÃO DO CLIENTE, DATA E VALIDADE
                                    // =========================================================
                                    col.Item().PaddingBottom(16)
                                        .CornerRadius(12)
                                        .Background("#E6FFFFFF")
                                        .Border(1).BorderColor("#E6C483")
                                        .PaddingVertical(10)
                                        .PaddingHorizontal(14)
                                        .Row(clientRow =>
                                        {
                                            clientRow.ConstantItem(36).Height(36).AlignMiddle()
                                                .Svg("<svg viewBox=\"0 0 40 40\"><polygon points=\"20,2 36,11 36,29 20,38 4,29 4,11\" fill=\"#F8C463\" stroke=\"#E09E19\" stroke-width=\"1.5\"/><circle cx=\"20\" cy=\"15\" r=\"4.5\" fill=\"#381300\"/><path d=\"M12 28c0-4.4 3.6-7 8-7s8 2.6 8 7\" fill=\"#381300\"/></svg>");

                                            clientRow.RelativeItem().PaddingLeft(10).AlignMiddle().Column(cCol =>
                                            {
                                                cCol.Item().Text("Cliente").FontSize(8.5f).FontColor("#7A6552");
                                                cCol.Item().Text(nomeCliente).FontSize(13).Bold().FontColor("#291506");
                                            });

                                            clientRow.AutoItem().PaddingHorizontal(10).AlignMiddle().Width(1).Height(28).Background("#DFC8A6");

                                            clientRow.AutoItem().AlignMiddle().Row(dRow =>
                                            {
                                                dRow.AutoItem().Width(14).Height(14).AlignMiddle()
                                                    .Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#381300\" d=\"M19 4h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V10h14v10zm0-12H5V6h14v2z\"/></svg>");

                                                dRow.AutoItem().PaddingLeft(6).Column(dCol =>
                                                {
                                                    dCol.Item().Text("Data do Orçamento").FontSize(7.5f).FontColor("#7A6552");
                                                    dCol.Item().Text(DateTime.Now.ToString("dd 'de' MMMM 'de' yyyy", culturaBR)).FontSize(8.5f).Bold().FontColor("#291506");
                                                });
                                            });

                                            clientRow.AutoItem().PaddingHorizontal(10).AlignMiddle().Width(1).Height(28).Background("#DFC8A6");

                                            clientRow.AutoItem().AlignMiddle().Row(vRow =>
                                            {
                                                vRow.AutoItem().Width(14).Height(14).AlignMiddle()
                                                    .Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#381300\" d=\"M12 2C6.5 2 2 6.5 2 12s4.5 10 10 10 10-4.5 10-10S17.5 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zm.5-13H11v6l5.2 3.2.8-1.3-4.5-2.7V7z\"/></svg>");

                                                vRow.AutoItem().PaddingLeft(6).Column(vCol =>
                                                {
                                                    vCol.Item().Text("Validade").FontSize(7.5f).FontColor("#7A6552");
                                                    vCol.Item().Text("por 30 dias").FontSize(8.5f).Bold().FontColor("#291506");
                                                });
                                            });
                                        });

                                    // =========================================================
                                    // 4. SEÇÃO: PRODUTO SOLICITADO (Aba alinhada + Card)
                                    // =========================================================
                                    col.Item().Layers(prodLayers =>
                                    {
                                        prodLayers.Layer().Width(180).Height(37)
                                            .Svg("<svg viewBox=\"0 0 180 37\"><path fill=\"#F8C463\" d=\"M0 10 Q0 0 10 0 L155 0 Q162 0 165 6 L175 25 L175 37 L0 37 Z\"/></svg>");

                                        prodLayers.PrimaryLayer().Column(prodSection =>
                                        {
                                            prodSection.Item().Width(180).Height(25).PaddingLeft(12).AlignMiddle().Row(r =>
                                            {
                                                r.AutoItem().Width(13).Height(13).AlignMiddle()
                                                    .Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#291506\" d=\"M21 16.5c0 .38-.21.71-.53.88l-7.9 4.44c-.16.12-.36.18-.57.18-.21 0-.41-.06-.57-.18l-7.9-4.44A.991.991 0 0 1 3 16.5v-9c0-.38.21-.71.53-.88l7.9-4.44c.16-.12.36-.18.57-.18.21 0 .41.06.57.18l7.9 4.44c.32.17.53.5.53.88v9zM12 4.15L6.04 7.5 12 10.85l5.96-3.35L12 4.15zM5 15.91l6 3.38v-6.71L5 9.21v6.7zm14 0v-6.7l-6 3.37v6.71l6-3.38z\"/></svg>");
                                                r.AutoItem().PaddingLeft(6).AlignMiddle().Text("Produto Solicitado").FontSize(10).Bold().FontColor("#291506");
                                            });

                                            prodSection.Item()
                                                .CornerRadius(12)
                                                .Background("#FFFDF9")
                                                .Border(1).BorderColor("#E6C483")
                                                .Padding(14)
                                                .Row(cardRow =>
                                                {
                                                    cardRow.ConstantItem(88).Height(88)
                                                        .CornerRadius(8)
                                                        .Background("#FFFFFF")
                                                        .Border(1).BorderColor("#E6C483")
                                                        .Padding(4)
                                                        .AlignCenter().AlignMiddle().Element(imgContainer =>
                                                        {
                                                            if (!string.IsNullOrWhiteSpace(prod.CaminhoImagem) && File.Exists(prod.CaminhoImagem))
                                                                imgContainer.Image(prod.CaminhoImagem);
                                                            else
                                                                imgContainer.Text("Sem foto").FontSize(8.5f).FontColor("#A99F96");
                                                        });

                                                    cardRow.RelativeItem().PaddingLeft(14).AlignMiddle().Column(specCol =>
                                                    {
                                                        specCol.Item().PaddingBottom(4).Text(prod.Nome).FontSize(15).Bold().FontColor("#291506");

                                                        specCol.Item().PaddingTop(2).Row(r =>
                                                        {
                                                            r.AutoItem().Width(11).Height(11).AlignMiddle()
                                                                .Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#5A3B1C\" d=\"M12 2L2 7l10 5 10-5-10-5zm0 14.5l-10-5v2.5l10 5 10-5V11.5l-10 5z\"/></svg>");
                                                            r.AutoItem().PaddingLeft(6).AlignMiddle().Text(t =>
                                                            {
                                                                t.Span("Material: ").Bold().FontSize(9.5f).FontColor("#381300");
                                                                t.Span(prod.FilamentoNome).FontSize(9.5f).FontColor("#381300");
                                                            });
                                                        });

                                                        specCol.Item().PaddingTop(2).Row(r =>
                                                        {
                                                            r.AutoItem().Width(11).Height(11).AlignMiddle()
                                                                .Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#5A3B1C\" d=\"M12 21c-4.418 0-8-3.582-8-8 0-4.667 8-11 8-11s8 6.333 8 11c0 4.418-3.582 8-8 8z\"/></svg>");
                                                            r.AutoItem().PaddingLeft(6).AlignMiddle().Text(t =>
                                                            {
                                                                t.Span("Cor: ").Bold().FontSize(9.5f).FontColor("#381300");
                                                                t.Span(prod.FilamentoDetalhe).FontSize(9.5f).FontColor("#381300");
                                                            });
                                                        });

                                                        specCol.Item().PaddingTop(2).Row(r =>
                                                        {
                                                            r.AutoItem().Width(11).Height(11).AlignMiddle()
                                                                .Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#5A3B1C\" d=\"M21 16.5c0 .38-.21.71-.53.88l-7.9 4.44c-.16.12-.36.18-.57.18-.21 0-.41-.06-.57-.18l-7.9-4.44A.991.991 0 0 1 3 16.5v-9c0-.38.21-.71.53-.88l7.9-4.44c.16-.12.36-.18.57-.18.21 0 .41.06.57.18l7.9 4.44c.32.17.53.5.53.88v9z\"/></svg>");
                                                            r.AutoItem().PaddingLeft(6).AlignMiddle().Text(t =>
                                                            {
                                                                t.Span("Quantidade: ").Bold().FontSize(9.5f).FontColor("#381300");
                                                                t.Span($"{qtdTexto} {sufixoUnidade}").FontSize(9.5f).FontColor("#381300");
                                                            });
                                                        });

                                                        specCol.Item().PaddingTop(2).Row(r =>
                                                        {
                                                            r.AutoItem().Width(11).Height(11).AlignMiddle()
                                                                .Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#5A3B1C\" d=\"M12 2C6.5 2 2 6.5 2 12s4.5 10 10 10 10-4.5 10-10S17.5 2 12 2zm0 18c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8zm.5-13H11v6l5.2 3.2.8-1.3-4.5-2.7V7z\"/></svg>");
                                                            r.AutoItem().PaddingLeft(6).AlignMiddle().Text(t =>
                                                            {
                                                                t.Span("Prazo de Produção: ").Bold().FontSize(9.5f).FontColor("#381300");
                                                                t.Span(prazoProducao).FontSize(9.5f).FontColor("#381300");
                                                            });
                                                        });
                                                    });

                                                    cardRow.ConstantItem(115).AlignMiddle().Element(priceBox =>
                                                    {
                                                        priceBox.CornerRadius(10)
                                                                .Background("#F9C661")
                                                                .Border(1).BorderColor("#E09E19")
                                                                .PaddingVertical(10)
                                                                .PaddingHorizontal(8)
                                                                .Column(pCol =>
                                                                {
                                                                    pCol.Item().AlignCenter().Text("Preço Unitário").FontSize(8.5f).FontColor("#4A2500");
                                                                    pCol.Item().PaddingTop(2).AlignCenter().Text(prod.PrecoSugeridoFormatado).FontSize(16).Bold().FontColor("#291506");
                                                                });
                                                    });
                                                });
                                        });
                                    });

                                    // =========================================================
                                    // 5. SEÇÃO: CUSTOS UTILIZADOS (Aba alinhada + Lista)
                                    // =========================================================
                                    col.Item().PaddingTop(16).Layers(costLayers =>
                                    {
                                        costLayers.Layer().Width(180).Height(37)
                                            .Svg("<svg viewBox=\"0 0 180 37\"><path fill=\"#F8C463\" d=\"M0 10 Q0 0 10 0 L155 0 Q162 0 165 6 L175 25 L175 37 L0 37 Z\"/></svg>");

                                        costLayers.PrimaryLayer().Column(costSection =>
                                        {
                                            costSection.Item().Width(180).Height(25).PaddingLeft(12).AlignMiddle().Row(r =>
                                            {
                                                r.AutoItem().Width(13).Height(13).AlignMiddle()
                                                    .Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#291506\" d=\"M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-5 14h-4v-2h4v2zm0-4h-4v-2h4v2zm0-4h-4V7h4v2zm4 8h-2v-6h2v6zm0-8h-2V7h2v2zM8 17H6v-2h2v2zm0-4H6v-2h2v2zm0-4H6V7h2v2z\"/></svg>");
                                                r.AutoItem().PaddingLeft(6).AlignMiddle().Text("Custos Utilizados").FontSize(10).Bold().FontColor("#291506");
                                            });

                                            costSection.Item()
                                                .CornerRadius(12)
                                                .Background("#FFFDF9")
                                                .Border(1).BorderColor("#E6C483")
                                                .PaddingVertical(6)
                                                .PaddingHorizontal(16)
                                                .Column(cListCol =>
                                                {
                                                    for (int i = 0; i < listaCustos.Count; i++)
                                                    {
                                                        var itemCusto = listaCustos[i];
                                                        bool temLinhaAbaixo = i < listaCustos.Count - 1;

                                                        string nomeLower = itemCusto.Nome.ToLower();
                                                        string svgIcone = "<svg viewBox=\"0 0 24 24\"><path fill=\"#5A3B1C\" d=\"M12 2C6.48 2 2 4.02 2 6.5v11C2 19.98 6.48 22 12 22s10-2.02 10-4.5v-11C22 4.02 17.52 2 12 2zm0 2c4.42 0 8 1.34 8 3s-3.58 3-8 3-8-1.34-8-3 3.58-3 8-3zm0 16c-4.42 0-8-1.34-8-3v-2.23c1.61 1.07 4.61 1.73 8 1.73s6.39-.66 8-1.73V17c0 1.66-3.58 3-8 3z\"/></svg>"; // Filamento

                                                        if (nomeLower.Contains("energia"))
                                                            svgIcone = "<svg viewBox=\"0 0 24 24\"><path fill=\"#5A3B1C\" d=\"M7 2v11h3v9l7-12h-4l4-8z\"/></svg>"; // Raio
                                                        else if (nomeLower.Contains("mão") || nomeLower.Contains("mao") || nomeLower.Contains("preparo"))
                                                            svgIcone = "<svg viewBox=\"0 0 24 24\"><path fill=\"#5A3B1C\" d=\"M22.7 19l-9.1-9.1c.9-2.3.4-5-1.5-6.9-2-2-5-2.4-7.4-1.3L9 6 6 9 1.6 4.7C.4 7.1.9 10.1 2.9 12.1c1.9 1.9 4.6 2.4 6.9 1.5l9.1 9.1c.4.4 1 .4 1.4 0l2.3-2.3c.5-.4.5-1.1.1-1.4z\"/></svg>"; // Ferramenta
                                                        else if (nomeLower.Contains("fixo"))
                                                            svgIcone = "<svg viewBox=\"0 0 24 24\"><path fill=\"#5A3B1C\" d=\"M21.41 11.58l-9-9C12.05 2.22 11.55 2 11 2H4c-1.1 0-2 .9-2 2v7c0 .55.22 1.05.59 1.42l9 9c.36.36.86.58 1.41.58.55 0 1.05-.22 1.41-.59l7-7c.37-.36.59-.86.59-1.41 0-.55-.23-1.06-.59-1.42zM5.5 7C4.67 7 4 6.33 4 5.5S4.67 4 5.5 4 7 4.67 7 5.5 6.33 7 5.5 7z\"/></svg>"; // Etiqueta
                                                        else if (nomeLower.Contains("outro") || nomeLower.Contains("embalag"))
                                                            svgIcone = "<svg viewBox=\"0 0 24 24\"><path fill=\"#5A3B1C\" d=\"M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2zm1.41 16.09V20h-2.67v-1.93c-1.71-.36-3.16-1.46-3.27-3.4h1.96c.1 1.05.82 1.87 2.65 1.87 1.96 0 2.4-.98 2.4-1.59 0-.83-.44-1.61-2.67-2.14-2.48-.6-4.18-1.62-4.18-3.67 0-1.72 1.39-2.84 3.11-3.21V4h2.67v1.95c1.86.45 2.79 1.86 2.85 3.39H14.3c-.05-1.11-.64-1.87-2.22-1.87-1.5 0-2.4.68-2.4 1.64 0 .84.65 1.39 2.67 1.91s4.18 1.39 4.18 3.91c-.01 1.83-1.38 2.83-3.12 3.16z\"/></svg>"; // Moeda

                                                        cListCol.Item()
                                                            .BorderBottom(temLinhaAbaixo ? 1 : 0)
                                                            .BorderColor("#EADCC9")
                                                            .PaddingVertical(8)
                                                            .Row(cRow =>
                                                            {
                                                                cRow.AutoItem().Width(14).Height(14).AlignMiddle().Svg(svgIcone);
                                                                cRow.RelativeItem().PaddingLeft(10).AlignMiddle().Text(itemCusto.Nome).FontSize(10).FontColor("#381300");
                                                                cRow.AutoItem().AlignMiddle().Text(itemCusto.Valor).FontSize(10).Bold().FontColor("#381300");
                                                            });
                                                    }
                                                });
                                        });
                                    });

                                    // =========================================================
                                    // 6. BARRA ESCURA: VALOR FINAL DO ORÇAMENTO (Deste produto)
                                    // =========================================================
                                    col.Item().PaddingTop(16)
                                        .CornerRadius(12)
                                        .Background("#522604")
                                        .Border(1.5f).BorderColor("#E09E19")
                                        .PaddingVertical(12)
                                        .PaddingHorizontal(18)
                                        .Row(totalRow =>
                                        {
                                            totalRow.AutoItem().Width(22).Height(22).AlignMiddle()
                                                .Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#F8C463\" d=\"M7 18c-1.1 0-1.99.9-1.99 2S5.9 22 7 22s2-.9 2-2-.9-2-2-2zM1 2v2h2l3.6 7.59-1.35 2.45c-.16.28-.25.61-.25.96 0 1.1.9 2 2 2h12v-2H7.42c-.14 0-.25-.11-.25-.25l.03-.12.9-1.63h7.45c.75 0 1.41-.41 1.75-1.03l3.58-6.49A1.003 1.003 0 0 0 20 4H5.21l-.94-2H1zm16 16c-1.1 0-1.99.9-1.99 2s.89 2 1.99 2 2-.9 2-2-.9-2-2-2z\"/></svg>");

                                            totalRow.RelativeItem().PaddingLeft(12).AlignMiddle()
                                                .Text("Valor Final do Orçamento").FontSize(13).SemiBold().FontColor("#FFF4E4");

                                            totalRow.AutoItem().AlignMiddle()
                                                .Text(valorFinalFormatado).FontSize(20).Bold().FontColor("#FFFFFF");
                                        });
                                });

                                // =========================================================
                                // 7. RODAPÉ (Fixado na base de cada página)
                                // =========================================================
                                page.Footer().PaddingTop(10).Row(footerRow =>
                                {
                                    footerRow.RelativeItem().AlignMiddle().Row(leftFooter =>
                                    {
                                        leftFooter.AutoItem().Width(20).Height(20).AlignMiddle()
                                            .Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#381300\" d=\"M12.22 19.85c-.18.18-.5.21-.71 0a.504.504 0 0 1 0-.71l3.39-3.39-1.41-1.41-3.39 3.39c-.19.2-.51.19-.71 0-.21-.21-.18-.53 0-.71l3.39-3.39-1.41-1.41-3.39 3.39c-.18.18-.5.21-.71 0-.19-.19-.19-.51 0-.71l3.39-3.39-1.42-1.41-3.39 3.39c-.19.2-.51.19-.71 0-.21-.21-.18-.53 0-.71L9.5 9.29l3.36 3.36c1.17 1.17 3.07 1.17 4.24 0l4.24-4.24-4.24-4.24-2.83 2.83c-.78.78-2.05.78-2.83 0L8.61 4.17 2.96 9.83c-1.17 1.17-1.17 3.07 0 4.24l6.36 6.36c1.17 1.17 3.07 1.17 4.24 0l6.36-6.36-1.41-1.41-6.3 6.19z\"/></svg>");

                                        leftFooter.AutoItem().PaddingLeft(8).AlignMiddle()
                                            .Text("Qualquer dúvida, estamos à disposição!").FontSize(9.5f).FontColor("#291506");
                                    });

                                    footerRow.AutoItem().AlignMiddle().Row(rightFooter =>
                                    {
                                        if (logoBytes != null)
                                        {
                                            rightFooter.ConstantItem(28).Height(28).AlignMiddle().Image(logoBytes);
                                        }

                                        rightFooter.AutoItem().PaddingLeft(8).AlignMiddle().Column(fCol =>
                                        {
                                            fCol.Item().Row(r =>
                                            {
                                                r.AutoItem().Text("Favo Studio ").FontSize(12).Bold().FontColor("#291506");
                                                r.AutoItem().Text(" 3D").FontSize(12).Bold().FontColor("#E09E19");
                                            });
                                            fCol.Item().Text("Inovação em cada detalhe").FontSize(6.5f).FontColor("#5A3B1C");
                                        });
                                    });
                                });
                            });
                        }
                    }).GeneratePdf(saveFileDialog.FileName);

                    Process.Start(new ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao gerar o Orçamento: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // =================================================================
        // AUXILIARES DO ORÇAMENTO (Janela de Cliente + Extração de Custos)
        // =================================================================
        private (string Cliente, string Prazo)? SolicitarDadosOrcamento()
        {
            var corMarromEscuro = (Brush)new BrushConverter().ConvertFrom("#381300")!;
            var corMarromTexto = (Brush)new BrushConverter().ConvertFrom("#3E2723")!;
            var corMarromSuave = (Brush)new BrushConverter().ConvertFrom("#795548")!;
            var corDourado = (Brush)new BrushConverter().ConvertFrom("#EBB45D")!;
            var corBordaSuave = (Brush)new BrushConverter().ConvertFrom("#EADCC9")!;
            var corFundoInput = (Brush)new BrushConverter().ConvertFrom("#FFFBF5")!;

            var dialog = new Window
            {
                Title = "Dados do Orçamento",
                Width = 430,
                SizeToContent = SizeToContent.Height,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Window.GetWindow(this),
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                ResizeMode = ResizeMode.NoResize
            };

            dialog.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) dialog.DragMove(); };

            var estiloTextBox = new Style(typeof(TextBox));
            estiloTextBox.Setters.Add(new Setter(Control.HeightProperty, 40.0));
            estiloTextBox.Setters.Add(new Setter(Control.FontSizeProperty, 13.5));
            estiloTextBox.Setters.Add(new Setter(Control.ForegroundProperty, corMarromTexto));
            estiloTextBox.Setters.Add(new Setter(Control.BackgroundProperty, corFundoInput));
            estiloTextBox.Setters.Add(new Setter(Control.BorderBrushProperty, corBordaSuave));
            estiloTextBox.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1.5)));
            estiloTextBox.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(12, 0, 12, 0)));
            estiloTextBox.Setters.Add(new Setter(Control.VerticalContentAlignmentProperty, System.Windows.VerticalAlignment.Center));

            var templateTextBox = new ControlTemplate(typeof(TextBox));
            var borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.Name = "BordaInput";
            borderFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
            borderFactory.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Control.BorderBrushProperty));
            borderFactory.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Control.BorderThicknessProperty));
            borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));

            var scrollViewerFactory = new FrameworkElementFactory(typeof(ScrollViewer));
            scrollViewerFactory.Name = "PART_ContentHost";
            scrollViewerFactory.SetValue(FrameworkElement.MarginProperty, new Thickness(2));
            borderFactory.AppendChild(scrollViewerFactory);
            templateTextBox.VisualTree = borderFactory;

            var triggerFoco = new Trigger { Property = UIElement.IsKeyboardFocusedProperty, Value = true };
            triggerFoco.Setters.Add(new Setter(Border.BorderBrushProperty, corDourado, "BordaInput"));
            triggerFoco.Setters.Add(new Setter(Border.BackgroundProperty, Brushes.White, "BordaInput"));
            templateTextBox.Triggers.Add(triggerFoco);
            estiloTextBox.Setters.Add(new Setter(Control.TemplateProperty, templateTextBox));

            Button CriarBotaoEstilizado(string texto, Brush fundo, Brush textoCor, Brush? borda)
            {
                var btn = new Button
                {
                    Content = texto,
                    Height = 40,
                    FontSize = 13.5,
                    FontWeight = FontWeights.Bold,
                    Background = fundo,
                    Foreground = textoCor,
                    BorderBrush = borda ?? Brushes.Transparent,
                    BorderThickness = borda != null ? new Thickness(1.5) : new Thickness(0),
                    Cursor = Cursors.Hand
                };

                var templateBtn = new ControlTemplate(typeof(Button));
                var borderBtn = new FrameworkElementFactory(typeof(Border));
                borderBtn.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
                borderBtn.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Control.BorderBrushProperty));
                borderBtn.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Control.BorderThicknessProperty));
                borderBtn.SetValue(Border.CornerRadiusProperty, new CornerRadius(10));

                var contentBtn = new FrameworkElementFactory(typeof(ContentPresenter));
                contentBtn.SetValue(FrameworkElement.HorizontalAlignmentProperty, System.Windows.HorizontalAlignment.Center);
                contentBtn.SetValue(FrameworkElement.VerticalAlignmentProperty, System.Windows.VerticalAlignment.Center);
                borderBtn.AppendChild(contentBtn);

                templateBtn.VisualTree = borderBtn;
                btn.Template = templateBtn;
                return btn;
            }

            var cardPrincipal = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(16),
                BorderBrush = corBordaSuave,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(20),
                Padding = new Thickness(28, 24, 28, 24),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = System.Windows.Media.Colors.Black,
                    Opacity = 0.22,
                    BlurRadius = 22,
                    ShadowDepth = 0
                }
            };

            var stack = new StackPanel();

            stack.Children.Add(new TextBlock
            {
                Text = "Dados do Orçamento",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = corMarromEscuro,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            });

            stack.Children.Add(new TextBlock
            {
                Text = "Preencha as informações para gerar o PDF do cliente.",
                FontSize = 12.5,
                Foreground = corMarromSuave,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                Margin = new Thickness(0, 4, 0, 20)
            });

            stack.Children.Add(new TextBlock
            {
                Text = "Nome do Cliente",
                FontSize = 12.5,
                FontWeight = FontWeights.Bold,
                Foreground = corMarromEscuro,
                Margin = new Thickness(2, 0, 0, 6)
            });

            var txtCliente = new TextBox
            {
                Style = estiloTextBox,
                Margin = new Thickness(0, 0, 0, 14)
            };
            stack.Children.Add(txtCliente);

            stack.Children.Add(new TextBlock
            {
                Text = "Prazo de Produção",
                FontSize = 12.5,
                FontWeight = FontWeights.Bold,
                Foreground = corMarromEscuro,
                Margin = new Thickness(2, 0, 0, 6)
            });

            var txtPrazo = new TextBox
            {
                Text = "até 2 dias úteis",
                Style = estiloTextBox,
                Margin = new Thickness(0, 0, 0, 24)
            };

            txtPrazo.TextChanged += (s, e) =>
            {
                int pos = txtPrazo.CaretIndex;
                string textoAtual = txtPrazo.Text;
                string textoCorrigido = System.Text.RegularExpressions.Regex.Replace(
                    textoAtual, @"\b1\s+dias\s+[uú]teis\b", "1 dia útil", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                textoCorrigido = System.Text.RegularExpressions.Regex.Replace(
                    textoCorrigido, @"\b([2-9]|\d{2,})\s+dia\s+[uú]til\b", "$1 dias úteis", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (textoCorrigido != textoAtual)
                {
                    txtPrazo.Text = textoCorrigido;
                    txtPrazo.CaretIndex = Math.Min(pos, txtPrazo.Text.Length);
                }
            };

            stack.Children.Add(txtPrazo);
            
            var gridBotoes = new Grid();
            gridBotoes.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            gridBotoes.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            gridBotoes.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var btnCancelar = CriarBotaoEstilizado("Cancelar", Brushes.White, corMarromEscuro, corDourado);
            btnCancelar.IsCancel = true;
            btnCancelar.Click += (s, e) => { dialog.DialogResult = false; dialog.Close(); };
            Grid.SetColumn(btnCancelar, 0);

            var btnConfirmar = CriarBotaoEstilizado("Gerar PDF", corDourado, corMarromTexto, null);
            btnConfirmar.IsDefault = true;
            btnConfirmar.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };
            Grid.SetColumn(btnConfirmar, 2);

            gridBotoes.Children.Add(btnCancelar);
            gridBotoes.Children.Add(btnConfirmar);
            stack.Children.Add(gridBotoes);

            cardPrincipal.Child = stack;
            dialog.Content = cardPrincipal;
            txtCliente.Focus();

            if (dialog.ShowDialog() == true)
                return (txtCliente.Text, txtPrazo.Text);

            return null;
        }

        private System.Collections.Generic.List<(string Nome, string Valor)> ExtrairCustosParaOrcamento(
            System.Collections.Generic.List<ProdutoItemModel> produtos,
            System.Globalization.CultureInfo cultura)
        {
            var resultado = new System.Collections.Generic.List<(string Nome, string Valor)>();
            var primeiroProd = produtos.First();
            string mat = string.IsNullOrWhiteSpace(primeiroProd.FilamentoNome) ? "PLA" : primeiroProd.FilamentoNome.ToUpper();

            if (primeiroProd.DadosCompletos != null)
            {
                var props = primeiroProd.DadosCompletos.GetType().GetProperties();
                foreach (var prop in props)
                {
                    if (prop.Name.Equals("Parametros", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                    {
                        if (prop.GetValue(primeiroProd.DadosCompletos) is System.Collections.IEnumerable lista)
                        {
                            foreach (var item in lista)
                            {
                                if (item == null) continue;

                                var pTitulo = item.GetType().GetProperty("Titulo")
                                           ?? item.GetType().GetProperty("Nome")
                                           ?? item.GetType().GetProperty("Rotulo")
                                           ?? item.GetType().GetProperty("Descricao");

                                var pValor = item.GetType().GetProperty("Valor")
                                          ?? item.GetType().GetProperty("ValorFormatado");

                                string nomeCusto = pTitulo?.GetValue(item)?.ToString() ?? "";
                                string valor = pValor?.GetValue(item)?.ToString() ?? "";

                                if (!string.IsNullOrWhiteSpace(nomeCusto) && valor.Contains("R$"))
                                {
                                    if (nomeCusto.Equals("Material (filamento)", StringComparison.OrdinalIgnoreCase))
                                        nomeCusto = $"Filamento ({mat})";

                                    resultado.Add((nomeCusto, valor));
                                }
                            }
                        }
                    }
                }
            }

            if (resultado.Count == 0)
            {
                resultado.Add(($"Filamento ({mat})", primeiroProd.CustoTotalFormatado ?? "R$ 0,00"));
                resultado.Add(("Energia elétrica", "R$ 0,00"));
                resultado.Add(("Mão de obra", "R$ 0,00"));
            }

            return resultado;
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