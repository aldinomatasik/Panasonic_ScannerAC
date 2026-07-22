using iniReal.Pages.CS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using System.IO.Ports;
using LossTimeComponent.Services;
using static iniReal.Pages.CS.IndexModel;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace iniReal.Pages.CU
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly LossTimeService _lossTimeService;

        public IndexModel(IConfiguration configuration, LossTimeService lossTimeService)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _lossTimeService = lossTimeService;
        }



        string MachineCode = "MCH1-01";
        // MasterData <=> Product
        // OEESN <==> iniReal

        public List<ProductInfo> listCS = new List<ProductInfo>();
        public List<UserInfo> listUsers = new List<UserInfo>();
        public UserInfo iniUser = new UserInfo();
        public string errorMessage = "";
        public string successMessage = "";
        public string temp = "";
        public int prodplan = 0;
        public string? ProdName { get; set; }
        //private static string? RlossVal = null; // Menyimpan reason loss
        //private static DateTime? TStartLossVal = null; // Menyimpan waktu produk terakhir
        DateTime WaktuSkrg;
        DateTime WaktuLoss;

        public void OnGet()
        {
            // Retrieve error message from TempData if it exists
            if (TempData.TryGetValue("ErrorMessage", out var errorMessage))
            {
                if (errorMessage != null)
                {
                    this.errorMessage = errorMessage.ToString();
                }
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string selectData = "SELECT * FROM Masterdata WHERE MachineCode = @MachineCode";
                    using (SqlCommand command = new SqlCommand(selectData, connection))
                    {
                        command.Parameters.AddWithValue("@MachineCode", MachineCode);
                        using (SqlDataReader dataReader = command.ExecuteReader())
                        {
                            while (dataReader.Read())
                            {
                                ProductInfo productInfo = new ProductInfo();

                                if (!dataReader.IsDBNull(0))
                                    productInfo.Product_Id = dataReader.GetString(0);

                                if (!dataReader.IsDBNull(1))
                                    productInfo.Marking = dataReader.GetString(1);

                                if (!dataReader.IsDBNull(2))
                                    productInfo.ProductName = dataReader.GetString(2);

                                if (!dataReader.IsDBNull(3))
                                    productInfo.MachineName = dataReader.GetString(3);

                                if (!dataReader.IsDBNull(4))
                                    productInfo.Description = dataReader.GetString(4);

                                if (!dataReader.IsDBNull(5))
                                    productInfo.ProdPlan = dataReader.GetInt32(5);

                                if (!dataReader.IsDBNull(6))
                                    productInfo.SUT = dataReader.GetInt32(6);

                                if (!dataReader.IsDBNull(7))
                                    productInfo.NoOfOperator = dataReader.GetInt32(7);

                                if (!dataReader.IsDBNull(8))
                                    productInfo.QtyHour = dataReader.GetInt32(8);

                                if (!dataReader.IsDBNull(9))
                                    productInfo.ProdHeadHour = dataReader.GetInt32(9);

                                if (!dataReader.IsDBNull(10))
                                    productInfo.CycleTimeVacum = dataReader.GetInt32(10);

                                if (!dataReader.IsDBNull(11))
                                    productInfo.WorkHour = dataReader.GetInt32(11);

                                listCS.Add(productInfo);
                            }
                        }
                    }

                    string selectUserQrSql = "SELECT TOP 2000 * FROM OEESN WHERE MachineCode = @MachineCode ORDER BY Date DESC";
                    //string selectUserQrSql = "SELECT * FROM OEESN ORDER BY Date DESC";
                    using (SqlCommand selectUserQrCommand = new SqlCommand(selectUserQrSql, connection))
                    {
                        selectUserQrCommand.Parameters.AddWithValue("@MachineCode", MachineCode);
                        using (SqlDataReader userQrReader = selectUserQrCommand.ExecuteReader())
                        {
                            while (userQrReader.Read())
                            {
                                UserInfo userInfo = new UserInfo();

                                // Gunakan pengecekan IsDBNull untuk setiap kolom
                                if (!userQrReader.IsDBNull(0)) userInfo.Date = userQrReader.GetDateTime(0);
                                if (!userQrReader.IsDBNull(1)) userInfo.SDate = userQrReader.GetDateTime(1);
                                if (!userQrReader.IsDBNull(2)) userInfo.EndDate = userQrReader.GetDateTime(2);

                                // Untuk Decimal, Int, dan String lainnya juga harus dicek:
                                if (!userQrReader.IsDBNull(3)) userInfo.ProductTime = userQrReader.GetDecimal(3);
                                if (!userQrReader.IsDBNull(4)) userInfo.TotalDownTime = userQrReader.GetDecimal(4);
                                if (!userQrReader.IsDBNull(5)) userInfo.TargetUnit = userQrReader.GetDecimal(5);
                                if (!userQrReader.IsDBNull(6)) userInfo.GoodUnit = userQrReader.GetDecimal(6);
                                if (!userQrReader.IsDBNull(7)) userInfo.EjectUnit = userQrReader.GetDecimal(7);
                                if (!userQrReader.IsDBNull(8)) userInfo.TotalUnit = userQrReader.GetDecimal(8);
                                if (!userQrReader.IsDBNull(9)) userInfo.OEE = userQrReader.GetDecimal(9);
                                if (!userQrReader.IsDBNull(10)) userInfo.Availability = userQrReader.GetDecimal(10);
                                if (!userQrReader.IsDBNull(11)) userInfo.Performance = userQrReader.GetDecimal(11);
                                if (!userQrReader.IsDBNull(12)) userInfo.Quality = userQrReader.GetDecimal(12);
                                if (!userQrReader.IsDBNull(13)) userInfo.CycleTime = userQrReader.GetInt32(13);

                                // Gunakan IsDBNull untuk string
                                if (!userQrReader.IsDBNull(14)) userInfo.MachineCode = userQrReader.GetString(14);
                                if (!userQrReader.IsDBNull(15)) userInfo.Product_Id = userQrReader.GetString(15);

                                if (!userQrReader.IsDBNull(16)) userInfo.NoOfOperator = userQrReader.GetInt32(16);
                                if (!userQrReader.IsDBNull(17)) userInfo.P_Target = userQrReader.GetDecimal(17);
                                if (!userQrReader.IsDBNull(18)) userInfo.P_Actual = userQrReader.GetDecimal(18);
                                if (!userQrReader.IsDBNull(19)) userInfo.IdleTime = userQrReader.GetDecimal(19);

                                if (!userQrReader.IsDBNull(20)) userInfo.SN_GOOD = userQrReader.GetString(20);
                                if (!userQrReader.IsDBNull(21)) userInfo.ID = userQrReader.GetInt32(21);

                                listUsers.Add(userInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }
        }

        public bool IsSnSequential(string currentSN, string previousSN)
        {
            // Hilangkan prefix huruf sebelum parse
            string cleanCurrent = currentSN.TrimStart('F', 'f');
            string cleanPrevious = previousSN.TrimStart('F', 'f');

            if (double.TryParse(cleanCurrent, out double currentNum) &&
                double.TryParse(cleanPrevious, out double previousNum))
            {
                return currentNum == previousNum + 1;
            }
            return true; // Kalau tidak bisa parse, skip warning
        }
        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Bagian validasi input
            string serialNumInput = Request.Form["SerialNum"];
            string operatInput = Request.Form["OP"];
            string prodPlanInput = Request.Form["PP"];
            string idleInput = Request.Form["IT"];
            string shiftModeInput = Request.Form["ShiftMode"];
            string isHolidayInput = Request.Form["IsHoliday"]; // ← BARU: dikirim dari popup Hari Kerja/Hari Libur
            string currentShiftMode;

            // ==== PENENTUAN SHIFT / OVERTIME ====
            // shiftModeInput yang dikirim FE sekarang berupa kode periode yang dipilih user
            // lewat popup: "NS", "1", "2", "3". Backend adalah sumber kebenaran:
            // jika jam saat ini TIDAK berada dalam window periode yang dipilih,
            // ATAU kalau user menandai hari ini sebagai Hari Libur,
            // otomatis dicatat sebagai OVERTIME.
            DateTime now = DateTime.Now;

            // ← BARU: flag hari libur, dikirim manual dari popup FE ("true"/"false")
            bool isHoliday = string.Equals(isHolidayInput, "true", StringComparison.OrdinalIgnoreCase);

            DateTime nsStart = now.Date.AddHours(7);
            DateTime nsEnd = now.Date.AddHours(16);

            DateTime shift1Start = now.Date.AddHours(7);
            DateTime shift1End = now.Date.AddHours(15).AddMinutes(45);
            DateTime shift2Start = now.Date.AddHours(15).AddMinutes(45);
            DateTime shift2End = now.Date.AddHours(23).AddMinutes(15);
            DateTime shift3Start = now.Date.AddHours(23).AddMinutes(15);
            DateTime shift3End = now.Date.AddDays(1).AddHours(7);
            DateTime shift3Start2 = now.Date.AddDays(-1).AddHours(23).AddMinutes(15);
            DateTime shift3End2 = now.Date.AddHours(7);

            bool isWithinWindow;

            switch (shiftModeInput)
            {
                case "NS":
                    isWithinWindow = (now >= nsStart && now < nsEnd) && !isHoliday;
                    currentShiftMode = isWithinWindow ? "NON-SHIFT" : "OVERTIME";
                    break;
                case "1":
                    isWithinWindow = (now >= shift1Start && now < shift1End) && !isHoliday;
                    currentShiftMode = isWithinWindow ? "SHIFT 1" : "OVERTIME SHIFT 1";
                    break;
                case "2":
                    isWithinWindow = (now >= shift2Start && now < shift2End) && !isHoliday;
                    currentShiftMode = isWithinWindow ? "SHIFT 2" : "OVERTIME SHIFT 2";
                    break;
                case "3":
                    isWithinWindow = ((now >= shift3Start && now < shift3End) || (now >= shift3Start2 && now < shift3End2)) && !isHoliday;
                    currentShiftMode = isWithinWindow ? "SHIFT 3" : "OVERTIME SHIFT 3";
                    break;
                default:
                    // Tidak ada periode terpilih / nilai tidak dikenali -> catat sebagai OVERTIME
                    currentShiftMode = "OVERTIME";
                    break;
            }
            // ==== END PENENTUAN SHIFT / OVERTIME ====

            if (!string.IsNullOrEmpty(serialNumInput) && serialNumInput.Contains("ERROR"))
            {
                serialNumInput = serialNumInput.Replace("ERROR", "");
            }

            if (!string.IsNullOrEmpty(serialNumInput))
            {
                int indexOfPercent = serialNumInput.IndexOf("%");
                if (indexOfPercent != -1)
                {
                    serialNumInput = serialNumInput.Substring(indexOfPercent + 1);
                }

                // Jaga-jaga jika masih ada % (double scan)
                int secondPercent = serialNumInput.IndexOf("%");
                if (secondPercent != -1)
                {
                    serialNumInput = serialNumInput.Substring(0, secondPercent);
                }
            }

            // UPDATE: Menambahkan panjang 23 ke dalam validasi agar contoh SN Anda bisa masuk
            if (serialNumInput?.Length != 10 && serialNumInput?.Length != 11
    && serialNumInput?.Length != 21 && serialNumInput?.Length != 22  // ← tambah ini
    && serialNumInput?.Length != 23)
            {
                TempData["ErrorMessage"] = "Serial Number Tidak valid (Panjang: " + serialNumInput?.Length + ")";
                return RedirectToPage();
            }

            iniUser.SN_GOOD = string.IsNullOrEmpty(serialNumInput) ? null : serialNumInput;
            if (string.IsNullOrEmpty(iniUser.SN_GOOD))
            {
                TempData["ErrorMessage"] = "Masukkan Serial Number";
                return RedirectToPage();
            }
            if (!int.TryParse(operatInput, out int operatValue) || operatValue == 0)
            {
                TempData["ErrorMessage"] = "Jumlah Operator Tidak Boleh Kosong";
                return RedirectToPage();
            }
            if (!int.TryParse(prodPlanInput, out int prodPlanValue) || prodPlanValue == 0)
            {
                TempData["ErrorMessage"] = "Jumlah Production Plan Tidak Boleh Kosong";
                return RedirectToPage();
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // 2. Logika untuk mendapatkan detail produk (DIPERBARUI)
                    // Menambahkan logika pencarian berdasarkan karakter ke-7 s.d 11
                    string selectDataSql = @"
SELECT TOP 1 Product_Id, MachineCode, SUT FROM Masterdata
WHERE MachineCode = @MachineCode AND
      ((@EmbeddedPrefix <> '' AND Product_Id LIKE @EmbeddedPrefix + '%') OR
       Product_Id LIKE @SerialNumPrefix7 + '%' OR
       Product_Id LIKE @SerialNumPrefix5 + '%' OR
       Product_Id = @SerialNumPrefix3)
ORDER BY CASE
    WHEN @EmbeddedPrefix <> '' AND Product_Id LIKE @EmbeddedPrefix + '%' THEN 1
    WHEN Product_Id LIKE @SerialNumPrefix7 + '%' THEN 2
    WHEN Product_Id LIKE @SerialNumPrefix5 + '%' THEN 3
    WHEN Product_Id = @SerialNumPrefix3 THEN 4
    ELSE 5
END;";

                    int SUT = 0;
                    using (SqlCommand selectDataCommand = new SqlCommand(selectDataSql, connection))
                    {
                        string serialNum = iniUser.SN_GOOD;

                        string embeddedPrefix = "";
                        if (serialNum.Length >= 11)
                        {
                            embeddedPrefix = serialNum.Substring(6, 5).ToUpper();
                        }

                        selectDataCommand.Parameters.AddWithValue("@MachineCode", MachineCode);
                        selectDataCommand.Parameters.AddWithValue("@EmbeddedPrefix", embeddedPrefix);
                        selectDataCommand.Parameters.AddWithValue("@SerialNumPrefix7",
                            (serialNum.Length >= 7 ? serialNum.Substring(0, 7) : serialNum).ToUpper());
                        selectDataCommand.Parameters.AddWithValue("@SerialNumPrefix5",
                            (serialNum.Length >= 5 ? serialNum.Substring(0, 5) : serialNum).ToUpper());
                        selectDataCommand.Parameters.AddWithValue("@SerialNumPrefix3",
                            (serialNum.Length >= 3 ? serialNum.Substring(0, 3) : serialNum).ToUpper());

                        using (SqlDataReader dataReader = await selectDataCommand.ExecuteReaderAsync())
                        {
                            if (await dataReader.ReadAsync())
                            {
                                iniUser.Product_Id = dataReader.GetString(0);
                                iniUser.MachineCode = dataReader.GetString(1);
                                if (!dataReader.IsDBNull(2))
                                {
                                    SUT = dataReader.GetInt32(2);
                                }
                            }
                        }

                        Console.WriteLine($"[DEBUG] Product_Id hasil query: '{iniUser.Product_Id}', SUT: {SUT}");
                    }

                    // Jika SUT tidak ada di query pertama, ambil secara terpisah
                    string selectSUTSql = "SELECT SUT FROM Masterdata WHERE Product_Id = @Product_Id;";
                    using (SqlCommand selectSUTCommand = new SqlCommand(selectSUTSql, connection))
                    {
                        selectSUTCommand.Parameters.AddWithValue("@Product_Id", iniUser.Product_Id);
                        var sutResult = await selectSUTCommand.ExecuteScalarAsync();
                        if (sutResult != null) SUT = (int)sutResult;
                    }
                    string checkDuplicateSql = "SELECT COUNT(*) FROM OEESN WHERE SN_GOOD = @SN_GOOD AND MachineCode = @MachineCode";
                    using (SqlCommand checkDupCmd = new SqlCommand(checkDuplicateSql, connection))
                    {
                        checkDupCmd.Parameters.AddWithValue("@SN_GOOD", iniUser.SN_GOOD);
                        checkDupCmd.Parameters.AddWithValue("@MachineCode", MachineCode);
                        int existingCount = (int)await checkDupCmd.ExecuteScalarAsync();
                        if (existingCount > 0)
                        {
                            TempData["ErrorMessage"] = $"⚠️ DUPLIKAT! Serial Number '{iniUser.SN_GOOD}' sudah pernah discan sebelumnya!";
                            return RedirectToPage();
                        }
                    }

                    // --- BAGIAN BAWAH TETAP SAMA SEPERTI KODE ASLI ---
                    int cycleTime = (operatValue > 0) ? (SUT * 60 / operatValue) : 0;
                    decimal idleValue = 0;

                    DateTime currentProductTime = DateTime.Now;
                    DateTime previousProductTime = currentProductTime;

                    string sql = @"
            SELECT TOP 1 SDate 
            FROM OEESN
            WHERE MachineCode = @MachineCode
            ORDER BY SDate DESC";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@MachineCode", MachineCode);
                        var result = await command.ExecuteScalarAsync();

                        if (result != null && result != DBNull.Value)
                        {
                            previousProductTime = (DateTime)result;
                        }
                    }

                    TimeSpan idleDuration = currentProductTime - previousProductTime;

                    if (idleDuration.TotalHours > 0 && idleDuration.TotalHours <= 2)
                    {
                        double netDowntimeSeconds = CalculateNetDowntimeSeconds(previousProductTime, currentProductTime);
                        double thresholdSeconds = cycleTime * 3;

                        if (netDowntimeSeconds > thresholdSeconds)
                        {
                            DateTime tentativeLossStart = SkipRestTime(previousProductTime);
                            DateTime lossEndTime = currentProductTime;
                            double pureLossSeconds = netDowntimeSeconds - cycleTime;

                            if (pureLossSeconds > 0)
                            {
                                await _lossTimeService.LogUnassignedLossTimeAsync(
                                    MachineCode,
                                    tentativeLossStart,
                                    lossEndTime,
                                    pureLossSeconds
                                );
                                idleValue = (decimal)cycleTime;
                            }
                            else
                            {
                                idleValue = (decimal)cycleTime;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[INFO] Net Idle ({netDowntimeSeconds} detik) <= {thresholdSeconds} (CycleTime x 3). Dianggap tidak ada Loss Time.");
                            idleValue = (decimal)(netDowntimeSeconds);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] IdleDuration di luar batas (lebih dari 2 jam): {idleDuration.TotalHours} jam");
                    }

                    string insertUserQrSql = @"INSERT INTO OEESN (Date, SDate, EndDate, ProductTime, TotalDownTime, TargetUnit, GoodUnit, EjectUnit, TotalUnit, OEE, 
                   Availability, Performance, Quality, CycleTime, MachineCode, Product_Id, NoOfOperator, P_Target, P_Actual, IdleTime, SN_GOOD, ShiftMode) 
                   VALUES (@Date, @SDate, @EndDate, @ProductTime, @TotalDownTime, @TargetUnit, @GoodUnit, @EjectUnit, (@GoodUnit + @EjectUnit), @OEE, 
                   @Availability, @Performance, @Quality, @CycleTime, @MachineCode, @Product_Id, @NoOfOperator, @P_Target, @P_Actual, @IdleTime, @SN_GOOD, @ShiftMode);";

                    using (SqlCommand insertUserQrCommand = new SqlCommand(insertUserQrSql, connection))
                    {
                        DateTime currentTimeForTarget = DateTime.Now;
                        DateTime shiftStartTime = currentTimeForTarget.Date.AddHours(7);
                        if (currentTimeForTarget.Hour < 7)
                        {
                            shiftStartTime = shiftStartTime.AddDays(-1);
                        }
                        double effectiveWorkingSeconds = CalculateNetDowntimeSeconds(shiftStartTime, currentTimeForTarget);
                        decimal calculatedTargetUnit = 0;
                        if (SUT > 0)
                        {
                            calculatedTargetUnit = Math.Floor((decimal)effectiveWorkingSeconds / SUT);
                        }

                        int dataAddedToday = CountDataAddedToday(connection);
                        int ejectUnit = 0;
                        int totalUnit = dataAddedToday + ejectUnit;
                        decimal performance = (calculatedTargetUnit > 0) ? ((decimal)totalUnit / calculatedTargetUnit) * 100 : 0;
                        decimal quality = (totalUnit > 0) ? (decimal)dataAddedToday / totalUnit * 100 : 0;
                        decimal p_actual = (cycleTime > 0) ? 3600m / (cycleTime * 1000m) : 0;

                        insertUserQrCommand.Parameters.AddWithValue("@Date", DateTime.Now);
                        insertUserQrCommand.Parameters.AddWithValue("@SDate", DateTime.Now);
                        insertUserQrCommand.Parameters.AddWithValue("@EndDate", DateTime.Now);
                        insertUserQrCommand.Parameters.AddWithValue("@ProductTime", SUT);
                        insertUserQrCommand.Parameters.AddWithValue("@TotalDownTime", 0);
                        insertUserQrCommand.Parameters.AddWithValue("@TargetUnit", calculatedTargetUnit);
                        insertUserQrCommand.Parameters.AddWithValue("@GoodUnit", dataAddedToday);
                        insertUserQrCommand.Parameters.AddWithValue("@EjectUnit", ejectUnit);
                        insertUserQrCommand.Parameters.AddWithValue("@OEE", 0);
                        insertUserQrCommand.Parameters.AddWithValue("@Availability", 0);
                        insertUserQrCommand.Parameters.AddWithValue("@Performance", performance);
                        insertUserQrCommand.Parameters.AddWithValue("@Quality", quality);
                        insertUserQrCommand.Parameters.AddWithValue("@CycleTime", cycleTime);
                        insertUserQrCommand.Parameters.AddWithValue("@MachineCode", iniUser.MachineCode);
                        insertUserQrCommand.Parameters.AddWithValue("@Product_Id", iniUser.Product_Id);
                        insertUserQrCommand.Parameters.AddWithValue("@NoOfOperator", operatValue);
                        insertUserQrCommand.Parameters.AddWithValue("@P_Target", 4);
                        insertUserQrCommand.Parameters.AddWithValue("@P_Actual", p_actual);
                        insertUserQrCommand.Parameters.AddWithValue("@IdleTime", idleValue);
                        insertUserQrCommand.Parameters.AddWithValue("@SN_GOOD", iniUser.SN_GOOD);
                        insertUserQrCommand.Parameters.AddWithValue("@ShiftMode", currentShiftMode);

                        await insertUserQrCommand.ExecuteNonQueryAsync();
                    }

                    string checkserialnum2 = @"SELECT TOP 2 SN_GOOD FROM OEESN WHERE Product_Id = @Product_Id AND MachineCode = @MachineCode ORDER BY SDate DESC;";
                    using (SqlCommand command = new SqlCommand(checkserialnum2, connection))
                    {
                        command.Parameters.AddWithValue("@Product_Id", iniUser.Product_Id);
                        command.Parameters.AddWithValue("@MachineCode", MachineCode);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            List<string> serialnumbers = new List<string>();
                            while (reader.Read())
                            {
                                serialnumbers.Add(reader.GetString(0));
                            }
                            if (serialnumbers.Count == 2)
                            {
                                string currentsn = Regex.Match(serialnumbers[0], @"\d+$").Value;
                                string previoussn = Regex.Match(serialnumbers[1], @"\d+$").Value;

                                if (!string.IsNullOrEmpty(currentsn) && !string.IsNullOrEmpty(previoussn))
                                {
                                    if (!IsSnSequential(currentsn, previoussn))
                                    {
                                        TempData["errormessage"] = "Serial Number tidak Berurutan";
                                    }
                                }
                            }
                        }
                    }

                    string dataWaktuSNTerbaru = @"SELECT TOP 2 SDate FROM OEESN WHERE Product_Id = @Product_Id AND MachineCode = @MachineCode ORDER BY SDate DESC;";
                    using (SqlCommand commandWaktuSNTerbaru = new SqlCommand(dataWaktuSNTerbaru, connection))
                    {
                        commandWaktuSNTerbaru.Parameters.AddWithValue("@Product_Id", iniUser.Product_Id);
                        commandWaktuSNTerbaru.Parameters.AddWithValue("@MachineCode", MachineCode);

                        using (SqlDataReader reader = commandWaktuSNTerbaru.ExecuteReader())
                        {
                            List<DateTime> WaktuBaru = new List<DateTime>();
                            while (reader.Read())
                            {
                                WaktuBaru.Add(reader.GetDateTime(0));
                            }
                            if (WaktuBaru.Count == 2)
                            {
                                WaktuSkrg = WaktuBaru[0];
                                WaktuLoss = WaktuBaru[1];
                            }
                        }
                    }
                }
                return RedirectToPage();
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    TempData["ErrorMessage"] = "Serial Number Sudah Ada (dicek oleh database).";
                }
                else
                {
                    TempData["ErrorMessage"] = "Terjadi error database: " + ex.Message;
                }
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Terjadi kesalahan: " + ex.Message;
                return RedirectToPage();
            }
        }

        // ============================================================
        // FITUR MISSING SERIAL
        // - Grouping berbasis Product_Id resmi dari Masterdata (bukan
        //   Substring(0,5) serial mentah) supaya model dengan prefix
        //   5-karakter yang sama (mis. 4216000 vs 4216001 vs 4216002)
        //   tidak tercampur.
        // - Trailing-digit generic parsing: ambil digit run di ujung
        //   SETIAP serial (bukan cuma suffix setelah Product_Id), jadi
        //   format campur huruf (mis. "4216001D1242890052779") tetap
        //   terbaca, tidak hilang begitu saja dari perhitungan.
        // - Gap-based detection: cuma cek gap ANTAR PASANGAN serial yang
        //   bersebelahan (bukan fill seluruh range min..max). Gap kecil
        //   (<= GAP_THRESHOLD) dianggap beneran hilang; gap gede (ganti
        //   lot/model/counter reset) di-skip, bukan dipaksa isi (yang
        //   bisa bikin server hang karena loop jutaan iterasi).
        // - Serial Pertama/Terakhir per-model: setiap entri di dropdown
        //   model juga membawa firstSerial/lastSerial miliknya SENDIRI
        //   (hasil scan pertama & terakhir untuk model itu di periode
        //   ini), supaya saat user filter ke model tertentu di FE,
        //   Serial Pertama/Terakhir bisa ikut berganti ke punya model
        //   itu (bukan punya keseluruhan periode).
        // ============================================================

        private const long MISSING_SERIAL_GAP_THRESHOLD = 50;

        // ─── Ambil daftar Product_Id resmi dari Masterdata, urut dari yang PALING PANJANG ───
        // Urutan panjang penting supaya prefix yang lebih spesifik (mis. "4216000")
        // dicoba/match duluan sebelum prefix yang lebih pendek/umum (mis. "421").
        private async Task<List<(string ProductId, string ProductName)>> GetMasterProductsAsync(SqlConnection connection, string machineCode)
        {
            var list = new List<(string, string)>();
            string sql = "SELECT Product_Id, ProductName FROM Masterdata WHERE MachineCode = @MachineCode";
            using (SqlCommand cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@MachineCode", machineCode);
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            string pid = reader.GetString(0);
                            string pname = reader.IsDBNull(1) ? "" : reader.GetString(1);
                            list.Add((pid, pname));
                        }
                    }
                }
            }
            return list.OrderByDescending(p => p.Item1.Length).ToList();
        }

        // ─── Cocokkan satu serial number ke Product_Id resmi (bukan tebak 5 karakter) ───
        private (string ProductId, string ProductName) MatchProduct(string sn, List<(string ProductId, string ProductName)> masterProducts)
        {
            foreach (var p in masterProducts)
            {
                if (sn.StartsWith(p.ProductId, StringComparison.OrdinalIgnoreCase))
                    return p;
            }
            // Fallback kalau memang tidak ada Product_Id yang match sama sekali
            string fallbackPrefix = sn.Length >= 5 ? sn.Substring(0, 5).ToUpper() : sn.ToUpper();
            return (fallbackPrefix, "Model Tidak Dikenal");
        }

        // ─── GET MISSING SERIALS (dipanggil oleh showShiftEndModal -> fetchCuMissingSerials) ───
        public async Task<IActionResult> OnGetMissingSerialsAsync(string machineCode, string startTime, string endTime)
        {
            try
            {
                if (!DateTime.TryParse(startTime, out DateTime start) || !DateTime.TryParse(endTime, out DateTime end))
                {
                    return new JsonResult(new { error = "Format waktu tidak valid" });
                }

                string firstSerial = null;
                string lastSerial = null;
                int totalToday = 0;

                // hasil akhir: list of { serial, productId, productName }
                var missingResult = new List<object>();
                // ringkasan model yang muncul di periode ini, buat isi dropdown FE
                // (plus firstSerial/lastSerial MILIK MODEL ITU SENDIRI di periode ini)
                var modelSummary = new Dictionary<string, (string ProductName, int MissingCount, string FirstSerial, string LastSerial)>();

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // 1. Ambil daftar Product_Id resmi untuk mesin ini dari Masterdata
                    var masterProducts = await GetMasterProductsAsync(connection, machineCode);

                    // 2. Ambil semua serial dalam window waktu periode ini, diurutkan
                    // berdasarkan WAKTU SCAN (SDate ASC) supaya bisa dapat serial
                    // pertama & terakhir yang benar-benar discan di periode ini.
                    string sql = @"
                SELECT SN_GOOD
                FROM OEESN
                WHERE SDate >= @StartTime AND SDate < @EndTime
                  AND MachineCode = @MachineCode
                  AND SN_GOOD IS NOT NULL
                ORDER BY SDate ASC";

                    var chronological = new List<string>(); // urut waktu scan (utk serial pertama/terakhir)

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@StartTime", start);
                        command.Parameters.AddWithValue("@EndTime", end);
                        command.Parameters.AddWithValue("@MachineCode", machineCode);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                if (!reader.IsDBNull(0))
                                    chronological.Add(reader.GetString(0));
                            }
                        }
                    }

                    if (chronological.Count > 0)
                    {
                        firstSerial = chronological.First();
                        lastSerial = chronological.Last();
                        totalToday = chronological.Count;
                    }

                    if (chronological.Count == 0)
                    {
                        return new JsonResult(new { serials = Array.Empty<object>(), models = Array.Empty<object>(), firstSerial, lastSerial, totalToday });
                    }

                    // ── Grouping berdasarkan Product_Id hasil MATCH ke Masterdata ──
                    var groups = chronological
                        .Select(sn => new { Serial = sn, Match = MatchProduct(sn, masterProducts) })
                        .GroupBy(x => x.Match.ProductId);

                    foreach (var group in groups)
                    {
                        string productId = group.Key;
                        string productName = group.First().Match.ProductName;

                        // Serial pertama & terakhir milik MODEL INI SENDIRI di periode ini
                        // (group sudah otomatis urut sesuai waktu scan, karena "chronological"
                        // dibaca ORDER BY SDate ASC dan GroupBy menjaga urutan sumbernya).
                        string groupFirstSerial = group.First().Serial;
                        string groupLastSerial = group.Last().Serial;

                        // ── Ambil trailing digits dari SETIAP serial (generik, bukan cuma
                        //    dari suffix setelah Product_Id). nonDigitPart = semua karakter
                        //    sebelum digit run terakhir (bisa cuma productId, atau
                        //    productId + huruf, mis. "4216001D") ──
                        var parsed = new List<(string nonDigitPart, long trailingNum, int digitLen, string original)>();

                        foreach (var item in group)
                        {
                            string sn = item.Serial;
                            var m = Regex.Match(sn, @"\d+$"); // digit run di paling ujung
                            if (!m.Success) continue;

                            string nonDigitPart = sn.Substring(0, sn.Length - m.Length);
                            if (long.TryParse(m.Value, out long num))
                            {
                                parsed.Add((nonDigitPart, num, m.Length, sn));
                            }
                        }

                        if (parsed.Count < 2) continue; // butuh minimal 2 titik buat ada "gap"

                        // ── Sub-grouping berdasarkan nonDigitPart, biar "4216001" dan
                        //    "4216001D" tetap dianggap dua deret nomor yang beda ──
                        var subGroups = parsed.GroupBy(x => x.nonDigitPart);

                        foreach (var sub in subGroups)
                        {
                            var sorted = sub.OrderBy(x => x.trailingNum).ToList();
                            if (sorted.Count < 2) continue;

                            string nonDigitPart = sub.Key;
                            int missingInGroup = 0;

                            for (int i = 0; i < sorted.Count - 1; i++)
                            {
                                long current = sorted[i].trailingNum;
                                long next = sorted[i + 1].trailingNum;
                                long gap = next - current;

                                if (gap <= 1) continue; // berurutan, tidak ada yang hilang

                                if (gap > MISSING_SERIAL_GAP_THRESHOLD)
                                {
                                    // Lompatan gede = kemungkinan besar ganti lot/model/counter reset,
                                    // BUKAN serial hilang. Skip, jangan dipaksa isi (jaga performa juga).
                                    continue;
                                }

                                int digitLen = sorted[i].digitLen;
                                for (long n = current + 1; n < next; n++)
                                {
                                    string paddedNum = n.ToString().PadLeft(digitLen, '0');
                                    string missingSn = nonDigitPart + paddedNum;

                                    missingResult.Add(new { serial = missingSn, productId, productName });
                                    missingInGroup++;
                                }
                            }

                            if (missingInGroup > 0)
                            {
                                if (modelSummary.ContainsKey(productId))
                                {
                                    var existing = modelSummary[productId];
                                    modelSummary[productId] = (productName, existing.MissingCount + missingInGroup, groupFirstSerial, groupLastSerial);
                                }
                                else
                                {
                                    modelSummary[productId] = (productName, missingInGroup, groupFirstSerial, groupLastSerial);
                                }
                            }
                        }
                    }
                }

                var modelsForDropdown = modelSummary
                    .Select(kv => new
                    {
                        productId = kv.Key,
                        productName = kv.Value.ProductName,
                        missingCount = kv.Value.MissingCount,
                        firstSerial = kv.Value.FirstSerial,
                        lastSerial = kv.Value.LastSerial
                    })
                    .OrderByDescending(m => m.missingCount)
                    .ToList();

                return new JsonResult(new { serials = missingResult, models = modelsForDropdown, firstSerial, lastSerial, totalToday });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception OnGetMissingSerialsAsync (CU): " + ex.ToString());
                return new JsonResult(new { error = ex.Message });
            }
        }

        // ─── ADD MISSING SERIAL (dipanggil oleh addCuSerial di JS) ───
        public class AddCuSerialRequest
        {
            public string MachineCode { get; set; }
            public string SerialNumber { get; set; }
            public string ProductId { get; set; }   // dikirim dari FE, hasil match yang sudah pasti benar
            public string StartTime { get; set; }
            public string EndTime { get; set; }
        }

        public async Task<IActionResult> OnPostAddSerialAsync([FromBody] AddCuSerialRequest req)
        {
            try
            {
                if (!DateTime.TryParse(req.StartTime, out DateTime start) || !DateTime.TryParse(req.EndTime, out DateTime end))
                {
                    return new JsonResult(new { success = false, error = "Format waktu tidak valid" });
                }

                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Cek duplikat dulu
                    string checkSql = "SELECT COUNT(*) FROM OEESN WHERE SN_GOOD = @SN_GOOD AND MachineCode = @MachineCode";
                    using (SqlCommand checkCmd = new SqlCommand(checkSql, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@SN_GOOD", req.SerialNumber);
                        checkCmd.Parameters.AddWithValue("@MachineCode", req.MachineCode);
                        int existing = (int)await checkCmd.ExecuteScalarAsync();
                        if (existing > 0)
                        {
                            return new JsonResult(new { success = false, error = "Serial sudah ada di database" });
                        }
                    }

                    // productId diutamakan dari hasil matching FE (req.ProductId) yang sudah akurat.
                    // Data referensi lain (ShiftMode, TargetUnit, dll) tetap diambil dari row terdekat di window yang sama.
                    string productId = !string.IsNullOrEmpty(req.ProductId) ? req.ProductId : null;
                    string shiftMode = "";
                    decimal targetUnit = 0, goodUnit = 0;
                    int noOfOperator = 0, cycleTime = 0;

                    string refSql = @"
                SELECT TOP 1 Product_Id, ShiftMode, TargetUnit, GoodUnit, NoOfOperator, CycleTime
                FROM OEESN
                WHERE SDate >= @StartTime AND SDate < @EndTime
                  AND MachineCode = @MachineCode
                ORDER BY SDate DESC";

                    using (SqlCommand refCmd = new SqlCommand(refSql, connection))
                    {
                        refCmd.Parameters.AddWithValue("@StartTime", start);
                        refCmd.Parameters.AddWithValue("@EndTime", end);
                        refCmd.Parameters.AddWithValue("@MachineCode", req.MachineCode);

                        using (SqlDataReader reader = await refCmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                if (productId == null)
                                    productId = reader.IsDBNull(0) ? null : reader.GetString(0);
                                shiftMode = reader.IsDBNull(1) ? "" : reader.GetString(1);
                                targetUnit = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                                goodUnit = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                                noOfOperator = reader.IsDBNull(4) ? 0 : reader.GetInt32(4);
                                cycleTime = reader.IsDBNull(5) ? 0 : reader.GetInt32(5);
                            }
                        }
                    }

                    if (productId == null)
                    {
                        return new JsonResult(new { success = false, error = "Tidak ada data referensi di periode ini untuk copy Product_Id" });
                    }

                    string insertSql = @"
                INSERT INTO OEESN (Date, SDate, EndDate, ProductTime, TotalDownTime, TargetUnit, GoodUnit, EjectUnit, TotalUnit, OEE,
                    Availability, Performance, Quality, CycleTime, MachineCode, Product_Id, NoOfOperator, P_Target, P_Actual, IdleTime, SN_GOOD, ShiftMode)
                VALUES (@Date, @SDate, @EndDate, 0, 0, @TargetUnit, @GoodUnit, 0, @GoodUnit, 0,
                    0, 0, 0, @CycleTime, @MachineCode, @Product_Id, @NoOfOperator, 0, 0, 0, @SN_GOOD, @ShiftMode)";

                    using (SqlCommand insCmd = new SqlCommand(insertSql, connection))
                    {
                        DateTime insertTime = start; // taruh di dalam window periode
                        insCmd.Parameters.AddWithValue("@Date", insertTime);
                        insCmd.Parameters.AddWithValue("@SDate", insertTime);
                        insCmd.Parameters.AddWithValue("@EndDate", insertTime);
                        insCmd.Parameters.AddWithValue("@TargetUnit", targetUnit);
                        insCmd.Parameters.AddWithValue("@GoodUnit", goodUnit);
                        insCmd.Parameters.AddWithValue("@CycleTime", cycleTime);
                        insCmd.Parameters.AddWithValue("@MachineCode", req.MachineCode);
                        insCmd.Parameters.AddWithValue("@Product_Id", productId);
                        insCmd.Parameters.AddWithValue("@NoOfOperator", noOfOperator);
                        insCmd.Parameters.AddWithValue("@SN_GOOD", req.SerialNumber);
                        insCmd.Parameters.AddWithValue("@ShiftMode", shiftMode);

                        await insCmd.ExecuteNonQueryAsync();
                    }
                }

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception OnPostAddSerialAsync (CU): " + ex.ToString());
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }

        public int CountDataAddedToday(SqlConnection connection)
        {
            string countDataSql = "SELECT COUNT(*) FROM OEESN WHERE Date >= @Today AND MachineCode = @MachineCode";

            DateTime referenceTime = DateTime.Now;
            DateTime productionStart = referenceTime.Date.AddHours(7);

            if (referenceTime.Hour < 7)
            {
                productionStart = productionStart.AddDays(-1);
            }

            using (SqlCommand countDataCommand = new SqlCommand(countDataSql, connection))
            {
                countDataCommand.Parameters.AddWithValue("@Today", productionStart);
                countDataCommand.Parameters.AddWithValue("@MachineCode", "MCH1-01");
                return (int)countDataCommand.ExecuteScalar() + 1;
            }
        }

        /// Kelas sederhana untuk menampung waktu mulai dan selesai istirahat.
        private List<RestPeriod> GetRestPeriods(DateTime forDate)
        {
            var periods = new List<RestPeriod>();

            string startColumn;
            string endColumn;

            if (forDate.DayOfWeek == DayOfWeek.Friday)
            {
                startColumn = "BreakTime2Start";
                endColumn = "BreakTime2End";
            }
            else
            {
                startColumn = "BreakTime1Start";
                endColumn = "BreakTime1End";
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    string sql = $"SELECT TOP 1 {startColumn}, {endColumn} FROM AdditionalBreakTimes " +
                                 $"ORDER BY CreatedAt DESC";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Date", forDate.Date);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {

                                if (!reader.IsDBNull(reader.GetOrdinal(startColumn)) && !reader.IsDBNull(reader.GetOrdinal(endColumn)))
                                {
                                    TimeSpan startTime = reader.GetTimeSpan(reader.GetOrdinal(startColumn));
                                    TimeSpan endTime = reader.GetTimeSpan(reader.GetOrdinal(endColumn));

                                    periods.Add(new RestPeriod { Start = startTime, End = endTime });
                                }
                                else
                                {
                                    Console.WriteLine($"Data istirahat (start/end) NULL di DB untuk tanggal {forDate.ToShortDateString()}.");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Tidak ada data waktu istirahat utama ditemukan di database untuk tanggal {forDate.ToShortDateString()}.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading main break time from database: {ex.Message}");
            }

            periods.Add(new RestPeriod { Start = new TimeSpan(6, 55, 0), End = new TimeSpan(7, 7, 0) });
            periods.Add(new RestPeriod { Start = new TimeSpan(9, 30, 0), End = new TimeSpan(9, 35, 0) });
            periods.Add(new RestPeriod { Start = new TimeSpan(14, 30, 0), End = new TimeSpan(14, 35, 0) });

            periods.Add(new RestPeriod { Start = new TimeSpan(15, 45, 0), End = new TimeSpan(16, 0, 0) });

            //shift 2
            periods.Add(new RestPeriod { Start = new TimeSpan(18, 0, 0), End = new TimeSpan(18, 30, 0) });
            periods.Add(new RestPeriod { Start = new TimeSpan(21, 0, 0), End = new TimeSpan(21, 15, 0) });
            periods.Add(new RestPeriod { Start = new TimeSpan(23, 0, 0), End = new TimeSpan(23, 15, 0) });

            //day +1 for night shift
            periods.Add(new RestPeriod { Start = new TimeSpan(3, 0, 0), End = new TimeSpan(3, 45, 0) });
            periods.Add(new RestPeriod { Start = new TimeSpan(4, 45, 0), End = new TimeSpan(5, 0, 0) });

            return periods;
        }

        private DateTime SkipRestTime(DateTime time)
        {
            bool moved;
            do
            {
                moved = false;
                var rests = GetRestPeriods(time.Date);

                foreach (var rest in rests)
                {
                    DateTime rs = time.Date.Add(rest.Start);
                    DateTime re = time.Date.Add(rest.End);

                    if (rest.End < rest.Start)
                        re = re.AddDays(1);

                    if (time >= rs && time < re)
                    {
                        time = re;
                        moved = true;
                        break;
                    }
                }
            }
            while (moved);

            return time;
        }



        public class RestPeriod
        {
            public TimeSpan Start { get; set; }
            public TimeSpan End { get; set; }
        }
        /// Menghitung durasi downtime bersih dalam detik, dengan mengabaikan waktu istirahat.
        private double CalculateNetDowntimeSeconds(DateTime startTime, DateTime endTime)
        {
            if (endTime <= startTime) return 0;

            double excludedSeconds = 0;

            for (var day = startTime.Date; day <= endTime.Date; day = day.AddDays(1))
            {
                var restPeriods = GetRestPeriods(day);

                foreach (var rest in restPeriods)
                {
                    DateTime restStart = day.Add(rest.Start);
                    DateTime restEnd = day.Add(rest.End);

                    if (rest.End < rest.Start)
                    {
                        restEnd = restEnd.AddDays(1);
                    }

                    DateTime overlapStart = restStart > startTime ? restStart : startTime;
                    DateTime overlapEnd = restEnd < endTime ? restEnd : endTime;

                    if (overlapEnd > overlapStart)
                    {
                        excludedSeconds += (overlapEnd - overlapStart).TotalSeconds;
                    }
                }
            }

            return (endTime - startTime).TotalSeconds - excludedSeconds;
        }

    }

    public class ProductInfo
    {
        public string? Product_Id { get; set; }
        public string? Marking { get; set; }
        public string? ProductName { get; set; }
        public string? MachineName { get; set; }
        public string? Description { get; set; }
        public int ProdPlan { get; set; }
        public int SUT { get; set; }
        public int NoOfOperator { get; set; }
        public int QtyHour { get; set; }
        public int ProdHeadHour { get; set; }
        public int CycleTimeVacum { get; set; }
        public int WorkHour { get; set; }
    }

    public class UserInfo
    {
        public DateTime Date { get; set; } = DateTime.Now;
        public DateTime SDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;
        public decimal ProductTime { get; set; }
        public decimal TotalDownTime { get; set; }
        public decimal TargetUnit { get; set; }
        public decimal GoodUnit { get; set; }
        public decimal EjectUnit { get; set; }
        public decimal TotalUnit { get; set; }
        public decimal OEE { get; set; }
        public decimal Availability { get; set; }
        public decimal Performance { get; set; }
        public decimal Quality { get; set; }
        public int CycleTime { get; set; }
        public string? MachineCode { get; set; }
        public string? Product_Id { get; set; }
        public int NoOfOperator { get; set; }
        public decimal P_Target { get; set; }
        public decimal P_Actual { get; set; }
        public decimal IdleTime { get; set; }
        public string? SN_GOOD { get; set; }
        public int ID { get; set; }
    }
}