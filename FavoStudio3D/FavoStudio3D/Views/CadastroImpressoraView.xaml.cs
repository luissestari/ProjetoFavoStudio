using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FavoStudio3D.Views
{
    public partial class CadastroImpressoraView : UserControl
    {
        public CadastroImpressoraView()
        {
            InitializeComponent();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            VoltarParaImpressoras();
        }

        private void BtnSalvarImpressora_Click(object sender, RoutedEventArgs e)
        {
            // Adiciona a nova impressora à lista em memória para aparecer na listagem
            var novaImpressora = new ImpressoraModel
            {
                NomeModelo = $"{TxtNomeImpressora.Text} {TxtModelo.Text}".Trim(),
                PotenciaFormatada = TxtPotencia.Text,
                CustoKwhFormatado = $"R$ {TxtCustoKwh.Text}",
                Observacoes = string.IsNullOrWhiteSpace(TxtObservacoes.Text) ? "-" : TxtObservacoes.Text
            };

            ResumoImpressorasView.ImpressorasBancoDados.Add(novaImpressora);

            // Retorna para a tela de listagem de impressoras
            var telaResumo = new ResumoImpressorasView();
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

        private void VoltarParaImpressoras()
        {
            var telaImpressoras = new ImpressorasView();
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            while (parent != null)
            {
                if (parent is ContentControl cc) { cc.Content = telaImpressoras; return; }
                if (parent is Panel panel)
                {
                    int index = panel.Children.IndexOf(this);
                    if (index >= 0) { panel.Children.RemoveAt(index); panel.Children.Insert(index, telaImpressoras); return; }
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
        }
    }
}