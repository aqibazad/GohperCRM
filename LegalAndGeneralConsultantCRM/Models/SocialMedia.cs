namespace LegalAndGeneralConsultantCRM.Models
{
	public class SocialMedia
	{
		public int Id { get; set; } // Auto-generated unique identifier
		public string UserId { get; set; } // User ID to associate social media links with a user

		public string? Facebook { get; set; }
		public string? Twitter { get; set; }
		public string? Instagram { get; set; }
		public string? TikTok { get; set; }
		public string? LinkedIn { get; set; }
		public string? YouTube { get; set; }
		public string? Snapchat { get; set; }
		public string? Pinterest { get; set; }
		public string? Reddit { get; set; }
		public string? Other { get; set; }
	}
}
