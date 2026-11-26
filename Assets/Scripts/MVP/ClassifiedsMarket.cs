using UnityEngine;

namespace DripKings.MVP
{
    /// <summary>
    /// Single-player prototype ledger for classified purchases, installations, and job applications.
    /// PlayerPrefs is NOT suitable for a real multiplayer or token-based economy.
    /// </summary>
    public static class ClassifiedsMarket
    {
        private const string Prefix = "DripKings.Classifieds.";
        private const string CreditsKey = "DripKings.MVP.Credits";
        private const string ShopKey = Prefix + "MechanicShop";
        private const string AppliedKey = Prefix + "MechanicApplication";

        public const int MechanicShopPrice = 500;
        public const int CustomPaintPrice = 25;

        public struct Part
        {
            public readonly string Name;
            public readonly string Description;
            public readonly int Price;
            public readonly int Labor;

            public Part(string name, string description, int price, int labor)
            {
                Name = name;
                Description = description;
                Price = price;
                Labor = labor;
            }
        }

        private static readonly Part[] Catalog =
        {
            new Part("Street Intake", "Entry-level engine airflow upgrade", 60, 20),
            new Part("Sport Brake Kit", "Better stopping hardware", 85, 30),
            new Part("Performance Tires", "Street-grip wheel package", 110, 25)
        };

        public static int PartCount => Catalog.Length;
        public static Part GetPart(int index) => IsValid(index) ? Catalog[index] : default;
        public static int Credits => LocalGarage.SavedCredits;
        public static bool OwnsShop => PlayerPrefs.GetInt(ShopKey, 0) == 1;
        public static bool MechanicApplicationSent => PlayerPrefs.GetInt(AppliedKey, 0) == 1;
        public static bool OwnsPart(int index) =>
            IsValid(index) && PlayerPrefs.GetInt(PartKey(index), 0) == 1;
        public static bool InstalledPart(int index) =>
            IsValid(index) && PlayerPrefs.GetInt(InstalledKey(index), 0) == 1;

        // Owning a mechanic shop unlocks wholesale discounts and the skill to self-install.
        public static int PartPrice(int index)
        {
            if (!IsValid(index)) return 0;
            int retail = Catalog[index].Price;
            return OwnsShop ? (retail * 3 + 3) / 4 : retail; // 25% wholesale discount; round up.
        }

        public static int LaborPrice(int index)
        {
            if (!IsValid(index)) return 0;
            return OwnsShop ? 0 : Catalog[index].Labor;
        }

        public static bool BuyPart(int index)
        {
            if (!IsValid(index) || OwnsPart(index)) return false;
            int price = PartPrice(index);
            if (!TrySpend(price)) return false;
            PlayerPrefs.SetInt(PartKey(index), 1);
            PlayerPrefs.Save();
            return true;
        }

        public static bool PayToInstall(int index)
        {
            if (!IsValid(index) || !OwnsPart(index) || InstalledPart(index)) return false;
            if (!TrySpend(LaborPrice(index))) return false;
            PlayerPrefs.SetInt(InstalledKey(index), 1);
            PlayerPrefs.Save();
            return true;
        }

        public static bool BuyMechanicShop()
        {
            if (OwnsShop || !TrySpend(MechanicShopPrice)) return false;
            PlayerPrefs.SetInt(ShopKey, 1);
            PlayerPrefs.Save();
            return true;
        }

        public static bool ApplyForMechanicJob()
        {
            if (MechanicApplicationSent) return false;
            PlayerPrefs.SetInt(AppliedKey, 1);
            PlayerPrefs.Save();
            return true;
        }

        private static bool TrySpend(int cost)
        {
            if (cost < 0 || Credits < cost) return false;
            int remaining = Credits - cost;
            PlayerPrefs.SetInt(CreditsKey, remaining);
            PlayerPrefs.Save();
            return true;
        }

        private static bool IsValid(int index) => index >= 0 && index < Catalog.Length;
        private static string PartKey(int index) => Prefix + "PartOwned." + index;
        private static string InstalledKey(int index) => Prefix + "PartInstalled." + index;
    }
}
