namespace LfsCruise.Core.UI;

public static class ClickIds
{
    public static class MenuCategory
    {
        public const byte Start = 1;
        public const byte End = 8;
    }

    public static class Hud
    {
        public const byte License = 50;
        public const byte Distance = 51;
        public const byte Money = 52;
        public const byte Discord = 53;
        public const byte Server = 54;
        public const byte Menu = 55;
        public const byte JobStatus = 56;
        public const byte GarageTow = 57;
        public const byte Wanted = 58;
        public const byte PoliceMenu = 59;
    }
    public static class Menu
    {
        public const byte Background = 60;
        public const byte Title = 61;
        public const byte Close = 62;
        public const byte Back = 63;

        public const byte Profile = 64;
        public const byte Shop = 65;
        public const byte Top10 = 66;
        public const byte Statistics = 67;
    }
    public static class Content
    {
        public const byte Start = 80;
        public const byte End = 119;

        public const byte PrevPage = 110;
        public const byte NextPage = 111;
        public const byte PageInfo = 112;
    }

    public static class Shop
    {
        public const byte CategoryStart = Content.Start;
        public const byte CategoryEnd = 99;

        public const byte VehicleStart = Content.Start;
        public const byte VehicleEnd = 84;

        public const byte PreviousPage = Content.PrevPage;
        public const byte NextPage = Content.NextPage;
        public const byte PageInfo = Content.PageInfo;
    }

    public static class Bank
    {
        public const byte Menu = 69;

        public const byte Balance = Content.Start;
        public const byte InterestInfo = 81;
        public const byte Deposit = 82;
        public const byte Withdraw = 83;
        public const byte History = 84;

        public const byte HistoryEntryStart = 85;
        public const byte HistoryEntryLast = 89;

        public const byte HistoryPrev = Content.PrevPage;
        public const byte HistoryNext = Content.NextPage;
        public const byte HistoryPageInfo = Content.PageInfo;
    }

    public static class Regitra
    {
        public const byte MenuButton = 72;

        public const byte Status = Content.Start;
        public const byte PlateStatus = 81;
        public const byte BuyPlate = 82;
        public const byte ChangePlate = 83;
        public const byte InsuranceStatus = 84;
        public const byte BuyInsurance = 85;
        public const byte InspectionStatus = 86;
        public const byte BuyInspection = 87;
    }

    public static class Market
    {
        public const byte MenuButton = 70;

        public const byte CategoryStart = Content.Start;
        public const byte CategoryEnd = 83;

        public const byte ListingInfoStart = 84;
        public const byte ListingInfoEnd = 88;

        public const byte ListingBuyStart = 89;
        public const byte ListingBuyEnd = 93;

        public const byte PrevPage = Content.PrevPage;
        public const byte NextPage = Content.NextPage;
        public const byte PageInfo = Content.PageInfo;
    }

    public static class Starter
    {
        public const byte CarStart = Content.Start;
        public const byte CarEnd = 89;
    }

    public static class Jobs
    {
        public const byte Menu = 71;

        public const byte Taxi = Content.Start;
        public const byte Delivery = 81;

        public const byte Join = 82;
        public const byte Leave = 83;
        public const byte Description = 84;
    }

    public static class Garage
    {
        public const byte MenuButton = 68;

        public const byte VehicleStart = Content.Start;
        public const byte VehicleEnd = 84;

        public const byte PrevPage = Content.PrevPage;
        public const byte NextPage = Content.NextPage;
        public const byte PageInfo = Content.PageInfo;

        public const byte SellToServer = 82;
        public const byte SellOnMarket = 83;
    }

    // NAUJA - policijos meniu puslapiai (Shop/Bank/Market stiliaus - bendras
    // Content diapazonas, nes tai standartiniai MenuPage'ai, niekada
    // nerodomi vienu metu su kitais meniu).
    public static class Police
    {
        public const byte MenuFines = Content.Start;
        public const byte MenuDocs = 81;
        public const byte MenuPursuits = 82;

        public const byte TargetStart = Content.Start;
        public const byte TargetEnd = 89;

        public const byte ViolationStart = Content.Start;
        public const byte ViolationEnd = 94;
        public const byte FineConfirm = 95;
        public const byte DocsIssueFine = 96;

        public const byte PursuitEntryStart = Content.Start;
        public const byte PursuitEntryEnd = 94;

        // Radaras - VISADA matomas kartu su kitais elementais (kaip GPS),
        // tad NEGALI naudoti bendro Content diapazono - reikia sava, unikalu.
        public const byte RadarBackground = 160;
        public const byte RadarRowStart = 161;
        public const byte RadarRowEnd = 163;
    }

    public static class Gps
    {
        public const byte Background = 130;
        public const byte TopLeft = 131;
        public const byte Top = 132;
        public const byte TopRight = 133;
        public const byte Left = 134;
        public const byte Center = 135;
        public const byte Right = 136;
        public const byte BottomLeft = 137;
        public const byte Bottom = 138;
        public const byte BottomRight = 139;
        public const byte Info = 140;
    }
}