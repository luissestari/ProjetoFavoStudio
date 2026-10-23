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
        // VARIÁVEL DE BACKEND: Guarda a soma real dos custos fixos para usar no cálculo sem mostrar na UI
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
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            CarregarDadosCadastrados();
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

                    // LEITURA BLINDADA DO VALOR: Procura ativamente por qualquer campo numérico
                    double val = 0;
                    try
                    {
                        object? rawValue = null;

                        // 1. Procura nas Propriedades (get; set;)
                        foreach (var prop in tipoDoObjeto.GetProperties())
                        {
                            string pName = prop.Name.ToLower();
                            if (pName.Contains("valor") || pName.Contains("preco") || pName.Contains("custo"))
                            {
                                rawValue = prop.GetValue(custo);
                                break;
                            }
                        }

                        // 2. Procura nos Campos comuns (caso não tenha get; set;)
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

                        // 3. Força a conversão matemática independente do formato de origem
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

                                // Tenta ler como PT-BR (12,50) ou Universal (12.50)
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

                // GUARDA O VALOR PARA CÁLCULO, MAS MANTÉM A UI INTACTA ("Calculado automaticamente")
                _totalCustosFixos = somatorioCustosFixos;
            }
        }

        private void ListaFilamentos_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void ListaImpressoras_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void ListaAcabamento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListaAcabamento.SelectedItem is ListBoxItem itemEscolhido && itemEscolhido.Content != null)
            {
                if (BtnAbrirAcabamento.Template.FindName("TxtAcabamentoSelecionado", BtnAbrirAcabamento) is TextBlock textoBotao)
                {
                    textoBotao.Text = itemEscolhido.Content.ToString();
                    textoBotao.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3E2723"));
                }
                BtnAbrirAcabamento.IsChecked = false;
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
            TxtNomeProduto.Text = string.Empty;
            TxtDescricao.Text = string.Empty;
            TxtPeso.Text = string.Empty;
            TxtHoras.Text = string.Empty;
            TxtMinutos.Text = string.Empty;
            TxtQuantidade.Text = "1";
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
            if (BtnAbrirAcabamento.Template.FindName("TxtAcabamentoSelecionado", BtnAbrirAcabamento) is TextBlock txtAcabamento)
            {
                txtAcabamento.Text = "Selecione o acabamento";
                txtAcabamento.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A99F96"));
            }

            foreach (var custo in custosDisponiveisVisuais)
            {
                custo.CheckVisivel = "Collapsed";
            }
            custosSelecionados.Clear();
            AtualizarTagsVisuais();

            ListaFilamentos.SelectedItem = null;
            ListaImpressoras.SelectedItem = null;
            ListaAcabamento.SelectedItem = null;
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
            var txtFilamento = BtnAbrirFilamento.Template.FindName("TxtFilamentoSelecionado", BtnAbrirFilamento) as TextBlock;
            var txtImpressora = BtnAbrirImpressora.Template.FindName("TxtImpressoraSelecionada", BtnAbrirImpressora) as TextBlock;
            var txtAcabamento = BtnAbrirAcabamento.Template.FindName("TxtAcabamentoSelecionado", BtnAbrirAcabamento) as TextBlock;

            if (string.IsNullOrWhiteSpace(TxtNomeProduto.Text) ||
                string.IsNullOrWhiteSpace(TxtPeso.Text) ||
                (string.IsNullOrWhiteSpace(TxtHoras.Text) && string.IsNullOrWhiteSpace(TxtMinutos.Text)) ||
                string.IsNullOrWhiteSpace(TxtQuantidade.Text) ||
                string.IsNullOrWhiteSpace(TxtMargem.Text) ||
                txtFilamento?.Text == "Selecione o filamento" ||
                txtImpressora?.Text == "Selecione a impressora" ||
                txtAcabamento?.Text == "Selecione o acabamento")
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

                MessageBox.Show(mensagemResultado, "Cálculo Efetuado com Sucesso! 🚀", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao realizar o cálculo: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Métodos auxiliares de apoio para leitura dinâmica
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

        private double ConverterParaDouble(object valorBruto)
        {
            if (valorBruto is double d) return d;
            if (valorBruto is decimal dec) return (double)dec;
            if (valorBruto is float f) return (double)f;
            if (valorBruto is int i) return i;

            string texto = valorBruto.ToString() ?? "0";
            texto = texto.Replace("R$", "").Replace(" ", "").Trim();

            if (double.TryParse(texto, NumberStyles.Any, new CultureInfo("pt-BR"), out double valPt))
                return valPt;
            if (double.TryParse(texto.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double valInv))
                return valInv;

            return 0;
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