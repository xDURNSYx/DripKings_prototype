#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using DripKings.MVP;
using UnityEditor;
using UnityEngine;

namespace DripKings.EditorTools
{
    /// <summary>Exercises the local classified ledger while restoring every existing player save key.</summary>
    public static class ClassifiedsSmokeTests
    {
        private struct SavedValue
        {
            public bool Existed;
            public int Value;
        }

        [MenuItem("Drip Kings/MVP/Run Newspaper Commerce Tests")]
        public static void Run()
        {
            var keys = new List<string>
            {
                "DripKings.MVP.Credits",
                "DripKings.MVP.Color",
                "DripKings.Classifieds.MechanicShop",
                "DripKings.Classifieds.MechanicApplication"
            };
            for (int i = 0; i < ClassifiedsMarket.PartCount; i++)
            {
                keys.Add("DripKings.Classifieds.PartOwned." + i);
                keys.Add("DripKings.Classifieds.PartInstalled." + i);
            }

            var snapshot = new Dictionary<string, SavedValue>();
            foreach (string key in keys)
                snapshot[key] = new SavedValue
                {
                    Existed = PlayerPrefs.HasKey(key),
                    Value = PlayerPrefs.GetInt(key, 0)
                };

            int assertions = 0;
            try
            {
                foreach (string key in keys) PlayerPrefs.DeleteKey(key);
                PlayerPrefs.SetInt("DripKings.MVP.Credits", 200);
                PlayerPrefs.Save();

                Assert(ClassifiedsMarket.PartCount == 3, "catalog has three real listings", ref assertions);
                Assert(!ClassifiedsMarket.BuyPart(-1) && !ClassifiedsMarket.PayToInstall(99),
                    "invalid listing rejected", ref assertions);
                Assert(ClassifiedsMarket.BuyPart(0), "buy intake", ref assertions);
                Assert(ClassifiedsMarket.Credits == 140 && ClassifiedsMarket.OwnsPart(0),
                    "purchase debits balance and persists ownership", ref assertions);
                Assert(!ClassifiedsMarket.BuyPart(0) && ClassifiedsMarket.Credits == 140,
                    "duplicate purchase cannot charge again", ref assertions);
                Assert(!ClassifiedsMarket.PayToInstall(1), "cannot install an unowned part", ref assertions);
                Assert(ClassifiedsMarket.PayToInstall(0), "hire mechanic to install intake", ref assertions);
                Assert(ClassifiedsMarket.Credits == 120 && ClassifiedsMarket.InstalledPart(0),
                    "labor paid and installation persists", ref assertions);
                Assert(!ClassifiedsMarket.PayToInstall(0) && ClassifiedsMarket.Credits == 120,
                    "duplicate labor payment rejected", ref assertions);
                Assert(ClassifiedsMarket.BuyPart(1) && ClassifiedsMarket.Credits == 35,
                    "second item purchased", ref assertions);
                Assert(!ClassifiedsMarket.BuyPart(2) && ClassifiedsMarket.Credits == 35,
                    "unaffordable part does not charge", ref assertions);
                Assert(!ClassifiedsMarket.BuyMechanicShop(),
                    "unaffordable shop cannot be owned", ref assertions);
                Assert(ClassifiedsMarket.ApplyForMechanicJob() &&
                       ClassifiedsMarket.MechanicApplicationSent, "job application saved", ref assertions);
                Assert(!ClassifiedsMarket.ApplyForMechanicJob(),
                    "duplicate application rejected", ref assertions);

                PlayerPrefs.SetInt("DripKings.MVP.Credits", 600);
                PlayerPrefs.Save();
                Assert(ClassifiedsMarket.BuyMechanicShop() &&
                       ClassifiedsMarket.Credits == 100 && ClassifiedsMarket.OwnsShop,
                    "shop purchase costs 500 and saves ownership", ref assertions);
                Assert(!ClassifiedsMarket.BuyMechanicShop(),
                    "cannot purchase shop twice", ref assertions);
                Assert(ClassifiedsMarket.PartPrice(2) == 83 &&
                       ClassifiedsMarket.LaborPrice(2) == 0,
                    "shop unlocks wholesale prices and installation skill", ref assertions);
                Assert(ClassifiedsMarket.BuyPart(2) && ClassifiedsMarket.Credits == 17,
                    "wholesale price applied", ref assertions);
                Assert(ClassifiedsMarket.PayToInstall(2) &&
                       ClassifiedsMarket.Credits == 17 && ClassifiedsMarket.InstalledPart(2),
                    "owned shop installs for zero labor charge", ref assertions);
                Assert(!ClassifiedsMarket.BuyPart(2) && ClassifiedsMarket.Credits == 17,
                    "discounted duplicate purchase rejected", ref assertions);

                Debug.Log("DRIP_KINGS_CLASSIFIEDS_PASS: " + assertions + " assertions passed");
            }
            catch (Exception exception)
            {
                Debug.LogError("DRIP_KINGS_CLASSIFIEDS_FAIL: " + exception);
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                throw;
            }
            finally
            {
                foreach (var item in snapshot)
                {
                    if (item.Value.Existed) PlayerPrefs.SetInt(item.Key, item.Value.Value);
                    else PlayerPrefs.DeleteKey(item.Key);
                }
                PlayerPrefs.Save();
            }
        }

        private static void Assert(bool condition, string message, ref int total)
        {
            if (!condition) throw new InvalidOperationException(message);
            total++;
        }
    }
}
#endif
