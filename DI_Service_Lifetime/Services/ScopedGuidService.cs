namespace DI_Service_Lifetime.Services
{
    public class ScopedGuidService : IScopedGuidService
    {
        private readonly Guid Id;

        public ScopedGuidService()
        {
            Id = Guid.NewGuid();
        }

        public string GetGuid() => Id.ToString();
    }
}
