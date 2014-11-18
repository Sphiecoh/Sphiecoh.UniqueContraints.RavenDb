using Raven.Client;
using Raven.Client.Document;
using UniqueConstraints.RavenDb.Enforcer;

namespace Sphiecoh.UniqueContraints.RavenDb.Sample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var store = NewDocumentStore())
            {
                var list = new RavenUniqueEnforcer<User>();
                list.AddProperty(x => x.Name);
                list.AddProperty(x => x.Email);

                using (var session = store.OpenSession())
                {
                    //Should save
                    var user = new User() { Name = "John", Email = "john@gmail.com" };
                    new RavenUniqueInserter().StoreUnique(session, user, list);

                    //Should throw a ConcurrencyException exception
                    var user1 = new User() { Name = "John", Email = "john@gmail.com" };
                    new RavenUniqueInserter().StoreUnique(session, user1, list);
                }
            }
        }

        private static IDocumentStore NewDocumentStore()
        {
            return new DocumentStore
              {
                  Url = "http://localhost:8080",
                  DefaultDatabase = "Test",
              }.Initialize();
        }
    }

    public class User
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}