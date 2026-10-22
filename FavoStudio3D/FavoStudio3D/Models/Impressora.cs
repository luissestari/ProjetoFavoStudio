namespace FavoStudio3D.Models
{
    public class Impressora
    {
        public string Nome { get; set; } = string.Empty;
        public string MarcaModelo { get; set; } = string.Empty;
        public double ConsumoWatts { get; set; }
        public decimal CustoKwh { get; set; }
        public string Status { get; set; } = "Ativa";

        // exibe o custo em tela (ex: "R$ 0,85/kWh")
        public string CustoKwhFormatado => $"R$ {CustoKwh:N2}/kWh";

        // exibe o consumo (ex: "350 W")
        public string ConsumoFormatado => $"{ConsumoWatts} W";
    }
}