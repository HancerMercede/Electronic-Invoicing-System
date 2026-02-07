using ElectronicInvoicing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElectronicInvoicing.Infrastructure.Context;

public class RepositoryContext(DbContextOptions<RepositoryContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        base.OnModelCreating(modelBuilder);
    }
    
    protected DbSet<Company>  Companies { get; set; } 
    protected DbSet<Invoice>  Invoices { get; set; }
    protected DbSet<InvoiceItem>  InvoiceItems { get; set; }
}