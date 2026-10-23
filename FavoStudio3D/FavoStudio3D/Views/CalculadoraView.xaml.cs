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

            // Mapeamento dinâmico
            string tempoDisplay = $"{TxtHoras.Text}h {TxtMinutos.Text}min".Trim();
            if (tempoDisplay.StartsWith("h")) tempoDisplay = $"{TxtMinutos.Text}min";
            if (tempoDisplay.EndsWith("min") && string.IsNullOrWhiteSpace(TxtMinutos.Text)) tempoDisplay = $"{TxtHoras.Text}h";

            string valorFrete = string.IsNullOrWhiteSpace(TxtFrete.Text) ? "0,00" : TxtFrete.Text;

            var resultado = new ResultadoCalculoModel
            {
                ProdutoNome = TxtNomeProduto.Text,
                ProdutoDescricao = string.IsNullOrWhiteSpace(TxtDescricao.Text) ? "Sem descrição" : TxtDescricao.Text,
                CustoTotalProducao = "R$ 26,10",
                PrecoVendaSugerido = "R$ 39,15"
            };

            resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeFilamentoForm.png", Rotulo = "Filamento", Valor = txtFilamento?.Text ?? "" });
            resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeImpressoraForm.png", Rotulo = "Tipo de impressora", Valor = txtImpressora?.Text ?? "" });
            resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconePeso.png", Rotulo = "Peso da peça", Valor = $"{TxtPeso.Text} g" });
            resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeTempo.png", Rotulo = "Tempo de impressão", Valor = tempoDisplay });
            resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeQuantidade.png", Rotulo = "Quantidade", Valor = $"{TxtQuantidade.Text} unidade(s)" });
            resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeFrete.png", Rotulo = "Frete", Valor = $"R$ {valorFrete}" });

            double somaAdicionais = 0;
            foreach (var c in custosSelecionados) somaAdicionais += c.Valor;
            resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeCustoAdicional.png", Rotulo = "Custo adicional (R$)", Valor = $"R$ {somaAdicionais:N2}" });

            resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeCustoFixo.png", Rotulo = "Custos fixos (R$)", Valor = $"R$ {_totalCustosFixos:N2}" });
            resultado.Parametros.Add(new ParametroInfo { Icone = "/Assets/Images/IconeMargem.png", Rotulo = "Margem de lucro (%)", Valor = $"{TxtMargem.Text}%" });

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
    }
}