using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUTX11_SSH_ConsoleApp
{
    public interface IUserLog
    {
        public void LogAuthenticationError(Attempt attempt);
        public void LogSmsError(Attempt attempt);
        public void LogSignalLostError(int retry, Attempt attempt);
        public IEnumerable<User> GetAllUsers();
        public Attempt SaveAttempt(User user);
        public IEnumerable<Attempt> GetUserAttempts(User user);
        public IEnumerable<Error> GetAttemptErrors(Attempt attempt);
        public void SaveUser(User user);
    }

    public class UserLog : IUserLog
    {
        public void LogAuthenticationError(Attempt attempt)
        {
            using (var db = new LogsContext())
            {
                var error = new Error
                {
                    Message = "Authentication failed: User details incorrect.",
                };
                attempt.Errors.Add(error);
                db.SaveChanges();
            }
        }

        public void LogSmsError(Attempt attempt)
        {
            using (var db = new LogsContext())
            {
                var error = new Error
                {
                    Message = "SMS Was not sent successfully",
                };
                attempt.Errors.Add(error);
                db.SaveChanges();
            }
        }

        public void LogSignalLostError(int retry, Attempt attempt)
        {
            using (var db = new LogsContext())
            {
                var error = new Error
                {
                    Message = $"Mobile signal lost after {retry} tries",
                };
                attempt.Errors.Add(error);
                db.SaveChanges();
            }
        }

        public IEnumerable<User> GetAllUsers()
        {
            using (var db = new LogsContext())
            {
                return db.Users.ToList();
            }
        }

        public Attempt SaveAttempt(User user)
        {
            using (var db = new LogsContext())
            {
                var attempt = new Attempt
                {
                    Timestamp = DateTime.Now,
                };
                user.Attempts.Add(attempt);
                db.SaveChanges();
                return attempt;
            }
        }

        public IEnumerable<Attempt> GetUserAttempts(User user)
        {
            using (var db = new LogsContext())
            {
                return user.Attempts.ToList();
            }
        }

        public IEnumerable<Error> GetAttemptErrors(Attempt attempt)
        {
            using (var db = new LogsContext())
            {
                return attempt.Errors.ToList();
            }
        }

        public void SaveUser(User user)
        {
            using (var db = new LogsContext())
            {
                db.Add(user);
                db.SaveChanges();
            }
        }
    }
}
