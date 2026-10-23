using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace FavoStudio3D.Views
{
    public partial class CadastroImpressoraView : UserControl
    {
        private ImpressoraModel? _impressoraEmEdicao = null;
        private string _caminhoImagemSelecionada = string.Empty;

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

                if (!string.IsNullOrEmpty(_impressoraEmEdicao.CaminhoImagem))
                {
                    _caminhoImagemSelecionada = _impressoraEmEdicao.CaminhoImagem;
                    try
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(_caminhoImagemSelecionada);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        ImgImpressoraPreview.Source = bitmap;
                        if (PainelPlaceholderImpressora != null)
                        {
                            PainelPlaceholderImpressora.Visibility = Visibility.Collapsed;
                        }
                    }
                    catch { }
                }
            }
        }

        private void BtnSalvarImpressora_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNomeImpressora.Text) ||
                string.IsNullOrWhiteSpace(TxtModelo.Text) ||
                string.IsNullOrWhiteSpace(TxtPotencia.Text) ||
                string.IsNullOrWhiteSpace(TxtCustoKwh.Text))
            {
                ModalValidacao.Visibility = Visibility.Visible;
                return;
            }

            string nomeOriginal = TxtNomeImpressora.Text.Trim();
            string modeloOriginal = TxtModelo.Text.Trim();

            string nomeCompleto = string.IsNullOrWhiteSpace(modeloOriginal)
                                ? nomeOriginal
                                : $"{nomeOriginal} {modeloOriginal}";

            if (_impressoraEmEdicao != null)
            {
                _impressoraEmEdicao.Nome = nomeOriginal;
                _impressoraEmEdicao.Modelo = modeloOriginal;
                _impressoraEmEdicao.NomeModelo = nomeCompleto;
                _impressoraEmEdicao.PotenciaFormatada = TxtPotencia.Text;
                _impressoraEmEdicao.CustoKwhFormatado = $"R$ {TxtCustoKwh.Text}";
                _impressoraEmEdicao.Observacoes = string.IsNullOrWhiteSpace(TxtObservacoes.Text) ? "-" : TxtObservacoes.Text;
                _impressoraEmEdicao.CaminhoImagem = _caminhoImagemSelecionada;
            }
            else
            {
                var novaImpressora = new ImpressoraModel
                {
                    Nome = nomeOriginal,
                    Modelo = modeloOriginal,
                    NomeModelo = nomeCompleto,
                    PotenciaFormatada = TxtPotencia.Text,
                    CustoKwhFormatado = $"R$ {TxtCustoKwh.Text}",
                    Observacoes = string.IsNullOrWhiteSpace(TxtObservacoes.Text) ? "-" : TxtObservacoes.Text,
                    CaminhoImagem = _caminhoImagemSelecionada
                };
                ResumoImpressorasView.ImpressorasBancoDados.Add(novaImpressora);
            }

            VoltarParaResumo();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            VoltarParaResumo();
        }

        private void BoxImagemImpressora_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Imagens (*.jpg; *.jpeg; *.png; *.webp)|*.jpg;*.jpeg;*.png;*.webp|Todos os arquivos (*.*)|*.*";
            openFileDialog.Title = "Selecione a imagem da impressora";

            if (openFileDialog.ShowDialog() == true)
            {
                _caminhoImagemSelecionada = openFileDialog.FileName;

                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(_caminhoImagemSelecionada);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    ImgImpressoraPreview.Source = bitmap;

                    if (PainelPlaceholderImpressora != null)
                    {
                        PainelPlaceholderImpressora.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao carregar a imagem: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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

        private void BtnFecharValidacao_Click(object sender, RoutedEventArgs e)
        {
            ModalValidacao.Visibility = Visibility.Collapsed;
        }
    }
}