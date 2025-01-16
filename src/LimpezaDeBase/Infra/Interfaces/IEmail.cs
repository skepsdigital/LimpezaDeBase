namespace LimpezaDeBase.Infra.Interfaces
{
    public interface IEmail
    {
        Task SendMessageAsync(string recipient, string content);
    }
}