using BlazorShared.Data.Contexts;
using BlazorShared.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorShared.Data.Configurations
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
