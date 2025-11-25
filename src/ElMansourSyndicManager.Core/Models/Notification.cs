using System;

namespace ElMansourSyndicManager.Models;

public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Icon { get; set; } = "Information";
    public string TypeColor { get; set; } = "#2196F3";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsRead { get; set; } = false;
    
    public string TimeAgo
    {
        get
        {
            var timeSpan = DateTime.Now - CreatedAt;
            
            if (timeSpan.TotalMinutes < 1)
                return "À l'instant";
            if (timeSpan.TotalMinutes < 60)
                return $"Il y a {(int)timeSpan.TotalMinutes} min";
            if (timeSpan.TotalHours < 24)
                return $"Il y a {(int)timeSpan.TotalHours}h";
            if (timeSpan.TotalDays < 7)
                return $"Il y a {(int)timeSpan.TotalDays}j";
            
            return CreatedAt.ToString("dd/MM/yyyy");
        }
    }
}
