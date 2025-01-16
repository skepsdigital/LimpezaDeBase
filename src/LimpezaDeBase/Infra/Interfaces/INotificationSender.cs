using LimpezaDeBase.Modelos.Notify;
using RestEase;

namespace LimpezaDeBase.Infra.Interfaces
{
    public interface INotificationSender
    {
        [Post("/commands")]
        Task<dynamic> Send(
            [Header("Authorization")] string authorization,
            [Body(BodySerializationMethod.Serialized)] string campaingRequest,
            [Header("Content-Type")] string type = "application/json");

        [Post("/commands")]
        Task<SummaryResponse> GetSumary(
            [Header("Authorization")] string authorization,
            [Body(BodySerializationMethod.Serialized)] string campaingRequest,
            [Header("Content-Type")] string type = "application/json");
    }
}
