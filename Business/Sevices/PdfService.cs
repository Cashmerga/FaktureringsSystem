using Faktureringsys.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Business.Sevices
{
    public class PdfService
    {
        public byte[] GenerateInvoicePdf(Invoice invoice, CompanyProfile company)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text(company.CompanyName)
                                .FontSize(20)
                                .Bold();

                            col.Item().Text($"Org nr: {company.OrganizationNumber}");

                            col.Item().Text(company.Street);
                            col.Item().Text($"{company.PostalCode} {company.City}");
                            col.Item().Text(company.Country);

                            col.Item().PaddingTop(5);

                            col.Item().Text($"Email: {company.Email}");
                            col.Item().Text($"Phone: {company.PhoneNumber}");
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("INVOICE")
                                .FontSize(24)
                                .Bold();

                            col.Item().Text($"Date: {invoice.CreatedAt:yyyy-MM-dd}");
                            col.Item().Text($"Invoice #: {invoice.Id}");

                            col.Item().PaddingTop(10);

                            col.Item().Text("Payment details")
                                .Bold();

                            if (!string.IsNullOrWhiteSpace(company.BankAccountNumber))
                                col.Item().Text($"Account: {company.BankAccountNumber}");

                            if (!string.IsNullOrWhiteSpace(company.ClearingNumber))
                                col.Item().Text($"Clearing: {company.ClearingNumber}");

                            if (!string.IsNullOrWhiteSpace(company.IBAN))
                                col.Item().Text($"IBAN: {company.IBAN}");

                            if (!string.IsNullOrWhiteSpace(company.SWIFT))
                                col.Item().Text($"SWIFT: {company.SWIFT}");
                        });
                    });

                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Item().Text("Customer")
                            .FontSize(16)
                            .Bold();

                        col.Item().Text(invoice.Customer.Name);
                        col.Item().Text(invoice.Customer.Email);

                        col.Item().PaddingTop(20);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Product").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Qty").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Price").Bold();
                                header.Cell().Element(CellStyle).AlignRight().Text("Total").Bold();
                            });

                            foreach (var item in invoice.Items)
                            {
                                table.Cell().Element(CellStyle)
                                    .Text(item.Product.Name);

                                table.Cell().Element(CellStyle)
                                    .AlignRight()
                                    .Text(item.Quantity.ToString());

                                table.Cell().Element(CellStyle)
                                    .AlignRight()
                                    .Text($"{item.UnitPrice:C}");

                                table.Cell().Element(CellStyle)
                                    .AlignRight()
                                    .Text($"{item.Quantity * item.UnitPrice:C}");
                            }

                            static IContainer CellStyle(IContainer container)
                            {
                                return container
                                    .PaddingVertical(5)
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Grey.Lighten2);
                            }
                        });

                        col.Item().AlignRight().PaddingTop(20).Column(total =>
                        {
                            total.Item()
                            .Text($"Subtotal: {invoice.SubTotal:C}");

                           total.Item()
                                .Text($"VAT ({invoice.VATPercentage}%): {invoice.VATAmount:C}");

                          total.Item()
                                .PaddingTop(5)
                                .Text($"Total: {invoice.TotalAmount:C}")
                                .FontSize(16)
                                .Bold();
                        });
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Thank you for your business");
                });
            });

            return document.GeneratePdf();
        }
    }
}
