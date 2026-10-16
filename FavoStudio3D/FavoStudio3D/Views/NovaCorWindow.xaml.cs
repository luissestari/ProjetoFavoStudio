using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FavoStudio3D.Views
{
    public partial class NovaCorWindow : Window
    {
        // O "?" diz ao C# que é normal começar vazio
        public CorFilamento? NovaCor { get; private set; }

        public NovaCorWindow()
        {
            InitializeComponent();
        }

        private void TxtHex_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var conversor = new BrushConverter();
                // O "as Brush" resolve o aviso de conversão nula
                var corPintada = conversor.ConvertFromString(TxtHex.Text.Trim()) as Brush;

                if (corPintada != null)
                {
                    PreviewCor.Fill = corPintada;
                }
                else
                {
                    PreviewCor.Fill = Brushes.Transparent;
                }
            }
            catch
            {
                PreviewCor.Fill = Brushes.Transparent;
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnAdicionar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNomeCor.Text) || string.IsNullOrWhiteSpace(TxtHex.Text))
            {
                MessageBox.Show("Por favor, preencha o nome e o código da cor.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NovaCor = new CorFilamento
            {
                Nome = TxtNomeCor.Text.Trim(),
                Hex = TxtHex.Text.Trim(),
                Borda = "#E0E0E0"
            };

            DialogResult = true;
            Close();
        }
    }
}