using TimeCapsule.Models;

namespace TimeCapsule.Services
{
    public class CapsuleService
    {
        private readonly TimeCapsuleContext _context;

        public CapsuleService(TimeCapsuleContext context)
        {
            _context = context;
        }

    }
}