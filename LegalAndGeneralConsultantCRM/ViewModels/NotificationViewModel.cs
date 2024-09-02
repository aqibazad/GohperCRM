namespace LegalAndGeneralConsultantCRM.ViewModels
{
	public class NotificationViewModel
	{
		public int NotificationId { get; set; }
		public string Message { get; set; }
		public bool IsRead { get; set; }
		public DateTime? NotificationTime { get; set; }
	}
}
