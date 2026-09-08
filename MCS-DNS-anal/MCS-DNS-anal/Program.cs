using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace MCS_DNS_anal
{
    class Bazis
    {
        //(név, jel, darabszám, százalék)
        public string nev;
        public char jel;
        public long darabszam;
        public double szazalek;

        public Bazis(string n, char j)
        {
            nev = n;
            jel = j;
            darabszam = 0;
            szazalek = 0.0;
        }

        public void SzazalekSzamol(long teljesMeret)
        {
            szazalek = teljesMeret > 0 ? (double)darabszam / teljesMeret * 100 : 0.0;
        }

        public void kiiratas(CultureInfo hu)
        {
            Console.Write($"{nev,-8}");
            Console.WriteLine($"{darabszam.ToString("N0", hu),10} darab, {szazalek.ToString("F2", hu),6}%");
        }

        public void kiiratasFajlba(StreamWriter ki, CultureInfo hu)
        {
            ki.WriteLine($"{char.ToLower(jel)}\t{darabszam}\t{szazalek.ToString("F2", hu)}%");
        }
    }

    class Program
    {
        static List<string> szignifikansEltresek(List<Bazis> blista, double atlag)
        {
            List<string> uzenetek = new List<string>();

            foreach (Bazis b in blista)
            {
                if (atlag > 0)
                {
                    double relEltres = Math.Abs(b.darabszam - atlag) / atlag;
                    if (relEltres > 0.05)
                    {
                        uzenetek.Add($"A(z) {b.nev} bázis száma szignifikánsan eltér az átlagtól ({(relEltres * 100).ToString("F2", CultureInfo.GetCultureInfo("hu-HU"))}%).");
                    }
                }
            }

            return uzenetek;
        }

        static void oszlopDiagramKiir(List<Bazis> blista)
        {
            int cimkeSzelesseg = 0;
            foreach (Bazis b in blista)
                cimkeSzelesseg = Math.Max(cimkeSzelesseg, b.nev.Length);

            int konzolSzelesseg;
            try { konzolSzelesseg = Console.BufferWidth; }
            catch { konzolSzelesseg = 0; }
            if (konzolSzelesseg <= 0) konzolSzelesseg = 120;

            int maxOszlopSzelesseg = Math.Max(10, konzolSzelesseg - cimkeSzelesseg - 3);

            long maxDb = 0;
            foreach (Bazis b in blista)
                maxDb = Math.Max(maxDb, b.darabszam);

            foreach (Bazis b in blista)
            {
                int csillagszam = maxDb > 0
                    ? (int)Math.Round((double)b.darabszam / maxDb * maxOszlopSzelesseg)
                    : 0;

                Console.WriteLine($"{b.nev.PadRight(cimkeSzelesseg)}: {new string('*', csillagszam)}");
            }
        }

        static void Main(string[] args)
        {
            #region fejlec
            /*
            MCS - DNS analízis
            MCS - 2025.xx.xx.
            */
            string fejlec = "MCS - DNS analízis";
            Console.WriteLine(fejlec);
            for (int i = 0; i < fejlec.Length; i++) Console.Write('-');
            Console.WriteLine();
            #endregion

            CultureInfo hu = CultureInfo.GetCultureInfo("hu-HU");

            #region bazisok_letrehozasa
            List<Bazis> bazisok = new List<Bazis>
            {
                new Bazis("adenin",  'A'),
                new Bazis("guanin",  'G'),
                new Bazis("citozin", 'C'),
                new Bazis("timin",   'T')
            };
            #endregion

            #region fajlbeolvasas
            // Nem tároljuk egyidejűleg a teljes állományt a memóriában,
            // csak az éppen beolvasott sort dolgozzuk fel, majd eldobjuk.
            string sor;
            string fejlecSor;
            long teljesMeret = 0;

            StreamReader be = new StreamReader("dna.txt");

            fejlecSor = be.ReadLine();   // az első sor NEM adat

            sor = be.ReadLine();
            while (sor != null)
            {
                foreach (char ch in sor)
                {
                    foreach (Bazis b in bazisok)
                    {
                        if (b.jel == ch)
                        {
                            b.darabszam++;
                            teljesMeret++;
                            break;
                        }
                    }
                }

                sor = be.ReadLine();
            }

            be.Close();

            foreach (Bazis b in bazisok)
                b.SzazalekSzamol(teljesMeret);
            #endregion

            #region feladat1_teljesmeret
            Console.WriteLine("\n1.feladat:");
            Console.WriteLine($"A teljes méret: {teljesMeret.ToString("N0", hu)}");
            #endregion

            #region feladat2_bazisstatisztika
            Console.WriteLine("\n2.feladat:");
            foreach (Bazis b in bazisok)
                b.kiiratas(hu);
            #endregion

            #region feladat3_szignifikanciavizsgalat
            Console.WriteLine("\n3.feladat:");

            double atlag = teljesMeret / 4.0;
            List<string> eltresek = szignifikansEltresek(bazisok, atlag);

            if (eltresek.Count == 0)
                Console.WriteLine("Nincs jelentős eltérés a darabszámokban.");
            else
                foreach (string uzenet in eltresek)
                    Console.WriteLine(uzenet);
            #endregion

            #region feladat4_oszlopdiagram
            Console.WriteLine("\n4.feladat:");
            oszlopDiagramKiir(bazisok);
            #endregion

            #region feladat5_fajlkiiras
            Console.WriteLine("\n5.feladat:");
            Console.Write("file-kiírás ... ");

            StreamWriter ki = new StreamWriter("DNA-result.txt", false, Encoding.UTF8);

            ki.WriteLine(fejlecSor);
            foreach (Bazis b in bazisok)
                b.kiiratasFajlba(ki, hu);

            ki.Close();

            Console.WriteLine("kész!");

            Console.WriteLine("\nA DNA-result.txt fájl tartalma:");
            StreamReader ellenorzo = new StreamReader("DNA-result.txt");
            string ellSor = ellenorzo.ReadLine();
            while (ellSor != null)
            {
                Console.WriteLine(ellSor);
                ellSor = ellenorzo.ReadLine();
            }
            ellenorzo.Close();
            #endregion

            Console.ReadLine();
        }
    }
}