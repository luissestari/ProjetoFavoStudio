using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FavoStudio3D.Views
{
    public partial class CadastroImpressoraView : UserControl
    {
        private ImpressoraModel? _impressoraEmEdicao = null;

        public CadastroImpressoraView(ImpressoraModel? impressoraSelecionada = null)
        {
            InitializeComponent();

            _impressoraEmEdicao = impressoraSelecionada;

            // Preenche os campos corretamente se for edição
            if (_impressoraEmEdicao != null)
            {
                TxtNomeImpressora.Text = _impressoraEmEdicao.Nome;
                TxtModelo.Text = _impressoraEmEdicao.Modelo;

                TxtPotencia.Text = _impressoraEmEdicao.PotenciaFormatada;
                TxtCustoKwh.Text = _impressoraEmEdicao.CustoKwhFormatado.Replace("R$ ", "").Trim();
                TxtObservacoes.Text = _impressoraEmEdicao.Observacoes == "-" ? "" : _impressoraEmEdicao.Observacoes;
            }
        }

        private void BtnSalvarImpressora_Click(object sender, RoutedEventArgs e)
        {
            // ==============================================================
            // VALIDAÇÃO: Chama o seu novo modal bonito em vez do MessageBox
            // ==============================================================
            if (string.IsNullOrWhiteSpace(TxtNomeImpressora.Text) ||
                string.IsNullOrWhiteSpace(TxtModelo.Text) ||
                string.IsNullOrWhiteSpace(TxtPotencia.Text) ||
                string.IsNullOrWhiteSpace(TxtCustoKwh.Text))
            {
                ModalValidacao.Visibility = Visibility.Visible; // Abre o modal
                return; // Impede que o código continue e salve os dados
            }

            string nomeOriginal = TxtNomeImpressora.Text.Trim();
            string modeloOriginal = TxtModelo.Text.Trim();

            // Junta os dois apenas para mostrar na tabela depois
            string nomeCompleto = string.IsNullOrWhiteSpace(modeloOriginal)
                                ? nomeOriginal
                                : $"{nomeOriginal} {modeloOriginal}";

            if (_impressoraEmEdicao != null)
            {
                // MODO EDIÇÃO
                _impressoraEmEdicao.Nome = nomeOriginal;
                _impressoraEmEdicao.Modelo = modeloOriginal;
                _impressoraEmEdicao.NomeModelo = nomeCompleto;
                _impressoraEmEdicao.PotenciaFormatada = TxtPotencia.Text;
                _impressoraEmEdicao.CustoKwhFormatado = $"R$ {TxtCustoKwh.Text}";
                _impressoraEmEdicao.Observacoes = string.IsNullOrWhiteSpace(TxtObservacoes.Text) ? "-" : TxtObservacoes.Text;
            }
            else
            {
                // MODO CRIAÇÃO
                var novaImpressora = new ImpressoraModel
                {
                    Nome = nomeOriginal,
                    Modelo = modeloOriginal,
                    NomeModelo = nomeCompleto,
                    PotenciaFormatada = TxtPotencia.Text,
                    CustoKwhFormatado = $"R$ {TxtCustoKwh.Text}",
                    Observacoes = string.IsNullOrWhiteSpace(TxtObservacoes.Text) ? "-" : TxtObservacoes.Text,
                    CaminhoImagem = string.Empty
                };
                ResumoImpressorasView.ImpressorasBancoDados.Add(novaImpressora);
            }

            // Só chega aqui (e muda de tela) se a validação lá em cima passou
            VoltarParaResumo();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            VoltarParaResumo();
        }

        private void VoltarParaResumo()
        {
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
        // Função que fecha o modal de aviso quando o utilizador clica em OK
        private void BtnFecharValidacao_Click(object sender, RoutedEventArgs e)
        {
            ModalValidacao.Visibility = Visibility.Collapsed;
        }
    }
}