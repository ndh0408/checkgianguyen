using Microsoft.EntityFrameworkCore;
using GiaNguyenCheck.Data;
using GiaNguyenCheck.Entities;
using GiaNguyenCheck.Interfaces;

namespace GiaNguyenCheck.Repositories
{
    public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Payment>> GetByTenantIdAsync(int tenantId)
        {
            return await _dbSet
                .Where(p => p.TenantId == tenantId && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<Payment?> GetByReferenceCodeAsync(string referenceCode)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.ReferenceCode == referenceCode && !p.IsDeleted);
        }

        public async Task<Payment?> GetByTransactionIdAsync(string transactionId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.TransactionId == transactionId && !p.IsDeleted);
        }

        public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
        {
            return await _dbSet
                .Where(p => p.Status == PaymentStatus.Pending && !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetExpiredPaymentsAsync()
        {
            return await _dbSet
                .Where(p => p.Status == PaymentStatus.Pending && 
                           p.ExpiresAt.HasValue && 
                           p.ExpiresAt < DateTime.UtcNow && 
                           !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(int paymentId, PaymentStatus status, string? transactionId = null)
        {
            try
            {
                var payment = await GetByIdAsync(paymentId);
                if (payment == null)
                    return false;

                payment.Status = status;
                if (!string.IsNullOrEmpty(transactionId))
                    payment.TransactionId = transactionId;
                
                if (status == PaymentStatus.Success)
                    payment.PaidAt = DateTime.UtcNow;

                payment.UpdatedAt = DateTime.UtcNow;

                await UpdateAsync(payment);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
} 