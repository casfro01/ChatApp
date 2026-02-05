using System;
using System.Collections.Generic;

namespace _3_dataaccess;

public partial class Room
{
    public string Id { get; set; } = null!;

    public string? Chatname { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
