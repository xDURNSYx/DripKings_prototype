using System;
using UnityEngine;

namespace DripKings.MVP
{
    /// <summary>Offline Gazette storefront, designed to be replaced by server-owned listings later.</summary>
    public sealed class DripKingsNewspaperView : MonoBehaviour
    {
        private enum Section { Parts, Services, Opportunities }
        private Section section = Section.Parts;
        private string notice = "Browse local test listings. All prices are game credits.";
        private static readonly Color Gold = new Color32(244, 171, 58, 255);
        private static readonly Color Cream = new Color32(244, 237, 220, 255);
        private static readonly Color Muted = new Color32(177, 187, 204, 255);
        private static readonly Color Panel = new Color32(33, 44, 63, 255);

        public void Draw(Action buyPaint)
        {
            Caption(new Rect(205, 301, 570, 27), "CLASSIFIEDS  /  DRIP KINGS GAZETTE", 17, Cream, FontStyle.Bold);
            Caption(new Rect(842, 301, 240, 27), ClassifiedsMarket.Credits + " CR AVAILABLE", 17, Gold, FontStyle.Bold, TextAnchor.MiddleRight);
            NavButton(new Rect(205, 334, 270, 36), "PARTS FOR SALE", Section.Parts);
            NavButton(new Rect(487, 334, 270, 36), "MECHANIC SERVICES", Section.Services);
            NavButton(new Rect(769, 334, 300, 36), "JOBS + PROPERTY", Section.Opportunities);

            switch (section)
            {
                case Section.Parts: DrawParts(); break;
                case Section.Services: DrawServices(buyPaint); break;
                case Section.Opportunities: DrawOpportunities(); break;
            }

            Caption(new Rect(364, 542, 726, 35), notice, 15, Gold);
        }

        private void DrawParts()
        {
            for (int i = 0; i < ClassifiedsMarket.PartCount; i++)
            {
                var part = ClassifiedsMarket.GetPart(i);
                int index = i;
                int y = 378 + i * 54;
                Box(new Rect(365, y, 710, 50));
                Caption(new Rect(381, y + 4, 400, 24), part.Name, 19, Cream, FontStyle.Bold);
                Caption(new Rect(382, y + 27, 445, 18), part.Description, 13, Muted);
                string text = ClassifiedsMarket.InstalledPart(index) ? "INSTALLED"
                    : ClassifiedsMarket.OwnsPart(index) ? "INSTALL " + ClassifiedsMarket.LaborPrice(index) + " CR"
                    : "BUY " + ClassifiedsMarket.PartPrice(index) + " CR";
                bool completed = ClassifiedsMarket.InstalledPart(index);
                if (completed)
                    Caption(new Rect(892, y + 8, 170, 33), "INSTALLED", 16, Gold, FontStyle.Bold, TextAnchor.MiddleCenter);
                else if (GUI.Button(new Rect(862, y + 8, 200, 34), text))
                {
                    if (ClassifiedsMarket.OwnsPart(index))
                        notice = ClassifiedsMarket.PayToInstall(index)
                            ? part.Name + " installed. Labor charged in credits."
                            : "Installation unavailable: insufficient credits or already installed.";
                    else
                        notice = ClassifiedsMarket.BuyPart(index)
                            ? "Purchased " + part.Name + ". Choose INSTALL to pay mechanic labor."
                            : "Purchase unavailable: check credits or ownership.";
                }
            }
        }

        private void DrawServices(Action buyPaint)
        {
            Box(new Rect(365, 380, 710, 67));
            Caption(new Rect(381, 387, 410, 30), "CUSTOM PAINT STUDIO", 20, Cream, FontStyle.Bold);
            Caption(new Rect(381, 417, 442, 20), "Apply your next Garage paint finish.", 14, Muted);
            if (GUI.Button(new Rect(866, 390, 196, 41), "PAINT 25 CR"))
            {
                int before = ClassifiedsMarket.Credits;
                buyPaint?.Invoke();
                notice = ClassifiedsMarket.Credits < before
                    ? "Paint service purchased. Your Garage and race car use the saved finish."
                    : "Paint order unavailable: insufficient credits.";
            }

            Box(new Rect(365, 457, 710, 66));
            Caption(new Rect(381, 464, 430, 28), "HIRE A MECHANIC", 20, Cream, FontStyle.Bold);
            Caption(new Rect(381, 493, 450, 20), "Buy a part first, then pay labor to install.", 14, Muted);
            if (GUI.Button(new Rect(866, 469, 196, 41), "VIEW PARTS"))
            {
                section = Section.Parts;
                notice = "Owned parts can be installed for the listed labor fee.";
            }
        }

        private void DrawOpportunities()
        {
            Box(new Rect(365, 380, 710, 67));
            Caption(new Rect(381, 387, 480, 28), "MECHANIC POSITION OPEN", 20, Cream, FontStyle.Bold);
            Caption(new Rect(381, 415, 465, 22), "Apply locally; work and wages are future features.", 14, Muted);
            if (ClassifiedsMarket.MechanicApplicationSent)
                Caption(new Rect(855, 391, 203, 37), "APPLIED", 18, Gold, FontStyle.Bold, TextAnchor.MiddleCenter);
            else if (GUI.Button(new Rect(866, 393, 196, 39), "APPLY"))
                notice = ClassifiedsMarket.ApplyForMechanicJob()
                    ? "Application saved in your local profile."
                    : "Application already recorded.";

            Box(new Rect(365, 457, 710, 66));
            Caption(new Rect(381, 464, 468, 29), "MECHANIC SHOP FOR SALE", 20, Cream, FontStyle.Bold);
            Caption(new Rect(381, 495, 468, 21), "Own the shop: wholesale parts + self-install skills.", 14, Muted);
            if (ClassifiedsMarket.OwnsShop)
                Caption(new Rect(855, 471, 205, 34), "SHOP OWNED", 17, Gold, FontStyle.Bold, TextAnchor.MiddleCenter);
            else if (GUI.Button(new Rect(866, 469, 196, 41), "BUY 500 CR"))
                notice = ClassifiedsMarket.BuyMechanicShop()
                    ? "Shop purchased! Wholesale prices and self-install unlocked."
                    : "Not enough local credits to purchase this shop.";
        }

        private void NavButton(Rect rect, string title, Section tab)
        {
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = section == tab ? Gold : new Color32(74, 86, 104, 255);
            if (GUI.Button(rect, title))
            {
                section = tab;
                notice = tab == Section.Parts ? "Select BUY, then INSTALL when the part is owned."
                    : tab == Section.Services ? "Book paint or pay to install a purchased part."
                    : "Apply for a role or purchase an in-game shop.";
            }
            GUI.backgroundColor = old;
        }

        private static void Box(Rect area)
        {
            Color old = GUI.color;
            GUI.color = Panel;
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private static void Caption(Rect area, string text, int size, Color color,
            FontStyle weight = FontStyle.Normal, TextAnchor align = TextAnchor.MiddleLeft)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = size,
                fontStyle = weight,
                alignment = align,
                wordWrap = false
            };
            style.normal.textColor = color;
            GUI.Label(area, text, style);
        }
    }
}
