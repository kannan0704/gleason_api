using System;
using System.Collections.Generic;

namespace GEMS.Model;

public partial class TblUser
{
    public int UserId { get; set; }

    public string Customer { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Roles { get; set; }

    public bool TrialUser { get; set; }
}
