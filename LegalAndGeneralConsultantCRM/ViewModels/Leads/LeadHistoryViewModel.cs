namespace LegalAndGeneralConsultantCRM.ViewModels.Leads
{
	public class LeadHistoryViewModel
	{
		public int LeadHistoryId { get; set; }
		public int? LeadId { get; set; }
		public string UserName { get; set; }
		public string FullName { get; set; }
		public string PhoneNumber { get; set; }
		public string Email { get; set; }
		public string Status { get; set; }
		public DateTime? LeadFollowupDate { get; set; }
	}
}
