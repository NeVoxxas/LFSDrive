using System.Diagnostics;

namespace LfsCruise.Core.UI;

public static class ClickIds
{
    public static class Hud
    {
        public const byte License = 50;
        public const byte Distance = 51;
        public const byte Money = 52;
        public const byte Discord = 53;
        public const byte Server = 54;
        public const byte Menu = 55;
        public const byte JobStatus = 56;
    }

    public static class Menu
    {
        public const byte Background = 100;
        public const byte Title = 101;
        public const byte Close = 102;
        public const byte Profile = 103;
        public const byte Shop = 104;
        public const byte Top10 = 105;
        public const byte Statistics = 106;
        public const byte Back = 107;
    }

    public static class Jobs
    {
        public const byte Menu = 108;

        public const byte Taxi = 234;
        public const byte Delivery = 235;

        public const byte Join = 236;
        public const byte Leave = 237;
    }
    public static class Bank
    {
        public const byte Menu = 109;

        public const byte Balance = 110;
        public const byte InterestInfo = 111;
        public const byte Deposit = 112;
        public const byte Withdraw = 113;
        public const byte History = 114;

        public const byte HistoryPrev = 115;
        public const byte HistoryNext = 116;
        public const byte HistoryPageInfo = 117;

        public const byte HistoryEntryStart = 118;
        public const byte HistoryEntryLast = 122;
    }

    public static class Regitra
    {
        public const byte MenuButton = 123;
        public const byte Status = 124;
        public const byte PlateStatus = 125;
        public const byte BuyPlate = 126;
        public const byte ChangePlate = 127;
        public const byte InsuranceStatus = 128;
        public const byte BuyInsurance = 129;
        public const byte InspectionStatus = 130;
        public const byte BuyInspection = 131;
    }

    public static class Market
    {
        public const byte MenuButton = 132;
        public const byte CategoryStart = 133;
        public const byte CategoryEnd = 136;
        public const byte ListingStart = 137;
        public const byte ListingEnd = 141;
        public const byte PrevPage = 142;
        public const byte NextPage = 143;
        public const byte PageInfo = 144;
    }

    public static class Shop
    {
        public const byte CategoryStart = 150;
        public const byte CategoryEnd = 179;
        public const byte VehicleStart = 180;
        public const byte VehicleEnd = 219;
        public const byte PreviousPage = 220;
        public const byte NextPage = 221;
        public const byte PageInfo = 222;
    }

    public static class Gps
    {
        public const byte Background = 223;
        public const byte TopLeft = 224;
        public const byte Top = 225;
        public const byte TopRight = 226;
        public const byte Left = 227;
        public const byte Center = 228;
        public const byte Right = 229;
        public const byte BottomLeft = 230;
        public const byte Bottom = 231;
        public const byte BottomRight = 232;
        public const byte Info = 233;
    }
}