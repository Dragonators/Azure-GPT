using Microsoft.EntityFrameworkCore;

namespace OpenAi_API.Model
{
	public class ChatDbContext:DbContext
	{
		public ChatDbContext(DbContextOptions<ChatDbContext> options):base(options)
		{
			
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<HistoryMessage>()
				.HasOne<Navlink>()
				.WithMany(e => e.chatMessages)
				.HasForeignKey(e => e.navId)
				.IsRequired();
		}
		public DbSet<Navlink> Navlinks { get; set; }
		public DbSet<HistoryMessage> HistoryMessages { get; set; }
	}
}
