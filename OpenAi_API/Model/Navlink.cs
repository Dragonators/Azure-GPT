using System.ComponentModel.DataAnnotations;

namespace OpenAi_API.Model
{
	public class Navlink
	{
		public DateTime latestAt { get; set; }
		[Key]
		public string navId { get; set; }
		[Required]
		public string userId { get; set; }
		public string navName { get; set; }
		public List<HistoryMessage> chatMessages { get; set; }
	}
}
