using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Controls;

namespace FavoStudio3D.Views
{
    public partial class DashboardWindow : Window
    {
        private DispatcherTimer? timerRelogio;

        // Cor dourada do botão ativo
        private readonly SolidColorBrush corAtiva = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F3D09E"));
        // Cor transparente para os botões inativos
        private readonly SolidColorBrush corInativa = Brushes.Transparent;

        public DashboardWindow()
        {
            InitializeComponent();
            ConfigurarRelogio();

            NavegarParaCalculadora();
        }

        private void ConfigurarRelogio()
        {
            timerRelogio = new DispatcherTimer();
            timerRelogio.Interval = TimeSpan.FromSeconds(1);
            timerRelogio.Tick += TimerRelogio_Tick;
            timerRelogio.Start();
            AtualizarDataEHora();
        }

        private void TimerRelogio_Tick(object? sender, EventArgs? e)
        {
            AtualizarDataEHora();
        }

        private void AtualizarDataEHora()
        {
            DateTime agora = DateTime.Now;
            string dataFormatada = agora.ToString("ddd, dd 'de' MMMM 'de' yyyy", new CultureInfo("pt-BR"));
            TextoData.Text = char.ToUpper(dataFormatada[0]) + dataFormatada.Substring(1);
            TextoHora.Text = agora.ToString("HH:mm");
        }

        // ==========================================
        // LÓGICA VISUAL DO MENU
        // ==========================================

        private void AtivarBotaoMenu(Border borderAtivo, TextBlock textoAtivo)
        {
            // 1. Reseta TODOS os botões para Transparente e fonte Normal
            MenuCalculadoraBorder.Background = corInativa;
            MenuCalculadoraTexto.FontWeight = FontWeights.Normal;

            MenuCustosBorder.Background = corInativa;
            MenuCustosTexto.FontWeight = FontWeights.Normal;

            MenuProdutosBorder.Background = corInativa;
            MenuProdutosTexto.FontWeight = FontWeights.Normal;

            MenuImpressoraBorder.Background = corInativa;
            MenuImpressoraTexto.FontWeight = FontWeights.Normal;

            // 2. Pinta apenas o botão que foi clicado de Dourado e coloca a fonte em SemiBold
            borderAtivo.Background = corAtiva;
            textoAtivo.FontWeight = FontWeights.SemiBold;
        }

        // ==========================================
        // EVENTOS DE NAVEGAÇÃO
        // ==========================================

        private void BotaoMenuCalculadora_Click(object sender, MouseButtonEventArgs e)
        {
            NavegarParaCalculadora();
        }

        private void BotaoMenuCustos_Click(object sender, MouseButtonEventArgs e)
        {
            AtivarBotaoMenu(MenuCustosBorder, MenuCustosTexto);

            TextoTituloPagina.Text = "Custos e materiais";

            // LÓGICA INTELIGENTE DO SUBTÍTULO
            // Verifica se as listas (bancos de dados temporários) estão vazias.
            bool temFilamentos = ResumoCustosMateriaisView.FilamentosBancoDados.Count > 0;
            bool temCustos = ResumoCustosMateriaisView.CustosBancoDados.Count > 0;

            if (temFilamentos || temCustos)
            {
                // Mostra o texto atualizado para gerir os custos
                TextoSubtituloPagina.Text = "Gerencie os valores dos seus custos e filamentos para manter os cálculos sempre atualizados.";
                AreaConteudo.Content = new ResumoCustosMateriaisView(); // Carrega a tela com as listas
            }
            else
            {
                // Mostra o texto original para primeiro registo
                TextoSubtituloPagina.Text = "Cadastre seus filamentos e custos para manter os valores sempre atualizados.";
                AreaConteudo.Content = new CustosMateriaisView(); // Carrega a tela vazia (botão gigante)
            }
        }

        private void BotaoMenuProdutos_Click(object sender, MouseButtonEventArgs e)
        {
            // Ativa o destaque dourado no botão de Produtos no menu lateral
            AtivarBotaoMenu(MenuProdutosBorder, MenuProdutosTexto);

            TextoTituloPagina.Text = "Produtos";

            // Verifica se já existem produtos salvos para alternar entre a tela vazia e o resumo
            if (ResumoProdutosView.ProdutosBancoDados != null && ResumoProdutosView.ProdutosBancoDados.Count > 0)
            {
                TextoSubtituloPagina.Text = "Os produtos cadastrados aparecem aqui após serem calculados e salvos na calculadora.";
                AreaConteudo.Content = new ResumoProdutosView();
            }
            else
            {
                TextoSubtituloPagina.Text = "Estime o custo e defina o preço dos seus produtos 3D.";
                AreaConteudo.Content = new ProdutosView();
            }
        }

        private void NavegarParaCalculadora()
        {
            AtivarBotaoMenu(MenuCalculadoraBorder, MenuCalculadoraTexto);

            TextoTituloPagina.Text = "Calculadora";
            TextoSubtituloPagina.Text = "Calcule os custos de produção e defina o preço de venda dos seus produtos 3D.";

            // ==========================================================
            // LÓGICA INTELIGENTE DE DESBLOQUEIO DA CALCULADORA
            // ==========================================================
            bool temImpressoras = ResumoImpressorasView.ImpressorasBancoDados.Count > 0;
            bool temFilamentos = ResumoCustosMateriaisView.FilamentosBancoDados.Count > 0;

            // Só libera a tela da calculadora se houver pelo menos 1 impressora e 1 filamento cadastrados
            if (temImpressoras && temFilamentos)
            {
                AreaConteudo.Content = new CalculadoraView();
            }
            else
            {
                AreaConteudo.Content = new CalculadoraBloqueadaView();
            }
        }

        private void BotaoSair_Click(object sender, MouseButtonEventArgs e)
        {
            MainWindow telaInicio = new MainWindow();
            telaInicio.Show();
            this.Close();
        }

        private void BotaoMenuImpressora_Click(object sender, MouseButtonEventArgs e)
        {
            AtivarBotaoMenu(MenuImpressoraBorder, MenuImpressoraTexto);
            TextoTituloPagina.Text = "Impressoras";
            TextoSubtituloPagina.Text = "Cadastre e gerencie as impressoras utilizadas na produção.";

            // Verifica se já existem impressoras cadastradas
            if (ResumoImpressorasView.ImpressorasBancoDados != null && ResumoImpressorasView.ImpressorasBancoDados.Count > 0)
            {
                AreaConteudo.Content = new ResumoImpressorasView();
            }
            else
            {
                AreaConteudo.Content = new ImpressorasView();
            }
        }
    }
}