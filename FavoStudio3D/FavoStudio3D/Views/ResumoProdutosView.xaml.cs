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
                            page.Margin(40);

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

                            page.DefaultTextStyle(x => x.FontSize(12).FontFamily(QFonts.Arial));

                            page.Content().Column(col =>
                            {
                                // =========================================================
                                // CABEÇALHO DO CATÁLOGO
                                // =========================================================
                                col.Item().PaddingBottom(20).Row(headerRow =>
                                {
                                    // LOGO FAVO STUDIO
                                    headerRow.ConstantItem(60).Height(60).Element(logoContainer =>
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
                                        catch (Exception) { /* Ignora se o logo não carregar */ }
                                    });

                                    // LINHA VERTICAL E TÍTULOS (Com as suas modificações mantidas)
                                    headerRow.RelativeItem().PaddingLeft(10).AlignMiddle().Row(titleRow =>
                                    {
                                        titleRow.AutoItem().Width(1).Height(40).Background("#381300"); // Linha separadora

                                        titleRow.RelativeItem().PaddingLeft(10).Column(titleCol =>
                                        {
                                            titleCol.Item().Row(r =>
                                            {
                                                r.AutoItem().Text("Favo Studio ").FontSize(22).Bold().FontColor("#291506");
                                                r.AutoItem().Text(" 3D").FontSize(22).Bold().FontColor("#E09E19");
                                            });
                                            titleCol.Item().PaddingTop(2).Text("Inovação em cada detalhe").FontSize(9).FontColor("#6B5A4B");
                                        });
                                    });

                                    // DATA (Caixa ampliada para 200 de largura para não espremer o texto)
                                    headerRow.ConstantItem(200).AlignRight().AlignMiddle().Element(dateBadge =>
                                    {
                                        dateBadge.Height(30).Layers(layers =>
                                        {
                                            // Caixa com borda arredondada e cor de fundo do design
                                            layers.Layer().Svg("<svg viewBox=\"0 0 200 30\" preserveAspectRatio=\"none\"><rect x=\"1\" y=\"1\" width=\"198\" height=\"28\" rx=\"8\" ry=\"8\" fill=\"#FDE5BC\" stroke=\"#E3C17D\" stroke-width=\"1\"/></svg>");

                                            // Ícone e Texto
                                            layers.PrimaryLayer().AlignCenter().AlignMiddle().Row(r =>
                                            {
                                                r.AutoItem().Width(12).PaddingRight(4).Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#381300\" d=\"M19 4h-1V2h-2v2H8V2H6v2H5c-1.11 0-1.99.9-1.99 2L3 20c0 1.1.89 2 2 2h14c1.1 0 2-.9 2-2V6c0-1.1-.9-2-2-2zm0 16H5V10h14v10zm0-12H5V6h14v2zm-7 5h5v5h-5z\"/></svg>");
                                                r.AutoItem().Text(DateTime.Now.ToString("dd 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("pt-BR"))).FontSize(9).FontColor("#381300");
                                            });
                                        });
                                    });
                                });

                                // TÍTULO DA SEÇÃO
                                col.Item().PaddingBottom(20).Text("Catálogo de Produtos Exclusivos").FontSize(24).Bold().FontColor("#291506");

                                // =========================================================
                                // LISTA DE PRODUTOS
                                // =========================================================
                                col.Item().Column(listaCol =>
                                {
                                    listaCol.Spacing(20);

                                    foreach (var prod in produtosSelecionados)
                                    {
                                        // O Card Inteiro
                                        listaCol.Item().Height(115).Layers(cardLayers =>
                                        {
                                            // FUNDO DO CARD CORRIGIDO: Branco sólido com 85% de opacidade para simular o translúcido (adeus azul!)
                                            cardLayers.Layer().Svg("<svg viewBox=\"0 0 1000 115\" preserveAspectRatio=\"none\"><rect x=\"1\" y=\"1\" width=\"998\" height=\"113\" rx=\"12\" ry=\"12\" fill=\"#FFFFFF\" fill-opacity=\"0.85\" stroke=\"#E3C17D\" stroke-width=\"1\"/></svg>");

                                            cardLayers.PrimaryLayer().Padding(12).Row(cardRow =>
                                            {
                                                // IMAGEM DO PRODUTO
                                                cardRow.ConstantItem(90).Height(90).Layers(imgLayers =>
                                                {
                                                    imgLayers.Layer().Svg("<svg viewBox=\"0 0 90 90\" preserveAspectRatio=\"none\"><rect x=\"1\" y=\"1\" width=\"88\" height=\"88\" rx=\"8\" ry=\"8\" fill=\"#FFFFFF\" stroke=\"#EADCC9\" stroke-width=\"1\"/></svg>");

                                                    imgLayers.PrimaryLayer().Padding(5).AlignCenter().AlignMiddle().Element(imgContainer =>
                                                    {
                                                        if (!string.IsNullOrWhiteSpace(prod.CaminhoImagem) && File.Exists(prod.CaminhoImagem))
                                                            imgContainer.Image(prod.CaminhoImagem);
                                                        else
                                                            imgContainer.Text("Sem foto").FontSize(9).FontColor("#A99F96");
                                                    });
                                                });

                                                // TEXTOS DO PRODUTO
                                                cardRow.RelativeItem().PaddingLeft(15).AlignMiddle().Column(texto =>
                                                {
                                                    texto.Item().Text(prod.Nome).FontSize(20).Bold().FontColor("#291506");

                                                    if (prod.DadosCompletos != null && !string.IsNullOrWhiteSpace(prod.DadosCompletos.ProdutoDescricao))
                                                    {
                                                        texto.Item().PaddingTop(2).Text(prod.DadosCompletos.ProdutoDescricao).FontSize(9).FontColor("#6B5A4B");
                                                    }

                                                    texto.Item().PaddingTop(6).Row(r =>
                                                    {
                                                        r.AutoItem().Width(10).Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#5A3B1C\" d=\"M12 2L2 7l10 5 10-5-10-5zm0 14.5l-10-5v2.5l10 5 10-5V11.5l-10 5z\"/></svg>");
                                                        r.AutoItem().PaddingLeft(4).Text($"Material: {prod.FilamentoNome}").FontSize(10).FontColor("#381300");
                                                    });

                                                    texto.Item().PaddingTop(2).Row(r =>
                                                    {
                                                        r.AutoItem().Width(10).Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#5A3B1C\" d=\"M12 21c-4.418 0-8-3.582-8-8 0-4.667 8-11 8-11s8 6.333 8 11c0 4.418-3.582 8-8 8z\"/></svg>");
                                                        r.AutoItem().PaddingLeft(4).Text($"Cor: {prod.FilamentoDetalhe}").FontSize(10).FontColor("#381300");
                                                    });

                                                    if (!string.IsNullOrWhiteSpace(prod.PesoGramaSalvo) && prod.PesoGramaSalvo != "0")
                                                    {
                                                        texto.Item().PaddingTop(2).Row(r =>
                                                        {
                                                            r.AutoItem().Width(10).Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#5A3B1C\" d=\"M19 13v6a2 2 0 01-2 2H7a2 2 0 01-2-2v-6c-1.1 0-2-.9-2-2 0-1 .7-1.8 1.6-2L7 3h10l2.4 6c.9.2 1.6 1 1.6 2 0 1.1-.9 2-2 2zm-7 4h4v-2h-4v2z\"/></svg>");
                                                            r.AutoItem().PaddingLeft(4).Text($"Peso: {prod.PesoGramaSalvo}g").FontSize(10).FontColor("#381300");
                                                        });
                                                    }

                                                    texto.Item().PaddingTop(6).Row(r =>
                                                    {
                                                        r.AutoItem().Width(10).Svg("<svg viewBox=\"0 0 24 24\"><path fill=\"#A99F96\" d=\"M3 4h2v16H3V4zm4 0h4v16H7V4zm6 0h2v16h-2V4zm4 0h4v16h-4V4z\"/></svg>");
                                                        r.AutoItem().PaddingLeft(4).Text($"Ref: {prod.CodigoSku}").FontSize(8).FontColor("#8D7E70");
                                                    });
                                                });

                                                // TAG DE PREÇO (BANDEIRINHA AMARELA)
                                                cardRow.ConstantItem(120).AlignMiddle().AlignRight().Element(priceBadge =>
                                                {
                                                    priceBadge.Width(120).Height(40).Layers(layers =>
                                                    {
                                                        layers.Layer().Svg("<svg viewBox=\"0 0 120 40\"><path fill=\"#FFC857\" d=\"M15 0 L120 0 L120 40 L15 40 L0 20 Z\"/></svg>");

                                                        layers.PrimaryLayer().PaddingLeft(15).AlignCenter().AlignMiddle()
                                                              .Text(prod.PrecoSugeridoFormatado).FontSize(16).Bold().FontColor("#381300");
                                                    });
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