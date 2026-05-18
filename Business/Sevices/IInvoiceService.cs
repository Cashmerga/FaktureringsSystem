using Business.Dtos.Invoices;
using Faktureringsys.Models;

namespace Business.Sevices
{
    public interface IInvoiceService
    {
        /// <summary>
        /// Creates a new invoice with validation and automatic price calculation
        /// </summary>
        /// <param name="dto">Invoice creation data</param>
        /// <returns>Created invoice with all navigation properties loaded</returns>
        /// <exception cref="Exception">Thrown when customer or products are not found</exception>
        Task<Invoice> CreateInvoiceAsync(CreateInvoiceDto dto);

        /// <summary>
        /// Updates an existing invoice
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <param name="dto">Invoice update data</param>
        /// <returns>Updated invoice</returns>
        /// <exception cref="Exception">Thrown when invoice, customer or products are not found</exception>
        Task<Invoice> UpdateInvoiceAsync(int id, UpdateInvoiceDto dto);

        /// <summary>
        /// Gets an invoice by ID with all navigation properties loaded
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <returns>Invoice or null if not found</returns>
        Task<Invoice?> GetInvoiceByIdAsync(int id);
    }
}
