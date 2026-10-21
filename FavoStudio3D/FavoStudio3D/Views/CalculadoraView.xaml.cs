using System.Windows;
using System.Windows.Controls;

namespace FavoStudio3D.Views
{
    public partial class CalculadoraView : UserControl
    {
        public CalculadoraView()
        {
            InitializeComponent();
        }

        private void BtnLimpar_Click(object sender, RoutedEventArgs e)
        {
            // Limpa todos os campos de texto
            TxtNomeProduto.Text = string.Empty;
            TxtDescricao.Text = string.Empty;
            TxtPeso.Text = string.Empty;
            TxtHoras.Text = string.Empty;
            TxtMinutos.Text = string.Empty;
            TxtQuantidade.Text = "1";
            TxtMargem.Text = string.Empty;

            // Reseta os ComboBoxes para a primeira opção
            CmbFilamento.SelectedIndex = 0;
            CmbImpressora.SelectedIndex = 0;
        }

        private void BtnCalcular_Click(object sender, RoutedEventArgs e)
        {
            // Validação dos campos obrigatórios (*)
            if (string.IsNullOrWhiteSpace(TxtNomeProduto.Text) ||
                CmbFilamento.SelectedIndex == 0 ||
                CmbImpressora.SelectedIndex == 0 ||
                string.IsNullOrWhiteSpace(TxtPeso.Text) ||
                (string.IsNullOrWhiteSpace(TxtHoras.Text) && string.IsNullOrWhiteSpace(TxtMinutos.Text)) ||
                string.IsNullOrWhiteSpace(TxtQuantidade.Text) ||
                string.IsNullOrWhiteSpace(TxtMargem.Text))
            {
                // Se faltar algum campo, mostra o seu modal bonito e centralizado
                ModalValidacao.Visibility = Visibility.Visible;
                return;
            }

            // ==============================================================
            // TODO: Aqui entrará a matemática da calculadora futuramente
            // Exemplo: double custoMaterial = (peso / 1000) * precoFilamento;
            // ==============================================================

            // Como teste, se tudo estiver preenchido, ele exibe um alerta de sucesso
            MessageBox.Show("Todos os dados foram validados! A calculadora fará o processamento aqui.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

            // Depois do cálculo, você poderá navegar para a "Tela de Resultado" usando o mesmo método MudarDeTela()
        }

        private void BtnFecharValidacao_Click(object sender, RoutedEventArgs e)
        {
            // Esconde o modal de validação para o utilizador corrigir o formulário
            ModalValidacao.Visibility = Visibility.Collapsed;
        }
    }
}