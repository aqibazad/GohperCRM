using System.ComponentModel.DataAnnotations;

namespace LegalAndGeneralConsultantCRM.ViewModels
{
	public class SocialMediaViewModel
	{
		
			public int Id { get; set; } // For the social media record
			public string UserId { get; set; } // To link the record to a user

			[Url]
			public string? Facebook { get; set; }

			[Url]
			public string? Twitter { get; set; }

			[Url]
			public string? Instagram { get; set; }

			[Url]
			public string? TikTok { get; set; }

			[Url]
			public string? LinkedIn { get; set; }

			[Url]
			public string? YouTube { get; set; }

			[Url]
			public string? Snapchat { get; set; }

			[Url]
			public string? Pinterest { get; set; }

			[Url]
			public string? Reddit { get; set; }

			public string? Other { get; set; }
		}
	}

