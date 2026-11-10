using Connectly.Domain.Entities;
using Connectly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Connectly.Web
{
    public class MyMethods
    {
        private readonly AppDbContext _context;
        public MyMethods(AppDbContext context)
        {
            _context = context;
        }

        public void Update_Basic()
        {
            Guid.TryParse("990E2330-B92B-4AC3-6DE7-08DF3659FC21", out Guid g);

            var up = _context.UserProfiles
                .FirstOrDefault(up => up.Id == g);

            up.Bio = "updated with Update_Basic";
            _context.SaveChanges();

            ClearChangeTracker();
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%__Update_Basic");
        }

        public void Update_UpdateMethod()
        {
            Guid.TryParse("990E2330-B92B-4AC3-6DE7-08DF3659FC21", out Guid g);

            var up = _context.UserProfiles
                .FirstOrDefault(up => up.Id == g);

            up.Bio = "updated with Update_UpdateMehod";
            _context.UserProfiles.Update(up);
            _context.SaveChanges();

            ClearChangeTracker();
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%__Update_UpdateMethod");
        }

        public void Update_AttachPattern()
        {
            Guid.TryParse("990E2330-B92B-4AC3-6DE7-08DF3659FC21", out Guid g);

            var up = new UserProfile
            {
                Id = g,
                Bio = "updated with Update_AttachPattern"
            };

            _context.UserProfiles.Attach(up);
            _context.Entry(up).Property(up => up.Bio).IsModified = true;

            _context.SaveChanges();

            ClearChangeTracker();
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%__Update_AttachPattern");
        }

        public void Update_ExecuteUpdateAsync_1()
        {
            Guid.TryParse("990E2330-B92B-4AC3-6DE7-08DF3659FC21", out Guid g);

            // updating one record
            _context.UserProfiles
                .Where(up => up.Id == g)
                .ExecuteUpdate(up => up.SetProperty(p => p.Bio, "updated with Update_ExecuteUpdateAsync_1"));

            ClearChangeTracker();
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%__Update_ExecuteUpdateAsync_1");
        }

        public void Update_ExecuteUpdateAsync_2()
        {
            Guid.TryParse("990E2330-B92B-4AC3-6DE7-08DF3659FC21", out Guid g);

            // updating multiple records
            _context.Notifications
                .Where(up => up.RecipientId == g)
                .ExecuteUpdate(up => up.SetProperty(p => p.IsRead, false));

            ClearChangeTracker();
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%__Update_ExecuteUpdateAsync_2");
        }

        public void Update_ExecuteUpdateAsync_3()
        {
            Guid.TryParse("80000000-0000-0000-0000-000000000001", out Guid g);

            // updating one record, multiple fields
            _context.Stories
                .Where(up => up.Id == g)
                .ExecuteUpdate(up => up.SetProperty(p => p.Text, "I Will Kill You!").SetProperty(p => p.BackgroundColor, "#111"));

            ClearChangeTracker();
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%__Update_ExecuteUpdateAsync_3");
        }

        public void TestOriginalCurrentValues()
        {
            Guid.TryParse("990E2330-B92B-4AC3-6DE7-08DF3659FC21", out Guid g);

            var up = _context.UserProfiles
                .FirstOrDefault(up => up.Id == g);

            up.Bio = "updated with TestOriginalCurrentValues";

            var entry = _context.Entry(up);
            var bioProperty = entry.Property(x => x.Bio);
            var originalValue = bioProperty.OriginalValue;
            var currentValue = bioProperty.CurrentValue;

            _context.UserProfiles.Update(up);
            _context.SaveChanges();

            ClearChangeTracker();
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%__TestOriginalCurrentValues");
        }

        public void Update_RelatedEntity()
        {
            Guid.TryParse("990E2330-B92B-4AC3-6DE7-08DF3659FC21", out Guid g);

            var notif = _context.Notifications
                .Include(n => n.Actor)
                .FirstOrDefault(n => n.ActorId == g);

            notif?.Actor.FirstName = "omid";
            var entry = _context.ChangeTracker.Entries<Notification>().ToList(); // here all props and navs are isModified = false, just the FirstName prop of Actor has isModified = true

            _context.Notifications.Update(notif);
            entry = _context.ChangeTracker.Entries<Notification>().ToList(); // here all props and navs are isModified = true, still just the FirstName prop of Actor has isModified = true

            _context.SaveChanges();
            ClearChangeTracker();
            Console.WriteLine("%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%__RelatedEntity");
        }

        private void ClearChangeTracker() => _context.ChangeTracker.Clear();

    }
}
