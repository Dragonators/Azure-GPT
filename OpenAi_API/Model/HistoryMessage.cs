using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenAi_API.Model
{
	public class HistoryMessage
	{

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public string Id { get; set; }
		[ForeignKey("navId")]
		public string navId { get; set; }
		public string message { get; set; }
		public string role { get; set;}
		public DateTime creatAt { get; set; }
		
	}
}
