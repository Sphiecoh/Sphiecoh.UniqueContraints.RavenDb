Sphiecoh.UniqueContraints.RavenDb
=================================
Add unique constraints your entities when using RavenDB.
<br/>
Example usage : 


~~~~
var uniqueProperties = new RavenUniqueEnforcer<User>();
 uniqueProperties.AddProperty(x => x.Name);
 uniqueProperties.AddProperty(x => x.Email);
 
 using (var session = store.OpenSession())
				{
				   try
               {
					       //Should save
					        var user = new User() { Name = "John", Email = "john@gmail.com" };
                    new RavenUniqueInserter().StoreUnique(session,user, list);

                    //Should throw a ConcurrencyException exception 
                    var user1 = new User() { Name = "John", Email = "john@gmail.com" };
                    new RavenUniqueInserter().StoreUnique(session, user1, list);
                    }
                     catch (ConcurrencyException)
                    {
                        // email address and name already in use 
                    }
										
				}
~~~~
	
