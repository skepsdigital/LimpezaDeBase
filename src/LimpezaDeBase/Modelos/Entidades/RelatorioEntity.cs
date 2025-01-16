namespace LimpezaDeBase.Modelos.Entidades
{
    public class RelatorioEntity
    {
        public int Id { get; set; }
        public string IdProcessamento { get; set; }
        public int NumerosTotais { get; set; }
        public int NumerosValidos { get; set; }
        public int NumerosInvalidos { get; set; }
        public int LimposSkeps { get; set; }
        public int LimposExterno { get; set; }
        public int LimposOptOut { get; set; }
        public bool EnviouNotificacao { get; set; }
        public string Contrato { get; set; }
        public string IdNotificacoes { get; set; }
    }
}
