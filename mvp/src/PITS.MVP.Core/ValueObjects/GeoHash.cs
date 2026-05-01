namespace PITS.MVP.Core.ValueObjects;

public static class GeoHash
{
    private static readonly char[] Base32Chars = 
        "0123456789bcdefghjkmnpqrstuvwxyz".ToCharArray();

    public static string Encode(double latitude, double longitude, int precision = 8)
    {
        if (precision < 1 || precision > 12)
            throw new ArgumentOutOfRangeException(nameof(precision), "Precision must be between 1 and 12");

        var hash = new char[precision];
        int bit = 0, ch = 0;
        double[] latRange = { -90.0, 90.0 };
        double[] lonRange = { -180.0, 180.0 };
        bool even = true;

        for (int i = 0; i < precision;)
        {
            if (even)
            {
                double mid = (lonRange[0] + lonRange[1]) / 2;
                if (longitude >= mid)
                {
                    ch |= 1 << (4 - bit);
                    lonRange[0] = mid;
                }
                else
                {
                    lonRange[1] = mid;
                }
            }
            else
            {
                double mid = (latRange[0] + latRange[1]) / 2;
                if (latitude >= mid)
                {
                    ch |= 1 << (4 - bit);
                    latRange[0] = mid;
                }
                else
                {
                    latRange[1] = mid;
                }
            }

            even = !even;
            bit++;

            if (bit == 5)
            {
                hash[i++] = Base32Chars[ch];
                bit = 0;
                ch = 0;
            }
        }

        return new string(hash);
    }

    public static (double Latitude, double Longitude) Decode(string geohash)
    {
        if (string.IsNullOrEmpty(geohash))
            throw new ArgumentNullException(nameof(geohash));

        double[] latRange = { -90.0, 90.0 };
        double[] lonRange = { -180.0, 180.0 };
        bool even = true;

        foreach (char c in geohash)
        {
            int cd = Array.IndexOf(Base32Chars, c);
            if (cd < 0)
                throw new ArgumentException($"Invalid character '{c}' in geohash", nameof(geohash));

            for (int i = 4; i >= 0; i--)
            {
                int mask = 1 << i;
                if (even)
                {
                    if ((cd & mask) != 0)
                        lonRange[0] = (lonRange[0] + lonRange[1]) / 2;
                    else
                        lonRange[1] = (lonRange[0] + lonRange[1]) / 2;
                }
                else
                {
                    if ((cd & mask) != 0)
                        latRange[0] = (latRange[0] + latRange[1]) / 2;
                    else
                        latRange[1] = (latRange[0] + latRange[1]) / 2;
                }
                even = !even;
            }
        }

        return ((latRange[0] + latRange[1]) / 2, (lonRange[0] + lonRange[1]) / 2);
    }
}
