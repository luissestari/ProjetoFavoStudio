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

// Substitua pelo seu namespace correto caso necessário
using FavoStudio3D.Models;

namespace FavoStudio3D.Views
{
    public partial class CalculadoraView : UserControl
    {
        // === MEMÓRIA ESTÁTICA PARA RASCUNHO (Impede perda de dados ao trocar de tela) ===
        public static class MemoriaRascunho
        {
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
        }

        // VARIÁVEL DE BACKEND: Guarda a soma real dos custos fixos
        private double _totalCustosFixos = 0;

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

            // Salva o rascunho automaticamente antes da tela ser fechada ou trocada
            this.Unloaded += (s, e) => SalvarRascunho();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CarregarDadosCadastrados();
            RestaurarRascunho(); // Preenche a tela com os dados salvos no rascunho
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

            if (MemoriaRascunho.FilamentoSelecionado != null)
            {
                ListaFilamentos.SelectedItem = MemoriaRascunho.FilamentoSelecionado;
                ListaFilamentos_SelectionChanged(null, null); // Força atualização visual
            }

            if (MemoriaRascunho.ImpressoraSelecionada != null)
            {
                ListaImpressoras.SelectedItem = MemoriaRascunho.ImpressoraSelecionada;
                ListaImpressoras_SelectionChanged(null, null); // Força atualização visual
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
            if (ResumoCustosMateriaisView.FilamentosBancoDados != null)
            {
                ListaFilamentos.ItemsSource = ResumoCustosMateriaisView.FilamentosBancoDados;
            }

            if (ResumoImpressorasView.ImpressorasBancoDados != null)
            {
                ListaImpressoras.ItemsSource = ResumoImpressorasView.ImpressorasBancoDados;
            }

            if (ResumoCustosMateriaisView.CustosBancoDados != null)
            {
                custosDisponiveisVisuais.Clear();
                double somatorioCustosFixos = 0;

                foreach (dynamic custo in ResumoCustosMateriaisView.CustosBancoDados)
                {
                    string tipoCusto = "";
                    Type tipoDoObjeto = custo.GetType();

                    PropertyInfo? propInfo = tipoDoObjeto.GetProperty("Tipo") ??
                                             tipoDoObjeto.GetProperty("TipoCusto") ??
                                             tipoDoObjeto.GetProperty("Categoria");

                    if (propInfo != null)
                    {
                        var valorEncontrado = propInfo.GetValue(custo);
                        tipoCusto = valorEncontrado != null ? valorEncontrado.ToString() : "";
                    }

                    // LEITURA BLINDADA DO VALOR
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
                                string valorTexto = rawValue.ToString() ?? "0";
                                valorTexto = valorTexto.Replace("R$", "").Replace(" ", "").Trim();

                                if (double.TryParse(valorTexto, NumberStyles.Any, new CultureInfo("pt-BR"), out double valPt))
                                    val = valPt;
                                else if (double.TryParse(valorTexto.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double valInv))
                                    val = valInv;
                            }
                        }
                    }
                    catch { }

                    // LEITURA BLINDADA DA DESCRIÇÃO
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
                        if (rawDesc != null) desc = rawDesc.ToString() ?? desc;
                    }
                    catch { }

                    // Filtra os custos Fixos e Adicionais
                    if (!string.IsNullOrEmpty(tipoCusto) && tipoCusto.IndexOf("Fixo", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        somatorioCustosFixos += val;
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
                    if (c.Descricao == custoEscolhido.Descricao)
                    {
                        jaExiste = true;
                        break;
                    }
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
            // Zera a memória de rascunho
            MemoriaRascunho.Nome = "";
            MemoriaRascunho.Descricao = "";
            MemoriaRascunho.Peso = "";
            MemoriaRascunho.Horas = "";
            MemoriaRascunho.Minutos = "";
            MemoriaRascunho.Quantidade = "1";
            MemoriaRascunho.Frete = "";
            MemoriaRascunho.Margem = "";
            MemoriaRascunho.FilamentoSelecionado = null;
            MemoriaRascunho.ImpressoraSelecionada = null;
            MemoriaRascunho.CustosSelecionadosDescricoes.Clear();

            // Limpa as caixas de texto visualmente
            TxtNomeProduto.Text = string.Empty;
            TxtDescricao.Text = string.Empty;
            TxtPeso.Text = string.Empty;
            TxtHoras.Text = string.Empty;
            TxtMinutos.Text = string.Empty;
            TxtQuantidade.Text = "1";
            TxtFrete.Text = string.Empty;
            TxtMargem.Text = string.Empty;

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

            foreach (var custo in custosDisponiveisVisuais)
            {
                custo.CheckVisivel = "Collapsed";
            }
            custosSelecionados.Clear();
            AtualizarTagsVisuais();

            ListaFilamentos.SelectedItem = null;
            ListaImpressoras.SelectedItem = null;
        }

        private void BtnAdicionarImagem_Click(object sender, RoutedEventArgs e)
        {
            // Cria a janela de diálogo para escolher o arquivo
            OpenFileDialog openFileDialog = new OpenFileDialog();

            // Filtra apenas para os formatos de imagem aceitos
            openFileDialog.Filter = "Imagens (*.jpg; *.jpeg; *.png; *.webp)|*.jpg;*.jpeg;*.png;*.webp|Todos os arquivos (*.*)|*.*";
            openFileDialog.Title = "Selecione a imagem do produto 3D";

            // Se o usuário selecionar uma imagem e clicar em "Abrir"
            if (openFileDialog.ShowDialog() == true)
            {
                string caminhoCompleto = openFileDialog.FileName;

                // Opcional: Se você tiver um controle Image na tela chamado 'ImgProduto' para mostrar a pré-visualização:
                // ImgProduto.Source = new BitmapImage(new Uri(caminhoCompleto));

                // Aqui você também pode guardar o caminho 'caminhoCompleto' numa variável 
                // ou no modelo do produto para salvar no banco/histórico depois!
                MessageBox.Show($"Imagem selecionada com sucesso:\n{caminhoCompleto}", "Arquivo Carregado", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnCalcular_Click(object sender, RoutedEventArgs e)
        {
            // Salva as informações logo antes de calcular
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
                // 1. Obter dados do Filamento selecionado
                dynamic filamentoEscolhido = ListaFilamentos.SelectedItem;
                double precoKgFilamento = 100.0;
                if (filamentoEscolhido != null)
                {
                    object? rawPreco = ObterValorDeObjeto(filamentoEscolhido, "preco", "valor");
                    if (rawPreco != null) precoKgFilamento = ConverterParaDouble(rawPreco);
                }

                // 2. Obter dados da Impressora selecionada
                dynamic impressoraEscolhida = ListaImpressoras.SelectedItem;
                double potenciaWatts = 350.0;
                double custoKwh = 0.85;
                if (impressoraEscolhida != null)
                {
                    object? rawPotencia = ObterValorDeObjeto(impressoraEscolhida, "potencia", "consumo", "watts");
                    if (rawPotencia != null) potenciaWatts = ConverterParaDouble(rawPotencia);

                    object? rawKwh = ObterValorDeObjeto(impressoraEscolhida, "kwh", "custokwh");
                    if (rawKwh != null) custoKwh = ConverterParaDouble(rawKwh);
                }

                // 3. Ler entradas do usuário com segurança
                double.TryParse(TxtPeso.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double pesoGramas);
                double.TryParse(TxtHoras.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double horas);
                double.TryParse(TxtMinutos.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double minutos);
                int.TryParse(TxtQuantidade.Text, out int quantidade);
                if (quantidade <= 0) quantidade = 1;
                double.TryParse(TxtMargem.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double margemLucro);

                // Tempo total de impressão em horas
                double tempoTotalHoras = horas + (minutos / 60.0);

                // 4. Seguindo estritamente a lógica da planilha:
                // Custo do Filamento por peça = (Preço do Kg * Peso em gramas) / 1000
                double custoFilamentoUnitario = (precoKgFilamento * pesoGramas) / 1000.0;

                // Custo de Energia por peça = (Potência em kW * Custo do kWh) * Horas de impressão
                double custoEnergiaPorHora = (potenciaWatts / 1000.0) * custoKwh;
                double custoEnergiaUnitario = custoEnergiaPorHora * tempoTotalHoras;

                // Soma dos custos adicionais / frete selecionados
                double custosAdicionaisUnitarios = 0;
                foreach (var c in custosSelecionados)
                {
                    custosAdicionaisUnitarios += c.Valor;
                }

                // Custo de Produção por Unidade (Base fixa da peça)
                double custoUnitarioTotal = custoFilamentoUnitario + custoEnergiaUnitario + custosAdicionaisUnitarios;

                // Preço de Venda Sugerido por Unidade
                double precoVendaUnitario = custoUnitarioTotal * (1 + (margemLucro / 100.0));

                // Multiplicando pelo total da Quantidade do lote (E3 da planilha)
                double custoTotalLote = custoUnitarioTotal * quantidade;
                double valorTotalLote = precoVendaUnitario * quantidade;

                // 5. Exibir resultado detalhado
                string mensagemResultado = $"Produto: {TxtNomeProduto.Text}\n" +
                                           $"Quantidade: {quantidade} un.\n" +
                                           $"Custo de Produção (Unit.): R$ {custoUnitarioTotal:N2}\n" +
                                           $"Preço de Venda (Unit.): R$ {precoVendaUnitario:N2}\n" +
                                           $"Valor Total do Lote: R$ {valorTotalLote:N2}";

            resultado.CustosDetalhados.Add(new CustoInfo { Icone = "/Assets/Images/IconeFilamentoForm.png", Titulo = "Material (filamento)", Descricao = "Custo do filamento utilizado", ValorFormatado = "R$ 15,50" });
            resultado.CustosDetalhados.Add(new CustoInfo { Icone = "/Assets/Images/IconeTempo.png", Titulo = "Energia elétrica", Descricao = "Custo estimado de energia", ValorFormatado = "R$ 3,20" });
            resultado.CustosDetalhados.Add(new CustoInfo { Icone = "/Assets/Images/IconeCustoFixo.png", Titulo = "Mão de obra", Descricao = "Tempo de preparo e acabamento", ValorFormatado = "R$ 5,00" });
            resultado.CustosDetalhados.Add(new CustoInfo { Icone = "/Assets/Images/IconeCustoAdicional.png", Titulo = "Outros custos", Descricao = "Embalagens, insumos, etc.", ValorFormatado = "R$ 2,40" });

            // Navegação
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

        private void BtnFecharValidacao_Click(object sender, RoutedEventArgs e)
        {
            ModalValidacao.Visibility = Visibility.Collapsed;
        }

        private void BoxImagem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Imagens (*.jpg; *.jpeg; *.png; *.webp)|*.jpg;*.jpeg;*.png;*.webp|Todos os arquivos (*.*)|*.*";
            openFileDialog.Title = "Selecione a imagem do produto 3D";

            if (openFileDialog.ShowDialog() == true)
            {
                string caminhoCompleto = openFileDialog.FileName;

                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(caminhoCompleto);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    ImgProdutoPreview.Source = bitmap;

                    if (PainelPlaceholderImagem != null)
                    {
                        PainelPlaceholderImagem.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao carregar a imagem: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}