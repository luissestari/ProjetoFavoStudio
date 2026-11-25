using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

using FavoStudio3D.Models;

namespace FavoStudio3D.Views
{
    public partial class CalculadoraView : UserControl
    {
        // === MEMÓRIA ESTÁTICA PARA RASCUNHO E PASSAGEM DE DADOS ===
        public static class MemoriaRascunho
        {
            public static object? ProdutoEmEdicaoRef = null;
            public static bool CalculoPendente = false;
            public static ResultadoCalculoModel? ResultadoPendente = null;

            public static string Nome = "";
            public static string Descricao = "";
            public static string Peso = "";
            public static string Horas = "";
            public static string Minutos = "";
            public static string Quantidade = "1";
            public static string Frete = "";
            public static string Margem = "";
            public static object? FilamentoSelecionado = null;
            public static object? ImpressoraSelecionada = null;
            public static List<string> CustosSelecionadosDescricoes = new List<string>();
            public static string CaminhoImagem = "";
            public static string Categoria = "";
            public static string ValorHora = "";
            public static string HorasMao = "";
            public static string MinutosMao = "";

            public static string NomeFilamentoVisual = "PLA";
            public static string CodigoHexVisual = "#000000";
            public static string NomeCorVisual = "Padrão";
            public static Brush CorFilamentoVisualBrush = Brushes.Black;
            public static string NomeImpressoraVisual = "Impressora 3D";
            public static string ModeloImpressoraVisual = "Principal";

            public static void LimparTudo()
            {
                CalculoPendente = false;
                ResultadoPendente = null;
                Nome = ""; Descricao = ""; Peso = ""; Horas = ""; Minutos = "";
                Quantidade = "1"; Frete = ""; Margem = ""; Categoria = "";
                ValorHora = ""; HorasMao = ""; MinutosMao = ""; CaminhoImagem = "";
                FilamentoSelecionado = null; ImpressoraSelecionada = null;
                CustosSelecionadosDescricoes.Clear();
                NomeFilamentoVisual = "PLA"; CodigoHexVisual = "#000000"; NomeCorVisual = "Padrão";
                CorFilamentoVisualBrush = Brushes.Black; NomeImpressoraVisual = "Impressora 3D"; ModeloImpressoraVisual = "Principal";
            }
        }

        private double _totalCustosFixos = 0;

        // Variáveis para controle de arraste e zoom da imagem
        private bool _arrastandoImagem = false;
        private bool _houveArraste = false;
        private Point _pontoInicialClique;
        private double _offsetInicialX;
        private double _offsetInicialY;

        public class CustoVisual : INotifyPropertyChanged
        {
            public dynamic? ItemOriginal { get; set; }
            public string Descricao { get; set; } = string.Empty;
            public double Valor { get; set; }

            private string _checkVisivel = "Collapsed";
            public string CheckVisivel
            {
                get { return _checkVisivel; }
                set { _checkVisivel = value; OnPropertyChanged("CheckVisivel"); }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        private List<CustoVisual> custosDisponiveisVisuais = new List<CustoVisual>();
        private List<CustoVisual> custosSelecionados = new List<CustoVisual>();

        public CalculadoraView()
        {
            InitializeComponent();
            this.Unloaded += (s, e) => SalvarRascunho();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (MemoriaRascunho.CalculoPendente && MemoriaRascunho.ResultadoPendente != null)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var telaResumo = new ResumoCalculadoraView(MemoriaRascunho.ResultadoPendente);
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
                }), System.Windows.Threading.DispatcherPriority.Send);

                return;
            }

            CarregarDadosCadastrados();
            RestaurarRascunho();
        }

        private void SalvarRascunho()
        {
            MemoriaRascunho.Nome = TxtNomeProduto.Text;
            MemoriaRascunho.Descricao = TxtDescricao.Text;
            MemoriaRascunho.Peso = TxtPeso.Text;
            MemoriaRascunho.Horas = TxtHoras.Text;
            MemoriaRascunho.Minutos = TxtMinutos.Text;
            MemoriaRascunho.Quantidade = TxtQuantidade.Text;
            MemoriaRascunho.Frete = TxtFrete.Text;
            MemoriaRascunho.Margem = TxtMargem.Text;
            MemoriaRascunho.Categoria = TxtCategoria.Text;
            MemoriaRascunho.ValorHora = TxtValorHora.Text;
            MemoriaRascunho.HorasMao = TxtHorasMaoDeObra.Text;
            MemoriaRascunho.MinutosMao = TxtMinutosMaoDeObra.Text;

            MemoriaRascunho.FilamentoSelecionado = ListaFilamentos.SelectedItem;
            MemoriaRascunho.ImpressoraSelecionada = ListaImpressoras.SelectedItem;

            MemoriaRascunho.CustosSelecionadosDescricoes.Clear();
            foreach (var c in custosSelecionados)
            {
                MemoriaRascunho.CustosSelecionadosDescricoes.Add(c.Descricao);
            }
        }

        private void RestaurarRascunho()
        {
            TxtNomeProduto.Text = MemoriaRascunho.Nome;
            TxtDescricao.Text = MemoriaRascunho.Descricao;
            TxtPeso.Text = MemoriaRascunho.Peso;
            TxtHoras.Text = MemoriaRascunho.Horas;
            TxtMinutos.Text = MemoriaRascunho.Minutos;
            TxtQuantidade.Text = string.IsNullOrWhiteSpace(MemoriaRascunho.Quantidade) ? "1" : MemoriaRascunho.Quantidade;
            TxtFrete.Text = MemoriaRascunho.Frete;
            TxtMargem.Text = MemoriaRascunho.Margem;
            TxtCategoria.Text = MemoriaRascunho.Categoria;
            TxtValorHora.Text = MemoriaRascunho.ValorHora;
            TxtHorasMaoDeObra.Text = MemoriaRascunho.HorasMao;
            TxtMinutosMaoDeObra.Text = MemoriaRascunho.MinutosMao;

            if (!string.IsNullOrWhiteSpace(MemoriaRascunho.CaminhoImagem))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(MemoriaRascunho.CaminhoImagem);
                    bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    ImgProdutoPreview.Source = bitmap;
                    PainelPlaceholderImagem.Visibility = Visibility.Collapsed;
                }
                catch { }
            }

            if (MemoriaRascunho.FilamentoSelecionado != null)
            {
                ListaFilamentos.SelectedItem = MemoriaRascunho.FilamentoSelecionado;
                ListaFilamentos_SelectionChanged(this, null!);
            }

            if (MemoriaRascunho.ImpressoraSelecionada != null)
            {
                ListaImpressoras.SelectedItem = MemoriaRascunho.ImpressoraSelecionada;
                ListaImpressoras_SelectionChanged(this, null!);
            }

            if (MemoriaRascunho.CustosSelecionadosDescricoes.Count > 0)
            {
                custosSelecionados.Clear();
                foreach (var desc in MemoriaRascunho.CustosSelecionadosDescricoes)
                {
                    var custoVisual = custosDisponiveisVisuais.Find(c => c.Descricao == desc);
                    if (custoVisual != null)
                    {
                        custoVisual.CheckVisivel = "Visible";
                        custosSelecionados.Add(custoVisual);
                    }
                }
                AtualizarTagsVisuais();
            }
        }

        private void CarregarDadosCadastrados()
        {
            if (ResumoCustosMateriaisView.FilamentosBancoDados != null) ListaFilamentos.ItemsSource = ResumoCustosMateriaisView.FilamentosBancoDados;
            if (ResumoImpressorasView.ImpressorasBancoDados != null) ListaImpressoras.ItemsSource = ResumoImpressorasView.ImpressorasBancoDados;
            if (ResumoCustosMateriaisView.CustosBancoDados != null)
            {
                custosDisponiveisVisuais.Clear();
                double somatorioCustosFixos = 0;

                foreach (dynamic custo in ResumoCustosMateriaisView.CustosBancoDados)
                {
                    string tipoCusto = "";
                    Type tipoDoObjeto = custo.GetType();

                    PropertyInfo? propInfo = tipoDoObjeto.GetProperty("Tipo") ?? tipoDoObjeto.GetProperty("TipoCusto") ?? tipoDoObjeto.GetProperty("Categoria");
                    if (propInfo != null)
                    {
                        var valorEncontrado = propInfo.GetValue(custo);
                        tipoCusto = valorEncontrado != null ? valorEncontrado.ToString() : "";
                    }

                    double val = 0;
                    try
                    {
                        object? rawValue = null;
                        foreach (var prop in tipoDoObjeto.GetProperties())
                        {
                            string pName = prop.Name.ToLower();
                            if (pName.Contains("valor") || pName.Contains("preco") || pName.Contains("custo"))
                            {
                                rawValue = prop.GetValue(custo);
                                break;
                            }
                        }
                        if (rawValue == null)
                        {
                            foreach (var field in tipoDoObjeto.GetFields())
                            {
                                string fName = field.Name.ToLower();
                                if (fName.Contains("valor") || fName.Contains("preco") || fName.Contains("custo"))
                                {
                                    rawValue = field.GetValue(custo);
                                    break;
                                }
                            }
                        }
                        if (rawValue != null)
                        {
                            if (rawValue is double d) val = d;
                            else if (rawValue is decimal dec) val = (double)dec;
                            else if (rawValue is float f) val = (double)f;
                            else if (rawValue is int i) val = i;
                            else
                            {
                                string valorTexto = rawValue?.ToString() ?? "0";
                                valorTexto = valorTexto.Replace("R$", "").Replace(" ", "").Trim();
                                if (double.TryParse(valorTexto, NumberStyles.Any, new CultureInfo("pt-BR"), out double valPt)) val = valPt;
                                else if (double.TryParse(valorTexto.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double valInv)) val = valInv;
                            }
                        }
                    }
                    catch { }

                    string desc = "Custo Adicional";
                    try
                    {
                        object? rawDesc = null;
                        foreach (var prop in tipoDoObjeto.GetProperties())
                        {
                            string pName = prop.Name.ToLower();
                            if (pName.Contains("desc") || pName.Contains("nome"))
                            {
                                rawDesc = prop.GetValue(custo);
                                break;
                            }
                        }
                        if (rawDesc != null) desc = rawDesc?.ToString() ?? desc;
                    }
                    catch { }

                    if (!string.IsNullOrEmpty(tipoCusto) && tipoCusto.IndexOf("Fixo", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        int vidaUtil = 0;
                        try
                        {
                            object? rawVidaUtil = tipoDoObjeto.GetProperty("VidaUtilEstimada")?.GetValue(custo);
                            if (rawVidaUtil != null)
                            {
                                if (rawVidaUtil is int iVida) vidaUtil = iVida;
                                else int.TryParse(rawVidaUtil.ToString(), out vidaUtil);
                            }
                        }
                        catch { }

                        double custoPorImpressao = vidaUtil > 0 ? val / vidaUtil : val;
                        somatorioCustosFixos += custoPorImpressao;
                    }
                    else
                    {
                        custosDisponiveisVisuais.Add(new CustoVisual
                        {
                            ItemOriginal = custo,
                            Descricao = desc,
                            Valor = val,
                            CheckVisivel = "Collapsed"
                        });
                    }
                }

                ListaCustosAdicionais.ItemsSource = custosDisponiveisVisuais;
                _totalCustosFixos = somatorioCustosFixos;
            }
        }

        private void ListaFilamentos_SelectionChanged(object sender, SelectionChangedEventArgs? e)
        {
            if (ListaFilamentos.SelectedItem != null)
            {
                dynamic filamentoEscolhido = ListaFilamentos.SelectedItem;
                if (BtnAbrirFilamento.Template.FindName("TxtFilamentoSelecionado", BtnAbrirFilamento) is TextBlock textoBotao)
                {
                    PropertyInfo? prop = filamentoEscolhido.GetType().GetProperty("Nome") ?? filamentoEscolhido.GetType().GetProperty("Descricao");
                    textoBotao.Text = prop != null ? prop.GetValue(filamentoEscolhido)?.ToString() : filamentoEscolhido.ToString();
                    textoBotao.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E2723"));
                }
                BtnAbrirFilamento.IsChecked = false;
            }
        }

        private void ListaImpressoras_SelectionChanged(object sender, SelectionChangedEventArgs? e)
        {
            if (ListaImpressoras.SelectedItem != null)
            {
                dynamic impressoraEscolhida = ListaImpressoras.SelectedItem;
                if (BtnAbrirImpressora.Template.FindName("TxtImpressoraSelecionada", BtnAbrirImpressora) is TextBlock textoBotao)
                {
                    PropertyInfo? prop = impressoraEscolhida.GetType().GetProperty("Nome") ?? impressoraEscolhida.GetType().GetProperty("Descricao");
                    textoBotao.Text = prop != null ? prop.GetValue(impressoraEscolhida)?.ToString() : impressoraEscolhida.ToString();
                    textoBotao.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E2723"));
                }
                BtnAbrirImpressora.IsChecked = false;
            }
        }

        private void ListaCustosAdicionais_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaCustosAdicionais.SelectedItem is CustoVisual custoEscolhido)
            {
                bool jaExiste = false;
                foreach (var c in custosSelecionados)
                {
                    if (c.Descricao == custoEscolhido.Descricao) { jaExiste = true; break; }
                }

                if (!jaExiste)
                {
                    custosSelecionados.Add(custoEscolhido);
                    custoEscolhido.CheckVisivel = "Visible";
                    AtualizarTagsVisuais();
                }
                else
                {
                    custosSelecionados.Remove(custoEscolhido);
                    custoEscolhido.CheckVisivel = "Collapsed";
                    AtualizarTagsVisuais();
                }
                ListaCustosAdicionais.SelectedItem = null;
            }
        }

        private void AtualizarTagsVisuais()
        {
            PainelTagsCustos.Children.Clear();
            for (int i = 0; i < custosSelecionados.Count; i++)
            {
                var custo = custosSelecionados[i];
                Border tagBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FCEBD3")),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(6, 2, 6, 2),
                    Margin = new Thickness(0, 0, 5, 5),
                    Cursor = Cursors.Hand
                };

                TextBlock tagText = new TextBlock
                {
                    Text = $"{custo.Descricao} x",
                    FontSize = 10,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B57218"))
                };
                tagBorder.Child = tagText;

                tagBorder.MouseLeftButtonDown += (s, args) =>
                {
                    custosSelecionados.Remove(custo);
                    custo.CheckVisivel = "Collapsed";
                    AtualizarTagsVisuais();
                };
                PainelTagsCustos.Children.Add(tagBorder);
            }
        }

        private void BtnLimpar_Click(object sender, RoutedEventArgs e)
        {
            MemoriaRascunho.LimparTudo();

            TxtNomeProduto.Text = string.Empty;
            TxtDescricao.Text = string.Empty;
            TxtPeso.Text = string.Empty;
            TxtHoras.Text = string.Empty;
            TxtMinutos.Text = string.Empty;
            TxtQuantidade.Text = "1";
            TxtFrete.Text = string.Empty;
            TxtMargem.Text = string.Empty;
            TxtCategoria.Text = string.Empty;
            TxtValorHora.Text = string.Empty;
            TxtHorasMaoDeObra.Text = string.Empty;
            TxtMinutosMaoDeObra.Text = string.Empty;

            EscalaImagem.ScaleX = 1.0;
            EscalaImagem.ScaleY = 1.0;
            PosicaoImagem.X = 0;
            PosicaoImagem.Y = 0;
            ImgProdutoPreview.Source = null;
            PainelPlaceholderImagem.Visibility = Visibility.Visible;

            if (BtnAbrirFilamento.Template.FindName("TxtFilamentoSelecionado", BtnAbrirFilamento) is TextBlock txtFilamento)
            {
                txtFilamento.Text = "Selecione o filamento";
                txtFilamento.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A99F96"));
            }
            if (BtnAbrirImpressora.Template.FindName("TxtImpressoraSelecionada", BtnAbrirImpressora) is TextBlock txtImpressora)
            {
                txtImpressora.Text = "Selecione a impressora";
                txtImpressora.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A99F96"));
            }

            foreach (var custo in custosDisponiveisVisuais) custo.CheckVisivel = "Collapsed";
            custosSelecionados.Clear();
            AtualizarTagsVisuais();

            ListaFilamentos.SelectedItem = null;
            ListaImpressoras.SelectedItem = null;
        }

        private void BtnAdicionarImagem_Click(object sender, RoutedEventArgs e)
        {
            SelecionarNovaImagem();
        }

        private void BoxImagem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ImgProdutoPreview.Source == null)
            {
                SelecionarNovaImagem();
                return;
            }

            _arrastandoImagem = true;
            _houveArraste = false;
            _pontoInicialClique = e.GetPosition(BoxImagemProduto);
            _offsetInicialX = PosicaoImagem.X;
            _offsetInicialY = PosicaoImagem.Y;
            BoxImagemProduto.CaptureMouse();
        }

        private void BoxImagem_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_arrastandoImagem) return;

            Point pontoAtual = e.GetPosition(BoxImagemProduto);
            double deltaX = pontoAtual.X - _pontoInicialClique.X;
            double deltaY = pontoAtual.Y - _pontoInicialClique.Y;

            if (Math.Abs(deltaX) > 4 || Math.Abs(deltaY) > 4)
            {
                _houveArraste = true;
                BoxImagemProduto.Cursor = Cursors.SizeAll;
                PosicaoImagem.X = _offsetInicialX + deltaX;
                PosicaoImagem.Y = _offsetInicialY + deltaY;
            }
        }

        private void BoxImagem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_arrastandoImagem) return;

            _arrastandoImagem = false;
            BoxImagemProduto.ReleaseMouseCapture();
            BoxImagemProduto.Cursor = Cursors.Hand;

            if (!_houveArraste)
            {
                SelecionarNovaImagem();
            }
        }

        private void BoxImagem_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ImgProdutoPreview.Source == null) return;

            double zoomPasso = e.Delta > 0 ? 0.1 : -0.1;
            double novaEscala = Math.Clamp(EscalaImagem.ScaleX + zoomPasso, 1.0, 3.0);

            EscalaImagem.ScaleX = novaEscala;
            EscalaImagem.ScaleY = novaEscala;

            if (novaEscala == 1.0)
            {
                PosicaoImagem.X = 0;
                PosicaoImagem.Y = 0;
            }

            e.Handled = true;
        }

        private void SelecionarNovaImagem()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Imagens (*.jpg; *.jpeg; *.png; *.webp)|*.jpg;*.jpeg;*.png;*.webp|Todos os arquivos (*.*)|*.*";
            openFileDialog.Title = "Selecione a imagem do produto 3D";

            if (openFileDialog.ShowDialog() == true)
            {
                string caminhoCompleto = openFileDialog.FileName;
                MemoriaRascunho.CaminhoImagem = caminhoCompleto;

                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(caminhoCompleto);
                    bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    EscalaImagem.ScaleX = 1.0;
                    EscalaImagem.ScaleY = 1.0;
                    PosicaoImagem.X = 0;
                    PosicaoImagem.Y = 0;

                    ImgProdutoPreview.Source = null;
                    ImgProdutoPreview.Source = bitmap;
                    PainelPlaceholderImagem.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao carregar a imagem: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnCalcular_Click(object sender, RoutedEventArgs e)
        {
            SalvarRascunho();

            var txtFilamento = BtnAbrirFilamento.Template.FindName("TxtFilamentoSelecionado", BtnAbrirFilamento) as TextBlock;
            var txtImpressora = BtnAbrirImpressora.Template.FindName("TxtImpressoraSelecionada", BtnAbrirImpressora) as TextBlock;

            if (string.IsNullOrWhiteSpace(TxtNomeProduto.Text) ||
                string.IsNullOrWhiteSpace(TxtPeso.Text) ||
                (string.IsNullOrWhiteSpace(TxtHoras.Text) && string.IsNullOrWhiteSpace(TxtMinutos.Text)) ||
                string.IsNullOrWhiteSpace(TxtQuantidade.Text) ||
                string.IsNullOrWhiteSpace(TxtMargem.Text) ||
                txtFilamento?.Text == "Selecione o filamento" ||
                txtImpressora?.Text == "Selecione a impressora")
            {
                ModalValidacao.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                dynamic filamentoEscolhido = ListaFilamentos.SelectedItem;
                double precoKgFilamento = 100.0;
                MemoriaRascunho.NomeImpressoraVisual = txtImpressora?.Text ?? "Impressora 3D";

                string nomePrincipal = "PLA";
                string nomeDaCor = "Padrão";
                string hexExato = "#000000";
                Brush brushDaCor = Brushes.Black;

                if (filamentoEscolhido != null)
                {
                    object? rawPreco = ObterValorDeObjeto(filamentoEscolhido, "preco", "valor");
                    if (rawPreco != null) precoKgFilamento = ConverterParaDouble(rawPreco);

                    object? rawTipo = ObterValorDeObjeto(filamentoEscolhido, "tipo", "categoria");
                    if (rawTipo != null && !string.IsNullOrWhiteSpace(rawTipo.ToString()))
                    {
                        nomePrincipal = rawTipo.ToString()!.Trim();
                    }
                    else
                    {
                        string[] partes = (txtFilamento?.Text ?? "PLA").Split(' ');
                        nomePrincipal = partes[0];
                    }

                    object? rawNome = ObterValorDeObjeto(filamentoEscolhido, "nome", "descricao");
                    string nomeCompleto = rawNome?.ToString() ?? txtFilamento?.Text ?? "";

                    if (nomeCompleto.StartsWith(nomePrincipal, StringComparison.OrdinalIgnoreCase))
                    {
                        nomeDaCor = nomeCompleto.Substring(nomePrincipal.Length).Trim();
                        if (nomeDaCor.StartsWith("-")) nomeDaCor = nomeDaCor.Substring(1).Trim();
                    }
                    else
                    {
                        nomeDaCor = nomeCompleto;
                    }

                    if (string.IsNullOrWhiteSpace(nomeDaCor)) nomeDaCor = "Padrão";

                    object? rawHex = ObterValorDeObjeto(filamentoEscolhido, "hex", "codigo", "corhex", "cor");
                    object? rawBrush = ObterValorDeObjeto(filamentoEscolhido, "corbrush", "brush");

                    if (rawHex != null)
                    {
                        string txtCor = rawHex?.ToString()?.Trim() ?? "";
                        if (txtCor.StartsWith("#") || txtCor.Length == 6 || txtCor.Length == 8)
                        {
                            hexExato = txtCor.StartsWith("#") ? txtCor : "#" + txtCor;
                        }
                    }
                    else if (rawBrush is SolidColorBrush sb)
                    {
                        hexExato = $"#{sb.Color.R:X2}{sb.Color.G:X2}{sb.Color.B:X2}";
                    }

                    try { brushDaCor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexExato)); } catch { }
                }

                MemoriaRascunho.CodigoHexVisual = hexExato;
                MemoriaRascunho.CorFilamentoVisualBrush = brushDaCor;
                MemoriaRascunho.NomeFilamentoVisual = nomePrincipal;
                MemoriaRascunho.NomeCorVisual = nomeDaCor;

                dynamic impressoraEscolhida = ListaImpressoras.SelectedItem;
                double potenciaWatts = 350.0;
                double custoKwh = 0.85;

                if (impressoraEscolhida != null)
                {
                    object? rawPotencia = ObterValorDeObjeto(impressoraEscolhida, "potencia", "consumo", "watts");
                    if (rawPotencia != null) potenciaWatts = ConverterParaDouble(rawPotencia);

                    object? rawKwh = ObterValorDeObjeto(impressoraEscolhida, "kwh", "custokwh");
                    if (rawKwh != null) custoKwh = ConverterParaDouble(rawKwh);

                    MemoriaRascunho.ModeloImpressoraVisual = ObterValorDeObjeto(impressoraEscolhida, "modelo", "detalhe")?.ToString() ?? "Principal";
                }

                double.TryParse(TxtPeso.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double pesoGramas);
                double.TryParse(TxtHoras.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double horas);
                double.TryParse(TxtMinutos.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double minutos);
                int.TryParse(TxtQuantidade.Text, out int quantidade);
                if (quantidade <= 0) quantidade = 1;
                double.TryParse(TxtMargem.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double margemLucro);

                string valorFrete = string.IsNullOrWhiteSpace(TxtFrete.Text) ? "0,00" : TxtFrete.Text;

                double.TryParse(TxtValorHora.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double valorHoraTrabalho);
                double.TryParse(TxtHorasMaoDeObra.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double horasMao);
                double.TryParse(TxtMinutosMaoDeObra.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double minutosMao);

                double tempoImpressaoHoras = horas + (minutos / 60.0);
                double tempoMaoDeObraHoras = horasMao + (minutosMao / 60.0);

                double custoFilamentoUnitario = (precoKgFilamento * pesoGramas) / 1000.0;
                double custoEnergiaPorHora = (potenciaWatts / 1000.0) * custoKwh;
                double custoEnergiaUnitario = custoEnergiaPorHora * tempoImpressaoHoras;
                double custoMaoDeObraUnitario = valorHoraTrabalho * tempoMaoDeObraHoras;

                double custosAdicionaisUnitarios = 0;
                foreach (var c in custosSelecionados) custosAdicionaisUnitarios += c.Valor;

                double custoUnitarioTotal = custoFilamentoUnitario + custoEnergiaUnitario + custoMaoDeObraUnitario + custosAdicionaisUnitarios + _totalCustosFixos;
                double custoTotalLote = custoUnitarioTotal * quantidade;
                double precoVendaUnitario = custoUnitarioTotal * (1 + (margemLucro / 100.0));

                string tempoMaoDisplay = $"{TxtHorasMaoDeObra.Text}h {TxtMinutosMaoDeObra.Text}min".Trim();
                if (tempoMaoDisplay.StartsWith("h")) tempoMaoDisplay = $"{TxtMinutosMaoDeObra.Text}min";
                if (tempoMaoDisplay.EndsWith("min") && string.IsNullOrWhiteSpace(TxtMinutosMaoDeObra.Text)) tempoMaoDisplay = $"{TxtHorasMaoDeObra.Text}h";
                if (string.IsNullOrWhiteSpace(tempoMaoDisplay) || tempoMaoDisplay == "h") tempoMaoDisplay = "0 min";

                string tempoImpressaoDisplay = $"{TxtHoras.Text}h {TxtMinutos.Text}min".Trim();
                if (tempoImpressaoDisplay.StartsWith("h")) tempoImpressaoDisplay = $"{TxtMinutos.Text}min";
                if (tempoImpressaoDisplay.EndsWith("min") && string.IsNullOrWhiteSpace(TxtMinutos.Text)) tempoImpressaoDisplay = $"{TxtHoras.Text}h";

                string? caminhoImagemLimpo = string.IsNullOrWhiteSpace(MemoriaRascunho.CaminhoImagem) ? null : MemoriaRascunho.CaminhoImagem;

                var resultado = new ResultadoCalculoModel
                {
                    ProdutoNome = string.IsNullOrWhiteSpace(TxtNomeProduto.Text) ? "Peça 3D" : TxtNomeProduto.Text,
                    ProdutoCategoria = string.IsNullOrWhiteSpace(TxtCategoria.Text) ? "Sem categoria" : TxtCategoria.Text,
                    ProdutoDescricao = string.IsNullOrWhiteSpace(TxtDescricao.Text) ? "Sem descrição" : TxtDescricao.Text,
                    CustoTotalProducao = $"R$ {custoUnitarioTotal:N2}",
                    PrecoVendaSugerido = $"R$ {precoVendaUnitario:N2}",
                    CaminhoImagemProduto = caminhoImagemLimpo ?? string.Empty,
                };

                resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeFilamentoForm.png", Rotulo = "Filamento", Valor = txtFilamento?.Text ?? "" });
                resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeImpressoraForm.png", Rotulo = "Tipo de impressora", Valor = txtImpressora?.Text ?? "" });
                resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconePeso.png", Rotulo = "Peso da peça", Valor = $"{pesoGramas} g" });
                resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeTempo.png", Rotulo = "Tempo de impressão", Valor = tempoImpressaoDisplay });
                resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeTempo.png", Rotulo = "Tempo de mão de obra", Valor = tempoMaoDisplay });
                resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeCustoFixo.png", Rotulo = "Mão de obra (R$/h)", Valor = $"R$ {valorHoraTrabalho:N2}" });
                resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeQuantidade.png", Rotulo = "Quantidade", Valor = $"{quantidade} unidade(s)" });
                resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeFrete.png", Rotulo = "Frete", Valor = $"R$ {valorFrete}" });
                resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeCustoAdicional.png", Rotulo = "Custo adicional (R$)", Valor = $"R$ {custosAdicionaisUnitarios:N2}" });
                resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeCustoFixo.png", Rotulo = "Custos fixos (R$)", Valor = $"R$ {_totalCustosFixos:N2}" });
                resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeMargem.png", Rotulo = "Margem de lucro (%)", Valor = $"{margemLucro}%" });

                resultado.CustosDetalhados.Add(new CustoInfo { Icone = "/Assets/Images/IconeFilamentoForm.png", Titulo = "Material (filamento)", Descricao = "Custo do filamento utilizado", ValorFormatado = $"R$ {custoFilamentoUnitario:N2}" });
                resultado.CustosDetalhados.Add(new CustoInfo
                {
                    Icone = "/Assets/Images/IconeEnergiaEletrica.png",
                    Titulo = "Energia elétrica",
                    Descricao = "Custo estimado de energia",
                    ValorFormatado = $"R$ {custoEnergiaUnitario:N2}",
                    TamanhoIcone = 24
                });

                if (custoMaoDeObraUnitario > 0)
                {
                    resultado.CustosDetalhados.Add(new CustoInfo { Icone = "/Assets/Images/IconeMaoObra.png", Titulo = "Mão de obra", Descricao = "Tempo de preparo e acabamento", ValorFormatado = $"R$ {custoMaoDeObraUnitario:N2}" });
                }

                if (_totalCustosFixos > 0)
                {
                    double custoFixoUnitario = _totalCustosFixos / Math.Max(1, quantidade);
                    resultado.CustosDetalhados.Add(new CustoInfo { Icone = "/Assets/Images/IconeCustoFixo.png", Titulo = "Custos fixos", Descricao = "Rateio de custos fixos mensais", ValorFormatado = $"R$ {custoFixoUnitario:N2}" });
                }

                resultado.CustosDetalhados.Add(new CustoInfo { Icone = "/Assets/Images/IconeCustoAdicional.png", Titulo = "Outros custos", Descricao = "Embalagens, insumos, etc.", ValorFormatado = $"R$ {custosAdicionaisUnitarios:N2}" });
                MemoriaRascunho.CalculoPendente = true;
                MemoriaRascunho.ResultadoPendente = resultado;

                var telaResumo = new ResumoCalculadoraView(resultado);
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
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao calcular: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnFecharValidacao_Click(object sender, RoutedEventArgs e)
        {
            ModalValidacao.Visibility = Visibility.Collapsed;
        }

        private object? ObterValorDeObjeto(object objeto, params string[] termosBuscados)
        {
            Type tipo = objeto.GetType();
            foreach (var prop in tipo.GetProperties())
            {
                string nomeProp = prop.Name.ToLower();
                foreach (var termo in termosBuscados)
                {
                    if (nomeProp.Contains(termo.ToLower()))
                        return prop.GetValue(objeto);
                }
            }
            return null;
        }

        private double ConverterParaDouble(object? valorBruto)
        {
            if (valorBruto == null) return 0;
            if (valorBruto is double d) return d;
            if (valorBruto is decimal dec) return (double)dec;
            if (valorBruto is float f) return (double)f;
            if (valorBruto is int i) return i;

            string texto = valorBruto?.ToString() ?? "0";
            texto = texto.Replace("R$", "").Replace(" ", "").Trim();

            if (double.TryParse(texto, NumberStyles.Any, new CultureInfo("pt-BR"), out double valPt)) return valPt;
            if (double.TryParse(texto.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double valInv)) return valInv;

            return 0;
        }
    }
}