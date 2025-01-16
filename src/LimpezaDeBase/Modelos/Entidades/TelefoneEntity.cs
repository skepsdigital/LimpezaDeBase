namespace LimpezaDeBase.Modelos.Entidades
{
    public class TelefoneEntity
    {
        public int ID { get; set; }
        public string Telefone { get; set; }
        public string DDI { get; set; }
        public string DDD { get; set; }
        public bool? PossuiWpp { get; set; }
        public string? NomeProprietario { get; set; }
        public DateTime Data { get; set; }
    }
}
