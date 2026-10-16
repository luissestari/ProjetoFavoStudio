using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace FavoStudio3D.Views
{
    public partial class CadastroCustosView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // PASSO 3: Inicializada com '= new ObservableCollection<CorFilamento>();' para remover o aviso CS8618
        public ObservableCollection<CorFilamento> ListaCores { get; set; } = new ObservableCollection<CorFilamento>();

        // PASSO 3: Inicializada com '= new CorFilamento();' para remover o aviso CS8618
        private CorFilamento _corSelecionada = new CorFilamento();

        public CorFilamento CorSelecionada
        {
            get { return _corSelecionada; }
            set
            {
                _corSelecionada = value;
                OnPropertyChanged(nameof(CorSelecionada));
            }
        }

        // --- PROPRIEDADES PARA A OPÇÃO 2 (Expansão do Popup) ---

        private Visibility _botaoOutraCorVisivel = Visibility.Visible;
        public Visibility BotaoOutraCorVisivel
        {
            get { return _botaoOutraCorVisivel; }
            set { _botaoOutraCorVisivel = value; OnPropertyChanged(nameof(BotaoOutraCorVisivel)); }
        }

        private Visibility _painelNovaCorVisivel = Visibility.Collapsed;
        public Visibility PainelNovaCorVisivel
        {
            get { return _painelNovaCorVisivel; }
            set { _painelNovaCorVisivel = value; OnPropertyChanged(nameof(PainelNovaCorVisivel)); }
        }

        private string _novaCorNome = string.Empty;
        public string NovaCorNome
        {
            get { return _novaCorNome; }
            set { _novaCorNome = value; OnPropertyChanged(nameof(NovaCorNome)); }
        }

        private string _novaCorHex = string.Empty;
        public string NovaCorHex
        {
            get { return _novaCorHex; }
            set { _novaCorHex = value; OnPropertyChanged(nameof(NovaCorHex)); }
        }

        public CadastroCustosView()
        {
            InitializeComponent();
            DataContext = this;
            CarregarCores();
        }

        private void CarregarCores()
        {
            ListaCores = new ObservableCollection<CorFilamento>
            {
                new CorFilamento { Nome = "Branco", Hex = "#FFFFFF", Borda = "#E0E0E0" },
                new CorFilamento { Nome = "Preto", Hex = "#212121", Borda = "Transparent" },
                new CorFilamento { Nome = "Cinza", Hex = "#9E9E9E", Borda = "Transparent" },
                new CorFilamento { Nome = "Vermelho", Hex = "#F44336", Borda = "Transparent" },
                new CorFilamento { Nome = "Azul", Hex = "#2196F3", Borda = "Transparent" },
                new CorFilamento { Nome = "Verde", Hex = "#4CAF50", Borda = "Transparent" },
                new CorFilamento { Nome = "Amarelo", Hex = "#FFEB3B", Borda = "Transparent" },
                new CorFilamento { Nome = "Laranja", Hex = "#FF9800", Borda = "Transparent" },
                new CorFilamento { Nome = "Roxo", Hex = "#9C27B0", Borda = "Transparent" },
                new CorFilamento { Nome = "Rosa", Hex = "#F48FB1", Borda = "Transparent" }
            };

            CorSelecionada = ListaCores[0];
        }

        // Clica em uma cor existente
        private void CorSelecionada_Click(object sender, RoutedEventArgs e)
        {
            var botao = sender as Button;
            if (botao?.DataContext is CorFilamento corClicada)
            {
                CorSelecionada = corClicada;
                BtnAbrirCores.IsChecked = false; // Fecha o popup
                ResetarPainelExpansao();         // Recolhe os campos extras para a próxima vez
            }
        }

        // --- LÓGICA DE EVENTOS DA OPÇÃO 2 ---

        private void BtnCorPersonalizada_Click(object sender, RoutedEventArgs e)
        {
            // Esconde o botão e revela os campos para digitar a nova cor
            BotaoOutraCorVisivel = Visibility.Collapsed;
            PainelNovaCorVisivel = Visibility.Visible;
        }

        private void BtnCancelarNovaCor_Click(object sender, RoutedEventArgs e)
        {
            // Recolhe os campos e volta ao normal
            ResetarPainelExpansao();
        }

        private void BtnSalvarNovaCor_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NovaCorNome) || string.IsNullOrWhiteSpace(NovaCorHex))
            {
                MessageBox.Show("Preencha o nome e o código HEX.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1. Prepara e corrige o código Hexadecimal
            string hexDigitado = NovaCorHex.Trim();

            // Adiciona o '#' automaticamente se o utilizador esquecer de colocar
            if (!hexDigitado.StartsWith("#"))
            {
                hexDigitado = "#" + hexDigitado;
            }

            // 2. Valida se o código HEX é uma cor real que o sistema consiga ler
            try
            {
                var conversor = new System.Windows.Media.BrushConverter();
                conversor.ConvertFromString(hexDigitado); // Faz o teste de conversão
            }
            catch
            {
                MessageBox.Show("Código HEX inválido!\nUse um formato correto, ex: #FFD700 para Dourado.", "Erro de Cor", MessageBoxButton.OK, MessageBoxImage.Error);
                return; // Impede que o popup feche para o utilizador corrigir
            }

            // 3. Cria a cor, adiciona na lista e seleciona automaticamente
            var novaCor = new CorFilamento
            {
                Nome = NovaCorNome.Trim(),
                Hex = hexDigitado,
                // Coloca borda cinza só se a cor for branca para não sumir no fundo, senão deixa transparente
                Borda = (hexDigitado.ToUpper() == "#FFFFFF" || hexDigitado.ToUpper() == "#FFF") ? "#E0E0E0" : "Transparent"
            };

            ListaCores.Add(novaCor);
            CorSelecionada = novaCor;

            BtnAbrirCores.IsChecked = false; // Fecha o popup
            ResetarPainelExpansao();
        }

        private void ResetarPainelExpansao()
        {
            NovaCorNome = string.Empty;
            NovaCorHex = string.Empty;
            PainelNovaCorVisivel = Visibility.Collapsed;
            BotaoOutraCorVisivel = Visibility.Visible;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CorFilamento
    {
        public string Nome { get; set; } = string.Empty;
        public string Hex { get; set; } = string.Empty;
        public string Borda { get; set; } = string.Empty;
    }
}