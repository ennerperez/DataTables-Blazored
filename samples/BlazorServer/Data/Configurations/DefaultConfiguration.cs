using BlazorServer.Data.Contexts;
using BlazorServer.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorServer.Data.Configurations
{
    
    [DbContext(typeof(DefaultContext))]
    public sealed class SettingConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> e)
        {
            e.ToTable("Employees");

            e.Property(m => m.Id).ValueGeneratedOnAdd();
        }
    }
}
