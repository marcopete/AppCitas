using System;
using System.Collections.Generic;

// Atributos de tipo DateTime requieren using System;
// Averiguar como elegir un pais y despliegue solo las ciudades de este

namespace DatingApp.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastActive { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

// ICollection requiere using System.Collections.Generic;
        public virtual ICollection<Photo> Photos { get; set; }
        public virtual ICollection<Like> Likers { get; set; }
        public virtual ICollection<Like> Likees { get; set; }

        public virtual ICollection<Message> MessagesSent { get; set; }
        public virtual ICollection<Message> MessagesReceived { get; set; }
    }
}