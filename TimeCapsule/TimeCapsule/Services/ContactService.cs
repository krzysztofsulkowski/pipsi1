using TimeCapsule.Models;

namespace TimeCapsule.Services
{
    public class ContactService
    {
        private readonly TimeCapsuleContext _context;

        public ContactService(TimeCapsuleContext context)
        {
            _context = context;
        }
    }
}
