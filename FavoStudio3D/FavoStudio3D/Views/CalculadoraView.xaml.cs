using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Globalization;

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

            // Exemplo de como você vai usar o valor do backend na hora do cálculo real:
            // double somaDosFixos = _totalCustosFixos;

            MessageBox.Show("Cálculo efetuado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnFecharValidacao_Click(object sender, RoutedEventArgs e)
        {
            ModalValidacao.Visibility = Visibility.Collapsed;
        }
    }
}