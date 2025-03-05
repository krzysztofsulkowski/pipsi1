using TimeCapsule.Models;

namespace TimeCapsule.Services
{
    public class ProfileService 
    {
        private readonly TimeCapsuleContext _context;

        public ProfileService(TimeCapsuleContext context)
        {
            _context = context;
        }
    }
}
