namespace LimpezaDeBase.Modelos.Entidades
{
    public class ProcessamentoEntity
    {
        public int ID { get; set; }
        public int NumeroDeContatosTotal { get; set; }
        public string ProcessId { get; set; }
        public string Email { get; set; }
        public string Contrato { get; set; }
        public string Roteador { get; set; }
        public DateTime Data { get; set; }
        public bool FoiProcessado { get; set; }
        public bool EnviarNotificacao { get; set; }
        public string Funcionalidade { get; set; }
    }
}
