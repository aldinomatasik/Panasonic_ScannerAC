using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LossTimeComponent.Services;
using LossTimeComponent.Models;
namespace iniReal.Pages.Scanner
{
    [IgnoreAntiforgeryToken]
    public class LossTimeCSModel : PageModel // <- Nama Model diubah
    {
        private readonly LossTimeService _lossTimeService;
        public List<UnassignedLossEvent> LossEvents { get; set; }
        public LossTimeCSModel(LossTimeService lossTimeService)
        {
            _lossTimeService = lossTimeService;
        }
        public async Task OnGetAsync()
        {
            // **** PERUBAHAN DI SINI ****
            // Ambil data HANYA untuk "CS"
            LossEvents = await _lossTimeService.GetUnassignedLossEventsAsync("MCH1-02");
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
            return RedirectToPage(); // Me-reload halaman LossTimeCS
        }
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync()
        {
            if (!int.TryParse(Request.Form["lossTimeId"], out var lossTimeId))
            {
                return BadRequest("lossTimeId tidak ditemukan atau tidak valid.");
            }
            await _lossTimeService.DeleteLossTimeAsync(lossTimeId);
            return RedirectToPage(); // Me-reload halaman LossTimeCS
        }
    }
}