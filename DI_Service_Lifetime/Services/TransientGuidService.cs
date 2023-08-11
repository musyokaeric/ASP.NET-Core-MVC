namespace DI_Service_Lifetime.Services
{
    public class TransientGuidService : ITransientGuidService
    {
        private readonly Guid Id;

        public TransientGuidService()
        {
            Id = Guid.NewGuid();
        }

        string ITransientGuidService.GetGuid() => Id.ToString();
    }
}
