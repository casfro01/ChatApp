using System;
using System.Collections.Generic;

namespace _3_dataaccess;

public partial class Message
{
    public int Id { get; set; }

    public string? Userid { get; set; }

    public string? Roomid { get; set; }

    public string? Chatmessage { get; set; }

    public virtual Room? Room { get; set; }

    public virtual User? User { get; set; }
}
