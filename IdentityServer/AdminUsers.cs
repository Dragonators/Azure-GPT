// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using System.Collections.Generic;
using System.Text.Json;
using IdentityServer.Model;
using static Duende.IdentityServer.Models.IdentityResources;

namespace IdentityServer
{
	public class AdminUsers
	{
		/*public static List<ApplicationUser> Users
        {
            get
            {
                var address = new
                {
                    street_address = "One Hacker Way",
                    locality = "Heidelberg",
                    postal_code = 69118,
                    country = "Germany"
                };

                return new List<ApplicationUser>
                {
                    new ApplicationUser
                    {
                        Address=JsonSerializer.Serialize(address),
                        Email="BobSmith@email.com",
                        Name="Bob Smith",
						GivenName="Bob",
                        FamilyName="Smith",
                        UserName="bob"
					}
                };
            }
        }
        */
		public static List<ApplicationUser> Users = new List<ApplicationUser>
		{
            new ApplicationUser
			{
			    Address=JsonSerializer.Serialize(new
		        {
			        street_address = "One Hacker Way",
			        locality = "Heidelberg",
			        postal_code = 69118,
			        country = "Germany"
		        }),
				Email="BobSmith@email.com",
				Name="Bob Smith",
				GivenName="Bob",
				FamilyName="Smith",
				UserName="bob"
			}
		};
	}
}