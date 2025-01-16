namespace LimpezaDeBase.Modelos.BlipContatos
{
    public class BlipContatoResponse
    {
        public string Type { get; set; }
        public Resource Resource { get; set; }
        public string Status { get; set; }
        public string Id { get; set; }
    }

    public class Resource
    {
        public int Total { get; set; }
        public List<Contact> Items { get; set; }
    }

    public class Contact
    {
        public string identity { get; set; }
        public string name { get; set; }
    }
}
