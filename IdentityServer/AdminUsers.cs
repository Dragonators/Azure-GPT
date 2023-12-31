﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer.Model;
using System.Text.Json;

namespace IdentityServer
{
	public class AdminUsers
	{
		public static List<ApplicationUser> Users = new List<ApplicationUser>
		{
            new ApplicationUser
			{
			    Address=JsonSerializer.Serialize(new Address_
		        {
			        street_address = "One Hacker Way",
			        locality = "Heidelberg",
			        postal_code = "69118",
			        country = "Germany"
		        }),
				Email="BobSmith@email.com",
				NickName="Bob Smith",
				GivenName="Bob",
				FamilyName="Smith",
				UserName="bob"
			}
		};
	}
}