namespace Ragnar.Questions.SqLite;

public class QuestionDbContext(DbContextOptions<QuestionDbContext> options)
    : DbContext(options)
{
    public DbSet<Question> Questions => Set<Question>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=questions.db");
    }
}
