namespace LfsCruise.Utils;

// Paverčia NplPacket.CarCode formatą (pvz. "5E-5D-C5-00") į SkinId (pvz. "C55D5E"),
// tiksliai taip pat, kaip tai daro NplPacket.Parse.
public static class LfsCarCode
{
    public static string ToSkinId(string carCode)
    {
        var parts = carCode.Split('-');

        if (parts.Length != 4)
            return carCode; // apsauga - jei formatas netikėtas

        try
        {
            var b0 = Convert.ToByte(parts[0], 16);
            var b1 = Convert.ToByte(parts[1], 16);
            var b2 = Convert.ToByte(parts[2], 16);

            var skinIdValue = (b2 << 16) | (b1 << 8) | b0;

            return skinIdValue.ToString("X6");
        }
        catch (FormatException)
        {
            return carCode;
        }
    }
}