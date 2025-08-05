using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Adidas.Context.Migrations
{
    /// <inheritdoc />
    public partial class fixUpdateTimesTamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-1111-2222-3333-444444444444",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "dd4e9741-0b86-446c-8987-bb3fbc5cd945", new DateTime(2025, 8, 5, 12, 56, 46, 109, DateTimeKind.Utc).AddTicks(4770), "AQAAAAIAAYagAAAAEA9Zvc3igJiA1WpzfofUClavNukLwlpRSQCM9IlCy7S2Aob3Po5MmugkzgM0u1MK6w==", "a05477ca-51e2-4cae-b3a1-0703791ad3c6" });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(8844), new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(8850) });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(8913), new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(8913) });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(8917), new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(8917) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(9029), new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(9029) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(9037), new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(9037) });

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(9366), new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(9366) });

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(9373), new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(9374) });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(9200), new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(9201) });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(9219), new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(9219) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(9140), new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(9123) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(9146), new DateTime(2025, 8, 5, 12, 56, 46, 201, DateTimeKind.Utc).AddTicks(9142) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "aaaaaaaa-1111-2222-3333-444444444444",
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ba754000-29e4-4b47-a7cb-4bd0012b729d", new DateTime(2025, 7, 29, 12, 36, 58, 52, DateTimeKind.Utc).AddTicks(4939), "AQAAAAIAAYagAAAAEEMD4hvo00ky0bZXEYBkwrYiaMj4iUZRdSi61JjWSFzlp201UyL4A5IucHH0cGhXdg==", "197786cc-5e55-42ea-94e5-be9f08dcf389" });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5651), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5657) });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5727), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5727) });

            migrationBuilder.UpdateData(
                table: "Brands",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5742), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5742) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5798), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5798) });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5803), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5804) });

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5980), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5980) });

            migrationBuilder.UpdateData(
                table: "ProductImages",
                keyColumn: "Id",
                keyValue: new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5989), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5989) });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: new Guid("88888888-8888-8888-8888-888888888888"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5939), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5940) });

            migrationBuilder.UpdateData(
                table: "ProductVariants",
                keyColumn: "Id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5948), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5948) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5883), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5872) });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("77777777-7777-7777-7777-777777777777"),
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5894), new DateTime(2025, 7, 29, 12, 36, 58, 117, DateTimeKind.Utc).AddTicks(5886) });
        }
    }
}
