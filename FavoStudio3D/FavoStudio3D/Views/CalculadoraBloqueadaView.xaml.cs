using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace FavoStudio3D.Views
{
    public partial class CalculadoraBloqueadaView : UserControl
    {
        public CalculadoraBloqueadaView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            bool semImpressoras = ResumoImpressorasView.ImpressorasBancoDados == null || ResumoImpressorasView.ImpressorasBancoDados.Count == 0;
            bool semFilamentos = ResumoCustosMateriaisView.FilamentosBancoDados == null || ResumoCustosMateriaisView.FilamentosBancoDados.Count == 0;

            BtnIrImpressoras.Visibility = semImpressoras ? Visibility.Visible : Visibility.Collapsed;
            BtnIrCustos.Visibility = semFilamentos ? Visibility.Visible : Visibility.Collapsed;

            // Remove a margem direita do primeiro botão se o segundo estiver oculto
            BtnIrImpressoras.Margin = (semImpressoras && semFilamentos) ? new Thickness(0, 0, 14, 0) : new Thickness(0);
        }

        private void BtnIrImpressoras_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is DashboardWindow dashboard)
            {
                if (dashboard.FindName("MenuImpressoraBorder") is Border borderImp &&
                    dashboard.FindName("MenuImpressoraTexto") is TextBlock textoImp)
                {
                    var metodoAtivar = dashboard.GetType().GetMethod("AtivarBotaoMenu", BindingFlags.NonPublic | BindingFlags.Instance);
                    metodoAtivar?.Invoke(dashboard, new object[] { borderImp, textoImp });
                }

                dashboard.TextoTituloPagina.Text = "Impressoras";
                dashboard.TextoSubtituloPagina.Text = "Cadastre e gerencie as impressoras utilizadas na produção.";

                if (ResumoImpressorasView.ImpressorasBancoDados != null && ResumoImpressorasView.ImpressorasBancoDados.Count > 0)
                    dashboard.AreaConteudo.Content = new ResumoImpressorasView();
                else
                    dashboard.AreaConteudo.Content = new ImpressorasView();
            }
        }

        private void BtnIrCustos_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is DashboardWindow dashboard)
            {
                if (dashboard.FindName("MenuCustosBorder") is Border borderCustos &&
                    dashboard.FindName("MenuCustosTexto") is TextBlock textoCustos)
                {
                    var metodoAtivar = dashboard.GetType().GetMethod("AtivarBotaoMenu", BindingFlags.NonPublic | BindingFlags.Instance);
                    metodoAtivar?.Invoke(dashboard, new object[] { borderCustos, textoCustos });
                }

                dashboard.TextoTituloPagina.Text = "Custos e materiais";

                bool temFilamentos = ResumoCustosMateriaisView.FilamentosBancoDados.Count > 0;
                bool temCustos = ResumoCustosMateriaisView.CustosBancoDados.Count > 0;

                if (temFilamentos || temCustos)
                {
                    dashboard.TextoSubtituloPagina.Text = "Gerencie os valores dos seus custos e filamentos para manter os cálculos sempre atualizados.";
                    dashboard.AreaConteudo.Content = new ResumoCustosMateriaisView();
                }
                else
                {
                    dashboard.TextoSubtituloPagina.Text = "Cadastre seus filamentos e custos para manter os valores sempre atualizados.";
                    dashboard.AreaConteudo.Content = new CustosMateriaisView();
                }
            }
        }
    }
}