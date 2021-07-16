// --------------------------------------------------------------------------------------------------------------------
// <copyright file="User.cs" company="Hämmer Electronics">
//   Copyright (c) 2020 All rights reserved.
// </copyright>
// <summary>
//   The <see cref="User" /> read from the config.json file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace com.koerber
{
   public class User
    {
        /// <summary>
        ///     Gets or sets the user name.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     Gets or sets the password.
        /// </summary>
        public string Password { get; set; }
    }
}