using System.Text;
using Forza.LastMile.TopPerformers.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Forza.LastMile.TopPerformers.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}

	public DbSet<RepartidorRendimiento> RepartidoresRendimiento { get; set; } = null!;

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

		foreach (var entity in modelBuilder.Model.GetEntityTypes())
		{
			var tableName = entity.GetTableName();
			if (!string.IsNullOrWhiteSpace(tableName))
			{
				entity.SetTableName(ToSnakeCase(tableName));
			}

			foreach (var property in entity.GetProperties())
			{
				property.SetColumnName(ToSnakeCase(property.Name));
			}

			foreach (var key in entity.GetKeys())
			{
				var keyName = key.GetName();
				if (!string.IsNullOrWhiteSpace(keyName))
				{
					key.SetName(ToSnakeCase(keyName));
				}
			}

			foreach (var foreignKey in entity.GetForeignKeys())
			{
				var constraintName = foreignKey.GetConstraintName();
				if (!string.IsNullOrWhiteSpace(constraintName))
				{
					foreignKey.SetConstraintName(ToSnakeCase(constraintName));
				}
			}

			foreach (var index in entity.GetIndexes())
			{
				var databaseName = index.GetDatabaseName();
				if (!string.IsNullOrWhiteSpace(databaseName))
				{
					index.SetDatabaseName(ToSnakeCase(databaseName));
				}
			}
		}
	}

	private static string ToSnakeCase(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return value;
		}

		var builder = new StringBuilder(value.Length + 8);

		for (var index = 0; index < value.Length; index++)
		{
			var currentChar = value[index];

			if (char.IsUpper(currentChar))
			{
				if (index > 0 && value[index - 1] != '_')
				{
					builder.Append('_');
				}

				builder.Append(char.ToLowerInvariant(currentChar));
				continue;
			}

			if (currentChar == '-' || currentChar == ' ')
			{
				builder.Append('_');
				continue;
			}

			builder.Append(char.ToLowerInvariant(currentChar));
		}

		return builder.ToString();
	}
}
