using System;

class Program
{
    static void Main()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("  TEST LOGIKA BARU LOSS TIME");
        Console.WriteLine("  Aturan:");
        Console.WriteLine("  - Threshold = CycleTime x 3");
        Console.WriteLine("  - Jika LossTime <= Threshold => Duration = 0 (tidak dicatat)");
        Console.WriteLine("  - Jika LossTime > Threshold  => Duration = LossTime - 1 CycleTime");
        Console.WriteLine("========================================\n");

        // ============================================
        // TEST CASE 1: Contoh dari user (LossTime 100, Cycle 25)
        // ============================================
        TestCase(
            testNumber: 1,
            description: "Contoh dari user: LossTime 100 detik, CycleTime 25 detik",
            netDowntimeSeconds: 100,
            cycleTime: 25,
            expectedShouldLog: true,
            expectedDuration: 75
        );

        // ============================================
        // TEST CASE 2: LossTime PERSIS sama dengan threshold (75 detik, cycle 25)
        // Harusnya TIDAK dicatat (0)
        // ============================================
        TestCase(
            testNumber: 2,
            description: "LossTime = Threshold (75 detik, CycleTime 25)",
            netDowntimeSeconds: 75,
            cycleTime: 25,
            expectedShouldLog: false,
            expectedDuration: 0
        );

        // ============================================
        // TEST CASE 3: LossTime DIBAWAH threshold (50 detik, cycle 25)
        // Harusnya TIDAK dicatat (0)
        // ============================================
        TestCase(
            testNumber: 3,
            description: "LossTime < Threshold (50 detik, CycleTime 25)",
            netDowntimeSeconds: 50,
            cycleTime: 25,
            expectedShouldLog: false,
            expectedDuration: 0
        );

        // ============================================
        // TEST CASE 4: LossTime sedikit diatas threshold (76 detik, cycle 25)
        // Harusnya dicatat: 76 - 25 = 51
        // ============================================
        TestCase(
            testNumber: 4,
            description: "LossTime sedikit > Threshold (76 detik, CycleTime 25)",
            netDowntimeSeconds: 76,
            cycleTime: 25,
            expectedShouldLog: true,
            expectedDuration: 51
        );

        // ============================================
        // TEST CASE 5: CycleTime berbeda (cycle 40 detik, loss 200 detik)
        // Threshold = 40 x 3 = 120. 200 > 120, jadi dicatat: 200 - 40 = 160
        // ============================================
        TestCase(
            testNumber: 5,
            description: "Model lain: LossTime 200 detik, CycleTime 40 detik",
            netDowntimeSeconds: 200,
            cycleTime: 40,
            expectedShouldLog: true,
            expectedDuration: 160
        );

        // ============================================
        // TEST CASE 6: CycleTime berbeda (cycle 40 detik, loss 120 detik)
        // Threshold = 40 x 3 = 120. 120 <= 120, jadi TIDAK dicatat
        // ============================================
        TestCase(
            testNumber: 6,
            description: "Model lain: LossTime 120 detik (= threshold), CycleTime 40 detik",
            netDowntimeSeconds: 120,
            cycleTime: 40,
            expectedShouldLog: false,
            expectedDuration: 0
        );

        // ============================================
        // TEST CASE 7: Data dari screenshot user (Duration 287, cycle?)
        // Misalkan CycleTime = 25, maka threshold = 75
        // Net downtime asli seharusnya = 287 + 25 = 312 (jika logic baru jalan)
        // Atau 287 apa adanya (jika logic lama masih jalan)
        // ============================================
        TestCase(
            testNumber: 7,
            description: "Simulasi screenshot: net downtime 312, CycleTime 25",
            netDowntimeSeconds: 312,
            cycleTime: 25,
            expectedShouldLog: true,
            expectedDuration: 287
        );

        // ============================================
        // TEST CASE 8: CycleTime = 0 (edge case, operator 0)
        // Threshold = 0 x 3 = 0. Loss > 0 => dicatat penuh
        // ============================================
        TestCase(
            testNumber: 8,
            description: "Edge Case: CycleTime 0 (operator kosong), LossTime 100",
            netDowntimeSeconds: 100,
            cycleTime: 0,
            expectedShouldLog: true,
            expectedDuration: 100
        );

        Console.WriteLine("\n========================================");
        Console.WriteLine("  SEMUA TEST SELESAI!");
        Console.WriteLine("========================================");
    }

    static void TestCase(int testNumber, string description, double netDowntimeSeconds, int cycleTime,
                          bool expectedShouldLog, double expectedDuration)
    {
        Console.WriteLine($"--- TEST CASE {testNumber}: {description} ---");
        Console.WriteLine($"  Input: netDowntimeSeconds = {netDowntimeSeconds}, cycleTime = {cycleTime}");

        // === LOGIKA BARU (PERSIS SAMA DENGAN KODE DI CS & CU) ===
        double thresholdSeconds = cycleTime * 3;
        bool shouldLog = false;
        double finalDuration = 0;

        if (netDowntimeSeconds > thresholdSeconds)
        {
            double pureLossSeconds = netDowntimeSeconds - cycleTime;
            if (pureLossSeconds > 0)
            {
                shouldLog = true;
                finalDuration = pureLossSeconds;
            }
        }
        // Jika <= threshold, shouldLog tetap false, finalDuration tetap 0
        // === AKHIR LOGIKA ===

        Console.WriteLine($"  Threshold (CycleTime x 3) = {thresholdSeconds}");
        Console.WriteLine($"  netDowntime ({netDowntimeSeconds}) > threshold ({thresholdSeconds})? => {(netDowntimeSeconds > thresholdSeconds ? "YA" : "TIDAK")}");
        Console.WriteLine($"  Hasil: shouldLog = {shouldLog}, Duration = {finalDuration}");
        Console.WriteLine($"  Expected: shouldLog = {expectedShouldLog}, Duration = {expectedDuration}");

        bool passed = (shouldLog == expectedShouldLog) && (finalDuration == expectedDuration);
        Console.ForegroundColor = passed ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"  >>> {(passed ? "PASSED ✅" : "FAILED ❌")}");
        Console.ResetColor();
        Console.WriteLine();
    }
}
