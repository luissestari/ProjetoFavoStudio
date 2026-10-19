using System.Windows.Media;

namespace FavoStudio3D.Models
{
    public class CustoAdicional
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty; // Ex: "Saco a vácuo", "Embalagem", "Lixas"
        public decimal Valor { get; set; }                    // Ex: 135.00
        public string Tipo { get; set; } = "Custo Adicional"; // Ex: "Custo Fixo" ou "Custo Adicional"

        // Propriedades formatadas para a interface WPF
        public string ValorFormatado => $"R$ {Valor:N2}";
        public Brush CorFundoTipo => Tipo == "Custo Fixo"
            ? new SolidColorBrush(Color.FromRgb(227, 242, 253)) // Azul fixo
            : new SolidColorBrush(Color.FromRgb(243, 229, 245)); // Roxo adicional
    }
}