using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using AccountService.Data;
using AccountService.Models;

namespace AccountService.Migrations
{
    [DbContext(typeof(AccountDbContext))]
    [Migration("20170711021829_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("AccountService.Data.Account", b =>
                {
                    b.Property<long>("Id");

                    b.Property<decimal>("Value");

                    b.HasKey("Id");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("AccountService.Data.AccountChange", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("AccountId");

                    b.Property<long>("ActionLogId");

                    b.Property<decimal>("Value");

                    b.HasKey("Id");

                    b.HasIndex("ActionLogId");

                    b.ToTable("LastAccountChanges");
                });

            modelBuilder.Entity("AccountService.Data.ActionLog", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount");

                    b.Property<DateTime>("DateUtc");

                    b.Property<long>("Receiver");

                    b.Property<long>("Sender");

                    b.HasKey("Id");

                    b.ToTable("ActionLogs");
                });

            modelBuilder.Entity("AccountService.Data.AccountChange", b =>
                {
                    b.HasOne("AccountService.Data.ActionLog", "ActionLog")
                        .WithMany()
                        .HasForeignKey("ActionLogId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
