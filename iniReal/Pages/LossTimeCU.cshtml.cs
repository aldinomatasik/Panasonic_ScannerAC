using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LossTimeComponent.Services;
using LossTimeComponent.Models;
// Pastikan namespace ini sesuai dengan lokasi folder Anda
// Saya berasumsi ini ada di 'Pages/Scanner', jadi namespace-nya adalah:
namespace iniReal.Pages.Scanner
{
    [IgnoreAntiforgeryToken]
    public class LossTimeCUModel : PageModel // <- Nama Model diubah
    {
        private readonly LossTimeService _lossTimeService;
        public List<UnassignedLossEvent> LossEvents { get; set; }
        public LossTimeCUModel(LossTimeService lossTimeService)
        {
            _lossTimeService = lossTimeService;
        }
        public async Task OnGetAsync()
        {
            // **** PERUBAHAN DI SINI ****
            // Ambil data HANYA untuk "CU"
            LossEvents = await _lossTimeService.GetUnassignedLossEventsAsync("MCH1-01");
        }
        // --- Handler OnPost disalin persis ---
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostAssignReasonAsync()
        {
            if (!int.TryParse(Request.Form["lossTimeId"], out var lossTimeId))
            {
                return BadRequest("lossTimeId tidak ditemukan atau tidak valid.");
            }
            string reason = Request.Form["reason"];
            string detailedReason = Request.Form["detailedReason"];
            if (string.IsNullOrEmpty(reason))
            {
                return BadRequest("Reason tidak boleh kosong.");
            }
            await _lossTimeService.AssignLossTimeReasonAsync(lossTimeId, reason, detailedReason);
            // RedirectToPage() akan me-reload halaman ini (LossTimeCU)
            return RedirectToPage();
        }
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync()
        {
            if (!int.TryParse(Request.Form["lossTimeId"], out var lossTimeId))
            {
                return BadRequest("lossTimeId tidak ditemukan atau tidak valid.");
            }
            await _lossTimeService.DeleteLossTimeAsync(lossTimeId);
            // RedirectToPage() akan me-reload halaman ini (LossTimeCU)
            return RedirectToPage();
        }
    }
}