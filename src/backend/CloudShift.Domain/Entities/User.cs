using System;
using System.Collections.Generic;

namespace CloudShift.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AppProfile> AppProfiles { get; set; } = new List<AppProfile>();
    public ICollection<ProjectMapping> ProjectMappings { get; set; } = new List<ProjectMapping>();
}
