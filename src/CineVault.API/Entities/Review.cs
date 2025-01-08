﻿namespace CineVault.API.Entities;

public sealed class Review
{
    public int Id { get; set; }
    public required int MovieId { get; set; }
    public required int UserId { get; set; }
    public required int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Movie? Movie { get; set; }
    public User? User { get; set; }
}